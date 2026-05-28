using ASP_MVC.Entities;
using ASP_MVC.Repository;
using ASP_MVC.Reports;
using ASP_MVC.ViewModels;

namespace ASP_MVC.Services;

/// <summary>
/// 帳票定義を起点にUI、XML、txt出力、逆反映を束ねるPoC用Engine。
/// </summary>
public class ReportEngine
{
    private const string ReportType = "UserFavorite";

    private readonly UserFavoriteReportDefinition _definition;
    private readonly CompanyRepository _companyRepository;
    private readonly ReportInstanceRepository _reportInstanceRepository;
    private readonly ReportRendererService _rendererService;
    private readonly ReportXmlService _xmlService;
    private readonly TemplateRenderService _templateRenderService;
    private readonly UserService _userService;

    public ReportEngine(
        UserFavoriteReportDefinition definition,
        CompanyRepository companyRepository,
        ReportInstanceRepository reportInstanceRepository,
        ReportRendererService rendererService,
        ReportXmlService xmlService,
        TemplateRenderService templateRenderService,
        UserService userService)
    {
        _definition = definition;
        _companyRepository = companyRepository;
        _reportInstanceRepository = reportInstanceRepository;
        _rendererService = rendererService;
        _xmlService = xmlService;
        _templateRenderService = templateRenderService;
        _userService = userService;
    }

    /// <summary>
    /// 保存済みEntityから帳票画面全体を組み立てる。
    /// </summary>
    public UserFavoriteReportViewModel BuildUserFavoriteReport(int userId, string? message = null)
    {
        var companies = _companyRepository.GetAll();
        var user = _userService.GetUserGraph(userId) ?? CreateEmptyUser(userId, companies.FirstOrDefault());

        return new UserFavoriteReportViewModel
        {
            UserId = user.Id,
            Fields = _rendererService.BuildFields(_definition, user, companies),
            PreviewText = _templateRenderService.Render(_definition, user),
            XmlData = _xmlService.Serialize(user),
            Message = message
        };
    }

    /// <summary>
    /// 帳票入力をEntityへ逆反映し、XMLスナップショットを保存する。
    /// </summary>
    public UserFavoriteReportViewModel SaveUserFavoriteReport(UserFavoriteReportPostViewModel input)
    {
        var user = _userService.SaveUserFavorite(input);
        var xmlData = _xmlService.Serialize(user);

        _reportInstanceRepository.Upsert(new ReportInstance
        {
            UserId = user.Id,
            ReportType = ReportType,
            XmlData = xmlData,
            CreatedAt = DateTime.UtcNow
        });

        return BuildUserFavoriteReport(user.Id, "保存しました");
    }

    /// <summary>
    /// 保存前の入力値から右側プレビューだけを生成する。
    /// </summary>
    public string RenderPreview(UserFavoriteReportPostViewModel input)
    {
        var user = _userService.BuildTransientUser(input);
        return _templateRenderService.Render(_definition, user);
    }

    /// <summary>
    /// txtテンプレートの差し込み結果をDocumentDownloadへ出力する。
    /// </summary>
    public string ExportUserFavoriteText(int userId)
    {
        var user = _userService.GetUserGraph(userId) ?? throw new InvalidOperationException("出力対象のユーザーが見つかりません。");
        var fileName = $"{Guid.NewGuid():N}-user-{user.Id}.txt";
        return _templateRenderService.Write(_definition, user, fileName);
    }

    /// <summary>
    /// 帳票入力中に開く企業マスターダイアログを組み立てる。
    /// </summary>
    public CompanyDialogViewModel BuildCompanyDialog(int userId, int companyId)
    {
        var company = _companyRepository.Get(companyId) ?? new Company();

        return new CompanyDialogViewModel
        {
            UserId = userId,
            CompanyId = company.Id,
            CompanyName = company.CompanyName
        };
    }

    /// <summary>
    /// ダイアログ入力を企業マスターへ反映する。
    /// </summary>
    public void SaveCompanyDialog(CompanyDialogViewModel input)
    {
        var company = new Company
        {
            Id = input.CompanyId,
            CompanyName = (input.CompanyName ?? "").Trim()
        };

        if (company.Id == 0)
        {
            _companyRepository.Add(company);
            return;
        }

        _companyRepository.Update(company);
    }

    private static User CreateEmptyUser(int userId, Company? company)
    {
        return new User
        {
            Id = userId,
            CompanyId = company?.Id ?? 0,
            Company = company,
            Favorites = []
        };
    }
}