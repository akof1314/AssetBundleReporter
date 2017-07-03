using System;
using System.Collections.Generic;
using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace WuHuan
{
    public static class AssetBundleFilesReporter
    {
        public static void CreateWorksheet(ExcelWorksheets wss)
        {
            ExcelWorksheet ws = wss.Add("AB文件列表");

            // 标签颜色
            ws.TabColor = ColorTranslator.FromHtml("#32b1fa");
            AssetBundleReporter.CreateWorksheetBase(ws, "AssetBundle 文件列表", 6);

            // 列头
            ws.Cells[2, 1].Value = "AssetBundle 名称";
            ws.Cells[2, 2].Value = AssetFileInfoType.mesh;
            ws.Cells[2, 3].Value = AssetFileInfoType.material;
            ws.Cells[2, 4].Value = AssetFileInfoType.texture2D;
            ws.Cells[2, 5].Value = AssetFileInfoType.sprite;
            ws.Cells[2, 6].Value = AssetFileInfoType.shader;

            using (var range = ws.Cells[2, 1, 2, 6])
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
            ws.Column(1).Width = 100;
            ws.Column(2).Width = 15;
            ws.Column(3).Width = 15;
            ws.Column(4).Width = 15;
            ws.Column(5).Width = 15;
            ws.Column(6).Width = 15;

            // 冻结前两行
            ws.View.FreezePanes(3, 1);
        }

        public static void FillWorksheet(ExcelWorksheet ws)
        {
            int startRow = 3;

            List<AssetBundleFileInfo> infos = AssetBundleFilesAnalyze.GetAllAssetBundleFileInfos();
            foreach (var info in infos)
            {
                ws.Cells[startRow, 1].Value = info.name;
                info.detailHyperLink = new ExcelHyperLink(String.Empty, info.name);
                ws.Cells[startRow, 1].Hyperlink = info.detailHyperLink;

                int count = info.GetAssetCount(AssetFileInfoType.mesh);
                if (count > 0)
                {
                    ws.Cells[startRow, 2].Value = count;
                }

                count = info.GetAssetCount(AssetFileInfoType.material);
                if (count > 0)
                {
                    ws.Cells[startRow, 3].Value = count;
                }

                count = info.GetAssetCount(AssetFileInfoType.texture2D);
                if (count > 0)
                {
                    ws.Cells[startRow, 4].Value = count;
                }

                count = info.GetAssetCount(AssetFileInfoType.sprite);
                if (count > 0)
                {
                    ws.Cells[startRow, 5].Value = count;
                }

                count = info.GetAssetCount(AssetFileInfoType.shader);
                if (count > 0)
                {
                    ws.Cells[startRow, 6].Value = count;
                }

                startRow++;
            }

            ws.Cells[1, 1].Value = ws.Cells[1, 1].Value + " (" + infos.Count + ")";
        }
    }
}