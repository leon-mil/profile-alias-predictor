# ProfileAliasPredictor Comprehensive Guide

## 1. Purpose

`ProfileAliasPredictor` is a small .NET project that adds custom predictive command suggestions to PowerShell 7.

It is designed to work with a companion PowerShell profile. The PowerShell profile defines the real commands and aliases; `ProfileAliasPredictor` supplies discoverable command text and descriptions while the user types.

The predictor does **not** execute commands and does **not** replace the PowerShell profile. Its role is to make the profile easier to learn and use.

At a high level:

```text
User types in PowerShell
        |
        v
PSReadLine requests predictions
        |
        v
PowerShell CommandPredictor subsystem
        |
        v
ProfileAliasPredictor.GetSuggestion()
        |
        v
AliasDefinitions are searched
        |
        v
Matching suggestions are returned to PSReadLine
        |
        v
User sees [ProfileAliases] suggestions
```

The project is intentionally simple: one C# source file contains the predictor implementation, the command catalog, and the module registration code.

---

## 2. Relationship to the PowerShell Profile

There are two separate projects:

```text
powershell-profile
profile-alias-predictor
```

They have different responsibilities.

### powershell-profile

The PowerShell profile repository contains the actual PowerShell behavior:

- functions
- aliases
- navigation helpers
- Git utilities
- history controls
- prediction controls
- backup and archive tools
- CPRS build/deployment commands
- ProfileAliasPredictor build/test helpers
- PSReadLine configuration

### profile-alias-predictor

This repository contains the predictive IntelliSense component:

- `ICommandPredictor` implementation
- command and alias suggestion definitions
- suggestion descriptions
- prefix matching
- module initialization
- module cleanup
- .NET project configuration

A suggestion in the predictor does not automatically create a PowerShell alias. Likewise, a new alias in `profile.ps1` does not automatically appear in the predictor.

When command names or descriptions change, both repositories should be reviewed so that the real command and its predictive help stay synchronized.

---

## 3. Recommended Directory Layout

The recommended layout is:

```text
C:\Users\<username>\Documents\PowerShell\
|
|-- profile.ps1
|-- powershell.config.json
|-- PowerShell.code-workspace
|-- README.md
|-- docs\
|
`-- Projects\
    `-- ProfileAliasPredictor\
        |-- ProfileAliasPredictor.cs
        |-- ProfileAliasPredictor.csproj
        |-- ProfileAliasPredictor.slnx
        |-- ProfileAliasPredictor.code-workspace
        |-- README.md
        |-- .gitignore
        |-- docs\
        |   `-- ProfileAliasPredictor_Guide.md
        |-- bin\
        `-- obj\
```

The reusable pattern is:

```text
$HOME\Documents\PowerShell\Projects\ProfileAliasPredictor
```

Using `$HOME` in the PowerShell profile makes the configuration portable across Windows user names.

---

## 4. Getting the Project from GitHub

For a new user, first create the parent project directory if it does not already exist:

```powershell
New-Item -ItemType Directory `
    -Path "$HOME\Documents\PowerShell\Projects" `
    -Force
```

Change to it:

```powershell
Set-Location "$HOME\Documents\PowerShell\Projects"
```

Clone the repository:

```powershell
git clone https://github.com/leon-mil/profile-alias-predictor.git ProfileAliasPredictor
```

Change into the project:

```powershell
Set-Location .\ProfileAliasPredictor
```

Verify the repository:

```powershell
git status
```

The recommended directory name is `ProfileAliasPredictor` even though the GitHub repository name is `profile-alias-predictor`. This keeps the local path aligned with the companion profile configuration.

---

## 5. Prerequisites

The project requires a modern PowerShell and .NET development environment.

Verify PowerShell:

```powershell
$PSVersionTable.PSVersion
```

Verify the .NET SDK:

```powershell
dotnet --version
```

Verify Git:

```powershell
git --version
```

The current project targets .NET 10, so the output directory is expected beneath:

```text
bin\Release\net10.0\
```

The companion PowerShell profile also uses PSReadLine and the PowerShell prediction subsystem.

Verify PSReadLine:

```powershell
Get-Module PSReadLine -ListAvailable
```

Verify the PowerShell subsystem command is available:

```powershell
Get-Command Get-PSSubsystem
```

---

## 6. Project Files

### ProfileAliasPredictor.cs

This is the main source file.

