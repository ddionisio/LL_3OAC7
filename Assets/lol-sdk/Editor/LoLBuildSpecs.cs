using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.Reflection;
using SimpleJSON;

namespace LoL.Editor
{
    /// <summary>
    /// Legends of Learning Build specs.
    /// Runs a Preprocess build function to get the current Application companyName, productName, version, unityVersion.
    /// Will also get current WebGL PlayerSettings and the directory structure of the project.
    /// The specs will be saved to the project's StreamingAssets directory as "lol_build_specs.json".
    /// </summary>
    public class LoLBuildSpecs : IPreprocessBuildWithReport
    {
        const string BuildSpecsFile = "lol_build_specs.json";
        const string AssetsPath = "Assets";
        const string LibraryPath = "Library";
        const string PackageCachePath = "PackageCache";
        string _projectBase;

        public int callbackOrder { get { return 0; } }

        /// <summary>
        /// Gets the static properties of a type
        /// and sets to JObject.
        /// </summary>
        /// <returns>The static properties.</returns>
        /// <param name="type">Type.</param>
        JSONNode GetStaticPropertiesToJson(System.Type type)
        {
            var values = JSON.Parse("{}");
            var properties = type.GetProperties(BindingFlags.Static | BindingFlags.Public);
            for (int i = 0; i < properties.Length; ++i)
            {
                var property = properties[i];
                // Ignore obsolete properties.
                if (System.Attribute.IsDefined(property, typeof(System.ObsoleteAttribute)))
                    continue;

                var value = property.GetValue(null, null);
                values[property.Name] = values == null ? string.Empty : value.ToString();
            }

            return values;
        }

        /// <summary>
        /// Gets the directory structure for the whole project.
        /// </summary>
        /// <returns>The directory structure.</returns>
        /// <param name="path">Path.</param>
        /// <param name="array">Array.</param>
        JSONArray GetDirectoryStructure(string path, JSONArray array = null, bool includeChildren = false)
        {
            if (array == null)
                array = new JSONArray();

            var subFolders = System.IO.Directory.GetDirectories(path);

            if (subFolders == null || subFolders.Length == 0)
                return null;

            for (int i = 0; i < subFolders.Length; ++i)
            {
                array.Add(subFolders[i].Substring(_projectBase.Length));
                if (includeChildren)
                    GetDirectoryStructure(subFolders[i], array, includeChildren);
            }

            return array;
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            // Set base project path to exclude it from json.
            _projectBase = Application.dataPath.Replace(AssetsPath, string.Empty);

            // Force RunInBackground false.
            Application.runInBackground= false;

            // Create the specs.
            var specs = JSON.Parse("{}");
            specs["companyName"] = Application.companyName;
            specs["productName"] = Application.productName;
            specs["version"] = Application.version;
            specs["unityVersion"] = Application.unityVersion;

            // Get the webgl settings.
            specs["webglSettings"] = GetStaticPropertiesToJson(typeof(PlayerSettings.WebGL));

            // Get the directory structure.
            var assetDirs = GetDirectoryStructure(Application.dataPath, null, true);
            specs["dir"] = GetDirectoryStructure(Application.dataPath.Replace(AssetsPath, System.IO.Path.Combine(LibraryPath, PackageCachePath)), assetDirs);

            // Serialize and save to streaming assets.
            System.IO.Directory.CreateDirectory(Application.streamingAssetsPath);
            System.IO.File.WriteAllText(System.IO.Path.Combine(Application.streamingAssetsPath, BuildSpecsFile), specs.ToString());

            AssetDatabase.Refresh();
        }
    }
}