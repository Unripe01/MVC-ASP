using System.ComponentModel.DataAnnotations;

namespace ASP_MVC.ViewModels;

/// <summary>
/// User CRUD画面で受け取る入力モデル。
/// </summary>
public class CrudUserInputViewModel
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "会社を選択してください。")]
    public int CompanyId { get; set; }

    [Required(ErrorMessage = "ユーザー名を入力してください。")]
    public string UserName { get; set; } = "";
}
