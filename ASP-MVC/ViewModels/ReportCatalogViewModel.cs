namespace ASP_MVC.ViewModels;

/// <summary>
/// 帳票一覧画面全体のViewModel。
/// </summary>
public class ReportCatalogViewModel
{
    public int UserId { get; set; }

    public IReadOnlyList<ReportCatalogItemViewModel> Reports { get; set; } = [];
}