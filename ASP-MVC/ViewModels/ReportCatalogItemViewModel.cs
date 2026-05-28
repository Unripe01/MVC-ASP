namespace ASP_MVC.ViewModels;

/// <summary>
/// 帳票一覧画面で表示する帳票項目。
/// </summary>
public class ReportCatalogItemViewModel
{
    public string ReportKey { get; set; } = "";

    public string ReportTitle { get; set; } = "";

    public string? Summary { get; set; }

    public string RouteAction { get; set; } = "";
}