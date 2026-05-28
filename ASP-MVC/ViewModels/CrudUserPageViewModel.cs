using ASP_MVC.Entities;

namespace ASP_MVC.ViewModels;

/// <summary>
/// User CRUD画面全体の表示モデル。
/// </summary>
public class CrudUserPageViewModel
{
    public IReadOnlyList<User> Users { get; set; } = [];

    public IReadOnlyList<CrudSelectOptionViewModel> Companies { get; set; } = [];

    public CrudUserInputViewModel Input { get; set; } = new();

    public string? Message { get; set; }
}
