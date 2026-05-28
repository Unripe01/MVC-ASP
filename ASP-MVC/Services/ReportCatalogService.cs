using ASP_MVC.ViewModels;

namespace ASP_MVC.Services;

/// <summary>
/// 帳票一覧で表示する帳票メタデータを組み立てるService。
/// </summary>
public class ReportCatalogService
{
    private readonly ReportWorkflowRegistry _workflowRegistry;

    public ReportCatalogService(ReportWorkflowRegistry workflowRegistry)
    {
        _workflowRegistry = workflowRegistry;
    }

    /// <summary>
    /// 帳票一覧画面の表示モデルを生成する。
    /// </summary>
    public ReportCatalogViewModel BuildCatalog(int userId)
    {
        return new ReportCatalogViewModel
        {
            UserId = userId,
            Reports = _workflowRegistry.Workflows
                .Select(workflow => new ReportCatalogItemViewModel
                {
                    ReportKey = workflow.ReportKey,
                    ReportTitle = workflow.ReportTitle,
                    Summary = workflow.Summary,
                    RouteAction = workflow.ReportKey
                })
                .ToList()
        };
    }
}