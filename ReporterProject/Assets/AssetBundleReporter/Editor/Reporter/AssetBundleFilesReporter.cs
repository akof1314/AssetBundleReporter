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
            AssetBundleReporter.CreateWorksheetBase(ws, "AssetBundle 文件列表", 11);

            // 列头
            int colIndex = 1;
            ws.Cells[2, colIndex++].Value = "AssetBundle 名称";
            ws.Cells[2, colIndex++].Value = "文件大小";
            ws.Cells[2, colIndex++].Value = "依赖AB数";
            ws.Cells[2, colIndex++].Value = "冗余资源数";
            ws.Cells[2, colIndex++].Value = AssetFileInfoType.mesh;
            ws.Cells[2, colIndex++].Value = AssetFileInfoType.material;
            ws.Cells[2, colIndex++].Value = AssetFileInfoType.texture2D;
            ws.Cells[2, colIndex++].Value = AssetFileInfoType.sprite;
            ws.Cells[2, colIndex++].Value = AssetFileInfoType.shader;
            ws.Cells[2, colIndex++].Value = AssetFileInfoType.animationClip;
            ws.Cells[2, colIndex].Value = AssetFileInfoType.audioClip;

            using (var range = ws.Cells[2, 1, 2, colIndex])
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
            for (int i = 2; i <= colIndex; i++)
            {
                ws.Column(i).Width = 15;
                ws.Column(i).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            }
            ws.Column(10).Width = 16;
            ws.Column(2).Style.Numberformat.Format = "#,##0";

            // 冻结前两行
            ws.View.FreezePanes(3, 1);
        }

        public static void FillWorksheet(ExcelWorksheet ws)
        {
            int startRow = 3;

            List<AssetBundleFileInfo> infos = AssetBundleFilesAnalyze.GetAllAssetBundleFileInfos();
            foreach (var info in infos)
            {
                int colIndex = 1;
                ws.Cells[startRow, colIndex].Value = info.name;
                info.detailHyperLink = new ExcelHyperLink(String.Empty, info.name);
                ws.Cells[startRow, colIndex++].Hyperlink = info.detailHyperLink;
                ws.Cells[startRow, colIndex++].Value = info.size;

                int count = info.allDepends.Length;
                if (count > 0)
                {
                    ws.Cells[startRow, colIndex].Value = count;
                }

                colIndex++;
                count = 0;
                foreach (var asset in info.assets)
                {
                    if (asset.includedBundles.Count > 1 && asset.type != AssetFileInfoType.monoScript)
                    {
                        count++;
                    }
                }
                if (count > 0)
                {
                    ws.Cells[startRow, colIndex].Value = count;
                }

                colIndex++;
                count = info.GetAssetCount(AssetFileInfoType.mesh);
                if (count > 0)
                {
                    ws.Cells[startRow, colIndex].Value = count;
                }

                colIndex++;
                count = info.GetAssetCount(AssetFileInfoType.material);
                if (count > 0)
                {
                    ws.Cells[startRow, colIndex].Value = count;
                }

                colIndex++;
                count = info.GetAssetCount(AssetFileInfoType.texture2D);
                if (count > 0)
                {
                    ws.Cells[startRow, colIndex].Value = count;
                }

                colIndex++;
                count = info.GetAssetCount(AssetFileInfoType.sprite);
                if (count > 0)
                {
                    ws.Cells[startRow, colIndex].Value = count;
                }

                colIndex++;
                count = info.GetAssetCount(AssetFileInfoType.shader);
                if (count > 0)
                {
                    ws.Cells[startRow, colIndex].Value = count;
                }

                colIndex++;
                count = info.GetAssetCount(AssetFileInfoType.animationClip);
                if (count > 0)
                {
                    ws.Cells[startRow, colIndex].Value = count;
                }

                colIndex++;
                count = info.GetAssetCount(AssetFileInfoType.audioClip);
                if (count > 0)
                {
                    ws.Cells[startRow, colIndex].Value = count;
                }

                startRow++;
            }

            ws.Cells[1, 1].Value = ws.Cells[1, 1].Value + " (" + infos.Count + ")";
        }
    }
}