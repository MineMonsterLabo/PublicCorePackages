using UnityEditor;

namespace MasterBuilder.Editor
{
    public static class PluginInitializer
    {
        [InitializeOnLoadMethod]
        public static void Initialize()
        {
            AssemblyReloadEvents.afterAssemblyReload += AfterAssemblyReload;
        }

        [MenuItem("Master/Generate Xlsx Sheet")]
        public static void GenerateSheet()
        {
            MasterSheetGenerator.Generate(MasterRegistry.MasterTypes);
        }

        [MenuItem("Master/Reimport Xlsx Sheet")]
        public static void ReImportSheet()
        {
        }

        private static void AfterAssemblyReload()
        {
            if (EditorOptions.IsEnableAutoRegisterType)
                MasterRegistry.RegisterMasterTypeFromAssemblies();

            if (EditorOptions.IsEnableAutoGenerateSheet)
                MasterSheetGenerator.Generate(MasterRegistry.MasterTypes);
        }
    }
}