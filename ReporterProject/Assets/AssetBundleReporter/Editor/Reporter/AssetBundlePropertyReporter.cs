using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace WuHuan
{
    public static class AssetBundlePropertyReporter
    {
        public static void CreateAndFillWorksheet(ExcelWorksheets wss, string typeName, string titleName, string[] columnNames)
        {
            ExcelWorksheet ws = wss.Add(titleName);

            int abCountCol = columnNames.Length + 2;
            int abDetailCol = columnNames.Length + 3;

            // 标签颜色
            ws.TabColor = ColorTranslator.FromHtml("#b490f5");
            AssetBundleReporter.CreateWorksheetBase(ws, titleName, abDetailCol);

            // 列头
            ws.Cells[2, 1].Value = "资源名称";
            for (int i = 0; i < columnNames.Length; i++)
            {
                ws.Cells[2, i + 2].Value = columnNames[i];
            }
            ws.Cells[2, abCountCol].Value = "AB文件数量";
            ws.Cells[2, abDetailCol].Value = "相应的AB文件";

            using (var range = ws.Cells[2, 1, 2, abDetailCol])
            {
                // 字体样式
                range.Style.Font.Bold = true;

                // 背景颜色
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#DDEBF7"));

                // 开启自动筛选
                range.AutoFilter = true;
            }

            // 列宽
            ws.Column(1).Width = 50;
            for (int i = 0; i < columnNames.Length; i++)
            {
                ws.Column(2 + i).Width = 15;
            }
            ws.Column(abCountCol).Width = 15;
            ws.Column(abCountCol).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            // 冻结前两行
            ws.View.FreezePanes(3, 1);

            int startRow = 3;
            int maxCol = abDetailCol;

            var dicts = AssetBundleFilesAnalyze.GetAllAssetFileInfo();
            foreach (var info in dicts.Values)
            {
                if (info.type != typeName)
                {
                    continue;
                }

                ws.Cells[startRow, 1].Value = info.name;

                int startCol = 2;
                foreach (var property in info.propertys)
                {
                    ws.Cells[startRow, startCol++].Value = property.Value;
                }
                ws.Cells[startRow, startCol++].Value = info.includedBundles.Count;

                foreach (var bundleFileInfo in info.includedBundles)
                {
                    ws.Cells[startRow, startCol].Value = bundleFileInfo.name;
                    ws.Cells[startRow, startCol++].Hyperlink = bundleFileInfo.detailHyperLink;
                }
                maxCol = System.Math.Max(--startCol, maxCol);
                startRow++;
            }

            ws.Cells[1, 1].Value = ws.Cells[1, 1].Value + " (" + (startRow - 3) + ")";

            // 具体所需要的列
            using (var range = ws.Cells[2, abDetailCol, 2, maxCol])
            {
                range.Merge = true;
            }
            for (int i = abDetailCol; i <= maxCol; i++)
            {
                ws.Column(i).Width = 100;
            }
        }
    }
}