It contains:

- the predictor namespace
- the `ProfileAliasPredictor` class
- the complete suggestion catalog
- predictor identity and description
- suggestion matching logic
- PowerShell feedback callbacks
- the internal `AliasDefinition` record
- the `Init` class that registers and unregisters the predictor

This is the file normally edited when adding, removing, or changing predictive suggestions.

### ProfileAliasPredictor.csproj

This is the .NET project file.

It controls build behavior such as:

- target framework
- compiler settings
- project references/packages
- PowerShell SDK dependency
- package versions
- output generation

Build commands read this file to determine how the project is compiled.

### ProfileAliasPredictor.slnx

This is the solution file used by modern .NET/Visual Studio tooling.

It provides a solution-level entry point for the project.

### ProfileAliasPredictor.code-workspace

This is the Visual Studio Code workspace for predictor development.

It can be opened directly:

```powershell
code .\ProfileAliasPredictor.code-workspace
```

The companion profile provides `predproj` as the shorter normal workflow.

### README.md

The repository landing page.

It should remain concise and point readers to this guide for full installation, architecture, API, maintenance, and troubleshooting information.

### .gitignore

Excludes generated and machine-specific files such as:

- `bin/`
- `obj/`
- `.vs/`
- IDE state
- logs
- temporary files
- test output
- backup files
- local caches

Source code, project files, workspace files, README files, and documentation remain tracked.

### docs/

Contains detailed project documentation.

The recommended comprehensive guide location is:

```text
docs\ProfileAliasPredictor_Guide.md
```

---

## 7. Source-Code Architecture

The main namespace is:

```csharp
namespace Leon.PowerShell.Prediction;
```

The primary class is:

```csharp
public sealed class ProfileAliasPredictor : ICommandPredictor
```

Implementing `ICommandPredictor` allows PowerShell's prediction subsystem to request suggestions from the project.

The second public class is:

```csharp
public sealed class Init : IModuleAssemblyInitializer, IModuleAssemblyCleanup
```

`Init` is responsible for registering the predictor when the compiled binary module is imported and unregistering it when the module is removed.

---

## 8. AliasDefinitions

The command catalog is stored in:

```csharp
private static readonly AliasDefinition[] AliasDefinitions
```

Each item has two values:

```csharp
new(
    "command-text",
    "Description shown to the user")
```

For example:

```csharp
new(
    "predtest",
    "Verify the PowerShell profile and predictor configuration")
```

The first string is what PowerShell can suggest.

The second string explains the suggestion.

The command catalog can contain simple aliases:

```text
predtest
gst
dtree
```

It can also contain multi-word command templates:

```text
dtree -Files
predkill -Force
predrebuild -Force
```

This is important because the current matching algorithm supports spaces in the input.

---

## 9. AliasDefinition Record

At the bottom of the predictor class is the internal record:

```csharp
private sealed record AliasDefinition(
    string Name,
    string Description);
```

Its purpose is to provide a small immutable data structure for a suggestion.

### Name

The exact command text that can be returned to PSReadLine.

### Description

The explanatory text displayed beside the prediction.

Because this is a record, the command catalog remains compact and readable.

---

## 10. Constructor

The predictor constructor is:

```csharp
internal ProfileAliasPredictor(Guid id)
```

It accepts the unique predictor identifier and stores it in:

```csharp
private readonly Guid _id;
```

The constructor is `internal` because predictor instances are created by the project itself during module initialization rather than by normal PowerShell users.

---

## 11. Id Property

```csharp
public Guid Id => _id;
```

`Id` exposes the unique identifier required by the PowerShell subsystem.

PowerShell uses this identifier to distinguish this predictor from other predictor implementations.

The same identifier is later used when unregistering the predictor.

---

## 12. Name Property

```csharp
public string Name => "ProfileAliases";
```

This is the predictor name displayed by PowerShell/PSReadLine.

When suggestions appear, the user can identify this provider as:

```text
[ProfileAliases]
```

This distinguishes custom profile suggestions from history suggestions and other plug-ins.

---

## 13. Description Property

The predictor exposes a human-readable description through:

```csharp
public string Description => ...
```

This describes the role of the predictor to the PowerShell subsystem.

It is metadata; it does not affect command matching.

---

## 14. GetSuggestion

The most important method is:

