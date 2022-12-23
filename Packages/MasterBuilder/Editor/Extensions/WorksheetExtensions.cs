using System.Globalization;
using NPOI.SS.UserModel;

namespace MasterBuilder.Editor.Extensions
{
    public static class WorksheetExtensions
    {
        public static ICell GetCell(this ISheet sheet, int row, int column)
        {
            var rowData = sheet.GetRow(row) ?? sheet.CreateRow(row);
            var cell = rowData.GetCell(column) ?? rowData.CreateCell(column);
            return cell;
        }

        public static string GetStringValue(this ICell cell)
        {
            var type = cell.CellType;
            switch (type)
            {
                case CellType.Boolean:
                    return cell.BooleanCellValue.ToString();

                case CellType.Numeric:
                    return cell.NumericCellValue.ToString(CultureInfo.InvariantCulture);

                case CellType.String:
                    return cell.StringCellValue;

                case CellType.Formula:
                    switch (cell.CachedFormulaResultType)
                    {
                        case CellType.Boolean:
                            return cell.BooleanCellValue.ToString();

                        case CellType.Numeric:
                            return cell.NumericCellValue.ToString(CultureInfo.InvariantCulture);

                        case CellType.String:
                            return cell.StringCellValue;

                        default:
                            return null;
                    }

                default:
                    return null;
            }
        }
    }
}