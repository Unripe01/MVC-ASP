namespace ASP_MVC.ViewModels;

/// <summary>
/// Favorite CRUD画面全体の表示モデル。
/// </summary>
public class CrudFavoritePageViewModel
{
    public IReadOnlyList<CrudFavoriteListItemViewModel> Favorites { get; set; } = [];

    public IReadOnlyList<CrudSelectOptionViewModel> Users { get; set; } = [];

    public CrudFavoriteInputViewModel Input { get; set; } = new();

    public string? Message { get; set; }
}
