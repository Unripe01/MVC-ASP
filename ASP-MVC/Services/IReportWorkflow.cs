using ASP_MVC.ViewModels;

namespace ASP_MVC.Services;

/// <summary>
/// 帳票ごとの画面表示・保存・マスター取得・逆反映・出力を統一的に扱う契約。
/// </summary>
public interface IReportWorkflow
{
    string ReportKey { get; }

    string ReportTitle { get; }

    string? Summary { get; }

    ReportWorkspaceViewModel Open(int userId);

    ReportWorkspaceViewModel Save(UserFavoriteReportPostViewModel input);

    string Preview(UserFavoriteReportPostViewModel input);

    ReportWorkspaceViewModel FetchMaster(UserFavoriteReportPostViewModel input);

    ReportWorkspaceViewModel ReverseReflect(UserFavoriteReportPostViewModel input);

    ReportWorkspaceViewModel Export(int userId);
}