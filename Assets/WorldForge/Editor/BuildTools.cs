using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace WorldForge.EditorTools
{
    /// <summary>Standalone builds. Menu: WorldForge > Build Linux Player / Build Windows Player.</summary>
    public static class BuildTools
    {
        [MenuItem("WorldForge/Build Linux Player", priority = 20)]
        public static void BuildLinux() => Build(BuildTarget.StandaloneLinux64, "Build/Linux/WorldForge.x86_64");

        [MenuItem("WorldForge/Build Windows Player", priority = 21)]
        public static void BuildWindows() => Build(BuildTarget.StandaloneWindows64, "Build/Windows/WorldForge.exe");

        static void Build(BuildTarget target, string path)
        {
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { SceneBuilder.ScenePath },
                locationPathName = path,
                target = target,
                options = BuildOptions.None,
            });
            var summary = report.summary;
            Debug.Log($"[WorldForge] Build {summary.result}: {path} ({summary.totalSize / (1024 * 1024)} MB, {summary.totalErrors} errors)");
            if (Application.isBatchMode && summary.result != BuildResult.Succeeded) EditorApplication.Exit(1);
        }
    }
}
