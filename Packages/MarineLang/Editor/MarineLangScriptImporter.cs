#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace MarineLang.Unity.Editor
{
    [ScriptedImporter(1, "mrn")]
    public class MarineLangScriptImporter : ScriptedImporter
    {
        public override bool SupportsRemappedAssetType(Type type)
        {
            return true;
        }

        public override void OnImportAsset(AssetImportContext ctx)
        {
            var scriptText = File.ReadAllText(ctx.assetPath);
            var obj = ScriptableObject.CreateInstance<MarineLangScriptAsset>();
            obj.scriptText = scriptText;

            ctx.AddObjectToAsset("script", obj);
            ctx.SetMainObject(obj);
        }
    }

    [CustomEditor(typeof(MarineLangScriptAsset))]
    public class MarineLangScriptAssetEditor : UnityEditor.Editor
    {
        [MenuItem("Assets/Create/MarineLang Script", priority = 30)]
        private static void CreateAsset()
        {
            var flag = BindingFlags.Static | BindingFlags.NonPublic;
            var projectWindowUtilType = typeof(ProjectWindowUtil);
            var methodInfo = projectWindowUtilType.GetMethod("GetActiveFolderPath", flag);
            if (methodInfo == null)
            {
                return;
            }

            var path = methodInfo.Invoke(null, Array.Empty<object>()) as string;
            if (string.IsNullOrWhiteSpace(path))
                return;

            var baseName = "script";
            var dir = new DirectoryInfo(path);
            var num = string.Empty;
            try
            {
                num += dir.GetFiles("*.mrn", SearchOption.TopDirectoryOnly).Select(e => e.Name)
                    .Where(e => e.StartsWith(baseName))
                    .Select(e => e.Replace(baseName, string.Empty).Replace(".mrn", string.Empty))
                    .Max(e => int.TryParse(e, out var val) ? val : 0) + 1;
            }
            catch
            {
                // ignored
            }

            var filePath = $"{path}/script{num}.mrn";
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, string.Empty);
                AssetDatabase.ImportAsset(filePath);

                var asset = AssetDatabase.LoadAssetAtPath<MarineLangScriptAsset>(filePath);
                Selection.activeInstanceID = asset.GetInstanceID();
            }
        }

        public override void OnInspectorGUI()
        {
            if (target is MarineLangScriptAsset asset)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    GUILayout.Box(asset.scriptText, EditorStyles.whiteLabel);
                }
                EditorGUILayout.EndVertical();
            }
        }
    }
}
#endif
