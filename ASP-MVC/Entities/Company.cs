namespace ASP_MVC.Entities;

/// <summary>
/// 帳票が参照する企業マスターを表すEntity。
/// </summary>
public class Company
{
    public int Id { get; set; }

    public string CompanyName { get; set; } = "";
}