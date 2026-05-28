namespace ASP_MVC.ViewModels;

/// <summary>
/// Favorite一覧表示の1行モデル。
/// </summary>
public class CrudFavoriteListItemViewModel
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string UserName { get; set; } = "";

    public string FavoriteName { get; set; } = "";
}
