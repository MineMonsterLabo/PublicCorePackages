using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MarineLang.LexicalAnalysis;
using MarineLang.MacroPlugins;
using MarineLang.PresetMacroPlugins;
using MarineLang.SyntaxAnalysis;
using MarineLang.Unity.Editor.Settings;
using UnityEditor;
using UnityEngine;

namespace MarineLang.Unity.Editor
{
    public class MarineLangScriptEditor : EditorWindow
    {
        private static Regex regex;
        private static MatchEvaluator evaluator;

        private static Func<string, string> _marineLangHighlighter
        {
            get
            {
                if (regex == null && evaluator == null)
                {
                    const string keyword = @"(fun|end|true|false|let|foreach|for|while|yield|if|else|ret|.await|new)";
                    // const string symbol = @"[{}()\[\]=,+\-*/<>]+";
                    const string str = "(\"[^\"\\n]*?\")";
                    const string digit = @"(?<![a-zA-Z_])[+-]?[0-9]+\.?[0-9]?(([eE][+-]?)?[0-9]+)?";
                    const string comment = @"/\*[\s\S]*?\*/|//.*";
                    const string type = @"[A-Z]([a-zA-z]|[0-9])+";
                    const string variable = @"[a-zA-Z]([a-z_]|[0-9])+";

                    var block = "(?<{0}>({1}))";
                    var pattern = "(" + string.Join("|", new string[]
                    {
                        string.Format(block, "comment", comment),
                        string.Format(block, "keyword", keyword),
                        string.Format(block, "type", type),
                        string.Format(block, "variable", variable),
                        string.Format(block, "string", str),
                        // string.Format(block, "symbol", symbol),
                        string.Format(block, "digit", digit),
                    }) + ")";

                    regex = new Regex(pattern, RegexOptions.Compiled);

                    var isProSkin = EditorGUIUtility.isProSkin;
                    var colorTable = new Dictionary<string, string>()
                    {
                        { "comment", isProSkin ? "#555555" : "#248700" },
                        { "keyword", isProSkin ? "#6C95EB" : "#0F54D6" },
                        { "type", isProSkin ? "#C191FF" : "#6B2FBA" },
                        { "variable", isProSkin ? "#38CB8A" : "#00855F" },
                        { "string", isProSkin ? "#C9A26D" : "#906F49" },
                        // { "symbol", isProSkin ? "#ff00ff" : "" },
                        { "digit", isProSkin ? "#84C367" : "#AB2F6B" },
                    };

                    evaluator = new MatchEvaluator(match =>
                    {
                        foreach (var pair in colorTable)
                        {
                            if (match.Groups[pair.Key].Success)
                            {
                                return string.Format("<color={1}>{0}</color>", match.Value, pair.Value);
                            }
                        }

                        return match.Value;
                    });
                }

                return code => regex.Replace(code, evaluator);
            }
        }

        private Vector2 _scrollBar;
        private Vector2 _errorScrollBar;

        private LexicalAnalyzer _lexicalAnalyzer;
        private SyntaxAnalyzer _syntaxAnalyzer;

        private SyntaxParseResult _parseResult;
        private string _cachedHighlightedCode;

        public event Action onSave;

        public bool isSaved;

        public string oldText;
        public string editingText;

        public string filePath;

        private Func<string, string> _highlighter;

        public static MarineLangScriptEditor Reshow(MarineLangScriptEditor window)
        {
            var newPopup = CreateWindow<MarineLangScriptEditor>(window.titleContent.text);
            newPopup.onSave = window.onSave;
            newPopup.isSaved = window.isSaved;
            newPopup.oldText = window.oldText;
            newPopup.editingText = window.editingText;
            newPopup.filePath = window.filePath;
            newPopup._highlighter = window._highlighter;
            newPopup._cachedHighlightedCode = window._cachedHighlightedCode;

            return newPopup;
        }

        public void SetText(string text, string path)
        {
            text ??= string.Empty;
            oldText = editingText = text;
            filePath = path;
            _highlighter = _marineLangHighlighter;
        }

        private void OnEnable()
        {
            var pluginContainer = new PluginContainer();
            pluginContainer.AddExprPlugin("constEval", new ConstExprPlugin());

            _lexicalAnalyzer = new LexicalAnalyzer();
            _syntaxAnalyzer = new SyntaxAnalyzer(pluginContainer);
        }

