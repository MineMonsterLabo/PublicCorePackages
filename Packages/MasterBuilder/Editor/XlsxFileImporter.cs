using System;
using System.Collections.Generic;
using System.IO;
using MasterBuilder.Editor.Extensions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace MasterBuilder.Editor
{
    [ScriptedImporter(1, "xlsx")]
    public class XlsxFileImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            using var steam = new FileStream(ctx.assetPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var workbook = new XSSFWorkbook(steam);

            var masterList = ScriptableObject.CreateInstance<MasterList>();
            ctx.AddObjectToAsset("Masters", masterList);
            ctx.SetMainObject(masterList);

            foreach (var workSheet in workbook)
            {
                var objects = CreateScriptableObject(workSheet);
                if (objects == null)
                    continue;

                foreach (var so in objects)
                {
                    ctx.AddObjectToAsset(so.name, so);
                    masterList.masters.Add(so);
                }
            }
        }

        private ScriptableObject[] CreateScriptableObject(ISheet workSheet)
        {
            var soList = new List<ScriptableObject>();
            var row = 2;
            var col = 2;
            var sheetName = workSheet.SheetName;
            var classNameCell = workSheet.GetCell(row, col + 1);
            var contextsCell = workSheet.GetCell(++row, col + 1);

            var type = Type.GetType(classNameCell.StringCellValue);
            if (type == null)
                return null;

            var collectionType = MasterRegistry.GetMasterCollectionType(type);
            if (collectionType == null)
                return null;

            var contexts = contextsCell.StringCellValue.Split(",");
            if (contexts.Length <= 0)
                return null;

            col = 3;
            row += 2;
            var infoStartRow = row;

            var columnInfos = new List<ColumnInfo>();
            while (true)
            {
                var columnNameCell = workSheet.GetCell(++row, col);
                var typeCell = workSheet.GetCell(++row, col);
                var requireCell = workSheet.GetCell(++row, col);
                var contextCell = workSheet.GetCell(++row, col);
                if (string.IsNullOrWhiteSpace(classNameCell.StringCellValue) ||
                    string.IsNullOrWhiteSpace(typeCell.StringCellValue) ||
                    string.IsNullOrWhiteSpace(requireCell.StringCellValue))
                    break;

                columnInfos.Add(new ColumnInfo
                {
                    Name = columnNameCell.StringCellValue,
                    Type = typeCell.StringCellValue,
                    IsRequire = requireCell.StringCellValue == "yes",
                    Context = contextCell.StringCellValue
                });

                ++col;
                row = infoStartRow;
            }

            infoStartRow += 5;
            var addMethod = collectionType.GetMethod("Add");
            if (addMethod == null)
                return null;

            foreach (var context in contexts)
            {
                col = 2;
                row = infoStartRow;
                var collectionInstance = ScriptableObject.CreateInstance(collectionType);
                if (!collectionInstance)
                    return null;

                var isEnd = false;
                while (true)
                {
                    var typeInstance = Activator.CreateInstance(type);
                    foreach (var columnInfo in columnInfos)
                    {
                        if (columnInfo.Name.StartsWith("D__"))
                        {
                            ++col;
                            continue;
                        }

                        var valueCell = workSheet.GetCell(row, ++col);
                        if (string.IsNullOrWhiteSpace(columnInfo.Context) || columnInfo.Context == context)
                        {
                            if (columnInfo.IsRequire && string.IsNullOrWhiteSpace(valueCell.StringCellValue))
                            {
                                isEnd = true;
                                break;
                            }

                            var property = type.GetProperty(columnInfo.Name);
                            if (property == null)
                            {
                                isEnd = true;
                                break;
                            }

                            if (!property.CanWrite)
                                property = property.DeclaringType?.GetProperty(columnInfo.Name);

                            var value = valueCell.StringCellValue ?? (property?.GetValue(typeInstance) ??
                                                                      GetDefaultValue(property?.PropertyType));
                            property?.SetValue(typeInstance, Convert.ChangeType(value, property.PropertyType));
                        }
                    }

                    if (isEnd)
                        break;

                    addMethod.Invoke(collectionInstance, new[] { typeInstance });

                    col = 2;
                    ++row;
                }

                collectionInstance.name = $"{sheetName}_{context}";
                soList.Add(collectionInstance);
            }

            return soList.ToArray();
        }

        private static object GetDefaultValue(Type type)
        {
            if (type == typeof(int))
                return default(int);
            if (type == typeof(float))
                return default(float);
            if (type == typeof(string))
                return string.Empty;
            if (type.IsSubclassOf(typeof(Enum)))
                return default(Enum);

            return default;
        }

        private class ColumnInfo
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public bool IsRequire { get; set; }
            public string Context { get; set; }
        }
    }
}