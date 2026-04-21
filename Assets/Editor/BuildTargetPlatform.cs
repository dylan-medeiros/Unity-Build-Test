using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class BuildTargetPlatform
{
    private static readonly string ArtifactRoot = @"C:\CI\Artifacts";

    public static void PerformBuild()
    {
        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
        string productName = Application.productName;

        // Date-only timestamp
        string date = DateTime.Now.ToString("yyyyMMdd");

        // Base folder: C:\CI\Artifacts\MyGame_20260420
        string baseFolder = Path.Combine(ArtifactRoot, $"{productName}_{date}");

        // Increment if folder exists
        var finalFolder = baseFolder;
        int counter = 1;

        while (Directory.Exists(finalFolder))
        {
            finalFolder = $"{baseFolder}_{counter}";
            counter++;
        }

        // Final Unity build path:
        // C:\CI\Artifacts\MyGame_20260420\MyGame
        string buildPath = Path.Combine(finalFolder, productName);

        var options = new BuildPlayerOptions
        {
            scenes = GetEnabledScenes(),
            locationPathName = buildPath,
            target = target,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            throw new Exception($"Build failed for {target}");
        }
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
}