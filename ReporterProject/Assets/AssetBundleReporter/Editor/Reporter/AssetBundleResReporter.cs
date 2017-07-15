using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace WuHuan
{
    public static class AssetBundleResReporter
    {
        public static void CreateWorksheet(ExcelWorksheets wss)
        {
            ExcelWorksheet ws = wss.Add("资源列表");

            // 标签颜色
            ws.TabColor = ColorTranslator.FromHtml("#70ad47");
            AssetBundleReporter.CreateWorksheetBase(ws, "全部资源列表", 4);

            // 列头
            ws.Cells[2, 1].Value = "资源名称";
            ws.Cells[2, 2].Value = "资源类型";
            ws.Cells[2, 3].Value = "AB文件数量";
            ws.Cells[2, 4].Value = "相应的AB文件";

            using (var range = ws.Cells[2, 1, 2, 4])
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
            ws.Column(2).Width = 30;
            ws.Column(3).Width = 15;
            ws.Column(3).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            // 冻结前两行
            ws.View.FreezePanes(3, 1);
        }

        public static void FillWorksheet(ExcelWorksheet ws)
        {
            int startRow = 3;
            int maxCol = 4;

            var dicts = AssetBundleFilesAnalyze.GetAllAssetFileInfo();
            if (dicts == null)
            {
                ws.Cells[1, 1].Value = ws.Cells[1, 1].Value + " (0)";
            }
            else
            {
                foreach (var info in dicts.Values)
                {
                    ws.Cells[startRow, 1].Value = info.name;
                    ws.Cells[startRow, 2].Value = info.type;
                    ws.Cells[startRow, 3].Value = info.includedBundles.Count;

                    info.detailHyperLink.ReferenceAddress = ws.Cells[startRow, 1].FullAddress;

                    int startCol = 4;
                    foreach (var bundleFileInfo in info.includedBundles)
                    {
                        ws.Cells[startRow, startCol].Value = bundleFileInfo.name;
                        ws.Cells[startRow, startCol++].Hyperlink = bundleFileInfo.detailHyperLink;
                    }
                    maxCol = System.Math.Max(--startCol, maxCol);
                    startRow++;
                }

                ws.Cells[1, 1].Value = ws.Cells[1, 1].Value + " (" + dicts.Values.Count + ")";
            }

            // 具体所需要的列
            using (var range = ws.Cells[2, 4, 2, maxCol])
            {
                range.Merge = true;
            }
            for (int i = 4; i <= maxCol; i++)
            {
                ws.Column(i).Width = 100;
            }
        }
    }
}