using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GroupEditor.Editor
{
    public class GroupEditorBase : UnityEditor.Editor
    {
        public const string OtherGroup = "Other";

        private Dictionary<string, bool> _foldoutGroup = new Dictionary<string, bool>();

        public override void OnInspectorGUI()
        {
            var dict = new Dictionary<string, List<PropertyItem>>();
            var iterator = serializedObject.GetIterator();
            iterator.NextVisible(true);
            while (iterator.NextVisible(false))
            {
                var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                var field = target.GetType().GetField(iterator.name, flags);
                if (field != null)
                {
                    var attribute = field.GetCustomAttribute<GroupAttribute>();
                    if (attribute != null)
                    {
                        var readOnly = field.GetCustomAttribute<ReadOnlyAttribute>();
                        AddGroupProperty(dict, attribute.Name,
                            new PropertyItem(attribute.Order, iterator.Copy(), readOnly?.IsReadOnly ?? false));
                    }
                    else
                    {
                        var readOnly = field.GetCustomAttribute<ReadOnlyAttribute>();
                        AddGroupProperty(dict, OtherGroup,
                            new PropertyItem(0, iterator.Copy(), readOnly?.IsReadOnly ?? false));
                    }
                }
                else
                {
                    AddGroupProperty(dict, OtherGroup, new PropertyItem(0, iterator.Copy(), false));
                }
            }

            foreach (var pair in dict)
            {
                if (!_foldoutGroup.ContainsKey(pair.Key))
                {
                    _foldoutGroup[pair.Key] = true;
                }

                var foldout = EditorGUILayout.Foldout(_foldoutGroup[pair.Key], pair.Key, true, EditorStyles.foldout);

                GUILayout.BeginVertical(GUI.skin.box);
                {
                    if (foldout)
                    {
                        EditorGUI.indentLevel++;

                        var properties = pair.Value.OrderBy(e => e.Order);
                        foreach (var property in properties)
                        {
                            EditorGUI.BeginDisabledGroup(property.IsReadOnly);
                            {
                                EditorGUILayout.PropertyField(property.Property, true);
                            }
                            EditorGUI.EndDisabledGroup();
                        }

                        EditorGUI.indentLevel--;
                    }
                }
                GUILayout.EndVertical();

                _foldoutGroup[pair.Key] = foldout;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void AddGroupProperty(Dictionary<string, List<PropertyItem>> dict, string groupName,
            PropertyItem propertyItem)
        {
            if (!dict.ContainsKey(groupName))
            {
                dict.Add(groupName, new List<PropertyItem>());
            }

            dict[groupName].Add(propertyItem);
        }

        private class PropertyItem
        {
            public int Order { get; }
            public SerializedProperty Property { get; }
            public bool IsReadOnly { get; }

            public PropertyItem(int order, SerializedProperty property, bool isReadOnly)
            {
                Order = order;
                Property = property;
                IsReadOnly = isReadOnly;
            }
        }
    }
}