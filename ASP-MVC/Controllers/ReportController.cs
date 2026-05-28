using ASP_MVC.Services;
using ASP_MVC.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ASP_MVC.Controllers;

/// <summary>
/// 帳票PoC画面のHTTP入口を担当するController。
/// </summary>
public class ReportController : Controller
{
    private readonly ReportEngine _reportEngine;

    public ReportController(ReportEngine reportEngine)
    {
        _reportEngine = reportEngine;
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
}