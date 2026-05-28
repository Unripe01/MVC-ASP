using ASP_MVC.Reports;
using ASP_MVC.ViewModels;

namespace ASP_MVC.Services;

/// <summary>
/// UserFavorite帳票の処理を共通Workflow契約へ適合させる。
/// </summary>
public class UserFavoriteReportWorkflow : IReportWorkflow
{
    private readonly ReportEngine _reportEngine;
    private readonly UserFavoriteReportDefinition _definition;

    public UserFavoriteReportWorkflow(
        ReportEngine reportEngine,
        UserFavoriteReportDefinition definition)
    {
        _reportEngine = reportEngine;
        _definition = definition;
    }

    public string ReportKey => _definition.ReportKey;

    public string ReportTitle => _definition.DisplayTitle;

    public string? Summary => _definition.Summary;

    public ReportWorkspaceViewModel Open(int userId)
    {
        return _reportEngine.BuildUserFavoriteReport(userId);
    }

    public ReportWorkspaceViewModel Save(UserFavoriteReportPostViewModel input)
    {
        return _reportEngine.SaveUserFavoriteReport(input);
    }

    public string Preview(UserFavoriteReportPostViewModel input)
    {
        return _reportEngine.RenderPreview(input);
    }

    public ReportWorkspaceViewModel FetchMaster(UserFavoriteReportPostViewModel input)
    {
        return _reportEngine.FetchMasterValues(input);
    }

    public ReportWorkspaceViewModel ReverseReflect(UserFavoriteReportPostViewModel input)
    {
        return _reportEngine.ReverseReflect(input);
    }

    public ReportWorkspaceViewModel Export(int userId)
    {
        var fileName = _reportEngine.ExportUserFavoriteText(userId);
        return _reportEngine.BuildUserFavoriteReport(userId, $"{fileName} を出力しました");
    }
}