```csharp
public SuggestionPackage GetSuggestion(
    PredictionClient client,
    PredictionContext context,
    CancellationToken cancellationToken)
```

This is the method PowerShell calls when it wants suggestions.

### Step 1 - Read the current command line

The implementation reads:

```csharp
context.InputAst.Extent.Text
```

and trims leading/trailing whitespace.

Conceptually:

```text
User types:  pred
Input becomes: pred
```

### Step 2 - Ignore empty input

If the command line is empty or whitespace, the method returns no suggestion package.

This prevents the predictor from dumping its entire command catalog into a blank prompt.

### Step 3 - Create a result list

A list of `PredictiveSuggestion` objects is created.

### Step 4 - Search AliasDefinitions

The method loops through each `AliasDefinition`.

### Step 5 - Honor cancellation

During the loop it checks:

```csharp
cancellationToken.IsCancellationRequested
```

If PowerShell cancels the prediction request, the predictor stops immediately.

### Step 6 - Prefix match

The central matching test uses:

```csharp
aliasDefinition.Name.StartsWith(
    input,
    StringComparison.OrdinalIgnoreCase)
```

This means matching is:

- prefix based
- case insensitive
- capable of matching spaces and parameters

For example:

```text
Input: pred
```

can match:

```text
predon
predoff
predstate
predproj
predbuild
predrebuild
predinfo
predtest
```

A multi-word prefix can also be matched:

```text
Input: predrebuild -
```

which can match:

```text
predrebuild -Force
```

There is deliberately no rule that rejects input containing spaces.

### Step 7 - Create PredictiveSuggestion

For each match:

```csharp
new PredictiveSuggestion(
    aliasDefinition.Name,
    aliasDefinition.Description)
```

is added to the results.

### Step 8 - Return the result

If no entries match, the method returns no package.

If one or more entries match, it returns:

```csharp
new SuggestionPackage(suggestions)
```

PSReadLine then decides how to display the returned suggestions.

---

## 15. CanAcceptFeedback

```csharp
public bool CanAcceptFeedback(
    PredictionClient client,
    PredictorFeedbackKind feedback)
```

The implementation returns:

```csharp
false
```

This means ProfileAliasPredictor does not currently use PSReadLine feedback to learn or adapt.

It does not change ranking based on:

- which suggestion was displayed
- which suggestion was accepted
- whether a command succeeded
- command history behavior

The predictor is deterministic: suggestions come from the static command catalog and prefix matching.

---

## 16. OnSuggestionDisplayed

```csharp
public void OnSuggestionDisplayed(
    PredictionClient client,
    uint session,
    int countOrIndex)
```

This callback is intentionally empty.

PowerShell can notify a predictor when suggestions are displayed, but this project does not currently need that information.

---

## 17. OnSuggestionAccepted

```csharp
public void OnSuggestionAccepted(
    PredictionClient client,
    uint session,
    string acceptedSuggestion)
```

This callback is intentionally empty.

The project does not collect acceptance statistics or alter later suggestions based on what the user accepted.

---

## 18. OnCommandLineAccepted

```csharp
public void OnCommandLineAccepted(
    PredictionClient client,
    IReadOnlyList<string> history)
```

This callback is intentionally empty.

The predictor does not maintain its own command history.

PowerShell/PSReadLine history remains separate.

---

## 19. OnCommandLineExecuted

```csharp
public void OnCommandLineExecuted(
    PredictionClient client,
    string commandLine,
    bool success)
```

This callback is intentionally empty.

The predictor does not modify its suggestions based on whether a command succeeded or failed.

---

## 20. Init Class

The `Init` class implements:

```csharp
IModuleAssemblyInitializer
IModuleAssemblyCleanup
```

This connects the compiled .NET assembly to PowerShell's module lifecycle.

Without this class, compiling the DLL alone would not automatically register the predictor with the PowerShell subsystem.

---

## 21. Predictor Identifier

`Init` contains a static GUID.

Conceptually:

```csharp
private static readonly Guid Identifier = new("...");
```

This value must remain stable for the predictor identity.

The same identifier is used for both:

- registration
- unregistration

Do not casually generate a new GUID during normal edits. Changing it changes the subsystem identity of the predictor.

---

## 22. Init.OnImport

```csharp
public void OnImport()
```

This runs when PowerShell imports the compiled assembly as a module.

It:

