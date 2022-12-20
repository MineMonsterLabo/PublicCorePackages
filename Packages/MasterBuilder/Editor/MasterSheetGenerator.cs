using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ClosedXML.Excel;
using MasterBuilder.Attributes;
using UnityEditor;
using UnityEngine;

namespace MasterBuilder.Editor
{
    public static class MasterSheetGenerator
    {
        public static void Generate(IReadOnlyDictionary<string, Type> types)
        {
            var generateInfoList = new Dictionary<string, GenerateInfo>();
            foreach (var pair in types)
            {
                var type = pair.Value;
                var masterAssetAttributes = type.GetCustomAttributes<MasterAssetAttribute>().ToArray();
                if (masterAssetAttributes.Length == 0)
                {
                    const string key = "Assets/Resources/Masters/Master";
                    if (!generateInfoList.TryGetValue(key, out var info))
                    {
                        info = new GenerateInfo
                        {
                            Attribute = new MasterAssetAttribute
                            {
                                AssetPath = key
                            }
                        };
                        generateInfoList.Add(key, info);
                    }

                    info.Types.Add(pair.Key, pair.Value);

                    continue;
                }

                foreach (var assetAttribute in masterAssetAttributes)
                {
                    var key = assetAttribute.AssetPath;
                    if (!generateInfoList.TryGetValue(key, out var info))
                    {
                        info = new GenerateInfo
                        {
                            Attribute = assetAttribute
                        };
                        generateInfoList.Add(key, info);
                    }

                    info.Types.Add(pair.Key, pair.Value);
                }
            }

            foreach (var pair in generateInfoList)
            {
                GenerateXlsxFile(pair.Value);
            }
        }

        private static void GenerateXlsxFile(GenerateInfo info)
        {
            using var workbook = File.Exists($"{info.Attribute.AssetPath}.xlsx")
                ? new XLWorkbook($"{info.Attribute.AssetPath}.xlsx")
                : new XLWorkbook();

            foreach (var pair in info.Types)
            {
                GenerateXlsxSheet(workbook, pair.Key, pair.Value);
            }

            if (workbook.Worksheets.Count == 0)
            {
                Debug.LogWarning("生成が必要なシートはないため、作成をスキップしました。");
                return;
            }

            workbook.SaveAs($"{info.Attribute.AssetPath}.xlsx");
            AssetDatabase.Refresh();
        }

        private static void GenerateXlsxSheet(XLWorkbook workbook, string name, Type type)
        {
            if (!workbook.TryGetWorksheet(name, out var workSheet))
            {
                workSheet = workbook.AddWorksheet(name);
            }

            var masterAttribute = type.GetCustomAttribute<MasterAttribute>() ?? new MasterAttribute
            {
                Name = name,
                Contexts = new[] { "en-US" }
            };
            var properties =
                type.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public |
                                   BindingFlags.NonPublic)
                    .OrderBy(e => e.GetCustomAttribute<MasterColumnAttribute>()?.Order ?? -1);

            var row = 2;
            var col = 2;
            var classNameLabelCell = workSheet.Cell(row, col);
            var classNameCell = workSheet.Cell(row, col + 1);
            var contextsLabelCell = workSheet.Cell(++row, col);
            var contextsCell = workSheet.Cell(row, col + 1);
            classNameLabelCell.Value = "ClassName:";
            classNameCell.Value = type.AssemblyQualifiedName;
            contextsLabelCell.Value = "Contexts:";
            contextsCell.Value = string.Join(",", masterAttribute.Contexts);

            col = 2;
            row += 2;
            var infoStartRow = row;
            var columnNameLabelCell = workSheet.Cell(++row, col);
            var typeLabelCell = workSheet.Cell(++row, col);
            var requireLabelCell = workSheet.Cell(++row, col);
            var contextLabelCell = workSheet.Cell(++row, col);
            columnNameLabelCell.Value = "ColumnName:";
            columnNameLabelCell.Style.Fill.BackgroundColor = XLColor.GreenYellow;
            typeLabelCell.Value = "Type:";
            requireLabelCell.Value = "Require:";
            contextLabelCell.Value = "Context:";
            contextLabelCell.Style.Fill.BackgroundColor = XLColor.Aquamarine;

            ++col;
            foreach (var propertyInfo in properties)
            {
                row = infoStartRow;

                var propertyType = propertyInfo.PropertyType;
                if (!IsSupportTypes(propertyType))
                    continue;

                var masterIgnoreAttribute = propertyInfo.GetCustomAttribute<MasterIgnoreAttribute>();
                if (masterIgnoreAttribute != null)
                    continue;

                GenerateXlsxSheetColumn(workSheet, masterAttribute.Contexts, propertyInfo, ref row, ref col);
            }

            workSheet.SheetView.Freeze(10, 3);
            workSheet.Columns().Width = 20;
            workSheet.Column(1).Width = 2;
        }

        private static void GenerateXlsxSheetColumn(IXLWorksheet workSheet, string[] contexts,
            PropertyInfo propertyInfo, ref int row, ref int col)
        {
            var infoStartRow = row;
            var propertyType = propertyInfo.PropertyType;
            var masterColumnAttribute = propertyInfo.GetCustomAttribute<MasterColumnAttribute>();
            var masterReferenceAttribute = propertyInfo.GetCustomAttribute<MasterReferenceAttribute>();

            var isContextSwitch = masterColumnAttribute?.IsContextSwitch ?? false;
            var isMultiColumn = propertyType.IsSubclassOf(typeof(Enum)) || masterReferenceAttribute != null;
            var enableContexts = isMultiColumn ? new[] { "any-context", "shadow-column" } : new[] { "any-context" };
            if (isContextSwitch)
            {
                enableContexts = contexts.Except(masterColumnAttribute?.DisableContexts ?? Array.Empty<string>())
                    .ToArray();
            }

            foreach (var context in enableContexts)
            {
                row = infoStartRow;

                var columnNameCell = workSheet.Cell(++row, col);
                var typeCell = workSheet.Cell(++row, col);
                var requireCell = workSheet.Cell(++row, col);
                var contextCell = workSheet.Cell(++row, col);
                var valueCell = workSheet.Cell(++row, col);

                ++col;
                if (context == "shadow-column")
                {
                    continue;
                }

                columnNameCell.Value = propertyInfo.Name;
                columnNameCell.Style.Fill.BackgroundColor = XLColor.GreenYellow;
                typeCell.Value = propertyType.Name;
                requireCell.Value = ToYesNoString(masterColumnAttribute?.IsAllowEmpty ?? true);
                contextCell.Value = isContextSwitch ? context : string.Empty;
                contextCell.Style.Fill.BackgroundColor = XLColor.Aquamarine;

                var validation = valueCell.GetDataValidation() ?? valueCell.CreateDataValidation();
                validation.AllowedValues = TypeFromAllowedValues(propertyType);
            }
        }

        private static string ToYesNoString(bool b)
        {
            return b ? "yes" : "no";
        }

        private static bool IsSupportTypes(Type type)
        {
            return type == typeof(int) || type == typeof(float) || type == typeof(string) ||
                   type.IsSubclassOf(typeof(Enum));
        }

        private static XLAllowedValues TypeFromAllowedValues(Type type)
        {
            if (type == typeof(int) || type == typeof(float))
                return XLAllowedValues.Decimal;

            return XLAllowedValues.AnyValue;
        }

        private class GenerateInfo
        {
            public MasterAssetAttribute Attribute { get; set; }

            public Dictionary<string, Type> Types { get; set; } = new Dictionary<string, Type>();
        }
    }
}