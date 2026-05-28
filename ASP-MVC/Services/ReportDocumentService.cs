using ASP_MVC.Entities;
using ASP_MVC.Repository;
using ASP_MVC.Reports;
using ASP_MVC.ViewModels;

namespace ASP_MVC.Services;

/// <summary>
/// 帳票定義を使った共通のUI生成、XML保存、txt出力を担当するService。
/// </summary>
public class ReportDocumentService
{
    private readonly CompanyRepository _companyRepository;
    private readonly ReportInstanceRepository _reportInstanceRepository;
    private readonly ReportRendererService _rendererService;
    private readonly ReportXmlService _xmlService;
    private readonly TemplateRenderService _templateRenderService;

    public ReportDocumentService(
        CompanyRepository companyRepository,
        ReportInstanceRepository reportInstanceRepository,
        ReportRendererService rendererService,
        ReportXmlService xmlService,
        TemplateRenderService templateRenderService)
    {
        _companyRepository = companyRepository;
        _reportInstanceRepository = reportInstanceRepository;
        _rendererService = rendererService;
        _xmlService = xmlService;
        _templateRenderService = templateRenderService;
    }

    /// <summary>
    /// 保存済みXMLまたは空データから共通の帳票画面を組み立てる。
    /// </summary>
    public ReportWorkspaceViewModel BuildWorkspace<TModel>(
        ReportDefinition<TModel> definition,
        TModel model,
        string reportKey,
        string reportTitle,
        string? message = null,
        IEnumerable<string>? selectedFieldIds = null,
        string? savedXmlData = null,
        string? previewTextOverride = null)
    {
        var companies = _companyRepository.GetAll();
        var selected = selectedFieldIds is null
            ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(selectedFieldIds.Where(value => !string.IsNullOrWhiteSpace(value)), StringComparer.OrdinalIgnoreCase);

        return new ReportWorkspaceViewModel
        {
            ReportKey = reportKey,
            ReportTitle = reportTitle,
            UserId = GetUserId(model),
            Fields = _rendererService.BuildFields(definition, model, companies, selected),
            PreviewText = previewTextOverride ?? _templateRenderService.Render(definition, model),
            XmlData = savedXmlData ?? "",
            Message = message
        };
    }

    /// <summary>
    /// XMLを保存してスナップショット文字列を返す。
    /// </summary>
    public string SaveSnapshot<TModel>(string reportType, int userId, TModel model)
    {
        var xmlData = _xmlService.Serialize(model);

        _reportInstanceRepository.Upsert(new ReportInstance
        {
            UserId = userId,
            ReportType = reportType,
            XmlData = xmlData,
            CreatedAt = DateTime.UtcNow
        });

        return xmlData;
    }

    /// <summary>
    /// 保存前の入力値からtxtプレビューを生成する。
    /// </summary>
    public string RenderPreview<TModel>(ReportDefinition<TModel> definition, TModel model)
    {
        return _templateRenderService.Render(definition, model);
    }

    /// <summary>
    /// 保存済みXML文字列を型付きEntityへ戻す。
    /// </summary>
    public TModel? Deserialize<TModel>(string xmlData)
    {
        return _xmlService.Deserialize<TModel>(xmlData);
    }

    /// <summary>
    /// 保存済みの最新XMLを取得する。
    /// </summary>
    public ReportInstance? GetLatestSnapshot(int userId, string reportType)
    {
        return _reportInstanceRepository.GetLatest(userId, reportType);
    }

    /// <summary>
    /// txtテンプレートの差し込み結果をDocumentDownloadへ出力する。
    /// </summary>
    public string Write<TModel>(ReportDefinition<TModel> definition, TModel model, string fileName)
    {
        return _templateRenderService.Write(definition, model, fileName);
    }

    private static int GetUserId<TModel>(TModel model)
    {
        if (model is User user)
        {
            return user.Id;
        }

        return 0;
    }
}