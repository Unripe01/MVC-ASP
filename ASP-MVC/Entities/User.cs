namespace ASP_MVC.Entities;

/// <summary>
/// 帳票Projectionの起点になるユーザーEntity。
/// </summary>
public class User
{
    public int Id { get; set; }

    public int CompanyId { get; set; }

    public string UserName { get; set; } = "";

    public Company? Company { get; set; }

    
    public List<Favorite> Favorites { get; set; } = [];
}