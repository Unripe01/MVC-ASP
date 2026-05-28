namespace ASP_MVC.Services;

/// <summary>
/// ReportKeyで帳票Workflowを解決するレジストリ。
/// </summary>
public class ReportWorkflowRegistry
{
    private readonly IReadOnlyList<IReportWorkflow> _workflows;
    private readonly IReadOnlyDictionary<string, IReportWorkflow> _workflowMap;

    public ReportWorkflowRegistry(IEnumerable<IReportWorkflow> workflows)
    {
        _workflows = workflows.ToList();
        _workflowMap = _workflows.ToDictionary(workflow => workflow.ReportKey, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<IReportWorkflow> Workflows => _workflows;

    public IReportWorkflow GetRequired(string reportKey)
    {
        if (string.IsNullOrWhiteSpace(reportKey))
        {
            throw new InvalidOperationException("ReportKeyが指定されていません。");
        }

        if (_workflowMap.TryGetValue(reportKey, out var workflow))
        {
            return workflow;
        }

        throw new InvalidOperationException($"未対応の帳票です: {reportKey}");
    }
}