using System.Collections.Generic;
using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using UnityEngine;

public static class AssetBundleAbFilesReporter
{
    public static void CreateWorksheetAbFiles(ExcelWorksheet ws)
    {
        // 标签颜色
        ws.TabColor = ColorTranslator.FromHtml("#32b1fa");

        // 全体颜色
        ws.Cells.Style.Font.Color.SetColor(ColorTranslator.FromHtml("#3d4d65"));
        {
            // 边框样式
            var border = ws.Cells.Style.Border;
            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style
                = ExcelBorderStyle.Thin;

            // 边框颜色
            var clr = ColorTranslator.FromHtml("#cad7e2");
            border.Bottom.Color.SetColor(clr);
            border.Top.Color.SetColor(clr);
            border.Left.Color.SetColor(clr);
            border.Right.Color.SetColor(clr);
        }

        // 标题
        ws.Cells[1, 1].Value = "AssetBundle 文件的资源使用情况";
        using (var range = ws.Cells[1, 1, 1, 5])
        {
            range.Merge = true;
            range.Style.Font.Bold = true;
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        }

        // 列头
        ws.Cells[2, 1].Value = "AssetBundle 名称";
        ws.Cells[2, 2].Value = "Mesh";
        ws.Cells[2, 3].Value = "Material";
        ws.Cells[2, 4].Value = "Texture2D";
        ws.Cells[2, 5].Value = "Shader";

        using (var range = ws.Cells[2, 1, 2, 5])
        {
            // 字体样式
            range.Style.Font.Bold = true;

            // 背景颜色
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#F2F5FA"));

            // 开启自动筛选
            range.AutoFilter = true;
        }

        // 列宽
        ws.Column(1).Width = 110;
        ws.Column(2).Width = 15;
        ws.Column(3).Width = 15;
        ws.Column(4).Width = 15;
        ws.Column(5).Width = 15;

        // 冻结前两行
        ws.View.FreezePanes(3, 1);
    }

    public static void FillWorksheetAbFiles(ExcelWorksheet ws)
    {
        // 测试数据
        //ws.Cells[3, 1].Value = "SubTerrainObjs_1_1.assetbundle";
        //ws.Cells[3, 1].Hyperlink = new ExcelHyperLink(AssetBundleReporter.kSheetNameAbDetail + "!A3", "SubTerrainObjs_1_1.assetbundle");
        //ws.Cells[3, 2].Value = 45;
        //ws.Cells[3, 3].Value = 150;

        //ws.Cells[4, 1].Value = "Terrain_Data_1_8.assetbundle";
        //ws.Cells[4, 1].Hyperlink = new ExcelHyperLink(AssetBundleReporter.kSheetNameAbDetail + "!A300", "Terrain_Data_1_8.assetbundle");
        //ws.Cells[4, 2].Value = 38;
        //ws.Cells[4, 3].Value = 130;

        //ws.Cells[5, 1].Value = "Terrain_Data_3_3.assetbundle";
        //ws.Cells[5, 2].Value = 22;
        //ws.Cells[5, 3].Value = 200;

        int startRow = 3;

        List<AssetBundleFileInfo> infos = AssetBundleFilesAnalyze.GetAllAssetBundleFileInfos();
        foreach (var info in infos)
        {
            ws.Cells[startRow, 1].Value = info.name;

            int count = info.GetAssetCount("Mesh");
            if (count > 0)
            {
                ws.Cells[startRow, 2].Value = count;
            }

            count = info.GetAssetCount("Material");
            if (count > 0)
            {
                ws.Cells[startRow, 3].Value = count;
            }

            count = info.GetAssetCount("Texture2D");
            if (count > 0)
            {
                ws.Cells[startRow, 4].Value = count;
            }

            count = info.GetAssetCount("Shader");
            if (count > 0)
            {
                ws.Cells[startRow, 5].Value = count;
            }

            startRow++;
        }
    }
}