# Changelog

All notable changes to BgLight. Format inspired by
[Keep a Changelog](https://keepachangelog.com/), versions follow
[SemVer](https://semver.org/).

## [1.2.0] - 2026-06-23

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

[1.2.0]: https://github.com/navanem/navanem_SysInfoTool/releases/tag/v1.2.0
[1.1.0]: https://github.com/navanem/navanem_SysInfoTool/releases/tag/v1.1.0
