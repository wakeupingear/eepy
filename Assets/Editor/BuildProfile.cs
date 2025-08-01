#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Eepy
{
    [CreateAssetMenu(fileName = "NewBuildProfile", menuName = "Build/Build Profile", order = 0)]
    public class BuildProfile : ScriptableObject
    {

        [SerializeField, Header("Build Settings"), Tooltip("The target platform for this build.")]
        private BuildTarget buildTarget = BuildTarget.StandaloneWindows64;
        
        [SerializeField, Tooltip("The target group for this build. Should match the BuildTarget.")]
        private BuildTargetGroup buildTargetGroup = BuildTargetGroup.Standalone;

        [SerializeField, Tooltip("Subfolder for the build output, relative to the main build path.")]
        private string outputFolder = "Windows/Standalone";

        [SerializeField, Tooltip("The name of the executable or app bundle.")]
        private string outputName = "MyGame";

        [SerializeField, Header("Scripting Defines"), Tooltip("A list of scriptable objects that represent the scripting defines for this build.")]
        private List<BuildDefine> defines = new List<BuildDefine>();

        public BuildTarget Target => buildTarget;
        public BuildTargetGroup TargetGroup => buildTargetGroup;
        public string OutputFolder => outputFolder;
        public string OutputName => outputName;
        public List<BuildDefine> Defines => defines;

        public string GetFullPath(string baseBuildPath)
        {
            string path = Path.Combine(baseBuildPath, outputFolder);
            string finalName = outputName;

            // Add platform-specific extensions
            switch (buildTarget)
            {
                case BuildTarget.StandaloneWindows64:
                case BuildTarget.StandaloneWindows:
                    finalName += ".exe";
                    break;
                case BuildTarget.StandaloneOSX:
                    finalName += ".app";
                    break;
                case BuildTarget.StandaloneLinux64:
                    finalName += ".x86_64";
                    break;
            }

            return Path.Combine(path, finalName);
        }

        public string GetScriptingDefines()
        {
            List<string> allDefines = new List<string>();
            foreach (var define in defines)
            {
                if (define != null)
                {
                    allDefines.Add(define.GetDefine());
                }
            }
            
            return string.Join(";", allDefines);
        }
    }
}
#endif
