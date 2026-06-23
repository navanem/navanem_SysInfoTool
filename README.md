# BgLight

**BgLight** est un outil léger de type *BGInfo* pour Windows : il dessine un panneau
d'informations système « premium » sur un fond uni et l'applique automatiquement comme
fond d'écran. C'est un exécutable *one-shot* (il s'exécute, met à jour le fond, puis se
termine immédiatement), silencieux, sans interface et sans dépendance externe
(.NET Framework 4.8, présent d'origine sur Windows 10/11).

![Aperçu du panneau BgLight](docs/screenshot.png)

> Capture générée avec des données fictives. Sur un poste réel, les valeurs proviennent
> du système (WMI + API .NET).

---

## Sommaire

- [Fonctionnalités](#fonctionnalités)
- [Informations affichées](#informations-affichées)
- [Prérequis](#prérequis)
- [Installation rapide (binaire)](#installation-rapide-binaire)
- [Utilisation](#utilisation)
- [Options de ligne de commande](#options-de-ligne-de-commande)
- [Déploiement en entreprise](#déploiement-en-entreprise)
- [Compilation depuis les sources](#compilation-depuis-les-sources)
- [Fonctionnement interne](#fonctionnement-interne)
- [Dépannage](#dépannage)
- [Versions](#versions)

---

## Fonctionnalités

- **Panneau « premium »** : carte sombre translucide à coins arrondis, anti-aliasée.
- **Position configurable** (par défaut en **haut à droite**).
- **En-tête** = nom du poste, en gras, souligné d'un **trait d'accent** dont la couleur
  est configurable (`/accentColor`, bleu `#0078D4` par défaut).
- **Deux colonnes alignées** (libellé / valeur) pour une lecture nette.
- **Pied de panneau** : `made by navanem.com` + numéro de version.
- **Robuste** : chaque source d'information est isolée ; en cas d'échec d'une requête,
  la valeur affiche `N/A` plutôt que de planter l'outil.
- **Léger et non résident** : aucun service, aucun processus en arrière-plan ; l'exe se
  termine dès la mise à jour effectuée.

## Informations affichées

| Champ | Source |
|---|---|
| Nom du poste (titre) | `Win32_ComputerSystem` / `Environment.MachineName` |
| **User** | session Windows (`DOMAINE\utilisateur`) |
| **Processor** | `Win32_Processor.Name` |
| **Serial No.** | `Win32_BIOS.SerialNumber` |
| **IPv4** | interfaces réseau actives (hors loopback) |
| **OS** | `Win32_OperatingSystem` (édition + build) |
| **RAM** | utilisée / totale (`Win32_ComputerSystem` + `Win32_OperatingSystem`) |
| **Disk (C:)** | libre / total (`Win32_LogicalDisk`) |
| **Domain** | domaine ou groupe de travail |
| **Generated** | date/heure de génération |

Les tailles sont affichées en **GB** (gibioctets, base 1024).

## Prérequis

- **Exécution** : Windows 10/11. Le .NET Framework 4.8 est préinstallé sur ces versions.
- **Compilation** : le SDK .NET suffit (voir [Compilation](#compilation-depuis-les-sources)) ;
  Visual Studio 2022 fonctionne aussi.

## Installation rapide (binaire)

1. Télécharger `BgLight-vX.Y.Z.exe` depuis la page
   [**Releases**](https://github.com/navanem/navanem_SysInfoTool/releases).
2. (Optionnel) le placer dans `%ProgramData%\BgLight\`.
3. Le lancer une fois pour vérifier le rendu, puis le planifier (voir
   [Déploiement](#déploiement-en-entreprise)).

## Utilisation

Valeurs par défaut (panneau en haut à droite, accent bleu) :

```bat
BgLight.exe
```

Avec arguments :

```bat
BgLight.exe /position=TopRight /accentColor=#0078D4 /fontSize=11 /fontName="Segoe UI"
```

## Options de ligne de commande

Les arguments sont de la forme `/clé=valeur` (insensibles à la casse). Une valeur invalide
est ignorée et la valeur par défaut est conservée.

| Argument | Défaut | Description |
|---|---|---|
| `/outputPath` | `C:\ProgramData\BgLight\wallpaper_info.bmp` | Chemin du BMP généré (et de `log.txt`) |
| `/position` | `TopRight` | `TopLeft`, `TopRight`, `BottomLeft`, `BottomRight` |
| `/accentColor` | `#0078D4` | Couleur du trait d'accent (hex `#RRGGBB`) |
| `/bgColor` | `#202020` | Couleur du fond uni (hex) |
| `/fontSize` | `11` | Taille de police du corps (points) ; le titre est légèrement plus grand |
| `/fontName` | `Segoe UI` | Police du texte |

Le journal d'erreurs (`log.txt`) est créé dans le dossier de `outputPath`.

## Déploiement en entreprise

### Tâche planifiée (recommandé pour rafraîchir régulièrement)

```bat
schtasks /create /tn "BgLight"         /tr "%ProgramData%\BgLight\BgLight.exe" /sc onlogon
schtasks /create /tn "BgLight-Refresh" /tr "%ProgramData%\BgLight\BgLight.exe" /sc minute /mo 30
```

- `onlogon` : à chaque ouverture de session.
- `/sc minute /mo 30` : rafraîchissement toutes les 30 minutes (utile pour la RAM/disque/IP).

### Script de logon (GPO)

1. Copier `BgLight.exe` dans `\\serveur\partage\BgLight\` ou `%ProgramData%\BgLight\`.
2. GPO → *Configuration utilisateur > Stratégies > Paramètres Windows > Scripts (ouverture de session)*.
3. Ajouter `deploy\run-bglight.bat`.

## Compilation depuis les sources

Le projet cible `net48`. Il référence `Microsoft.NETFramework.ReferenceAssemblies`
**uniquement pour la compilation**, ce qui permet de construire `net48` avec le seul SDK .NET
(sans pack de ciblage installé) ; aucune dépendance n'est ajoutée à l'exe livré.

```bash
# Compilation
dotnet build BgLight.sln -c Release
# -> src/BgLight/bin/Release/net48/BgLight.exe

# Tests
dotnet test -c Debug
```

Avec Visual Studio : ouvrir `BgLight.sln`, configuration **Release**, générer la solution.

## Fonctionnement interne

Pipeline *one-shot* exécuté à chaque lancement :

1. **`AppConfig.Parse`** — lit les arguments `/clé=valeur` et applique les défauts.
2. **`SystemInfoCollector.Collect`** — récolte les infos système (WMI + API .NET), chaque
   source isolée dans son propre `try/catch` (jamais d'exception remontée).
3. **`WallpaperRenderer.Render`** — dessine le panneau (GDI+) sur un bitmap plein écran et
   l'enregistre en BMP.
4. **`WallpaperSetter.Apply`** — applique le BMP comme fond d'écran via `SystemParametersInfo`.

Découpage des sources :

| Fichier | Rôle |
|---|---|
| `Program.cs` | Orchestration one-shot + gestion d'erreurs racine |
| `AppConfig.cs` | Analyse des arguments et valeurs par défaut |
| `SystemInfoCollector.cs` | Collecte des informations système |
| `SystemInfoData.cs` | Modèle de données (titre + lignes libellé/valeur) |
| `Format.cs` | Formatage (tailles en GB, jointures) |
| `WallpaperRenderer.cs` | Rendu GDI+ du panneau |
| `WallpaperSetter.cs` | Application du fond d'écran (P/Invoke) |
| `Logger.cs` | Journalisation des erreurs |

## Dépannage

- **Le fond ne change pas / `log.txt` mentionne un accès refusé** : choisir un
  `outputPath` accessible en écriture (ex. un dossier sous `%LOCALAPPDATA%`), ou exécuter
  avec des droits suffisants sur `%ProgramData%`.
- **Une stratégie GPO « fond d'écran imposé » est active** : elle peut écraser le fond.
  Désactiver la stratégie ou déployer le BMP via cette même stratégie.
- **Valeurs `N/A`** : la requête WMI correspondante a échoué (droits, pilote, WMI
  corrompu) ; les autres champs restent renseignés.

## Versions

Voir [`CHANGELOG.md`](CHANGELOG.md) et la page
[Releases](https://github.com/navanem/navanem_SysInfoTool/releases).

---

made by [navanem.com](https://navanem.com)