1. creates a `ProfileAliasPredictor` instance
2. passes the stable identifier to the constructor
3. calls `SubsystemManager.RegisterSubsystem`
4. registers it as a `CommandPredictor`

Conceptually:

```text
Import-Module ProfileAliasPredictor.dll
        |
        v
Init.OnImport()
        |
        v
new ProfileAliasPredictor(Identifier)
        |
        v
RegisterSubsystem(CommandPredictor, predictor)
```

After registration, `Get-PSSubsystem -Kind CommandPredictor` can see the predictor.

---

## 23. Init.OnRemove

```csharp
public void OnRemove(PSModuleInfo psModuleInfo)
```

This runs when PowerShell removes the binary module.

It calls:

```csharp
SubsystemManager.UnregisterSubsystem(...)
```

using the same predictor identifier.

This is important because the subsystem should not retain a predictor that belongs to a module that has been removed.

---

## 24. Build Configuration

The normal release build is:

```powershell
dotnet build .\ProfileAliasPredictor.csproj --configuration Release
```

A shorter equivalent is:

```powershell
dotnet build .\ProfileAliasPredictor.csproj -c Release
```

With the current target framework, the DLL is expected at:

```text
bin\Release\net10.0\ProfileAliasPredictor.dll
```

The `bin` and `obj` directories are generated artifacts and should not be committed to Git.

---

## 25. First Build for a New User

After cloning:

```powershell
Set-Location "$HOME\Documents\PowerShell\Projects\ProfileAliasPredictor"
```

Restore dependencies:

```powershell
dotnet restore .\ProfileAliasPredictor.csproj
```

Build:

```powershell
dotnet build .\ProfileAliasPredictor.csproj -c Release
```

Verify the DLL:

```powershell
Test-Path .\bin\Release\net10.0\ProfileAliasPredictor.dll
```

Expected:

```text
True
```

---

## 26. Manually Loading the Predictor

The companion PowerShell profile normally handles loading automatically, but it is useful to understand the underlying operation.

The DLL can be loaded as a PowerShell binary module:

```powershell
Import-Module `
    "$HOME\Documents\PowerShell\Projects\ProfileAliasPredictor\bin\Release\net10.0\ProfileAliasPredictor.dll"
