using ASP_MVC.Entities;
using ASP_MVC.Services;
using ASP_MVC.Reports;
using ASP_MVC.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ASP_MVC.Controllers;

/// <summary>
/// 帳票PoC画面のHTTP入口を担当するController。
/// </summary>
public class ReportController : Controller
{
    private readonly ReportCatalogService _reportCatalogService;
    private readonly ReportDocumentService _reportDocumentService;
    private readonly ReportEngine _reportEngine;
    private readonly UserInfoReportDefinition _userInfoReportDefinition;

    public ReportController(
        ReportCatalogService reportCatalogService,
        ReportDocumentService reportDocumentService,
        ReportEngine reportEngine,
        UserInfoReportDefinition userInfoReportDefinition)
    {
        _reportCatalogService = reportCatalogService;
        _reportDocumentService = reportDocumentService;
        _reportEngine = reportEngine;
        _userInfoReportDefinition = userInfoReportDefinition;
    }

    /// <summary>
    /// 帳票一覧を表示する。
    /// </summary>
    [HttpGet]
    public IActionResult Index(int userId = 1)
    {
        var model = _reportCatalogService.BuildCatalog(userId);
        return View(model);
    }

    /// <summary>
    /// ユーザー好きなもの帳票の初期画面を表示する。
    /// </summary>
    [HttpGet]
    public IActionResult UserFavorite(int id = 1)
    {
        var model = _reportEngine.BuildUserFavoriteReport(id);
        return View(model);
    }

    /// <summary>
    /// ユーザー情報帳票の初期画面を表示する。
    /// </summary>
    [HttpGet]
    public IActionResult UserInfo(int id = 1)
    {
        var snapshot = _reportDocumentService.GetLatestSnapshot(id, "UserInfo");
        var user = snapshot is null
            ? CreateEmptyUser(id)
            : _reportDocumentService.Deserialize<User>(snapshot.XmlData) ?? CreateEmptyUser(id);

        user = UserService.NormalizeReportUser(user);

        var model = _reportDocumentService.BuildWorkspace(
            _userInfoReportDefinition,
            user,
            reportKey: _userInfoReportDefinition.ReportKey,
            reportTitle: _userInfoReportDefinition.DisplayTitle,
            supportsMasterActions: _userInfoReportDefinition.SupportsMasterActions,
            savedXmlData: snapshot?.XmlData ?? "");

        return View(model);
    }

    /// <summary>
    /// 帳票入力をEntityへ逆反映し、XML Snapshotを保存する。
    /// </summary>
    [HttpPost]
    public IActionResult SaveUserFavorite(UserFavoriteReportPostViewModel input)
    {
        var model = _reportEngine.SaveUserFavoriteReport(input);
        return PartialView("_ReportWorkspace", model);
    }

    /// <summary>
    /// 保存前の入力値からtxtプレビューを部分更新する。
    /// </summary>
    [HttpPost]
    public IActionResult PreviewUserFavorite(UserFavoriteReportPostViewModel input)
    {
        var model = new ReportPreviewViewModel
        {
            PreviewText = _reportEngine.RenderPreview(input)
        };

        return PartialView("_ReportPreview", model);
    }

    /// <summary>
    /// 選択された項目だけをマスターから取得して帳票へ反映する。
    /// </summary>
    [HttpPost]
    public IActionResult FetchMasterValues(UserFavoriteReportPostViewModel input)
    {
        var model = _reportEngine.FetchMasterValues(input);
        return PartialView("_ReportWorkspace", model);
    }

    /// <summary>
    /// 選択された項目だけをマスターへ逆反映する。
    /// </summary>
    [HttpPost]
    public IActionResult ReverseReflect(UserFavoriteReportPostViewModel input)
    {
        var model = _reportEngine.ReverseReflect(input);
        return PartialView("_ReportWorkspace", model);
    }

    /// <summary>
    /// 企業マスター編集ダイアログを部分表示する。
    /// </summary>
    [HttpGet]
    public IActionResult CompanyDialog(UserFavoriteReportPostViewModel input)
    {
        var model = _reportEngine.BuildCompanyDialog(input.UserId, input.CompanyId);
        return PartialView("_CompanyDialog", model);
    }

