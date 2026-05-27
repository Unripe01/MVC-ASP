namespace ASP_MVC.ViewModels;

/// <summary>
/// ユーザー好きなもの帳票のPOST入力を受け取るViewModel。
/// </summary>
public class UserFavoriteReportPostViewModel
{
    public int UserId { get; set; }

    public int CompanyId { get; set; }

    public string UserName { get; set; } = "";

    public List<string> FavoriteNames { get; set; } = [];
}