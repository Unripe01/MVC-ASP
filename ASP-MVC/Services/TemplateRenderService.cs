using ASP_MVC.Reports;

namespace ASP_MVC.Services;

/// <summary>
/// txtテンプレートの単純文字列置換とDocumentDownload出力を担当するService。
/// </summary>
public class TemplateRenderService
{
    private readonly IWebHostEnvironment _environment;

    public TemplateRenderService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    /// <summary>
    /// ReportDefinitionのレンダーキーに従ってtxtプレビュー文字列を生成する。
    /// </summary>
    public string Render<TModel>(ReportDefinition<TModel> definition, TModel model)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(model);

        var output = LoadTemplate(definition.TemplateFileName);

        foreach (var field in definition.TemplateFields)
        {
            output = output.Replace(field.TemplatePlaceholder, field.ReadTemplateValue(model));
        }

        return output;
    }

    /// <summary>
    /// txtテンプレート差し込み結果をDocumentDownloadへ保存する。
    /// </summary>
    public string Write<TModel>(ReportDefinition<TModel> definition, TModel model, string fileName)
    {
        var output = Render(definition, model);
        var outputDirectory = Path.Combine(_environment.ContentRootPath, "DocumentDownload");
        Directory.CreateDirectory(outputDirectory);

        var outputPath = Path.Combine(outputDirectory, fileName);
        File.WriteAllText(outputPath, output);
        return fileName;
    }

    private string LoadTemplate(string templateFileName)
    {
        if (string.IsNullOrWhiteSpace(templateFileName))
        {
            throw new InvalidOperationException("帳票定義にtxtテンプレートファイル名が設定されていません。");
        }

        var templatePath = Path.Combine(_environment.ContentRootPath, "DocumentTemplates", templateFileName);
        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException("txtテンプレートが見つかりません。", templatePath);
        }

        return File.ReadAllText(templatePath);
    }
}