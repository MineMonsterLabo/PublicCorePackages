using System.IO;
using UnityEditor;
using UnityEngine;

namespace MarineLang.Unity.Editor.Settings
{
    public class MarineLangGeneralSettings : ScriptableObject
    {
        private const string MarineLangSettingsFolder = "Assets/Editor/MarineLang/Settings";
        private const string MarineLangGeneralSettingsPath = MarineLangSettingsFolder + "/MarineLangGeneralSettings.asset";

        internal const string MarineLangVSCodePath = "VSCodePath";

        public string marineScriptPath = "Assets/Resources/MarineScripts";

        public static MarineLangGeneralSettings GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<MarineLangGeneralSettings>(MarineLangGeneralSettingsPath);
            if (settings == null)
            {
                settings = CreateInstance<MarineLangGeneralSettings>();
                settings.marineScriptPath = "Assets/Resources/MarineScripts";
                Directory.CreateDirectory(MarineLangSettingsFolder);
                AssetDatabase.CreateAsset(settings, MarineLangGeneralSettingsPath);
                AssetDatabase.SaveAssetIfDirty(settings);
            }

            return settings;
        }
    }

    static class MarineLangGeneralSettingsRegister
    {
        [SettingsProvider]
        public static SettingsProvider CreateMarineLangGeneralSettingsProvider()
        {
            var vsCodePath = EditorUserSettings.GetConfigValue(MarineLangGeneralSettings.MarineLangVSCodePath);
            var settings = new SerializedObject(MarineLangGeneralSettings.GetOrCreateSettings());
            var provider = new SettingsProvider("Project/MarineLang/GeneralSettings", SettingsScope.Project)
            {
                label = "MarineLang General",
                guiHandler = searchContext =>
                {
                    EditorGUILayout.SelectableLabel(vsCodePath);
                    if (GUILayout.Button("VSCodeのパスを参照"))
                    {
                        var path = EditorUtility.OpenFilePanel("VSCodeのパスを参照", string.Empty, "exe");
                        if (!string.IsNullOrWhiteSpace(path))
                        {
                            EditorUserSettings.SetConfigValue(MarineLangGeneralSettings.MarineLangVSCodePath, path);
                            vsCodePath = path;
                        }
                    }

                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(settings.FindProperty("marineScriptPath"),
                        new GUIContent("MarineLangパス"));

                    settings.ApplyModifiedPropertiesWithoutUndo();

                    AssetDatabase.SaveAssetIfDirty(settings.targetObject);
                },
                keywords = SettingsProvider.GetSearchKeywordsFromSerializedObject(settings)
            };

            return provider;
        }
    }
}