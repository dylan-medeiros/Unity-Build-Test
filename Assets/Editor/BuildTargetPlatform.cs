using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Linq;

using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Build;

public class BuildTargetPlatform
{
    private const string CommandLineBuildFolderArg = "-buildFolder";

    public static void PerformBuild()
    {
        // Read custom output path from command line
        string[] args = Environment.GetCommandLineArgs();
        string buildFolder = null;

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == CommandLineBuildFolderArg && i + 1 < args.Length)
            {
                buildFolder = args[i + 1];
            }
        }

        if (string.IsNullOrEmpty(buildFolder))
        {
            Debug.Log("No build folder specified from command line. Using default fallback.");
            buildFolder = GetDefaultBuildFolder();
            Directory.CreateDirectory(buildFolder);
        }

        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
        string buildPath = GetBuildExtension(buildFolder, Application.productName, target);

        var options = new BuildPlayerOptions
        {
            scenes = GetEnabledScenes(),
            locationPathName = buildPath,
            target = target,
            options = BuildOptions.None
        };

        BuildAddressables();

        Debug.Log("Building project...");
        var report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            throw new Exception($"Build failed: {report.summary.result}");
        }

        Debug.Log($"Build succeeded: {buildPath}");
    }

    private static void BuildAddressables()
    {
        Debug.Log("Building Addressables...");

        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;

        if (settings == null)
        {
            throw new Exception("AddressableAssetSettings not found.");
        }

        AddressableAssetSettings.CleanPlayerContent();

        AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);

        if (!string.IsNullOrEmpty(result.Error))
        {
            throw new Exception($"Addressables build failed: {result.Error}");
        }

        Debug.Log("Addressables build completed.");
    }

    private static string[] GetEnabledScenes()
    {
        var scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        if (scenes.Length == 0)
            throw new Exception("No enabled scenes found in Build Settings.");

        return scenes;
    }

    private static string GetBuildExtension(string folder, string projectName, BuildTarget target)
    {
        switch (target)
        {
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                return Path.Combine(folder, projectName + ".exe");

            default:
                return Path.Combine(folder, projectName);
        }
    }

    private static string GetDefaultBuildFolder()
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        return Path.Combine(projectRoot, "Builds");
    }
}