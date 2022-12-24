using System.IO;
using UnityEditor;
using UnityEngine;

namespace MasterBuilder.Editor.Settings
{
    public class MasterBuilderSettings : ScriptableObject
    {
        private const string SettingsFolder = "Assets/Editor/MasterBuilder/Settings";
        private const string SettingsPath = SettingsFolder + "/MasterBuilderSettings.asset";

        public bool isEnableAutoRegisterType = true;
        public bool isEnableAutoGenerateSheet = false;

        public static MasterBuilderSettings GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<MasterBuilderSettings>(SettingsFolder);
            if (settings == null)
            {
                settings = CreateInstance<MasterBuilderSettings>();
                settings.isEnableAutoRegisterType = true;
                settings.isEnableAutoGenerateSheet = false;
                Directory.CreateDirectory(SettingsFolder);
                AssetDatabase.CreateAsset(settings, SettingsPath);
                AssetDatabase.SaveAssetIfDirty(settings);
            }

            return settings;
        }
    }

    static class MineGodGeneralSettingsRegister
    {
        [SettingsProvider]
        public static SettingsProvider CreateMineGodGeneralSettingsProvider()
        {
            var settings = new SerializedObject(MasterBuilderSettings.GetOrCreateSettings());
            var provider = new SettingsProvider("Project/MasterBuilder/Settings", SettingsScope.Project)
            {
                label = "Settings",
                guiHandler = searchContext =>
                {
                    EditorGUILayout.PropertyField(
                        settings.FindProperty(nameof(MasterBuilderSettings.isEnableAutoRegisterType)));
                    EditorGUILayout.PropertyField(
                        settings.FindProperty(nameof(MasterBuilderSettings.isEnableAutoGenerateSheet)));

                    settings.ApplyModifiedPropertiesWithoutUndo();

                    AssetDatabase.SaveAssetIfDirty(settings.targetObject);
                },
                keywords = SettingsProvider.GetSearchKeywordsFromSerializedObject(settings)
            };

            return provider;
        }
    }
}