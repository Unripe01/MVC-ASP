using ASP_MVC.Entities;

namespace ASP_MVC.ViewModels;

/// <summary>
/// Company CRUD画面全体の表示モデル。
/// </summary>
public class CrudCompanyPageViewModel
{
    public IReadOnlyList<Company> Companies { get; set; } = [];

    public CrudCompanyInputViewModel Input { get; set; } = new();

    public string? Message { get; set; }
}
