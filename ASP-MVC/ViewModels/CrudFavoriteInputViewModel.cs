using System.ComponentModel.DataAnnotations;

namespace ASP_MVC.ViewModels;

/// <summary>
/// Favorite CRUD画面で受け取る入力モデル。
/// </summary>
public class CrudFavoriteInputViewModel
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "ユーザーを選択してください。")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "好きなものを入力してください。")]
    public string FavoriteName { get; set; } = "";
}
