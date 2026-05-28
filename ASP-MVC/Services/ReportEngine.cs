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
    private static readonly StringComparer FieldComparer = StringComparer.OrdinalIgnoreCase;

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
        var snapshot = _reportInstanceRepository.GetLatest(userId, ReportType);
        if (snapshot is null)
        {
            return BuildWorkspace(CreateEmptyUser(userId), message, savedXmlData: "", previewTextOverride: "");
        }

        var user = UserService.NormalizeReportUser(_xmlService.Deserialize<User>(snapshot.XmlData) ?? CreateEmptyUser(userId));
        return BuildWorkspace(user, message, savedXmlData: snapshot.XmlData);
    }

    /// <summary>
    /// 帳票入力をEntityへ逆反映し、XMLスナップショットを保存する。
    /// </summary>
    public UserFavoriteReportViewModel SaveUserFavoriteReport(UserFavoriteReportPostViewModel input)
    {
        var user = UserService.NormalizeReportUser(_userService.BuildReportUser(input));
        var savedXmlData = SaveSnapshot(user);
        return BuildWorkspace(user, "XMLを保存しました", input.SelectedFieldIds, savedXmlData);
    }

    /// <summary>
    /// 保存前の入力値から右側プレビューだけを生成する。
    /// </summary>
    public string RenderPreview(UserFavoriteReportPostViewModel input)
    {
        var user = UserService.NormalizeReportUser(_userService.BuildReportUser(input));
        return _templateRenderService.Render(_definition, user);
    }

    /// <summary>
    /// 選択された項目だけをマスターから取得して帳票XMLへ反映する。
    /// </summary>
    public UserFavoriteReportViewModel FetchMasterValues(UserFavoriteReportPostViewModel input)
    {
        var selectedFieldIds = ToSelectedFieldSet(input.SelectedFieldIds);
        var user = UserService.NormalizeReportUser(_userService.BuildReportUser(input));
        var currentSnapshot = _reportInstanceRepository.GetLatest(input.UserId, ReportType);

        if (selectedFieldIds.Count == 0)
        {
            return BuildWorkspace(user, "マスター取得対象を選択してください", input.SelectedFieldIds, currentSnapshot?.XmlData);
        }

        var masterUser = input.UserId == 0 ? null : _userService.GetUserGraph(input.UserId);

        if (selectedFieldIds.Contains("UserName") && masterUser is not null)
        {
            user.UserName = masterUser.UserName;
        }

        if (selectedFieldIds.Contains("Company_CompanyName"))
        {
            var company = ResolveCompanyForFetch(input.CompanyId, masterUser);
            user.CompanyId = company?.Id ?? 0;
            user.Company = company is null
                ? new Company()
                : new Company
                {
                    Id = company.Id,
                    CompanyName = company.CompanyName
                };
        }

        if (selectedFieldIds.Contains("Favorites") && masterUser is not null)
        {
            user.Favorites = masterUser.Favorites
                .Select(favorite => new Favorite
                {
                    Id = favorite.Id,
                    UserId = favorite.UserId,
                    FavoriteName = favorite.FavoriteName
                })
                .ToList();
        }

        return BuildWorkspace(user, "選択項目をマスターから取得しました", input.SelectedFieldIds, currentSnapshot?.XmlData);
    }

    /// <summary>
    /// 選択された項目だけをマスターへ逆反映し、反映結果をXMLへ戻す。
    /// </summary>
    public UserFavoriteReportViewModel ReverseReflect(UserFavoriteReportPostViewModel input)
    {
        var selectedFieldIds = ToSelectedFieldSet(input.SelectedFieldIds);
        var user = UserService.NormalizeReportUser(_userService.BuildReportUser(input));
        var currentSnapshot = _reportInstanceRepository.GetLatest(input.UserId, ReportType);

        if (selectedFieldIds.Count == 0)
        {
            return BuildWorkspace(user, "逆反映対象を選択してください", input.SelectedFieldIds, currentSnapshot?.XmlData);
        }

        var reflectedUser = UserService.NormalizeReportUser(_userService.ReverseReflect(user, selectedFieldIds));
        var mergedUser = MergeReflectedFields(user, reflectedUser, selectedFieldIds);

        return BuildWorkspace(mergedUser, "選択項目をマスターへ逆反映しました", input.SelectedFieldIds, currentSnapshot?.XmlData);
    }

    /// <summary>
    /// txtテンプレートの差し込み結果をDocumentDownloadへ出力する。
    /// </summary>
    public string ExportUserFavoriteText(int userId)
    {
        var snapshot = _reportInstanceRepository.GetLatest(userId, ReportType)
            ?? throw new InvalidOperationException("出力対象の保存済み帳票が見つかりません。");
        var user = UserService.NormalizeReportUser(_xmlService.Deserialize<User>(snapshot.XmlData) ?? throw new InvalidOperationException("保存済み帳票のXMLを復元できません。"));
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

    private UserFavoriteReportViewModel BuildWorkspace(
        User user,
        string? message = null,
        IEnumerable<string>? selectedFieldIds = null,
        string? savedXmlData = null,
        string? previewTextOverride = null)
    {
        var normalizedUser = UserService.NormalizeReportUser(user);
        var companies = _companyRepository.GetAll();
        var selected = ToSelectedFieldSet(selectedFieldIds);

        return new UserFavoriteReportViewModel
        {
            ReportKey = _definition.ReportKey,
            ReportTitle = _definition.DisplayTitle,
            SupportsMasterActions = _definition.SupportsMasterActions,
            UserId = normalizedUser.Id,
            Fields = _rendererService.BuildFields(_definition, normalizedUser, companies, selected),
            PreviewText = previewTextOverride ?? _templateRenderService.Render(_definition, normalizedUser),
            XmlData = savedXmlData ?? "",
            Message = message
        };
    }

    private string SaveSnapshot(User user)
    {
        var xmlData = _xmlService.Serialize(user);

        _reportInstanceRepository.Upsert(new ReportInstance
        {
            UserId = user.Id,
            ReportType = ReportType,
            XmlData = xmlData,
            CreatedAt = DateTime.UtcNow
        });

        return xmlData;
    }

    private static User MergeReflectedFields(User sourceUser, User reflectedUser, IReadOnlySet<string> selectedFieldIds)
    {
        var mergedUser = UserService.NormalizeReportUser(sourceUser);

        if (selectedFieldIds.Contains("UserName"))
        {
            mergedUser.UserName = reflectedUser.UserName;
        }

        if (selectedFieldIds.Contains("Company_CompanyName"))
        {
            mergedUser.CompanyId = reflectedUser.CompanyId;
            mergedUser.Company = reflectedUser.Company ?? new Company();
        }

        if (selectedFieldIds.Contains("Favorites"))
        {
            mergedUser.Favorites = reflectedUser.Favorites;
        }

        if (mergedUser.Id == 0)
        {
            mergedUser.Id = reflectedUser.Id;
        }

        return mergedUser;
    }

    private Company? ResolveCompanyForFetch(int companyId, User? masterUser)
    {
        if (companyId != 0)
        {
            return _companyRepository.Get(companyId);
        }

        if (masterUser?.Company is null)
        {
            return null;
        }

        return _companyRepository.Get(masterUser.CompanyId) ?? masterUser.Company;
    }

    private static HashSet<string> ToSelectedFieldSet(IEnumerable<string>? selectedFieldIds)
    {
        return selectedFieldIds is null
            ? new HashSet<string>(FieldComparer)
            : new HashSet<string>(selectedFieldIds.Where(value => !string.IsNullOrWhiteSpace(value)), FieldComparer);
    }

    private static User CreateEmptyUser(int userId)
    {
        return new User
        {
            Id = userId,
            Company = new Company(),
            Favorites = []
        };
    }
}