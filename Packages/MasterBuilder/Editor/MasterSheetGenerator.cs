using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MasterBuilder.Attributes;
using MasterBuilder.Editor.Extensions;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
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
            var isFileExists = File.Exists($"{info.Attribute.AssetPath}.xlsx");
            var workbook = isFileExists
                ? new XSSFWorkbook(new FileStream($"{info.Attribute.AssetPath}.xlsx", FileMode.Open, FileAccess.Read,
                    FileShare.ReadWrite))
                : new XSSFWorkbook();
            foreach (var pair in info.Types)
            {
                GenerateXlsxSheet(workbook, pair.Key, pair.Value);
            }

            if (workbook.Count == 0)
            {
                Debug.LogWarning("生成が必要なシートはないため、作成をスキップしました。");
                return;
            }

            using var steam = new FileStream($"{info.Attribute.AssetPath}.xlsx", FileMode.OpenOrCreate,
                FileAccess.ReadWrite);
            workbook.Write(steam, false);

            AssetDatabase.Refresh();
        }

        private static void GenerateXlsxSheet(IWorkbook workbook, string name, Type type)
        {
            var workSheet = workbook.GetSheet(name) ?? workbook.CreateSheet(name);
            var masterAttribute = type.GetCustomAttribute<MasterAttribute>() ?? new MasterAttribute
            {
                Name = name,
                Contexts = new[] { "default" }
            };
            var properties =
                type.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public |
                                   BindingFlags.NonPublic)
                    .OrderBy(e => e.GetCustomAttribute<MasterColumnAttribute>()?.Order ?? -1);

            var row = 2;
            var col = 2;
            var classNameLabelCell = workSheet.GetCell(row, col);
            var classNameCell = workSheet.GetCell(row, col + 1);
            var contextsLabelCell = workSheet.GetCell(++row, col);
            var contextsCell = workSheet.GetCell(row, col + 1);
            classNameLabelCell.SetCellValue("ClassName:");
            classNameCell.SetCellValue(type.AssemblyQualifiedName);
            contextsLabelCell.SetCellValue("Contexts:");
            contextsCell.SetCellValue(string.Join(",", masterAttribute.Contexts));

            col = 2;
            row += 2;
            var infoStartRow = row;
            var columnNameLabelCell = workSheet.GetCell(++row, col);
            var typeLabelCell = workSheet.GetCell(++row, col);
            var requireLabelCell = workSheet.GetCell(++row, col);
            var contextLabelCell = workSheet.GetCell(++row, col);
            columnNameLabelCell.SetCellValue("ColumnName:");
            columnNameLabelCell.CellStyle.FillBackgroundColor = HSSFColor.LightGreen.Index;
            typeLabelCell.SetCellValue("Type:");
            requireLabelCell.SetCellValue("Require:");
            contextLabelCell.SetCellValue("Context:");
            contextLabelCell.CellStyle.FillBackgroundColor = HSSFColor.Aqua.Index;

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

                GenerateXlsxSheetColumn(workbook, workSheet, masterAttribute.Contexts, propertyInfo, ref row, ref col);
            }

            workSheet.CreateFreezePane(4, 10);
        }

        private static void GenerateXlsxSheetColumn(IWorkbook workbook, ISheet workSheet, string[] contexts,
            PropertyInfo propertyInfo, ref int row, ref int col)
        {
            var infoStartRow = row;
            var propertyType = propertyInfo.PropertyType;
            var masterColumnAttribute = propertyInfo.GetCustomAttribute<MasterColumnAttribute>();
            var masterReferenceAttribute = propertyInfo.GetCustomAttribute<MasterReferenceAttribute>();

            var isContextSwitch = masterColumnAttribute?.IsContextSwitch ?? false;
            var isMultiColumn = masterReferenceAttribute != null;
            var enableContexts = isMultiColumn ? new[] { "any-context", "shadow-column" } : new[] { "any-context" };
            if (isContextSwitch)
            {
                enableContexts = contexts.Except(masterColumnAttribute?.DisableContexts ?? Array.Empty<string>())
                    .ToArray();
            }

            foreach (var context in enableContexts)
            {
                row = infoStartRow;

                var columnNameCell = workSheet.GetCell(++row, col);
                var typeCell = workSheet.GetCell(++row, col);
                var requireCell = workSheet.GetCell(++row, col);
                var contextCell = workSheet.GetCell(++row, col);
                var valueCell = workSheet.GetCell(++row, col);

                ++col;
                if (context == "shadow-column")
                {
                    columnNameCell.SetCellValue($"D__{propertyInfo.Name}");
                    columnNameCell.CellStyle.FillBackgroundColor = HSSFColor.LightGreen.Index;
                    typeCell.SetCellValue("Reference");
                    requireCell.SetCellValue("no");
                    // contextCell.Value = isContextSwitch ? context : string.Empty;
                    contextCell.CellStyle.FillBackgroundColor = HSSFColor.Aqua.Index;

                    // var masterName = MasterRegistry.GetTypeFromMasterName(masterReferenceAttribute?.ReferenceType);
                    // var listValidation = valueCell.GetDataValidation() ?? valueCell.CreateDataValidation();
                    // listValidation.AllowedValues = XLAllowedValues.List;
                    // listValidation.List($"=OFFSET({masterName}!$D$10, 0, 0, COUNTA(D:D), 0)");
                    continue;
                }

                columnNameCell.SetCellValue(propertyInfo.Name);
                columnNameCell.CellStyle.FillBackgroundColor = HSSFColor.LightGreen.Index;
                typeCell.SetCellValue(propertyType.Name);
                requireCell.SetCellValue(ToYesNoString(!masterColumnAttribute?.IsAllowEmpty ?? true));
                contextCell.SetCellValue(isContextSwitch ? context : string.Empty);
                contextCell.CellStyle.FillBackgroundColor = HSSFColor.Aqua.Index;

                /*var validation = valueCell.GetDataValidation() ?? valueCell.CreateDataValidation();
                validation.AllowedValues = TypeFromAllowedValues(propertyType);

                if (validation.AllowedValues == XLAllowedValues.WholeNumber)
                    validation.WholeNumber.Between(int.MinValue, int.MaxValue);

                if (validation.AllowedValues == XLAllowedValues.Decimal)
                    validation.Decimal.Between(Double.MinValue, Double.MaxValue);*/
            }
        }

        private static string ToYesNoString(bool b)
        {
            return b ? "yes" : "no";
        }

        private static bool IsSupportTypes(Type type)
        {
            return type == typeof(int) || type == typeof(float) || type == typeof(string);
        }

        // private static XLAllowedValues TypeFromAllowedValues(Type type)
        // {
        //     if (type == typeof(int))
        //         return XLAllowedValues.WholeNumber;
        //     if (type == typeof(float))
        //         return XLAllowedValues.Decimal;
        //
        //     return XLAllowedValues.AnyValue;
        // }

        private class GenerateInfo
        {
            public MasterAssetAttribute Attribute { get; set; }

            public Dictionary<string, Type> Types { get; set; } = new Dictionary<string, Type>();
        }
    }
}