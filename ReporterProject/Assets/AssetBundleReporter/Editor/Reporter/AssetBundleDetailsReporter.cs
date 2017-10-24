using System.Collections.Generic;
using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace WuHuan
{
    public static class AssetBundleDetailsReporter
    {
        public static void CreateWorksheet(ExcelWorksheets wss)
        {
            ExcelWorksheet ws = wss.Add("每个所包含的具体资源");

            // 标签颜色
            ws.TabColor = ColorTranslator.FromHtml("#f9c40f");
            AssetBundleReporter.CreateWorksheetBase(ws, "每个 AssetBundle 文件所包含的具体资源", 4);

            // 列宽
            ws.Column(1).Width = 50;
            ws.Column(2).Width = 50;
            ws.Column(3).Width = 50;
            ws.Column(4).Width = 50;
        }

        public static void FillWorksheet(ExcelWorksheet ws)
        {
            int startRow = 2;

            List<AssetBundleFileInfo> infos = AssetBundleFilesAnalyze.GetAllAssetBundleFileInfos();
            foreach (var info in infos)
            {
                ws.Cells[startRow, 1].Value = info.name + " 所包含的具体资源";
                using (var range = ws.Cells[startRow, 1, startRow, 4])
                {
                    range.Merge = true;
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#BDD7EE"));
                }
                info.detailHyperLink.ReferenceAddress = ws.Cells[startRow, 1].FullAddress;

                FillAssetByType(info, AssetFileInfoType.mesh, ws, ref startRow);
                FillAssetByType(info, AssetFileInfoType.material, ws, ref startRow);
                FillAssetByType(info, AssetFileInfoType.texture2D, ws, ref startRow);
                FillAssetByType(info, AssetFileInfoType.sprite, ws, ref startRow);
                FillAssetByType(info, AssetFileInfoType.shader, ws, ref startRow);
                FillAssetByType(info, AssetFileInfoType.animatorController, ws, ref startRow);
                FillAssetByType(info, AssetFileInfoType.animatorOverrideController, ws, ref startRow);
                FillAssetByType(info, AssetFileInfoType.animationClip, ws, ref startRow);
                FillAssetByType(info, AssetFileInfoType.audioClip, ws, ref startRow);
                FillAssetByType(info, AssetFileInfoType.monoScript, ws, ref startRow);

                FillAssetDepends(info, ws, ref startRow);

                startRow += 6;
            }
        }

        private static void FillAssetByType(AssetBundleFileInfo info, string type, ExcelWorksheet ws, ref int startRow)
        {
            int count = info.GetAssetCount(type);
            if (count == 0)
            {
                return;
            }

            startRow++;
            ws.Cells[startRow, 1].Value = type + " (" + count + ")";
            SetRangeStyle(ws.Cells[startRow, 1, startRow, 4]);
            Color redColor = ColorTranslator.FromHtml("#FF0049");

            int startCol = 1;
            foreach (var fileInfo in info.assets)
            {
                if (fileInfo.type == type)
                {
                    if (startCol == 1)
                    {
                        startRow++;
                    }

                    ws.Cells[startRow, startCol].Value = fileInfo.name;
                    ws.Cells[startRow, startCol].Hyperlink = fileInfo.detailHyperLink;

                    // 冗余则红色显示
                    if (fileInfo.includedBundles.Count > 1 && fileInfo.type != AssetFileInfoType.monoScript)
                    {
                        ws.Cells[startRow, startCol].Style.Font.Color.SetColor(redColor);
                    }

                    startCol++;
                    if (startCol > 4)
                    {
                        startCol = 1;
                    }
                }
            }
        }

        private static void FillAssetDepends(AssetBundleFileInfo info, ExcelWorksheet ws, ref int startRow)
        {
            if (info.allDepends == null && info.beDepends == null)
            {
                return;
            }
            if (info.allDepends != null && info.beDepends != null &&
                info.allDepends.Length == 0 && info.beDepends.Length == 0)
            {
                return;
            }

            int rowAdd = 0;
            int titleRow = ++startRow;
            if (info.allDepends != null && info.allDepends.Length != 0)
            {
                ws.Cells[titleRow, 3].Value = "它依赖哪些AssetBundle文件？ (" + info.allDepends.Length + ")";
                SetRangeStyle(ws.Cells[titleRow, 3, titleRow, 4]);

                int dependRow = titleRow;
                foreach (var depend in info.allDepends)
                {
                    if (string.IsNullOrEmpty(depend))
                    {
                        continue;
                    }
                    dependRow++;
                    var dependInfo = AssetBundleFilesAnalyze.GetAssetBundleFileInfo(depend);
                    ws.Cells[dependRow, 3].Value = dependInfo.name;
                    ws.Cells[dependRow, 3].Hyperlink = dependInfo.detailHyperLink;
                    ws.Cells[dependRow, 3, dependRow, 4].Merge = true;
                }

                rowAdd = dependRow - titleRow;
            }

            if (info.beDepends != null && info.beDepends.Length != 0)
            {
                ws.Cells[titleRow, 1].Value = "哪些AssetBundle文件依赖它？ (" + info.beDepends.Length + ")";
                SetRangeStyle(ws.Cells[titleRow, 1, titleRow, 2]);

                int dependRow = titleRow;
                foreach (var depend in info.beDepends)
                {
                    dependRow++;
                    var dependInfo = AssetBundleFilesAnalyze.GetAssetBundleFileInfo(depend);
                    ws.Cells[dependRow, 1].Value = dependInfo.name;
                    ws.Cells[dependRow, 1].Hyperlink = dependInfo.detailHyperLink;
                    ws.Cells[dependRow, 1, dependRow, 2].Merge = true;
                }

                int rowAdd2 = dependRow - titleRow;
                if (rowAdd2 > rowAdd)
                {
                    rowAdd = rowAdd2;
                }
            }

            startRow += rowAdd;
        }

        private static void SetRangeStyle(ExcelRange range)
        {
            range.Merge = true;
            range.Style.Font.Bold = true;
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#DDEBF7"));
        }
    }
}