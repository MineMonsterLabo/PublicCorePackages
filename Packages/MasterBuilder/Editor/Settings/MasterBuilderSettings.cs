using System.Collections.Generic;
using System.IO;
using MasterBuilder.BuildIn;
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

        public List<string> generateIgnoreMasters = new List<string>
            { "Packages/MasterBuilder/Resources/MasterBuilder/Localize" };

        public static MasterBuilderSettings GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<MasterBuilderSettings>(SettingsFolder);
            if (settings == null)
            {
                settings = CreateInstance<MasterBuilderSettings>();
                settings.isEnableAutoRegisterType = true;
                settings.isEnableAutoGenerateSheet = false;
                settings.generateIgnoreMasters = new List<string>
                    { "Packages/MasterBuilder/Resources/MasterBuilder/Localize" };
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
                label = "settings".InternalLocalizeString(),
                guiHandler = searchContext =>
                {
                    EditorGUILayout.PropertyField(
                        settings.FindProperty(nameof(MasterBuilderSettings.isEnableAutoRegisterType)),
                        new GUIContent("enable_automatic_generation_of_master_definition".InternalLocalizeString()));
                    EditorGUILayout.PropertyField(
                        settings.FindProperty(nameof(MasterBuilderSettings.isEnableAutoGenerateSheet)),
                        new GUIContent("enable_automatic_generation_of_master_sheets".InternalLocalizeString()));

                    EditorGUILayout.PropertyField(
                        settings.FindProperty(nameof(MasterBuilderSettings.generateIgnoreMasters)));

                    settings.ApplyModifiedPropertiesWithoutUndo();

                    AssetDatabase.SaveAssetIfDirty(settings.targetObject);
                },
                keywords = SettingsProvider.GetSearchKeywordsFromSerializedObject(settings)
            };

            return provider;
        }
    }
}