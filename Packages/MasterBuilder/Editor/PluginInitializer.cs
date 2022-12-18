using MasterBuilder.BuildIn;
using UnityEditor;

namespace MasterBuilder.Editor
{
    public static class PluginInitializer
    {
        [InitializeOnLoadMethod]
        public static void Initialize()
        {
            MasterRegistry.RegisterMasterType(typeof(LocalizeStringCollection));

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
            MasterSheetGenerator.Generate(MasterRegistry.MasterTypes);
        }
    }
}