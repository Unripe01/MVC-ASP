using ASP_MVC.Entities;

namespace ASP_MVC.Reports;

/// <summary>
/// ユーザーと好きなものをtxtテンプレートへ投影するPoC帳票定義。
/// </summary>
public class UserFavoriteReportDefinition : ReportDefinition<User>
{
    /// <summary>
    /// UI、マスター連携、逆反映、1:N項目をFieldとして定義する。
    /// </summary>
    protected override void Configure()
    {
        Field(user => user.UserName)
            .Label("名前")
            .Input("UserName")
            .FromMaster()
            .AllowReverseReflect();

        Field(user => user.Company!.CompanyName)
            .Label("会社")
            .Input("CompanyId")
            .FromMaster("Company")
            .OpenDialog("/Report/CompanyDialog");

        Field(user => user.Favorites)
            .Label("好きなもの")
            .Input("FavoriteNames")
            .AsCollection()
            .ResolveWith<FavoriteResolver>();
    }
}