using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace UnitySetup.Editor
{
    public class SetupWindow : EditorWindow
    {
        private static SetupWindow _setupWindow;

        [SerializeField] private Vector2 _scrollBar;

        private ContentInfo _selectContent;
        private List<ContentInfo> _contents = new List<ContentInfo>();
        private Dictionary<int, bool> _installList = new Dictionary<int, bool>();

        private int _installPackageCount = 0;
        private List<string> _installedPackage = new List<string>();
        private List<string> _packageUrls = new List<string>();

        [MenuItem("Setup/Open Window")]
        public static void Open()
        {
            if (_setupWindow == null)
            {
                _setupWindow = CreateWindow<SetupWindow>(typeof(SceneView));
                _setupWindow.titleContent = new GUIContent("Setup");
            }

            _setupWindow.Show();
        }

        private void OnEnable()
        {
            InitializeContents();

            AssetDatabase.importPackageCompleted += AssetDatabaseOnimportPackageCompleted;
            AssetDatabase.importPackageCancelled += AssetDatabaseOnimportPackageCancelled;
        }

        private void OnDisable()
        {
            AssetDatabase.importPackageCompleted -= AssetDatabaseOnimportPackageCompleted;
            AssetDatabase.importPackageCancelled -= AssetDatabaseOnimportPackageCancelled;
        }

        private void AssetDatabaseOnimportPackageCompleted(string packageName)
        {
            _installedPackage.Add(packageName);

            if (_installedPackage.Count == _installPackageCount)
            {
                EditorApplication.UnlockReloadAssemblies();
            }
        }

        private void AssetDatabaseOnimportPackageCancelled(string packagename)
        {
            _installPackageCount = 0;
            _installedPackage.Clear();

            EditorApplication.UnlockReloadAssemblies();
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            if (GUILayout.Button("Install"))
            {
                InstallContents();
            }

            GUILayout.Space(10);
            if (GUILayout.Button("Reload"))
            {
                InitializeContents();
            }

            GUILayout.Space(5);

            _scrollBar = EditorGUILayout.BeginScrollView(_scrollBar, EditorStyles.helpBox);

            if (_contents.Count == 0)
            {
                EditorGUILayout.HelpBox("Content Not Found.", MessageType.Error);
            }

            foreach (var content in _contents)
            {
                var index = _contents.IndexOf(content);
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                _installList[index] = EditorGUILayout.Toggle(_installList[index]);
                EditorGUILayout.LabelField(content.Title);

                if (GUILayout.Button("Detail"))
                {
                    _selectContent = content;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            if (_selectContent == null)
            {
                EditorGUILayout.LabelField("<Details>");
                EditorGUILayout.LabelField(String.Empty);
                EditorGUILayout.LabelField(String.Empty);
                EditorGUILayout.LabelField(String.Empty);
            }
            else
            {
                EditorGUILayout.LabelField("<Details>");
                EditorGUILayout.LabelField($"ContentTitle: {_selectContent?.Title}");
                EditorGUILayout.LabelField($"Description: {_selectContent?.Description}");
                EditorGUILayout.LabelField($"Type: {_selectContent?.Type.ToString()}");
            }

            EditorGUILayout.EndVertical();
        }

        private void InitializeContents()
        {
            _contents.Clear();

            var textAsset = Resources.Load<TextAsset>("contents");
            var jObject = JObject.Parse(textAsset.text);
            var contentArray = jObject.GetValue("contents") as JArray;
            if (contentArray == null)
                return;

            foreach (var contentObj in contentArray)
            {
                if (contentObj.Value<bool>("ignore"))
                    continue;

                var content = new ContentInfo
                {
                    Title = contentObj.Value<string>("title"),
                    Description = contentObj.Value<string>("description"),
                    Content = contentObj.Value<string>("content"),
                    Type = (ContentType)contentObj.Value<int>("type")
                };
                _contents.Add(content);
                _installList[_contents.IndexOf(content)] = true;
            }
        }

        private void InstallContents()
        {
            EditorApplication.LockReloadAssemblies();

            var orderBy = _installList.Select(e =>
            {
                if (!e.Value)
                    return null;

                var content = _contents[e.Key];
                return content;
            }).Where(e => e != null).GroupBy(e => e.Type, e => e);
            _installPackageCount = orderBy.GetHashCode();

            foreach (var group in orderBy)
            {
                foreach (var contentInfo in group)
                {
                    switch (group.Key)
                    {
                        case ContentType.Zip:
                            InstallZipContent(contentInfo);
                            break;
                        case ContentType.UnityPackage:
                            InstallUnityPackageContent(contentInfo);
                            break;
                        case ContentType.PackageManager:
                            if (string.IsNullOrWhiteSpace(contentInfo.Content))
                            {
                                Debug.LogError($"Skip Install Content {contentInfo.Title}");
                                return;
                            }

                            _packageUrls.Add(contentInfo.Content);
                            break;
                    }
                }

                if (group.Key == ContentType.PackageManager)
                {
                    InstallPackageManagerContents();
                }
            }

            InstallPackageManagerContents();
        }

        private void InstallZipContent(ContentInfo contentInfo)
        {
            throw new NotImplementedException();
        }

        private void InstallUnityPackageContent(ContentInfo contentInfo)
        {
            EditorUtility.DisplayProgressBar("Download Package", $"Downloading {contentInfo.Title}", 0.5f);
            WebClient webClient = new WebClient();
            webClient.DownloadFile(contentInfo.Content, $"{contentInfo.Title}.unitypackage");

            EditorUtility.ClearProgressBar();

            AssetDatabase.ImportPackage($"{contentInfo.Title}.unitypackage", false);

            File.Delete($"{contentInfo.Title}.unitypackage");
        }

        private void InstallPackageManagerContents()
        {
            var packages = _packageUrls.ToArray();
            Client.AddAndRemove(packages);

            _installedPackage.AddRange(packages);
        }
    }

    public class ContentInfo
    {
        public string Title { get; set; }
        public string Description { get; set; }

        public string Content { get; set; }

        public ContentType Type { get; set; }
    }

    public enum ContentType
    {
        PackageManager,
        Zip,
        UnityPackage,
    }
}