```

Verify the module:

```powershell
Get-Module ProfileAliasPredictor
```

Verify the predictor subsystem:

```powershell
Get-PSSubsystem -Kind CommandPredictor
```

Look for:

```text
ProfileAliases
```

---

## 27. Required PSReadLine Configuration

Registering a predictor is only one half of the setup. PSReadLine must also be configured to display plug-in predictions.

A typical configuration is:

```powershell
Set-PSReadLineOption -PredictionSource HistoryAndPlugin
Set-PSReadLineOption -PredictionViewStyle ListView
```

`HistoryAndPlugin` means both sources can participate:

```text
History
ProfileAliases
```

If only plug-in predictions are wanted:

```powershell
Set-PSReadLineOption -PredictionSource Plugin
```

If predictions are disabled, the predictor may be correctly registered but nothing will appear in the command-line UI.

---

## 28. Companion Profile Integration

The companion `powershell-profile` repository provides the normal user experience around this project.

Recommended location:

```text
$HOME\Documents\PowerShell\profile.ps1
```

Predictor project:

```text
$HOME\Documents\PowerShell\Projects\ProfileAliasPredictor
```

Compiled DLL:

```text
$HOME\Documents\PowerShell\Projects\ProfileAliasPredictor\bin\Release\net10.0\ProfileAliasPredictor.dll
```

The profile is responsible for:

- locating the DLL
- importing it
- configuring PSReadLine
- exposing management commands
- testing predictor health

---

## 29. Profile Development Commands

The companion profile exposes several predictor-management aliases.

### predcd

Purpose:

Change to the ProfileAliasPredictor project directory.

Use:

```powershell
predcd
```

Equivalent target:

```text
$HOME\Documents\PowerShell\Projects\ProfileAliasPredictor
```

### predproj

Purpose:

Open the predictor project in Visual Studio Code.

Use:

```powershell
predproj
```

This is normally the fastest way to start predictor development.

### predcmd

Purpose:

Open Command Prompt already positioned in the predictor project.

Use:

```powershell
predcmd
```

This is useful for rebuild workflows where PowerShell itself must be terminated because it holds the predictor DLL open.

### predbuild

Purpose:

Build ProfileAliasPredictor in Release mode.

Use:

```powershell
predbuild
```

Use this when the output DLL is not locked by a running PowerShell process.

### predkill

Purpose:

Stop PowerShell 7 processes so the loaded predictor DLL can be released.

Use:

```powershell
predkill
```

Force form:

```powershell
predkill -Force
```

Save work in PowerShell sessions before using this command.

### predrebuild

Purpose:

Handle the predictor's common rebuild problem: PowerShell has loaded the DLL and Windows therefore prevents the build process from replacing it.

Use:

```powershell
predrebuild
```

Force form:

```powershell
predrebuild -Force
```

This is the preferred workflow after changing `ProfileAliasPredictor.cs`.

### predinfo

Purpose:

Display project/build information such as:

- project path
- C# source path
- `.csproj` path
- compiled DLL path
- whether the DLL exists
- DLL modification time
- whether the module is loaded

Use:

```powershell
predinfo
```

### predtest

Purpose:

Run an end-to-end health check.

It verifies important integration points such as:

- DLL exists
- module is loaded
- `CommandPredictor` is registered
- PSReadLine prediction is configured
- history prediction state
- plug-in prediction state
- profile aliases are registered

Use:

```powershell
predtest
```

This is the preferred validation command after a rebuild.

### predstate

Purpose:

Show current prediction configuration.

Use:

```powershell
predstate
```

A normal full-featured state should show `HistoryAndPlugin`.

### predon

Purpose:

Enable plug-in predictions.

Use:

```powershell
predon
```

### predoff

Purpose:

Disable predictor plug-ins.

Use:

```powershell
predoff
```

### histon

Purpose:

Enable history-based suggestions.

Use:

```powershell
histon
```

### histoff

Purpose:

Hide history suggestions without deleting the saved history.

Use:

```powershell
histoff
```

This can be especially useful during predictor testing because it makes `[ProfileAliases]` suggestions easier to isolate.

---

## 30. New User Setup - Complete Start-to-Finish

The following sequence is the recommended installation model.

### Step 1 - Install prerequisites

Install:

- PowerShell 7
- .NET SDK compatible with the project target
- Git
- Visual Studio Code
- PSReadLine

### Step 2 - Clone powershell-profile

Clone the companion repository into:

```text
$HOME\Documents\PowerShell
```

Repository:

```text
https://github.com/leon-mil/powershell-profile.git
```

The critical file is:

```text
profile.ps1
```

### Step 3 - Create Projects directory

```powershell
New-Item `
    -ItemType Directory `
    -Path "$HOME\Documents\PowerShell\Projects" `
    -Force
```

### Step 4 - Clone ProfileAliasPredictor

```powershell
Set-Location "$HOME\Documents\PowerShell\Projects"

git clone `
    https://github.com/leon-mil/profile-alias-predictor.git `
    ProfileAliasPredictor
```

### Step 5 - Build the predictor

```powershell
Set-Location .\ProfileAliasPredictor

dotnet restore .\ProfileAliasPredictor.csproj

dotnet build .\ProfileAliasPredictor.csproj -c Release
```

### Step 6 - Verify the DLL

```powershell
Test-Path `
    .\bin\Release\net10.0\ProfileAliasPredictor.dll
```

Expected:

```text
True
```

### Step 7 - Start a new PowerShell 7 session

The companion profile should load automatically if it is configured as the active profile.

Check the active path:

```powershell
$PROFILE.CurrentUserAllHosts
```

The expected design is:

```text
$HOME\Documents\PowerShell\profile.ps1
```

### Step 8 - Verify profile commands

```powershell
predinfo
```

Then:

```powershell
predtest
```

Then:

```powershell
predstate
```

### Step 9 - Test prediction

Type:

```text
pred
```

Do not immediately press Enter.

PSReadLine should show matching `[ProfileAliases]` suggestions.

### Step 10 - Test a multi-word prediction

Type the beginning of a command that has a parameterized template, for example:

```text
predrebuild -
```

The matching parameterized suggestion should be eligible to appear.

### Step 11 - Confirm normal operation

The installation is healthy when:

```text
Profile loads successfully
DLL exists
Module is loaded
ProfileAliases predictor is registered
PSReadLine plug-in predictions are enabled
predtest succeeds
custom suggestions appear while typing
```

---

## 31. How to Add a New Prediction

Suppose a new profile alias is added:

```text
examplecmd
```

