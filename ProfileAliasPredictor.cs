using System;
using System.Collections.Generic;
using System.Threading;
using System.Management.Automation;
using System.Management.Automation.Subsystem;
using System.Management.Automation.Subsystem.Prediction;

namespace Leon.PowerShell.Prediction;

/// <summary>
/// Provides predictive suggestions for aliases defined in Leon's
/// PowerShell profile.
/// </summary>
public sealed class ProfileAliasPredictor : ICommandPredictor
{
    private static readonly AliasDefinition[] AliasDefinitions =
    [
        // Profile management
        new(
            "phelp",
            "Display the PowerShell profile help menu"),

        new(
            "palias",
            "List aliases managed by the PowerShell profile"),

        new(
            "ppath",
            "Display the active PowerShell profile path"),

        new(
            "eprof",
            "Open the PowerShell development workspace in Visual Studio Code"),

        new(
            "rprof",
            "Reload the PowerShell profile"),

        // Command history
        new(
            "hpath",
            "Display the persistent PowerShell history-file path"),

        new(
            "hsearch",
            "Search persistent PowerShell command history"),

        new(
            "ehist",
            "Open persistent PowerShell command history in Visual Studio Code"),

        // Prediction controls
        new(
            "histon",
            "Enable history-based command suggestions"),

        new(
            "histoff",
            "Hide history suggestions without deleting history"),

        new(
            "predon",
            "Enable ProfileAliases and other predictor plug-ins"),

        new(
            "predoff",
            "Disable predictor plug-ins"),

        new(
            "predstate",
            "Display the current prediction configuration"),

        // CPRS        
        new(
            "cprs-build",
            "Rebuild the CPRS client in Release mode"),


        // CPRS client TEST deployment
        new(
            "cprs-test-deploy",
            "Build CPRS, archive Current, and deploy to V:\\TEST\\EXE\\Current"),

        new(
            "cprs-test-deploy -SkipBuild",
            "Use the existing Release build, archive Current, and replace Current"),

        new(
            "cprs-test-deploy -WhatIf",
            "Preview a Current deployment without building or changing files"),

        new(
            "cprs-test-deploy -SkipBuild -WhatIf",
            "Preview deploying the existing Release build to Current"),

        new(
            "cprs-test-deploy -Folder <folder>",
            "Build CPRS and replace V:\\TEST\\EXE\\<folder> without touching Current"),

        new(
            "cprs-test-deploy -Folder <folder> -SkipBuild",
            "Use the existing Release build and replace V:\\TEST\\EXE\\<folder>"),

        new(
            "cprs-test-deploy -Folder <folder> -WhatIf",
            "Preview building and deploying CPRS to a custom TEST folder"),

        new(
            "cprs-test-deploy -Folder <folder> -SkipBuild -WhatIf",
            "Preview deploying the existing Release build to a custom TEST folder"),
        
        // CPRS client production deployment
        new(
            "cprs-prod-deploy",
            @"Promote V:\TEST\EXE\Current to V:\PROD\EXE\CPRS II"),

        new(
            "cprs-prod-deploy -WhatIf",
            @"Preview V:\TEST\EXE\Current -> V:\PROD\EXE\CPRS II"),

        new(
            "cprs-prod-deploy -Source <folder>",
            @"Promote V:\TEST\EXE\<folder> to V:\PROD\EXE\CPRS II"),

        new(
            "cprs-prod-deploy -Source <folder> -WhatIf",
            @"Preview V:\TEST\EXE\<folder> -> V:\PROD\EXE\CPRS II"),

        new(
            @"cprs-prod-deploy -Source V:\TEST\EXE\<folder>",
            @"Promote a full TEST path to V:\PROD\EXE\CPRS II"),

        new(
            @"cprs-prod-deploy -Source V:\TEST\EXE\<folder> -WhatIf",
            "Preview promotion from a full TEST path"),

        new(
            "cprs-prod-deploy -Folder <folder>",
            @"Promote V:\TEST\EXE\<folder>; -Folder is an alias for -Source"),

        new(
            "cprs-prod-deploy -Folder <folder> -WhatIf",
            "Preview production promotion using the -Folder alias"),

        new(
            "cprs-ce-deploy",
            "CE Residential Improvements deployment commands"),

        new(
            "cprs-ce-deploy preview feature local",
            "PREVIEW  FEATURE -> LOCAL"),
        new(
            "cprs-ce-deploy deploy feature local",
            "DEPLOY   FEATURE -> LOCAL"),

        new(
            "cprs-ce-deploy preview feature dev",
            "PREVIEW  FEATURE -> DEV"),
        new(
            "cprs-ce-deploy deploy feature dev",
            "DEPLOY   FEATURE -> DEV"),

        new(
            "cprs-ce-deploy preview feature test",
            "PREVIEW  FEATURE -> TEST"),
        new(
            "cprs-ce-deploy deploy feature test",
            "DEPLOY   FEATURE -> TEST"),

        new(
            "cprs-ce-deploy preview feature prod",
            "PREVIEW  FEATURE -> PROD"),
        new(
            "cprs-ce-deploy deploy feature prod",
            "DEPLOY   FEATURE -> PROD"),

        new(
            "cprs-ce-deploy preview local dev",
            "PREVIEW  LOCAL -> DEV"),
        new(
            "cprs-ce-deploy deploy local dev",
            "DEPLOY   LOCAL -> DEV"),

        new(
            "cprs-ce-deploy preview local test",
            "PREVIEW  LOCAL -> TEST"),
        new(
            "cprs-ce-deploy deploy local test",
            "DEPLOY   LOCAL -> TEST"),

        new(
            "cprs-ce-deploy preview local prod",
            "PREVIEW  LOCAL -> PROD"),
        new(
            "cprs-ce-deploy deploy local prod",
            "DEPLOY   LOCAL -> PROD"),

        new(
            "cprs-ce-deploy preview dev test",
            "PREVIEW  DEV -> TEST"),
        new(
            "cprs-ce-deploy deploy dev test",
            "DEPLOY   DEV -> TEST"),

        new(
            "cprs-ce-deploy preview dev prod",
            "PREVIEW  DEV -> PROD"),
        new(
            "cprs-ce-deploy deploy dev prod",
            "DEPLOY   DEV -> PROD"),

        new("cprs-ce-deploy preview test prod",
            "PREVIEW  TEST -> PROD"),
        new(
            "cprs-ce-deploy deploy test prod",
            "DEPLOY   TEST -> PROD"),

        // Utilities
        new(
            "c",
            "Clear the PowerShell console"),

        new(
            "which",
            "Find a command, alias, function, or executable"),

        new(
            "la",
            "List all files and directories, including hidden items"),

        new(
            "up",
            "Move up one directory"),

        new(
            "up2",
            "Move up two directories"),

        new(
            "home",
            "Open the current user home directory"),

        new(
            "here",
            "Open the current directory in File Explorer"),

        new(
            "codehere",
            "Open the current directory in Visual Studio Code"),

        new(
            "path",
            "Display PATH entries one per line"),

        new(
            "admin",
            "Open an elevated PowerShell 7 session"),

        new(
            "groot",
            "Move to the root of the current Git repository"),

        new(
            "ltr",
            "Detailed listing sorted oldest to newest like ls -ltr"),

        new(
            "dtree",
            "Display the current directory tree"),

        new(
            "dtree -Files",
            "Display the directory tree including files"),
        new(
            "backup-file",
            "Create a verified timestamped backup of a file"),

        new(
            "backup-dir",
            "Create a verified timestamped backup of a directory"),

        new(
            "zip-file",
            "Create and verify a ZIP archive containing one file"),

        new(
            "zip-dir",
            "Create and verify a ZIP archive of a directory"),
        new(
            "unzip",
            "Extract a ZIP using automatic destination selection"),

        new(
            "unzip -Destination ",
            "Extract a ZIP to a specific directory"),

        new(
            "unzip -WhatIf",
            "Preview ZIP extraction without changing files"),

        new(
            "unzip -Force",
            "Extract and allow existing destination files to be replaced"),
        new(
            "clear-dir",
            "Clear the current directory while preserving the directory"),

        new(
            "clear-dir -WhatIf",
            "Preview clearing the current directory without deleting anything"),

        new(
            "clear-dir \"C:\\Users\\mil00001\\Pictures\\Screenshots\"",
            "Clear all contents from the Screenshots directory"),

        new(
            "clear-dir \"C:\\Users\\mil00001\\Pictures\\Screenshots\" -WhatIf",
            "Preview clearing the Screenshots directory without deleting anything"),
        new(
            "clear-bin",
            "Permanently empty the Windows Recycle Bin"),

        new(
            "clear-bin -WhatIf",
            "Preview emptying the Windows Recycle Bin"),

        new(
            "clear-bin -Confirm",
            "Empty the Windows Recycle Bin with confirmation"),


        // Local development
        new(
            "cdev",
            @"Open C:\Development-CPRS"),

        new(
            "csas",
            @"Open C:\Development-CPRS\cprs-sasprogs"),

        new(
            "cbatch",
            @"Open C:\Development-CPRS\cprs-batch"),

        new(
            "fileops",
            @"Open V:\DEV\Utilities\FileOpsTool\FileOpsManager.code-workspace"),

        
        // Profile development
        new(
            "pdir",
            "Open the PowerShell profile development directory"),

        new(
            "predcd",
            "Open the ProfileAliasPredictor project directory"),

        new(
            "predcmd",
            "Open Command Prompt in the predictor project"),

        new(
            "predbuild",
            "Build ProfileAliasPredictor in Release mode"),

        new(
            "predkill",
            "Stop all PowerShell 7 processes to release the predictor DLL"),

        new(
            "predkill -Force",
            "Stop all PowerShell 7 processes without confirmation"),

        new(
            "predrebuild",
            "Stop PowerShell and rebuild ProfileAliasPredictor"),

        new(
            "predrebuild -Force",
            "Stop PowerShell and rebuild ProfileAliasPredictor without confirmation"),
        new(
            "predproj",
            "Open the ProfileAliasPredictor project in Visual Studio Code"),
        new(
            "predinfo",
            "Display ProfileAliasPredictor project and build information"),

        new(
            "predtest",
            "Verify the PowerShell profile and predictor configuration"),

        // Production
        new(
            "pbatch",
            @"Open V:\PROD\BATCH"),

        new(
            "psas",
            @"Open V:\PROD\SASPRGS"),

        new(
            "plogs",
            @"Open V:\PROD\LOGS\SASLOGS"),

        // Git
        new(
            "gst",
            "Show the current Git branch and repository status"),

        new(
            "glog",
            "Show recent Git commits with dates and changed files"),

        new(
            "gfile",
            "Show staged, unstaged, and untracked Git files")
    ];

