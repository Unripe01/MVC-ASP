namespace ASP_MVC.ViewModels;

/// <summary>
/// 帳票画面全体の共通ViewModel。
/// </summary>
public class ReportWorkspaceViewModel
{
    public string ReportKey { get; set; } = "";

    public string ReportTitle { get; set; } = "";

    public bool SupportsMasterActions { get; set; }

    public int UserId { get; set; }

    public IReadOnlyList<ReportFieldInputViewModel> Fields { get; set; } = [];

    public string PreviewText { get; set; } = "";

    public string XmlData { get; set; } = "";

    public string? Message { get; set; }
}