namespace ASP_MVC.Reports;

/// <summary>
/// Entityには存在しないが、帳票XMLだけに保存する入力値を表す。
/// </summary>
public class ReportFieldValue
{
    public string Key { get; set; } = "";

    public string Value { get; set; } = "";
}