    private readonly Guid _id;

    internal ProfileAliasPredictor(Guid id)
    {
        _id = id;
    }

    /// <summary>
    /// Gets the unique identifier for this predictor.
    /// </summary>
    public Guid Id => _id;

    /// <summary>
    /// Gets the predictor name displayed by PSReadLine.
    /// </summary>
    public string Name => "ProfileAliases";

    /// <summary>
    /// Gets the predictor description.
    /// </summary>
    public string Description =>
        "Predicts custom aliases defined in Leon's PowerShell profile.";

    /// <summary>
    /// Returns matching alias suggestions for the text currently being typed.
    /// </summary>
    public SuggestionPackage GetSuggestion(
        PredictionClient client,
        PredictionContext context,
        CancellationToken cancellationToken)
    {
        string input = context.InputAst.Extent.Text.Trim();

        // Do not generate suggestions for an empty command line.
        if (string.IsNullOrWhiteSpace(input))
        {
            return default;
        }

        var suggestions = new List<PredictiveSuggestion>();

        foreach (AliasDefinition aliasDefinition in AliasDefinitions)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return default;
            }

            if (!aliasDefinition.Name.StartsWith(
                    input,
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            suggestions.Add(
                new PredictiveSuggestion(
                    aliasDefinition.Name,
                    aliasDefinition.Description));
        }

        return suggestions.Count == 0
            ? default
            : new SuggestionPackage(suggestions);
    }

