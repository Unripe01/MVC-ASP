using ASP_MVC.Services;
using ASP_MVC.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ASP_MVC.Controllers;

/// <summary>
/// 帳票PoC画面のHTTP入口を担当するController。
/// </summary>
public class ReportController : Controller
{
    private const string UserFavoriteReportKey = "UserFavorite";
    private const string UserInfoReportKey = "UserInfo";

    private readonly ReportCatalogService _reportCatalogService;
    private readonly ReportWorkflowRegistry _workflowRegistry;
    private readonly ReportEngine _reportEngine;

    public ReportController(
        ReportCatalogService reportCatalogService,
        ReportWorkflowRegistry workflowRegistry,
        ReportEngine reportEngine)
    {
        _reportCatalogService = reportCatalogService;
        _workflowRegistry = workflowRegistry;
        _reportEngine = reportEngine;
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
        var model = _workflowRegistry.GetRequired(UserFavoriteReportKey).Open(id);
        return View(model);
    }

    /// <summary>
    /// ユーザー情報帳票の初期画面を表示する。
    /// </summary>
    [HttpGet]
    public IActionResult UserInfo(int id = 1)
    {
        var model = _workflowRegistry.GetRequired(UserInfoReportKey).Open(id);
        return View(model);
    }

    /// <summary>
    /// 帳票入力をEntityへ逆反映し、XML Snapshotを保存する。
    /// </summary>
    [HttpPost]
    public IActionResult SaveUserFavorite(UserFavoriteReportPostViewModel input)
    {
        input.ReportKey = UserFavoriteReportKey;
        var model = _workflowRegistry.GetRequired(UserFavoriteReportKey).Save(input);
        return PartialView("_ReportWorkspace", model);
    }

    /// <summary>
    /// 保存前の入力値からtxtプレビューを部分更新する。
    /// </summary>
    [HttpPost]
    public IActionResult PreviewUserFavorite(UserFavoriteReportPostViewModel input)
    {
        input.ReportKey = UserFavoriteReportKey;
        var model = new ReportPreviewViewModel
        {
            PreviewText = _workflowRegistry.GetRequired(UserFavoriteReportKey).Preview(input)
        };

        return PartialView("_ReportPreview", model);
    }

    /// <summary>
    /// 選択された項目だけをマスターから取得して帳票へ反映する。
    /// </summary>
    [HttpPost]
    public IActionResult FetchMasterValues(UserFavoriteReportPostViewModel input)
    {
        var reportKey = string.IsNullOrWhiteSpace(input.ReportKey) ? UserFavoriteReportKey : input.ReportKey;
        var model = _workflowRegistry.GetRequired(reportKey).FetchMaster(input);
        return PartialView("_ReportWorkspace", model);
    }

    /// <summary>
    /// 選択された項目だけをマスターへ逆反映する。
    /// </summary>
    [HttpPost]
    public IActionResult ReverseReflect(UserFavoriteReportPostViewModel input)
    {
        var reportKey = string.IsNullOrWhiteSpace(input.ReportKey) ? UserFavoriteReportKey : input.ReportKey;
        var model = _workflowRegistry.GetRequired(reportKey).ReverseReflect(input);
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

        var model = _workflowRegistry.GetRequired(UserFavoriteReportKey).Open(input.UserId);
        model.Message = "会社マスターを更新しました";
        return PartialView("_ReportWorkspace", model);
    }

    /// <summary>
    /// txtテンプレートの差し込み結果をDocumentDownloadへ出力する。
    /// </summary>
    [HttpPost]
    public IActionResult ExportUserFavorite(int userId)
    {
        var model = _workflowRegistry.GetRequired(UserFavoriteReportKey).Export(userId);
        return PartialView("_ReportWorkspace", model);
    }

    /// <summary>
    /// ユーザー情報帳票をXML保存する。
    /// </summary>
    [HttpPost]
    public IActionResult SaveUserInfo(UserFavoriteReportPostViewModel input)
    {
        input.ReportKey = UserInfoReportKey;
        var model = _workflowRegistry.GetRequired(UserInfoReportKey).Save(input);
        return PartialView("_ReportWorkspace", model);
    }

    /// <summary>
    /// ユーザー情報帳票の保存前プレビューを更新する。
    /// </summary>
    [HttpPost]
    public IActionResult PreviewUserInfo(UserFavoriteReportPostViewModel input)
    {
        input.ReportKey = UserInfoReportKey;
        var model = new ReportPreviewViewModel
        {
            PreviewText = _workflowRegistry.GetRequired(UserInfoReportKey).Preview(input)
        };

        return PartialView("_ReportPreview", model);
    }

    /// <summary>
    /// ユーザー情報帳票のtxt出力結果を保存する。
    /// </summary>
    [HttpPost]
    public IActionResult ExportUserInfo(int userId)
    {
        var model = _workflowRegistry.GetRequired(UserInfoReportKey).Export(userId);
        return PartialView("_ReportWorkspace", model);
    }
}
