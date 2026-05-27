namespace ASP_MVC.ViewModels;

/// <summary>
/// ユーザー好きなもの帳票画面全体のViewModel。
/// </summary>
public class UserFavoriteReportViewModel
{
    public int UserId { get; set; }

    public IReadOnlyList<ReportFieldInputViewModel> Fields { get; set; } = [];

    public string PreviewText { get; set; } = "";

    public string XmlData { get; set; } = "";

    public string? Message { get; set; }
}