namespace ASP_MVC.Reports;

/// <summary>
/// 帳票専用入力値をFieldDefinitionから読み出すための共通契約。
/// </summary>
public interface IReportFieldValueDocument
{
    string GetReportFieldValue(string key);
}