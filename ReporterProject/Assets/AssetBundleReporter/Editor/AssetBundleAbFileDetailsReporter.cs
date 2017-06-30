using System.Collections.Generic;
using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Style;

public static class AssetBundleAbFileDetailsReporter
{
    public static void CreateWorksheetAbDetails(ExcelWorksheets wss)
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

    public static void FillWorksheetAbDetails(ExcelWorksheet ws)
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
            info.detailLink = ws.Cells[startRow, 1].FullAddress;

            FillAssetByType(info, AssetFileInfoType.mesh, ws, ref startRow);
            FillAssetByType(info, AssetFileInfoType.material, ws, ref startRow);
            FillAssetByType(info, AssetFileInfoType.texture2D, ws, ref startRow);
            FillAssetByType(info, AssetFileInfoType.shader, ws, ref startRow);

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
        using (var range = ws.Cells[startRow, 1, startRow, 4])
        {
            range.Merge = true;
            range.Style.Font.Bold = true;
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#DDEBF7"));
        }

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

                startCol++;
                if (startCol > 4)
                {
                    startCol = 1;
                }
            }
        }
    }
}