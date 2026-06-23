using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace BgLight
{
    public static class SystemInfoCollector
    {
        public static SystemInfoData Collect()
        {
            var data = new SystemInfoData();

            // .NET fallbacks guaranteed non-empty
            try { data.ComputerName = Environment.MachineName; } catch { }
            try { data.User = Environment.UserDomainName + "\\" + Environment.UserName; } catch { }
            try { data.GeneratedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture); } catch { }

            ulong totalRamBytes = 0;
            ulong freeRamBytes = 0;

            // Win32_ComputerSystem: computer name, domain/workgroup, total RAM
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem"))
                using (var results = searcher.Get())
                {
                    foreach (ManagementObject mo in results)
                    {
                        using (mo)
                        {
                            var name = mo["Name"] as string;
                            if (!string.IsNullOrWhiteSpace(name)) data.ComputerName = name;

                            bool partOfDomain = false;
                            try { partOfDomain = (bool)mo["PartOfDomain"]; } catch { }
                            var domain = mo["Domain"] as string;
                            var workgroup = mo["Workgroup"] as string;
                            data.DomainOrWorkgroup =
                                partOfDomain && !string.IsNullOrWhiteSpace(domain) ? domain
                                : !string.IsNullOrWhiteSpace(workgroup) ? workgroup
                                : !string.IsNullOrWhiteSpace(domain) ? domain
                                : "N/A";

                            try { totalRamBytes = Convert.ToUInt64(mo["TotalPhysicalMemory"]); } catch { }
                            try { var mfr = (mo["Manufacturer"] as string ?? "").Trim(); if (!string.IsNullOrWhiteSpace(mfr)) data.Manufacturer = mfr; } catch { }
                            try { var mdl = (mo["Model"] as string ?? "").Trim(); if (!string.IsNullOrWhiteSpace(mdl)) data.Model = mdl; } catch { }
                        }
                        break;
                    }
                }
            }
            catch { }

            // Win32_OperatingSystem: OS + build, free RAM
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem"))
                using (var results = searcher.Get())
                {
                    foreach (ManagementObject mo in results)
                    {
                        using (mo)
                        {
                            var caption = (mo["Caption"] as string ?? "").Trim();
                            var build = mo["BuildNumber"] as string;
                            data.OsVersion = string.IsNullOrWhiteSpace(caption)
                                ? "N/A"
                                : caption + (string.IsNullOrWhiteSpace(build) ? "" : " (build " + build + ")");

                            // FreePhysicalMemory is in kilobytes
                            try { freeRamBytes = Convert.ToUInt64(mo["FreePhysicalMemory"]) * 1024UL; } catch { }
                            try
                            {
                                var lbu = mo["LastBootUpTime"] as string;
                                if (!string.IsNullOrWhiteSpace(lbu))
                                {
                                    var boot = ManagementDateTimeConverter.ToDateTime(lbu);
                                    data.Uptime = Format.Uptime(DateTime.Now - boot);
                                }
                            }
                            catch { }
                        }
                        break;
                    }
                }
            }
            catch { }

            // Total / used RAM
            try
            {
                if (totalRamBytes > 0)
                {
                    data.RamTotal = Format.Giga(totalRamBytes);
                    if (freeRamBytes > 0 && freeRamBytes <= totalRamBytes)
                    {
                        data.RamUsed = Format.Giga(totalRamBytes - freeRamBytes);
                    }
                }
            }
            catch { }

            // Network: IPv4 from all active adapters; MAC, DHCP, DNS from the first active
            // non-loopback IPv4 adapter (the primary one).
            try
            {
                var ips = new List<string>();
                bool primaryCaptured = false;
                foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus != OperationalStatus.Up) continue;
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;

                    var ipProps = ni.GetIPProperties();

                    bool hasIpv4 = false;
                    foreach (var ua in ipProps.UnicastAddresses)
                    {
                        if (ua.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            ips.Add(ua.Address.ToString());
                            hasIpv4 = true;
                        }
                    }
                    if (!hasIpv4 || primaryCaptured) continue;

                    // First IPv4 adapter wins for MAC / DHCP / DNS.
                    primaryCaptured = true;

                    var phys = ni.GetPhysicalAddress().GetAddressBytes();
                    if (phys != null && phys.Length > 0)
                    {
                        data.Mac = string.Join(":", phys.Select(b => b.ToString("X2")));
                    }

                    try
                    {
                        foreach (var dh in ipProps.DhcpServerAddresses)
                        {
                            if (dh.AddressFamily == AddressFamily.InterNetwork)
                            {
                                data.Dhcp = dh.ToString();
                                break;
                            }
                        }
                    }
                    catch { }

                    var dnsList = new List<string>();
                    foreach (var dns in ipProps.DnsAddresses)
                    {
                        if (dns.AddressFamily == AddressFamily.InterNetwork)
                        {
                            dnsList.Add(dns.ToString());
                        }
                    }
                    if (dnsList.Count > 0) data.Dns = Format.Join(dnsList.Distinct());
                }
                data.IPv4 = Format.Join(ips.Distinct());
            }
            catch { }

            // FQDN
            try
            {
                var props = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties();
                var host = props.HostName;
                var dom = props.DomainName;
                if (!string.IsNullOrWhiteSpace(host))
                {
                    data.Fqdn = string.IsNullOrWhiteSpace(dom) ? host : host + "." + dom;
                }
            }
            catch { }

            // Win32_Processor: processor name (first CPU)
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor"))
                using (var results = searcher.Get())
                {
                    foreach (ManagementObject mo in results)
                    {
                        using (mo)
                        {
                            var name = (mo["Name"] as string ?? "").Trim();
                            if (!string.IsNullOrWhiteSpace(name)) data.Cpu = name;
                        }
                        break;
                    }
                }
            }
            catch { }

            // Win32_BIOS: computer serial number
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BIOS"))
                using (var results = searcher.Get())
                {
                    foreach (ManagementObject mo in results)
                    {
                        using (mo)
                        {
                            var serial = (mo["SerialNumber"] as string ?? "").Trim();
                            if (!string.IsNullOrWhiteSpace(serial)) data.SerialNumber = serial;
                        }
                        break;
                    }
                }
            }
            catch { }

            // Win32_SystemEnclosure : asset tag
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT SMBIOSAssetTag FROM Win32_SystemEnclosure"))
                using (var results = searcher.Get())
                {
                    foreach (ManagementObject mo in results)
                    {
                        using (mo)
                        {
                            var tag = (mo["SMBIOSAssetTag"] as string ?? "").Trim();
                            if (!string.IsNullOrWhiteSpace(tag)) data.AssetTag = tag;
                        }
                        break;
                    }
                }
            }
            catch { }

            // All fixed disks (DriveType=3)
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT DeviceID, Size, FreeSpace FROM Win32_LogicalDisk WHERE DriveType=3"))
                using (var results = searcher.Get())
                {
                    foreach (ManagementObject mo in results)
                    {
                        using (mo)
                        {
                            var id = mo["DeviceID"] as string;
                            if (string.IsNullOrWhiteSpace(id)) continue;
                            string total = "N/A", free = "N/A";
                            try { total = Format.Giga(Convert.ToUInt64(mo["Size"])); } catch { }
                            try { free = Format.Giga(Convert.ToUInt64(mo["FreeSpace"])); } catch { }
                            data.Disks.Add((id, free + " free / " + total));
                        }
                    }
                }
            }
            catch { }

            // Win32_Battery (laptops only)
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT EstimatedChargeRemaining, BatteryStatus FROM Win32_Battery"))
                using (var results = searcher.Get())
                {
                    foreach (ManagementObject mo in results)
                    {
                        using (mo)
                        {
                            int pct = -1, status = -1;
                            try { pct = Convert.ToInt32(mo["EstimatedChargeRemaining"]); } catch { }
                            try { status = Convert.ToInt32(mo["BatteryStatus"]); } catch { }
                            if (pct >= 0)
                            {
                                string state = status == 1 ? "on battery"
                                    : status == 2 ? "plugged in"
                                    : (status >= 6 && status <= 9) ? "charging"
                                    : "";
                                data.Battery = pct + "%" + (state.Length > 0 ? " (" + state + ")" : "");
                            }
                        }
                        break;
                    }
                }
            }
            catch { }

            // BitLocker protection status of the system drive
            try
            {
                var scope = new ManagementScope(@"\\.\root\cimv2\security\MicrosoftVolumeEncryption");
                scope.Connect();
                string sysDrive = (Environment.GetEnvironmentVariable("SystemDrive") ?? "C:").TrimEnd('\\');
                using (var searcher = new ManagementObjectSearcher(scope, new ObjectQuery("SELECT DriveLetter, ProtectionStatus FROM Win32_EncryptableVolume")))
                using (var results = searcher.Get())
                {
                    foreach (ManagementObject mo in results)
                    {
                        using (mo)
                        {
                            var dl = (mo["DriveLetter"] as string ?? "").TrimEnd('\\');
                            int ps = -1;
                            try { ps = Convert.ToInt32(mo["ProtectionStatus"]); } catch { }
                            if (!string.IsNullOrEmpty(dl) && string.Equals(dl, sysDrive, StringComparison.OrdinalIgnoreCase))
                            {
                                data.BitLocker = ps == 1 ? "On (" + sysDrive + ")" : "Off";
                            }
                        }
                    }
                }
            }
            catch { }

            // Windows activation status
            try
            {
                using (var searcher = new ManagementObjectSearcher(
                    "SELECT LicenseStatus FROM SoftwareLicensingProduct WHERE ApplicationID='55c92734-d682-4d71-983e-d6ec3f16059f' AND PartialProductKey IS NOT NULL"))
                using (var results = searcher.Get())
                {
                    foreach (ManagementObject mo in results)
                    {
                        using (mo)
                        {
                            int ls = -1;
                            try { ls = Convert.ToInt32(mo["LicenseStatus"]); } catch { }
                            data.Activation = ls == 1 ? "Activated" : "Not activated";
                        }
                        break;
                    }
                }
            }
            catch { }

            // Antivirus (client OS, SecurityCenter2)
            try
            {
                var scope = new ManagementScope(@"\\.\root\SecurityCenter2");
                scope.Connect();
                using (var searcher = new ManagementObjectSearcher(scope, new ObjectQuery("SELECT displayName, productState FROM AntiVirusProduct")))
                using (var results = searcher.Get())
                {
                    foreach (ManagementObject mo in results)
                    {
                        using (mo)
                        {
                            var name = (mo["displayName"] as string ?? "").Trim();
                            int state = 0;
                            try { state = Convert.ToInt32(mo["productState"]); } catch { }
                            // productState packs status in nibbles: bits 12-15 = scanner (non-zero = enabled),
                            // bits 4-7 = definitions (non-zero = out of date).
                            bool enabled = (state & 0xF000) != 0;
                            bool upToDate = (state & 0xF0) == 0;
                            if (!string.IsNullOrWhiteSpace(name))
                            {
                                data.Antivirus = name + " (" + (enabled ? "enabled" : "disabled")
                                    + ", " + (upToDate ? "up to date" : "outdated") + ")";
                            }
                        }
                        break;
                    }
                }
            }
            catch { }

            return data;
        }
    }
}
