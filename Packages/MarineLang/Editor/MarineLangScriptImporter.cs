using System;
using System.IO;
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