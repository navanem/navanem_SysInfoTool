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

            // Win32_LogicalDisk WHERE DeviceID = 'C:'
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_LogicalDisk WHERE DeviceID = 'C:'"))
                using (var results = searcher.Get())
                {
                    foreach (ManagementObject mo in results)
                    {
                        using (mo)
                        {
                            try { data.DiskTotal = Format.Giga(Convert.ToUInt64(mo["Size"])); } catch { }
                            try { data.DiskFree = Format.Giga(Convert.ToUInt64(mo["FreeSpace"])); } catch { }
                        }
                        break;
                    }
                }
            }
            catch { }

            // IPv4 via NetworkInterface
            try
            {
                var ips = new List<string>();
                foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus != OperationalStatus.Up) continue;
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;

                    foreach (var ua in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ua.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            ips.Add(ua.Address.ToString());
                        }
                    }
                }
                data.IPv4 = Format.Join(ips.Distinct());
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

            return data;
        }
    }
}