and the real PowerShell function already exists.

Open:

```text
ProfileAliasPredictor.cs
```

Add an entry to `AliasDefinitions` in the appropriate category:

```csharp
new(
    "examplecmd",
    "Describe what examplecmd does"),
```

Save the file.

Rebuild:

```powershell
predrebuild
```

Open/reopen PowerShell as required by the rebuild workflow.

Validate:

```powershell
predtest
```

Then type:

```text
example
```

and confirm the suggestion appears.

---

## 32. How to Add a Multi-Word Prediction

The predictor supports multi-word prefixes.

Example:

```csharp
new(
    "examplecmd -WhatIf",
    "Preview the example operation"),
```

Because matching uses `StartsWith` against the complete input, typing:

```text
examplecmd -
```

can match the parameterized suggestion.

No special tokenization is required.

---

## 33. How to Rename a Command Safely

A rename should be treated as a two-repository change.

For example:

```text
old-command
```

to:

```text
new-command
```

Update:

1. the real alias/function registration in `powershell-profile`
2. profile help/description
3. `ProfileAliasPredictor.cs`
4. any predictor templates using the old name
5. documentation in both repositories
6. tests or smoke checks

Then:

```powershell
rprof
```

for the profile change, followed by:

```powershell
predrebuild
```

for the C# change.

Finally:

```powershell
predtest
```

This prevents stale predictions that suggest commands no longer registered in PowerShell.

---

## 34. Why the DLL Can Be Locked

After the profile imports:

```text
ProfileAliasPredictor.dll
```

the DLL is loaded into the running `pwsh.exe` process.

On Windows, the build may then be unable to overwrite that DLL.

Typical symptoms include messages such as:

```text
Could not copy ...
The process cannot access the file ...
ProfileAliasPredictor.dll is being used by another process
```

This does not normally indicate a C# code problem.

It means one or more PowerShell processes still have the DLL loaded.

Use:

```powershell
predrebuild
```

instead of repeatedly running `dotnet build` from a session that has the predictor loaded.

---

## 35. Manual DLL-Lock Recovery

If the helper workflow is unavailable, close every PowerShell 7 process that has imported the predictor.

From a Command Prompt that is not hosted by the affected `pwsh.exe` process:

```cmd
taskkill /IM pwsh.exe /F
```

Then:

```cmd
cd /d C:\Users\<username>\Documents\PowerShell\Projects\ProfileAliasPredictor
```

Build:

```cmd
dotnet build ProfileAliasPredictor.csproj -c Release
```

Then start a new PowerShell 7 session.

Use this manual procedure only when needed. `predrebuild` is the normal supported workflow.

---

## 36. Testing the Subsystem Manually

Check the binary module:

```powershell
Get-Module ProfileAliasPredictor
```

Check predictor implementations:

```powershell
Get-PSSubsystem -Kind CommandPredictor
```

The registered name should include:

```text
ProfileAliases
```

Check PSReadLine:

```powershell
Get-PSReadLineOption |
    Select-Object PredictionSource, PredictionViewStyle
```

Typical configuration:

```text
PredictionSource : HistoryAndPlugin
PredictionView   : ListView
```

The exact property display can vary with PSReadLine version.

---

## 37. Troubleshooting

### No suggestions appear

Run:

```powershell
predstate
```

Then:

```powershell
predtest
```

Then:

```powershell
predinfo
```

Check that:

- the DLL exists
- the module is loaded
- `ProfileAliases` is registered
- plug-in prediction is enabled

### History appears but ProfileAliases does not

Temporarily run:

```powershell
histoff
```

Then type a known predictor prefix.

This isolates plug-in predictions from history suggestions.

### Suggestion exists but command fails

The predictor and profile are separate.

Check the real command:

```powershell
Get-Command <command-name> -ErrorAction SilentlyContinue
```

Check the alias if applicable:

```powershell
Get-Alias <alias-name> -ErrorAction SilentlyContinue
```

If the predictor knows the text but PowerShell does not know the command, synchronize the profile and predictor.

### Changed C# but old suggestions still appear

The source file changed but the loaded DLL did not.

Run:

```powershell
predrebuild
```

Then start the new/reopened PowerShell session and run:

```powershell
predtest
```

### Normal build fails with file-in-use errors

The DLL is loaded.

Use:

```powershell
predrebuild
```

### Project path is wrong

