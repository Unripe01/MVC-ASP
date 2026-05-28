namespace ASP_MVC.ViewModels;

/// <summary>
/// ユーザー好きなもの帳票のPOST入力を受け取るViewModel。
/// </summary>
public class UserFavoriteReportPostViewModel
{
    public string ReportKey { get; set; } = "";

    public int UserId { get; set; }

    public int CompanyId { get; set; }

    public string CompanyName { get; set; } = "";

    public string UserName { get; set; } = "";

    public string Nationality { get; set; } = "";

    public string Age { get; set; } = "";

    public string BloodType { get; set; } = "";

    public string Birthday { get; set; } = "";

    public List<int> FavoriteIds { get; set; } = [];

    public List<string> FavoriteNames { get; set; } = [];

    public List<string> SelectedFieldIds { get; set; } = [];
}