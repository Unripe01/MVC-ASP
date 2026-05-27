namespace ASP_MVC.Entities;

/// <summary>
/// ユーザーに紐づく好きなものを表す1:N検証用Entity。
/// </summary>
public class Favorite
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string FavoriteName { get; set; } = "";
}