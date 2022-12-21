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
    }
}