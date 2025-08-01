using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace Eepy
{
    public static class ExportPackage
    {
        private static readonly string Eol = Environment.NewLine;

        public static void Export()
        {
            // Gather values from args
            Dictionary<string, string> options = GetValidatedOptions();

            // Set version for this package if provided
            if (options.TryGetValue("packageVersion", out string packageVersion) && packageVersion != "none")
            {
                PlayerSettings.bundleVersion = packageVersion;
            }

            // Perform the export
            string[] assetPaths = options.TryGetValue("assetPaths", out string paths)
                ? paths.Split(',')
                : new string[] { "Assets/Eepy" };

            ExportPackageOptions exportOptions = ExportPackageOptions.Recurse;
            if (options.TryGetValue("includeLibraryAssets", out string includeLibrary) && includeLibrary.ToLower() == "true")
            {
                exportOptions |= ExportPackageOptions.IncludeLibraryAssets;
            }

            // Create directory if it doesn't exist
            string directory = Path.GetDirectoryName(options["exportPath"]);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            try
            {
                AssetDatabase.ExportPackage(assetPaths, options["exportPath"], exportOptions);
                ReportSuccess(options["exportPath"]);
                EditorApplication.Exit(0);
            }
            catch (Exception e)
            {
                ReportError(e.Message);
                EditorApplication.Exit(1);
            }
        }

        private static Dictionary<string, string> GetValidatedOptions()
        {
            ParseCommandLineArguments(out Dictionary<string, string> validatedOptions);

            if (!validatedOptions.TryGetValue("projectPath", out string _))
            {
                Console.WriteLine("Missing argument -projectPath");
                EditorApplication.Exit(110);
            }

            if (!validatedOptions.TryGetValue("exportPath", out string exportPath))
            {
                const string defaultPath = "Build/Eepy.unitypackage";
                Console.WriteLine($"Missing argument -exportPath, defaulting to {defaultPath}");
                validatedOptions.Add("exportPath", defaultPath);
            }

            return validatedOptions;
        }

        private static void ParseCommandLineArguments(out Dictionary<string, string> providedArguments)
        {
            providedArguments = new Dictionary<string, string>();
            string[] args = Environment.GetCommandLineArgs();

            Console.WriteLine(
                $"{Eol}" +
                $"###########################{Eol}" +
                $"#    Parsing settings     #{Eol}" +
                $"###########################{Eol}" +
                $"{Eol}"
            );

            // Extract flags with optional values
            for (int current = 0, next = 1; current < args.Length; current++, next++)
            {
                // Parse flag
                bool isFlag = args[current].StartsWith("-");
                if (!isFlag) continue;
                string flag = args[current].TrimStart('-');

                // Parse optional value
                bool flagHasValue = next < args.Length && !args[next].StartsWith("-");
                string value = flagHasValue ? args[next].TrimStart('-') : "";

                // Assign
                Console.WriteLine($"Found flag \"{flag}\" with value \"{value}\"");
                providedArguments.Add(flag, value);
            }
        }

        private static void ReportSuccess(string exportPath)
        {
            Console.WriteLine(
                $"{Eol}" +
                $"###########################{Eol}" +
                $"#      Export results     #{Eol}" +
                $"###########################{Eol}" +
                $"{Eol}" +
                $"Package exported successfully to: {exportPath}{Eol}" +
                $"{Eol}"
            );
        }

        private static void ReportError(string error)
        {
            Console.WriteLine(
                $"{Eol}" +
                $"###########################{Eol}" +
                $"#      Export failed      #{Eol}" +
                $"###########################{Eol}" +
                $"{Eol}" +
                $"Error: {error}{Eol}" +
                $"{Eol}"
            );
        }
    }
}