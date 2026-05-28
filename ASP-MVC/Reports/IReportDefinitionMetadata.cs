namespace ASP_MVC.Reports;

/// <summary>
/// 帳票一覧や起動導線に必要な帳票メタデータを表す契約。
/// </summary>
public interface IReportDefinitionMetadata
{
    string ReportKey { get; }

    string DisplayTitle { get; }

    string? Summary { get; }

    bool SupportsMasterActions { get; }

    string TemplateFileName { get; }
}