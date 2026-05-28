namespace ASP_MVC.Entities;

/// <summary>
/// 帳票のXMLスナップショットを保存するEntity。
/// </summary>
public class ReportInstance
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string ReportType { get; set; } = "";

    public string XmlData { get; set; } = "";

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}