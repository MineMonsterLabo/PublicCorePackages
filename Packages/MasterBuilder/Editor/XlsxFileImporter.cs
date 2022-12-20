using System;
using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;
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
            using var workbook = new XLWorkbook(steam);

            var masterList = ScriptableObject.CreateInstance<MasterList>();
            ctx.AddObjectToAsset("Master", masterList);
            ctx.SetMainObject(masterList);

            foreach (var workSheet in workbook.Worksheets)
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

        private ScriptableObject[] CreateScriptableObject(IXLWorksheet workSheet)
        {
            var soList = new List<ScriptableObject>();
            var row = 2;
            var col = 2;
            var sheetName = workSheet.Name;
            var classNameCell = workSheet.Cell(row, col + 1);
            var contextsCell = workSheet.Cell(++row, col + 1);

            var type = Type.GetType(classNameCell.CachedValue.ToString());
            if (type == null)
                return null;

            var collectionType = MasterRegistry.GetMasterCollectionType(type);
            if (collectionType == null)
                return null;

            var contexts = contextsCell.CachedValue.ToString().Split(",");
            if (contexts.Length <= 0)
                return null;

            col = 3;
            row += 2;
            var infoStartRow = row;

            var columnInfos = new List<ColumnInfo>();
            while (true)
            {
                var columnNameCell = workSheet.Cell(++row, col);
                var typeCell = workSheet.Cell(++row, col);
                var requireCell = workSheet.Cell(++row, col);
                var contextCell = workSheet.Cell(++row, col);
                if (classNameCell.IsEmpty() || typeCell.IsEmpty() || requireCell.IsEmpty())
                    break;

                columnInfos.Add(new ColumnInfo
                {
                    Name = columnNameCell.CachedValue.ToString(),
                    Type = typeCell.CachedValue.ToString(),
                    IsRequire = requireCell.CachedValue.ToString() == "yes",
                    Context = contextCell.CachedValue.ToString()
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
                        var valueCell = workSheet.Cell(row, ++col);
                        if (string.IsNullOrWhiteSpace(columnInfo.Context) || columnInfo.Context == context)
                        {
                            if (columnInfo.IsRequire && valueCell.IsEmpty())
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

                            var value = valueCell.CachedValue ?? (property?.GetValue(typeInstance) ??
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