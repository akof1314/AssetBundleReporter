using System.IO;
using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Style;

/// <summary>
/// AssetBundle报告
/// 生成各个资源的报告Excel
/// </summary>
public class AssetBundleReporter
{
    public const string kSheetNameAbAssets = "资源使用情况";
    public const string kSheetNameAbDetail = "每个所包含的具体资源";

    public static void Print(string bundlePath, string outputPath)
    {
        AssetBundleFilesAnalyze.Analyze(bundlePath);
        var newFile = new FileInfo(outputPath);
        if (newFile.Exists)
        {
            newFile.Delete();
        }

        using (var package = new ExcelPackage(newFile))
        {
            AssetBundleAbFilesReporter.CreateWorksheetAbAssets(package.Workbook.Worksheets.Add(kSheetNameAbAssets));
            CreateWorksheetAbDetail(package.Workbook.Worksheets.Add(kSheetNameAbDetail));

            AssetBundleAbFilesReporter.FillWorksheetAbAssets(package.Workbook.Worksheets[1]);
            package.Save();
        }

        AssetBundleFilesAnalyze.Clear();
    }

    private static void CreateWorksheetAbDetail(ExcelWorksheet ws)
    {
        // 测试数据
        ws.Cells[3, 1].Value = "SubTerrainObjs_1_1.assetbundle";
        ws.Cells[300, 1].Value = "Terrain_Data_1_8.assetbundle";
        ws.Cells[3000, 3].Value = "Terrain_Data_3_3.assetbundle";
    }
}