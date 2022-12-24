using MasterBuilder.Editor.Settings;
using UnityEditor;

namespace MasterBuilder.Editor
{
    public static class PluginInitializer
    {
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            AssemblyReloadEvents.afterAssemblyReload += AfterAssemblyReload;
        }

        [MenuItem("Master/Generate Xlsx Sheet")]
        private static void GenerateSheet()
        {
            MasterSheetGenerator.Generate(MasterRegistry.MasterTypes);
        }

        private static void AfterAssemblyReload()
        {
            var setting = MasterBuilderSettings.GetOrCreateSettings();
            if (setting.isEnableAutoRegisterType)
                MasterRegistry.RegisterMasterTypeFromAssemblies();

            if (setting.isEnableAutoGenerateSheet)
                MasterSheetGenerator.Generate(MasterRegistry.MasterTypes);
        }
    }
}