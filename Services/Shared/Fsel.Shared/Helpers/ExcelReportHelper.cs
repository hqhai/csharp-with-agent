// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    using OfficeOpenXml;

    public static class ExcelReportHelper
    {
        public static void ProcessCell(ExcelWorksheet worksheet, int row, int sourceCol, int targetCol, string name, string charter = "%")
        {
            if (worksheet == null)
            {
                return;
            }
            var currentCell = worksheet.Cells[row, sourceCol];
            if (currentCell.Value == null || string.IsNullOrEmpty(currentCell.Value.ToString()))
            {
                return;
            }

            worksheet.Cells[row, targetCol].Value = currentCell.Value.ToString().Contains(charter) ? currentCell.Value : $"{name}";
            worksheet.Cells[row, targetCol].StyleID = currentCell.StyleID;
        }

        // Hàm tính tên cột Excel dựa trên chỉ số
        public static string GetExcelColumnName(int columnNumber)
        {
            int alphabetCount = 26; // Số lượng chữ cái trong bảng chữ cái tiếng Anh (A-Z)
            int asciiValueOfA = 65; // Mã ASCII của chữ cái 'A'
            string columnName = string.Empty;
            while (columnNumber > 0)
            {
                int modulo = (columnNumber - 1) % alphabetCount; // Tính phần dư khi chia cho 26
                columnName = Convert.ToChar(asciiValueOfA + modulo) + columnName; // Chuyển phần dư thành ký tự và cộng vào tên cột
                columnNumber = (columnNumber - modulo) / alphabetCount; // Cập nhật lại columnIndex để xử lý phần còn lại
            }
            return columnName;
        }
    }
}
