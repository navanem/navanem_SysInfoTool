# Changelog

Toutes les évolutions notables de BgLight. Format inspiré de
[Keep a Changelog](https://keepachangelog.com/), versions selon
[SemVer](https://semver.org/lang/fr/).

## [1.2.0] - 2026-06-23

### Modifié
- Libellés et unités du panneau en **anglais** (`User`, `Processor`, `Serial No.`,
  `IPv4`, `OS`, `RAM`, `Disk (C:)`, `Domain`, `Generated` ; tailles en `GB`).
- Panneau **plus transparent** (fond translucide adouci).

### Ajouté
- **Pied de panneau** : `made by navanem.com` + numéro de version.

## [1.1.0] - 2026-06-23

### Ajouté
- Champs **Processor** (`Win32_Processor`) et **Serial No.** (`Win32_BIOS`).
- Option `/accentColor` pour la couleur du trait d'accent (défaut `#0078D4`).

### Modifié
- Rendu **« premium »** : panneau à coins arrondis, en-tête (nom du poste) en gras
  souligné d'un trait d'accent, deux colonnes alignées.
- Position par défaut : **haut à droite** (`TopRight`).

## 1.0.0 - 2026-06-22

### Ajouté
- Version initiale : collecte des informations système, rendu d'un panneau sur fond uni,
  application comme fond d'écran (`SystemParametersInfo`), exécutable one-shot.
- Options `/outputPath`, `/position`, `/bgColor`, `/fontSize`, `/fontName`.
- Déploiement par tâche planifiée / GPO (`deploy/run-bglight.bat`).

[1.2.0]: https://github.com/navanem/navanem_SysInfoTool/releases/tag/v1.2.0
[1.1.0]: https://github.com/navanem/navanem_SysInfoTool/releases/tag/v1.1.0
