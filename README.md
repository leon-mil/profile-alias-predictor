# ProfileAliasPredictor

A custom .NET `CommandPredictor` for PowerShell 7 that provides intelligent
command, alias, and workflow suggestions through PSReadLine.

ProfileAliasPredictor is designed to work with the companion
[`powershell-profile`](https://github.com/leon-mil/powershell-profile)
repository.

## Overview

The PowerShell profile defines functions, aliases, navigation helpers,
development utilities, Git commands, deployment workflows, and other
interactive commands.

ProfileAliasPredictor complements that profile by presenting useful command
suggestions while the user types.

The predictor:

- Integrates with the PowerShell `CommandPredictor` subsystem.
- Works with PSReadLine prediction support.
- Suggests profile aliases and commands.
- Supports multi-word command-prefix matching.
- Provides descriptions for suggested commands.
- Does not execute commands automatically.
- Keeps frequently used workflows discoverable without memorizing every alias.

## Architecture

```text
PowerShell
    |
    +-- profile.ps1
    |      |
    |      +-- Functions
    |      +-- Aliases
    |      +-- PSReadLine configuration
    |      +-- Predictor management commands
    |
    +-- ProfileAliasPredictor.dll
           |
           +-- CommandPredictor implementation
           +-- Command definitions
           +-- Prefix matching
           +-- Suggestion descriptions
                    |
                    v
             PSReadLine Suggestions
```

The PowerShell profile is responsible for implementing the commands.

ProfileAliasPredictor is responsible for helping the user discover and enter
those commands.

## Repository Structure

```text
ProfileAliasPredictor/
|
|-- ProfileAliasPredictor.cs
|-- ProfileAliasPredictor.csproj
|-- ProfileAliasPredictor.slnx
|-- ProfileAliasPredictor.code-workspace
|-- README.md
|-- .gitignore
|
|-- docs/
|
|-- bin/                 Generated build output - ignored by Git
`-- obj/                 Generated build output - ignored by Git
```

## Requirements

The project is intended for PowerShell 7 and modern .NET.

Recommended development environment:

- PowerShell 7
- .NET SDK 10 or compatible SDK required by the project
- PSReadLine
- Git
- Visual Studio Code
- C# extension for Visual Studio Code

Verify the main prerequisites:

```powershell
$PSVersionTable.PSVersion
dotnet --version
git --version
```

## Recommended Installation Location

The companion PowerShell profile expects the predictor project to be located
under the user's PowerShell development directory.

Recommended structure:

```text
C:\Users\<username>\Documents\PowerShell\
|
|-- profile.ps1
|-- PowerShell.code-workspace
|
`-- Projects\
    `-- ProfileAliasPredictor\
        |-- ProfileAliasPredictor.cs
        |-- ProfileAliasPredictor.csproj
        `-- ...
```

For example:

```text
C:\Users\mil00001\Documents\PowerShell\Projects\ProfileAliasPredictor
```

## Clone the Repository

From the PowerShell `Projects` directory:

```powershell
git clone https://github.com/leon-mil/profile-alias-predictor.git
```

Then enter the project:

```powershell
cd .\profile-alias-predictor
```

## Build

Build the predictor in Release configuration:

```powershell
dotnet build .\ProfileAliasPredictor.csproj --configuration Release
```

The compiled predictor DLL is written beneath:

```text
bin\Release\<target-framework>\
```

For the current .NET 10 configuration, this is typically:

```text
bin\Release\net10.0\ProfileAliasPredictor.dll
```

## PowerShell Profile Integration

The companion PowerShell profile loads the compiled predictor DLL and
registers it with PowerShell.

The profile also provides commands for managing predictor development.

Common commands include:

| Command | Purpose |
|---|---|
| `predproj` | Open the ProfileAliasPredictor project |
| `predcd` | Change to the predictor project directory |
| `predinfo` | Display predictor configuration and build information |
| `predtest` | Test predictor integration |
| `predbuild` | Build the predictor |
| `predrebuild` | Rebuild the predictor and handle the loaded DLL |
| `predkill` | Stop PowerShell processes when needed during development |
| `predon` | Enable plug-in predictions |
| `predoff` | Disable plug-in predictions |
| `predstate` | Show the current PowerShell prediction configuration |

## Why `predrebuild` Is Useful

After PowerShell loads `ProfileAliasPredictor.dll`, the DLL may remain locked
by the running PowerShell process.

That can interfere with rebuilding the project in place.

The PowerShell profile therefore provides:

```powershell
predrebuild
```

This command supports the development workflow for rebuilding and reloading
the predictor more safely than manually managing the loaded assembly.

## Typical Development Workflow

Open the project:

```powershell
predproj
```

Modify:

```text
ProfileAliasPredictor.cs
```

Then rebuild:

```powershell
predrebuild
```

Test the integration:

```powershell
predtest
```

Check the prediction configuration:

```powershell
predstate
```

Then begin typing a known command or alias and verify that the expected
suggestions appear.

## Prediction Behavior

The predictor performs prefix-based matching against its registered command
definitions.

For example, entering:

```text
pred
```

can surface commands such as:

```text
predon
predoff
predstate
predproj
predrebuild
predtest
```

Matching also supports command prefixes containing spaces.

This allows the predictor to suggest parameterized workflows rather than being
limited to single-word aliases.

Examples can include patterns such as:

```text
unzip -Destination
git ...
cprs-...
```

depending on the command definitions registered in the predictor.

## PSReadLine

ProfileAliasPredictor works with PowerShell's PSReadLine prediction interface.

A typical profile configuration uses:

```text
PredictionSource = HistoryAndPlugin
PredictionView   = ListView
```

This combines:

- PowerShell command history
- ProfileAliasPredictor suggestions

The companion PowerShell profile provides commands for enabling and disabling
the individual prediction features.

## Adding a New Prediction

New predictor entries are defined in:

```text
ProfileAliasPredictor.cs
```

A typical command definition contains:

1. The command or command prefix.
2. A concise description explaining what the command does.

After adding or changing a prediction:

```powershell
predrebuild
```

Then validate it:

```powershell
predtest
```

Finally, type the beginning of the new command and verify that it appears in
PSReadLine.

## Keeping the Predictor and Profile Synchronized

The predictor and PowerShell profile are maintained as separate repositories:

```text
powershell-profile
profile-alias-predictor
```

The separation is intentional.

### powershell-profile

Contains the actual PowerShell implementation:

- Functions
- Aliases
- PSReadLine configuration
- Git utilities
- Navigation commands
- Backup and archive utilities
- Development helpers
- CPRS build and deployment commands
- Predictor management commands

### profile-alias-predictor

Contains the .NET prediction component:

- CommandPredictor implementation
- Prediction definitions
- Command descriptions
- Matching behavior
- Build project files

When a profile alias is renamed, added, or removed, its predictor definition
should also be reviewed so that suggestions remain synchronized with the
actual profile.

## Git Workflow

Review changes:

```powershell
git status
git diff
```

Stage changes:

```powershell
git add -A
```

Commit:

```powershell
git commit -m "Describe the predictor change"
```

Push:

```powershell
git push origin main
```

## Generated Files

The repository intentionally excludes generated and machine-specific files,
including:

- `bin/`
- `obj/`
- `.vs/`
- IDE caches
- logs
- test output
- temporary files
- backup files
- local configuration

See `.gitignore` for the complete list.

## Troubleshooting

### Predictor suggestions do not appear

Check the current prediction configuration:

```powershell
predstate
```

Then test the integration:

```powershell
predtest
```

### Build fails because the DLL is in use

Use:

```powershell
predrebuild
```

rather than rebuilding the loaded predictor manually.

### Check project information

Run:

```powershell
predinfo
```

### Open the project

Run:

```powershell
predproj
```

## Related Project

Companion PowerShell profile:

<https://github.com/leon-mil/powershell-profile>

## Repository

ProfileAliasPredictor:

<https://github.com/leon-mil/profile-alias-predictor>
