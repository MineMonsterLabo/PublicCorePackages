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
            if (EditorOptions.IsEnableAutoRegisterType)
                MasterRegistry.RegisterMasterTypeFromAssemblies();

            if (EditorOptions.IsEnableAutoGenerateSheet)
                MasterSheetGenerator.Generate(MasterRegistry.MasterTypes);
        }
    }
}