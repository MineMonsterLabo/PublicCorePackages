using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MasterBuilder.Attributes;
using MasterBuilder.BuildIn;
using MasterBuilder.Editor.Extensions;
using MasterBuilder.Editor.Settings;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
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
            var setting = MasterBuilderSettings.GetOrCreateSettings();
            if (setting.generateIgnoreMasters.Contains(info.Attribute.AssetPath))
            {
                Debug.LogWarning("skipped_creation_as_there_are_no_sheets_generated");
                return;
            }

            var filePath = $"{info.Attribute.AssetPath}.xlsx";
            var isFileExists = File.Exists(filePath);
            var workbook = isFileExists
                ? WorkbookFactory.Create(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                : new XSSFWorkbook();
            var changed = false;
            foreach (var pair in info.Types)
            {
                changed |= GenerateXlsxSheet(workbook, pair.Key, pair.Value);
            }

            if (workbook.NumberOfSheets == 0)
            {
                Debug.LogWarning("skipped_creation_as_there_are_no_sheets_generated".InternalLocalizeString());
                return;
            }

            var folderPath = Path.GetDirectoryName(filePath);
            if (folderPath != null)
            {
                Directory.CreateDirectory(folderPath);
            }

            if (!changed)
            {
                Debug.LogWarning("skipped_creation_as_there_are_no_changes_in_the_sheet".InternalLocalizeString());
                return;
            }

            using var steam = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite);
            workbook.Write(steam, false);

            AssetDatabase.Refresh();
        }

        private static bool GenerateXlsxSheet(IWorkbook workbook, string name, Type type)
        {
            var workSheet = workbook.GetSheet(name) ?? workbook.CreateSheet(name);
            var patriarch = workSheet.DrawingPatriarch ?? workSheet.CreateDrawingPatriarch();
            var masterAttribute = type.GetCustomAttribute<MasterAttribute>() ?? new MasterAttribute
            {
                Name = name
            };
            var properties =
                type.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public |
                                   BindingFlags.NonPublic)
                    .OrderBy(e => e.GetCustomAttribute<MasterColumnAttribute>()?.Order ?? -1);

            var row = 1;
            var col = 2;
            var versionLabelCell = workSheet.GetCell(row, col);
            var versionCell = workSheet.GetCell(row, col + 1);
            var classNameLabelCell = workSheet.GetCell(++row, col);
            var classNameCell = workSheet.GetCell(row, col + 1);
            var contextsLabelCell = workSheet.GetCell(++row, col);
            var contextsCell = workSheet.GetCell(row, col + 1);
            versionLabelCell.SetCellValue("version:".InternalLocalizeString());

            var versionString = versionCell.GetStringValue();
            if (!string.IsNullOrWhiteSpace(versionString) && int.TryParse(versionString, out var version))
            {
                if (version != -1 && version >= masterAttribute.Version)
                    return false;
            }

            versionCell.SetCellType(CellType.String);
            versionCell.SetCellValue(masterAttribute.Version);
            classNameLabelCell.SetCellValue("name_of_the_class:".InternalLocalizeString());
            classNameCell.SetCellValue(type.AssemblyQualifiedName);
            contextsLabelCell.SetCellValue("context_list:".InternalLocalizeString());
            contextsCell.SetCellValue(string.Join(",", masterAttribute.Contexts));

            workSheet.SetColumnWidth(col, 4000);

            col = 2;
            row += 2;
            var columnNameColor = IndexedColors.LightYellow;
            var contextNameColor = IndexedColors.LightTurquoise;
            var infoStartRow = row;
            var columnNameLabelCell = workSheet.GetCell(++row, col);
            var typeLabelCell = workSheet.GetCell(++row, col);
            var requireLabelCell = workSheet.GetCell(++row, col);
            var contextLabelCell = workSheet.GetCell(++row, col);

            columnNameLabelCell.SetCellValue("column_name:".InternalLocalizeString());
            SetCellColor(columnNameLabelCell, columnNameColor);

            typeLabelCell.SetCellValue("type:".InternalLocalizeString());
            requireLabelCell.SetCellValue("required:".InternalLocalizeString());

            contextLabelCell.SetCellValue("context:".InternalLocalizeString());
            SetCellColor(contextLabelCell, contextNameColor);

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

            return true;
        }

        private static void GenerateXlsxSheetColumn(IWorkbook workbook, ISheet workSheet, string[] contexts,
            PropertyInfo propertyInfo, ref int row, ref int col)
        {
            var patriarch = workSheet.DrawingPatriarch;
            var columnNameColor = IndexedColors.LightYellow;
            var contextNameColor = IndexedColors.LightTurquoise;
            var infoStartRow = row;
            var propertyType = propertyInfo.PropertyType;
            var masterColumnAttribute = propertyInfo.GetCustomAttribute<MasterColumnAttribute>();
            var masterReferenceAttribute = propertyInfo.GetCustomAttribute<MasterReferenceAttribute>();

            var isContextSwitch = masterColumnAttribute?.IsContextSwitch ?? false;
            var isMultiColumn = masterReferenceAttribute != null;
            var enableContexts = isMultiColumn ? new[] { "any-context", "shadow-column" } : new[] { "any-context" };
            if (isContextSwitch)
            {
                enableContexts = contexts.Except(masterColumnAttribute.DisableContexts ?? Array.Empty<string>())
                    .ToArray();
            }

            var columnName = masterColumnAttribute?.Name ?? propertyInfo.Name;
            foreach (var context in enableContexts)
            {
                row = infoStartRow;

                var columnNameCell = workSheet.GetCell(++row, col);
                var anchor = new XSSFClientAnchor(0, 0, 0, 0, 4, 2, 6, 8);
                var cellNameComment = columnNameCell.CellComment ?? patriarch.CreateCellComment(anchor);
                var typeCell = workSheet.GetCell(++row, col);
                var requireCell = workSheet.GetCell(++row, col);
                var contextCell = workSheet.GetCell(++row, col);
                var valueCell = workSheet.GetCell(++row, col);

                workSheet.SetColumnWidth(col, 6000);

                ++col;
                if (context == "shadow-column")
                {
                    columnNameCell.SetCellValue($"D__{columnName}");
                    cellNameComment.String = new XSSFRichTextString(masterColumnAttribute?.Description ??
                                                                    "no_description".InternalLocalizeString());
                    columnNameCell.CellComment = cellNameComment;
                    SetCellColor(columnNameCell, columnNameColor);

                    typeCell.SetCellValue("Reference");

                    requireCell.SetCellValue("no");
                    // contextCell.Value = isContextSwitch ? context : string.Empty;
                    SetCellColor(contextCell, contextNameColor);

                    var masterName = MasterRegistry.GetTypeFromMasterName(masterReferenceAttribute?.ReferenceType);
                    var baseReference = new CellAddress("D11");
                    var displayAddress =
                        CellReference.ConvertNumToColString(baseReference.Column +
                            masterReferenceAttribute?.DisplayColumnIndex ?? 1);
                    var validationHelper = workSheet.GetDataValidationHelper();
                    var constraint =
                        validationHelper.CreateFormulaListConstraint(
                            $"OFFSET({masterName}!${displayAddress}$11,0,0,COUNTA({masterName}!{displayAddress}:{displayAddress}))");
                    var validationData =
                        validationHelper.CreateValidation(constraint,
                            new CellRangeAddressList(row, row, col - 1, col - 1));
                    validationData.ShowErrorBox = true;
                    workSheet.GetDataValidations().Clear();
                    workSheet.AddValidationData(validationData);
                    // var listValidation = valueCell.GetDataValidation() ?? valueCell.CreateDataValidation();
                    // listValidation.AllowedValues = XLAllowedValues.List;
                    // listValidation.List($"=OFFSET({masterName}!$D$10, 0, 0, COUNTA(D:D), 0)");
                    continue;
                }

                columnNameCell.SetCellValue(columnName);
                cellNameComment.String =
                    new XSSFRichTextString(masterColumnAttribute?.Description ??
                                           "no_description".InternalLocalizeString());
                columnNameCell.CellComment = cellNameComment;
                SetCellColor(columnNameCell, columnNameColor);

                typeCell.SetCellValue(propertyType.Name);
                requireCell.SetCellValue(ToYesNoString(!masterColumnAttribute?.IsAllowEmpty ?? true));

                contextCell.SetCellValue(isContextSwitch ? context : string.Empty);
                SetCellColor(contextCell, contextNameColor);

                if (isMultiColumn)
                {
                    var masterName = MasterRegistry.GetTypeFromMasterName(masterReferenceAttribute?.ReferenceType);
                    var baseReference = new CellAddress("D11");
                    var keyAddress =
                        CellReference.ConvertNumToColString(baseReference.Column +
                                                            masterReferenceAttribute.ReferenceKeyColumnIndex);
                    var displayAddress =
                        CellReference.ConvertNumToColString(baseReference.Column +
                                                            masterReferenceAttribute.DisplayColumnIndex);
                    var colAddress = CellReference.ConvertNumToColString(valueCell.Address.Column + 1);
                    valueCell.SetCellType(CellType.Formula);
                    valueCell.SetCellFormula(
                        $"INDEX(OFFSET({masterName}!${keyAddress}$11,0,0,COUNTA({masterName}!{keyAddress}:{keyAddress}),2),MATCH(${colAddress}11,OFFSET({masterName}!${displayAddress}$11,0,0,COUNTA({masterName}!{displayAddress}:{displayAddress}),1),0),1)");
                }

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

        private static void SetCellColor(ICell cell, IndexedColors colors)
        {
            CellUtil.SetCellStyleProperty(cell, CellUtil.FILL_FOREGROUND_COLOR, colors.Index);
            CellUtil.SetCellStyleProperty(cell, CellUtil.FILL_PATTERN, FillPattern.SolidForeground);
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