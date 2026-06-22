# BgLight

Outil BGInfo-like léger : dessine les informations système sur un fond uni et l'applique
comme fond d'écran Windows. EXE one-shot, silencieux, sans dépendance externe (.NET Framework 4.8).

## Informations affichées

Nom du PC, utilisateur connecté, IPv4, OS + build, RAM utilisée/totale, disque C: libre/total,
domaine ou workgroup, date/heure de génération.

## Prérequis

- Windows 10/11 (.NET Framework 4.8 préinstallé).
- Pour compiler : Visual Studio 2022 ou le SDK .NET avec le pack de ciblage .NET Framework 4.8.
  (Ce dépôt référence `Microsoft.NETFramework.ReferenceAssemblies` en dépendance de **build**
  uniquement, ce qui permet de compiler `net48` avec le seul SDK .NET — aucune dépendance n'est
  ajoutée à l'EXE livré.)

## Compilation

### Avec Visual Studio
1. Ouvrir `BgLight.sln`.
2. Sélectionner la configuration **Release**.
3. Générer la solution (Ctrl+Maj+B). L'EXE : `src\BgLight\bin\Release\net48\BgLight.exe`.

### En ligne de commande
```bash
dotnet build -c Release
```
ou
```bash
msbuild BgLight.sln /p:Configuration=Release
```

## Utilisation

Simple (valeurs par défaut) :
```bat
BgLight.exe
```

Avec arguments :
```bat
BgLight.exe /outputPath="C:\ProgramData\BgLight\wallpaper_info.bmp" /fontSize=11 /position=TopLeft /bgColor=#202020 /fontName="Segoe UI"
```

| Argument | Défaut | Description |
|---|---|---|
| `/outputPath` | `C:\ProgramData\BgLight\wallpaper_info.bmp` | Chemin du BMP généré |
| `/fontSize` | `11` | Taille de police (points) |
| `/position` | `TopLeft` | `TopLeft`, `TopRight`, `BottomLeft`, `BottomRight` |
| `/bgColor` | `#202020` | Couleur du fond uni (hex) |
| `/fontName` | `Segoe UI` | Police du texte |

Le log d'erreurs (`log.txt`) est créé dans le dossier de `outputPath`.

## Déploiement

### Tâche planifiée (recommandé pour rafraîchir régulièrement)
```bat
schtasks /create /tn "BgLight" /tr "%ProgramData%\BgLight\BgLight.exe" /sc onlogon
schtasks /create /tn "BgLight-Refresh" /tr "%ProgramData%\BgLight\BgLight.exe" /sc minute /mo 30
```
- `onlogon` : à chaque ouverture de session.
- `/sc minute /mo 30` : rafraîchissement toutes les 30 minutes.

### Script de logon (GPO)
1. Copier `BgLight.exe` dans `\\serveur\partage\BgLight\` ou `%ProgramData%\BgLight\`.
2. GPO → *Configuration utilisateur > Stratégies > Paramètres Windows > Scripts (ouverture de session)*.
3. Ajouter `deploy\run-bglight.bat`.

Le processus se termine immédiatement après la mise à jour (aucun résident, faible consommation).