        private void OnGUI()
        {
            var prevBackgroundColor = GUI.backgroundColor;

            var backStyle = new GUIStyle(EditorStyles.textArea);
            backStyle.normal.textColor = Color.clear;
            backStyle.hover.textColor = Color.clear;
            backStyle.active.textColor = Color.clear;
            backStyle.focused.textColor = Color.clear;
            backStyle.wordWrap = false;

            _scrollBar = EditorGUILayout.BeginScrollView(_scrollBar, GUILayout.Height(position.height - 230));
            {
                EditorGUILayout.BeginHorizontal();
                {
                    int lineCount = editingText.Split('\n').Length;
                    StringBuilder lineString = new StringBuilder();
                    for (int i = 0; i < lineCount; i++)
                    {
                        lineString.Append(i + 1);
                        lineString.Append("\n");
                    }

                    lineString.Remove(lineString.Length - 1, 1);

                    EditorGUILayout.BeginVertical(GUILayout.Width(30));
                    {
                        EditorGUI.BeginDisabledGroup(true);
                        {
                            EditorGUILayout.TextArea(lineString.ToString(), GUILayout.ExpandHeight(true));
                        }
                        EditorGUI.EndDisabledGroup();
                    }
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical();
                    {
                        var editedCode = EditorGUILayout.TextArea(editingText, backStyle, GUILayout.ExpandHeight(true));

                        if (string.IsNullOrEmpty(_cachedHighlightedCode) || editedCode != editingText)
                        {
                            var code = editedCode;
                            if (string.IsNullOrWhiteSpace(filePath))
                            {
                                code = $"fun func_expr()\r\n  {editingText}\r\nend";
                            }

                            _parseResult = _syntaxAnalyzer.Parse(_lexicalAnalyzer.GetTokens(code));
                            _cachedHighlightedCode = _highlighter(editedCode);
                            editingText = editedCode;
                        }

                        GUI.backgroundColor = Color.clear;

                        var foreStyle = new GUIStyle(EditorStyles.textArea);
                        foreStyle.richText = true;
                        foreStyle.wordWrap = false;

                        EditorGUI.TextArea(GUILayoutUtility.GetLastRect(), _cachedHighlightedCode, foreStyle);

                        GUI.backgroundColor = prevBackgroundColor;
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            var count = _parseResult.parseErrorInfos?.Count() ?? 0;
            var isFolding = EditorGUILayout.Foldout(_parseResult.IsError, $"エラー一覧({count})");
            _errorScrollBar = EditorGUILayout.BeginScrollView(_errorScrollBar,
                GUILayout.Height(200 - EditorGUIUtility.singleLineHeight));
            {
                if (isFolding)
                {
                    if (!_parseResult.IsError || count <= 0)
                    {
                        EditorGUILayout.HelpBox("エラーはありません。", MessageType.Info);
                    }
                    else if (_parseResult.parseErrorInfos != null)
                    {
                        foreach (var errorInfo in _parseResult.parseErrorInfos)
                        {
                            var style = EditorStyles.helpBox;
                            style.fontSize = EditorStyles.label.fontSize;
                            GUILayout.Box(errorInfo.FullErrorMessage, style);
                        }
                    }
                }
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("保存"))
                {
                    onSave?.Invoke();
                    isSaved = true;
                    Close();
                }

                EditorGUI.BeginDisabledGroup(oldText != editingText);
                {
                    if (GUILayout.Button("Vscodeで開く"))
                    {
                        var vsCodePath = EditorUserSettings.GetConfigValue(MarineLangGeneralSettings.MarineLangVSCodePath);
                        var directory =
                            new DirectoryInfo(MarineLangGeneralSettings.GetOrCreateSettings().marineScriptPath);
                        directory.Create();

                        var openFilePath = filePath;
                        var isTempFile = false;
                        if (string.IsNullOrWhiteSpace(openFilePath))
                        {
                            isTempFile = true;
                            filePath = openFilePath = $"{directory.FullName}/{Guid.NewGuid()}.mrn";
                            var code = $"fun expr()\r\n\r\n{editingText}\r\n\r\nend";
                            File.WriteAllText(filePath, code);
                        }

                        var process = Process.Start(vsCodePath, $"-r \"{directory.FullName}\"");
                        // while ((process?.MainWindowHandle ?? IntPtr.Zero) != IntPtr.Zero)
                        // {
                        // }

                        process = Process.Start(vsCodePath, $"-w \"{openFilePath}\"");
                        if (isTempFile)
                        {
                            process?.WaitForExit();

                            if (File.Exists(filePath))
                            {
                                editingText = File.ReadAllText(filePath).Replace("fun expr()\r\n\r\n", string.Empty)
                                    .Replace("\r\n\r\nend", string.Empty);
                                File.Delete(filePath);
                                filePath = _cachedHighlightedCode = string.Empty;
                            }
                        }
                        else
                        {
                            Close();
                        }
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}