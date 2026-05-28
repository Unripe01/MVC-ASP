using System.ComponentModel.DataAnnotations;

namespace ASP_MVC.ViewModels;

/// <summary>
/// Company CRUD画面で受け取る入力モデル。
/// </summary>
public class CrudCompanyInputViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "会社名を入力してください。")]
    public string CompanyName { get; set; } = "";
}
