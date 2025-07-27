using UnityEditor;
using System.IO;
using UnityEngine;

public class ExportPackage
{
    static void PerformExport()
    {
        string exportPath = "Build/ExampleProject.unitypackage";
        string[] assetPaths = new string[] {
            "Assets/MyCustomPackage"
        };

        if (!Directory.Exists("Build"))
            Directory.CreateDirectory("Build");

        AssetDatabase.ExportPackage(assetPaths, exportPath, ExportPackageOptions.Recurse);
        Debug.Log("Package exported to " + exportPath);
    }
}
