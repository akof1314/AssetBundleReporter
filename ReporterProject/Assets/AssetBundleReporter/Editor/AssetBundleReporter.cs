using System;
using System.IO;
using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using UnityEditor;

/// <summary>
/// AssetBundle报告
/// 生成各个资源的报告Excel
/// </summary>
public class AssetBundleReporter
{
    [MenuItem("AssetBundleReporter/Print")]
    public static void AnalyzePrint()
    {
        string path = EditorUtility.OpenFolderPanel("AssetBundle 保存目录", "", "");
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        string bundlePath = path;
        string outputPath = Path.Combine(path, DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx");
        Print(bundlePath, outputPath);
    }

    public static void Print(string bundlePath, string outputPath)
    {
        EditorUtility.DisplayProgressBar("AssetBundle报告", "正在分析 AssetBundle 文件...", 0.35f);
        if (!AssetBundleFilesAnalyze.Analyze(bundlePath))
        {
            EditorUtility.ClearProgressBar();
            return;
        }

        EditorUtility.DisplayProgressBar("AssetBundle报告", "正在写入 Excel 文件...", 0.85f);
        var newFile = new FileInfo(outputPath);
        if (newFile.Exists)
        {
            newFile.Delete();
        }

        using (var package = new ExcelPackage(newFile))
        {
            AssetBundleAbFilesReporter.CreateWorksheetAbFiles(package.Workbook.Worksheets);
            AssetBundleAbFileDetailsReporter.CreateWorksheetAbDetails(package.Workbook.Worksheets);

            AssetBundleAbFilesReporter.FillWorksheetAbFiles(package.Workbook.Worksheets[1]);
            AssetBundleAbFileDetailsReporter.FillWorksheetAbDetails(package.Workbook.Worksheets[2]);

            package.Save();
        }

        AssetBundleFilesAnalyze.Clear();
        EditorUtility.ClearProgressBar();
    }

    public static void CreateWorksheetBase(ExcelWorksheet ws, string title, int colCount)
    {
        // 全体颜色
        ws.Cells.Style.Font.Color.SetColor(ColorTranslator.FromHtml("#3d4d65"));
        {
            // 边框样式
            var border = ws.Cells.Style.Border;
            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style
                = ExcelBorderStyle.Thin;

            // 边框颜色
            var clr = ColorTranslator.FromHtml("#B2C6C9");
            border.Bottom.Color.SetColor(clr);
            border.Top.Color.SetColor(clr);
            border.Left.Color.SetColor(clr);
            border.Right.Color.SetColor(clr);
        }

        // 标题
        ws.Cells[1, 1].Value = title;
        using (var range = ws.Cells[1, 1, 1, colCount])
        {
            range.Merge = true;
            range.Style.Font.Bold = true;
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        }
        ws.Row(1).Height = 30;
    }
}