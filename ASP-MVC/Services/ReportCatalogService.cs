using ASP_MVC.Reports;
using ASP_MVC.ViewModels;

namespace ASP_MVC.Services;

/// <summary>
/// 帳票一覧で表示する帳票メタデータを組み立てるService。
/// </summary>
public class ReportCatalogService
{
    private readonly IReadOnlyList<IReportDefinitionMetadata> _reportDefinitions;

    public ReportCatalogService(
        UserFavoriteReportDefinition userFavoriteReportDefinition,
        UserInfoReportDefinition userInfoReportDefinition)
    {
        _reportDefinitions = new IReportDefinitionMetadata[]
        {
            userFavoriteReportDefinition,
            userInfoReportDefinition
        };
    }

    /// <summary>
    /// 帳票一覧画面の表示モデルを生成する。
    /// </summary>
    public ReportCatalogViewModel BuildCatalog(int userId)
    {
        return new ReportCatalogViewModel
        {
            UserId = userId,
            Reports = _reportDefinitions
                .Select(definition => new ReportCatalogItemViewModel
                {
                    ReportKey = definition.ReportKey,
                    ReportTitle = definition.DisplayTitle,
                    Summary = definition.Summary,
                    RouteAction = definition.ReportKey
                })
                .ToList()
        };
    }
}