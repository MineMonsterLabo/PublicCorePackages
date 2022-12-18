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
    }
}