The intended portable location is:

```text
$HOME\Documents\PowerShell\Projects\ProfileAliasPredictor
```

Update the companion profile if a different local layout is intentionally used.

### `ProfileAliases` is registered but PSReadLine does not display it

Check:

```powershell
(Get-PSReadLineOption).PredictionSource
```

It must include plug-in prediction, such as:

```text
Plugin
```

or:

```text
HistoryAndPlugin
```

---

## 38. Recommended Development Workflow

For a predictor-only change:

```text
predproj
    |
edit ProfileAliasPredictor.cs
    |
save
    |
predrebuild
    |
new/reopened PowerShell 7 session
    |
predtest
    |
type the changed prefix
    |
verify [ProfileAliases] suggestion
    |
git diff
    |
commit
    |
push
```

For a profile-only change:

```text
eprof
    |
edit profile.ps1
    |
save
    |
rprof
    |
test command
```

A .NET rebuild is normally unnecessary if `ProfileAliasPredictor.cs` did not change.

For a command rename or description change that affects both repositories:

```text
update profile
    |
rprof
    |
update predictor
    |
predrebuild
    |
predtest
    |
commit both repositories
```

---

## 39. Git Workflow

Check repository status:

```powershell
git status
```

Review source changes:

```powershell
git diff
```

Stage:

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

Confirm clean state:

```powershell
git status
```

Do not commit generated `bin` or `obj` output.

---

## 40. Suggested Repository Documentation Structure

A maintainable documentation structure is:

```text
docs\
|-- ProfileAliasPredictor_Guide.md
|-- architecture.md
|-- command-catalog.md
|-- development.md
`-- troubleshooting.md
```

This guide can serve as the main comprehensive manual, while smaller documents can be added later if individual topics grow large.

---

## 41. Maintenance Rules

Keep these rules in mind during future work.

1. `profile.ps1` defines real PowerShell behavior.
2. `ProfileAliasPredictor.cs` defines predictive suggestions.
3. A suggestion is not a command implementation.
4. Keep profile aliases and predictor entries synchronized.
5. Use `predrebuild` after C# changes.
6. Use `rprof` after profile-only changes.
7. Use `predtest` after predictor or integration changes.
8. Do not commit `bin/` or `obj/`.
9. Preserve the predictor GUID unless there is a deliberate reason to create a new subsystem identity.
10. Keep matching case-insensitive and multi-word capable unless the predictor design intentionally changes.
11. Add new predictions to the logical category that matches their purpose.
12. Keep descriptions short enough to scan in PSReadLine but specific enough to explain the command.
13. Update documentation whenever a user-facing command name changes.

---

## 42. Quick Reference

| Task | Command |
|---|---|
| Open predictor project | `predproj` |
| Go to predictor directory | `predcd` |
| Open CMD in project | `predcmd` |
| Build predictor | `predbuild` |
| Rebuild loaded predictor | `predrebuild` |
| Stop PowerShell processes | `predkill` |
| Show predictor information | `predinfo` |
| Test integration | `predtest` |
| Show prediction state | `predstate` |
| Enable plug-in predictions | `predon` |
| Disable plug-in predictions | `predoff` |
| Enable history predictions | `histon` |
| Hide history predictions | `histoff` |
| Manual Release build | `dotnet build .\ProfileAliasPredictor.csproj -c Release` |
| Verify subsystem | `Get-PSSubsystem -Kind CommandPredictor` |
| Verify PSReadLine | `Get-PSReadLineOption` |

---

## 43. Final New-User Checklist

Before considering installation complete, verify:

```text
[ ] PowerShell 7 is installed
[ ] Compatible .NET SDK is installed
[ ] Git is installed
[ ] powershell-profile is installed
[ ] ProfileAliasPredictor is cloned under Documents\PowerShell\Projects
[ ] dotnet restore succeeds
[ ] Release build succeeds
[ ] ProfileAliasPredictor.dll exists
[ ] profile.ps1 loads successfully
[ ] ProfileAliasPredictor module is loaded
[ ] ProfileAliases appears in Get-PSSubsystem
[ ] PSReadLine plug-in prediction is enabled
[ ] predinfo works
[ ] predtest succeeds
[ ] predstate shows the expected configuration
[ ] typing a known prefix displays ProfileAliases suggestions
```

Once these checks pass, the predictor is installed, integrated, and ready for normal use.