    /// <summary>
    /// Indicates that this predictor does not process PSReadLine feedback.
    /// </summary>
    public bool CanAcceptFeedback(
        PredictionClient client,
        PredictorFeedbackKind feedback)
    {
        return false;
    }

    public void OnSuggestionDisplayed(
        PredictionClient client,
        uint session,
        int countOrIndex)
    {
    }

    public void OnSuggestionAccepted(
        PredictionClient client,
        uint session,
        string acceptedSuggestion)
    {
    }

    public void OnCommandLineAccepted(
        PredictionClient client,
        IReadOnlyList<string> history)
    {
    }

    public void OnCommandLineExecuted(
        PredictionClient client,
        string commandLine,
        bool success)
    {
    }

    private sealed record AliasDefinition(
        string Name,
        string Description);
}

/// <summary>
/// Registers the predictor when the binary module is imported and unregisters
/// it when the module is removed.
/// </summary>
public sealed class Init : IModuleAssemblyInitializer, IModuleAssemblyCleanup
{
    private static readonly Guid Identifier =
        new("7196c538-e218-470f-9f19-a305d65cc9dc");

    public void OnImport()
    {
        var predictor = new ProfileAliasPredictor(Identifier);

        SubsystemManager.RegisterSubsystem(
            SubsystemKind.CommandPredictor,
            predictor);
    }

    public void OnRemove(PSModuleInfo psModuleInfo)
    {
        SubsystemManager.UnregisterSubsystem(
            SubsystemKind.CommandPredictor,
            Identifier);
    }
}