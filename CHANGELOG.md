# Changelog

All notable changes to BgLight. Format inspired by
[Keep a Changelog](https://keepachangelog.com/), versions follow
[SemVer](https://semver.org/).

## [1.3.0] - 2026-06-23

### Added
- Information grouped into **sections**: System, Hardware, Network, Storage, Security.
- New fields: Manufacturer, Model, Asset tag, Uptime, MAC, FQDN, DHCP, DNS, all fixed
  disks, Battery (laptops), BitLocker, Windows activation, Antivirus.
- `/bgImage` option: draw the panel over a background image (falls back to the solid color).
- **Multi-monitor**: one panel per monitor, rendered across the whole virtual desktop.
- **DPI awareness** (PerMonitorV2) and **Span** wallpaper style for pixel-accurate output.
- Panel **drop shadow and border**.

## [1.2.1] - 2026-06-23

### Changed
- Full **English** documentation (README, CHANGELOG, deployment script) and source code
  comments; the error log message is now in English.

## 1.2.0 - 2026-06-23

### Changed
- Panel labels and units are now in **English** (`User`, `Processor`, `Serial No.`,
  `IPv4`, `OS`, `RAM`, `Disk (C:)`, `Domain`, `Generated`; sizes in `GB`).
- Panel is **more transparent** (softened translucent background).

### Added
- **Panel footer**: `made by navanem.com` + version number.

## [1.1.0] - 2026-06-23

### Added
- **Processor** (`Win32_Processor`) and **Serial No.** (`Win32_BIOS`) fields.
- `/accentColor` option for the accent line color (default `#0078D4`).

### Changed
- **"Premium"** rendering: rounded-corner panel, header (computer name) in bold underlined
  by an accent line, two aligned columns.
- Default position: **top-right** (`TopRight`).

## 1.0.0 - 2026-06-22

### Added
- Initial version: system information collection, panel rendered on a solid background,
  applied as the wallpaper (`SystemParametersInfo`), one-shot executable.
- `/outputPath`, `/position`, `/bgColor`, `/fontSize`, `/fontName` options.
- Deployment via scheduled task / GPO (`deploy/run-bglight.bat`).

[1.3.0]: https://github.com/navanem/navanem_SysInfoTool/releases/tag/v1.3.0
[1.2.1]: https://github.com/navanem/navanem_SysInfoTool/releases/tag/v1.2.1
[1.1.0]: https://github.com/navanem/navanem_SysInfoTool/releases/tag/v1.1.0
