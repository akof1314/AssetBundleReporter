using System.Collections.Generic;
using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using UnityEngine;

public static class AssetBundleAbFileDetailsReporter
{
    public static void CreateWorksheetAbDetails(ExcelWorksheet ws)
    {
        // 标签颜色
        ws.TabColor = ColorTranslator.FromHtml("#f9c40f");

        // 全体颜色
        ws.Cells.Style.Font.Color.SetColor(ColorTranslator.FromHtml("#3d4d65"));
    }

    public static void FillWorksheetAbDetails(ExcelWorksheet ws)
    {
        
    }
}