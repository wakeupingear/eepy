#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Eepy
{
    public class BuildProfileWindow : EditorWindow
    {
        private struct LogEntry
        {
            public string message;
            public LogType type;
        }

        private Vector2 scrollPosition;
        private BuildProfile[] buildProfiles;
        private int selectedProfileIndex = -1;
        private static readonly string BaseBuildPath = Path.Combine("..", "Builds");
        private static List<LogEntry> logEntries = new List<LogEntry>();
        // Script Debugging is not supported in the public API (at least I couldn't find it /shrug)
        private bool developmentBuild, autoConnectProfiler, allowDebugging, allowDeepProfiling;

        [MenuItem("Window/Build Profiles")]
        public static void ShowWindow()
        {
            GetWindow<BuildProfileWindow>("Build Profiles");
        }

        private void OnEnable()
        {
            LoadBuildProfiles();
        }

        private void LoadBuildProfiles()
        {
            string[] guids = AssetDatabase.FindAssets("t:BuildProfile");
            buildProfiles = new BuildProfile[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                buildProfiles[i] = AssetDatabase.LoadAssetAtPath<BuildProfile>(path);
            }
            buildProfiles = buildProfiles.OrderBy(p => p.name).ToArray();
        }

        private void OnGUI()
        {
            // Defers logging to the next frame so the logs aren't lost during the script reload
            if (logEntries.Count > 0)
            {
                foreach (LogEntry logEntry in logEntries)
                {
                    switch (logEntry.type)
                    {
                        case LogType.Error:
                            Debug.LogError(logEntry.message);
                            break;
                        case LogType.Warning:
                            Debug.LogWarning(logEntry.message);
                            break;
                        default:
                            Debug.Log(logEntry.message);
                            break;
                    }
                }
                logEntries.Clear();
            }

            EditorGUILayout.LabelField("Build Profiles", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            if (buildProfiles == null || buildProfiles.Length == 0)
            {
                EditorGUILayout.HelpBox("No Build Profiles found in the project. Create some from the Assets/Create/Build/Build Profile menu.", MessageType.Info);
                if (GUILayout.Button("Refresh Profiles"))
                {
                    LoadBuildProfiles();
                }
                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));
            for (int i = 0; i < buildProfiles.Length; i++)
            {
                bool isSelected = EditorGUILayout.ToggleLeft(buildProfiles[i].name, selectedProfileIndex == i);
                if (isSelected && selectedProfileIndex != i)
                {
                    selectedProfileIndex = i;
                }
                else if (!isSelected && selectedProfileIndex == i)
                {
                    selectedProfileIndex = -1;
                }
            }
            EditorGUILayout.EndScrollView();

            if (selectedProfileIndex != -1)
            {
                BuildProfile selectedProfile = buildProfiles[selectedProfileIndex];
                if (selectedProfile.Target == BuildTarget.StandaloneOSX)
                {
                    // TODO: figure out why this toggle causes an error: "The build cannot be appended"
                    // UnityEditor.OSXStandalone.UserBuildSettings.createXcodeProject = EditorGUILayout.Toggle("Create Xcode Project", UnityEditor.OSXStandalone.UserBuildSettings.createXcodeProject);
                }
                else if (selectedProfile.Target == BuildTarget.StandaloneWindows64 || selectedProfile.Target == BuildTarget.StandaloneWindows)
                {
                    UnityEditor.WindowsStandalone.UserBuildSettings.createSolution = EditorGUILayout.Toggle("Create Visual Studio Solution", UnityEditor.WindowsStandalone.UserBuildSettings.createSolution);
                    UnityEditor.WindowsStandalone.UserBuildSettings.copyPDBFiles = EditorGUILayout.Toggle("Copy PDB Files", UnityEditor.WindowsStandalone.UserBuildSettings.copyPDBFiles);
                }
            }

            developmentBuild = EditorGUILayout.Toggle("Development Build", developmentBuild);
            EditorGUI.BeginDisabledGroup(!developmentBuild);
            autoConnectProfiler = EditorGUILayout.Toggle("Autoconnect Profiler", autoConnectProfiler);
            allowDebugging = EditorGUILayout.Toggle("Allow Debugging", allowDebugging);
            allowDeepProfiling = EditorGUILayout.Toggle("Allow Deep Profiling", allowDeepProfiling);
            EditorGUI.EndDisabledGroup();

            GUI.enabled = selectedProfileIndex != -1;
            if (GUILayout.Button("Build Selected Profile", GUILayout.Height(40)))
            {
                BuildProfile selectedProfile = buildProfiles[selectedProfileIndex];
                BuildProjectCmd(selectedProfile, developmentBuild, autoConnectProfiler, allowDebugging, allowDeepProfiling);
            }
            GUI.enabled = true;

            EditorGUILayout.Space();
            if (GUILayout.Button("Refresh Profiles"))
            {
                LoadBuildProfiles();
            }
        }

        // Defers the build until after the AssetDatabase is free
        public static void BuildProjectCmd(BuildProfile profile, bool developmentBuild, bool autoConnectProfiler, bool allowDebugging, bool allowDeepProfiling)
        {
            BuildForProfile(profile, developmentBuild, autoConnectProfiler, allowDebugging, allowDeepProfiling);
        }


        private static void BuildForProfile(BuildProfile profile, bool developmentBuild, bool autoConnectProfiler, bool allowDebugging, bool allowDeepProfiling)
        {
            if (profile == null)
            {
                logEntries.Add(new LogEntry { message = "Build failed: No build profile provided.", type = LogType.Error });
                return;
            }

            string outputPath = profile.GetFullPath(BaseBuildPath);
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

            string currentDefine = PlayerSettings.GetScriptingDefineSymbolsForGroup(profile.TargetGroup);

            try
            {
                string[] scenes = EditorBuildSettings.scenes
                    .Where(s => s.enabled)
                    .Select(s => s.path)
                    .ToArray();

                BuildOptions currentOptions = BuildOptions.None;
                if (developmentBuild)
                {
                    currentOptions |= BuildOptions.Development;
                    if (allowDebugging)
                        currentOptions |= BuildOptions.AllowDebugging;
                    if (allowDeepProfiling)
                        currentOptions |= BuildOptions.EnableDeepProfilingSupport;
                    if (autoConnectProfiler)
                        currentOptions |= BuildOptions.ConnectWithProfiler;
                }

                switch (profile.Target)
                {
                    case BuildTarget.iOS:
                        currentOptions |= BuildOptions.AcceptExternalModificationsToPlayer;
                        break;
                    case BuildTarget.Android:
                        currentOptions |= BuildOptions.AcceptExternalModificationsToPlayer;
                        EditorUserBuildSettings.exportAsGoogleAndroidProject = true;
                        break;
                    case BuildTarget.StandaloneOSX:
                        if (UnityEditor.OSXStandalone.UserBuildSettings.createXcodeProject)
                        {
                            currentOptions |= BuildOptions.AcceptExternalModificationsToPlayer;
                        }
                        break;
                    default:
                        break;
                }

                BuildPlayerOptions buildOptions = new BuildPlayerOptions
                {
                    scenes = scenes,
                    locationPathName = outputPath,
                    target = profile.Target,
                    options = currentOptions,
                    targetGroup = profile.TargetGroup
                };

                string newDefines = profile.GetScriptingDefines();
                PlayerSettings.SetScriptingDefineSymbolsForGroup(profile.TargetGroup, newDefines);
                Debug.Log($"Building '{profile.name}' with defines: {newDefines}");

                BuildReport report = BuildPipeline.BuildPlayer(buildOptions);

                if (report.summary.result == BuildResult.Succeeded)
                {
                    logEntries.Add(new LogEntry { message = $"Build SUCCEEDED for profile '{profile.name}' at: {report.summary.outputPath}", type = LogType.Log });
                    string containerPath = Path.Combine(BaseBuildPath, profile.OutputFolder);
                    logEntries.Add(new LogEntry { message = $"Revealing build in Finder: {containerPath}", type = LogType.Log });
                    EditorUtility.RevealInFinder(containerPath);
                }
                else
                {
                    logEntries.Add(new LogEntry { message = $"Build FAILED for profile '{profile.name}'. Check the console for details.", type = LogType.Error });
                }
            }
            catch (Exception e)
            {
                logEntries.Add(new LogEntry { message = $"An exception occurred during build for profile '{profile.name}': {e.Message}\n{e.StackTrace}", type = LogType.Error });
            }
            finally
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(profile.TargetGroup, currentDefine);
                AssetDatabase.Refresh();
            }
        }
    }
}
#endif