    /// <summary>
    /// ダイアログで編集した企業マスターを保存して帳票へ戻る。
    /// </summary>
    [HttpPost]
    public IActionResult SaveCompanyDialog(CompanyDialogViewModel input)
    {
        _reportEngine.SaveCompanyDialog(input);
        Response.Headers["HX-Trigger"] = "report-modal-close";

        var model = _reportEngine.BuildUserFavoriteReport(input.UserId, "会社マスターを更新しました");
        return PartialView("_ReportWorkspace", model);
    }

    /// <summary>
    /// txtテンプレートの差し込み結果をDocumentDownloadへ出力する。
    /// </summary>
    [HttpPost]
    public IActionResult ExportUserFavorite(int userId)
    {
        var fileName = _reportEngine.ExportUserFavoriteText(userId);
        var model = _reportEngine.BuildUserFavoriteReport(userId, $"{fileName} を出力しました");
        return PartialView("_ReportWorkspace", model);
    }

    /// <summary>
    /// ユーザー情報帳票をXML保存する。
    /// </summary>
    [HttpPost]
    public IActionResult SaveUserInfo(UserFavoriteReportPostViewModel input)
    {
        var user = UserService.NormalizeReportUser(BuildInfoUser(input));

        var savedXmlData = _reportDocumentService.SaveSnapshot("UserInfo", input.UserId, user);
        var model = _reportDocumentService.BuildWorkspace(
            _userInfoReportDefinition,
            user,
            reportKey: _userInfoReportDefinition.ReportKey,
            reportTitle: _userInfoReportDefinition.DisplayTitle,
            supportsMasterActions: _userInfoReportDefinition.SupportsMasterActions,
            message: "XMLを保存しました",
            savedXmlData: savedXmlData);

        return PartialView("_ReportWorkspace", model);
    }

    /// <summary>
    /// ユーザー情報帳票の保存前プレビューを更新する。
    /// </summary>
    [HttpPost]
    public IActionResult PreviewUserInfo(UserFavoriteReportPostViewModel input)
    {
        var user = BuildInfoUser(input);

        var model = new ReportPreviewViewModel
        {
            PreviewText = _reportDocumentService.RenderPreview(_userInfoReportDefinition, user)
        };

        return PartialView("_ReportPreview", model);
    }

    /// <summary>
    /// ユーザー情報帳票のtxt出力結果を保存する。
    /// </summary>
    [HttpPost]
    public IActionResult ExportUserInfo(int userId)
    {
        var snapshot = _reportDocumentService.GetLatestSnapshot(userId, "UserInfo")
            ?? throw new InvalidOperationException("出力対象の保存済み帳票が見つかりません。");

        var user = _reportDocumentService.Deserialize<User>(snapshot.XmlData)
            ?? throw new InvalidOperationException("保存済み帳票のXMLを復元できません。");

        user = UserService.NormalizeReportUser(user);
        var fileName = $"{Guid.NewGuid():N}-user-info-{user.Id}.txt";
        _reportDocumentService.Write(_userInfoReportDefinition, user, fileName);

        var model = _reportDocumentService.BuildWorkspace(
            _userInfoReportDefinition,
            user,
            reportKey: _userInfoReportDefinition.ReportKey,
            reportTitle: _userInfoReportDefinition.DisplayTitle,
            supportsMasterActions: _userInfoReportDefinition.SupportsMasterActions,
            message: $"{fileName} を出力しました",
            savedXmlData: snapshot.XmlData);

        return PartialView("_ReportWorkspace", model);
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

    private static User BuildInfoUser(UserFavoriteReportPostViewModel input)
    {
        return new User
        {
            Id = input.UserId,
            CompanyId = input.CompanyId,
            UserName = input.UserName ?? "",
            Nationality = input.Nationality ?? "",
            Age = input.Age ?? "",
            BloodType = input.BloodType ?? "",
            Birthday = input.Birthday ?? "",
            Company = new Company
            {
                Id = input.CompanyId,
                CompanyName = input.CompanyName ?? ""
            },
            Favorites = []
        };
    }
}