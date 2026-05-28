using ASP_MVC.Entities;

namespace ASP_MVC.Reports;

/// <summary>
/// ユーザーの基本情報をtxtテンプレートへ投影するPoC帳票定義。
/// </summary>
public class UserInfoReportDefinition : ReportDefinition<User>
{
    /// <summary>
    /// 基本情報をFieldとして定義する。
    /// </summary>
    protected override void Configure()
    {
        DefineReport(
            reportKey: "UserInfo",
            displayTitle: "ユーザー情報帳票",
            summary: "ユーザー基本情報を保存・出力する帳票",
            supportsMasterActions: true);

        Template("user_info.txt");

        Field(user => user.UserName)
            .Label("名前")
            .Input("UserName")
            .TemplateKey("User.UserName");

        Field(user => user.Nationality)
            .Label("国籍")
            .Input("Nationality")
            .TemplateKey("User.Nationality");

        Field(user => user.Company!.CompanyName)
            .Label("企業")
            .Input("CompanyId")
            .FromMaster("Company")
            .TemplateKey("Company.CompanyName");

        Field(user => user.Age)
            .Label("年齢")
            .Input("Age")
            .TemplateKey("User.Age");

        Field(user => user.BloodType)
            .Label("血液型")
            .Input("BloodType")
            .TemplateKey("User.BloodType");

        Field(user => user.Birthday)
            .Label("生年月日")
            .Input("Birthday")
            .TemplateKey("User.Birthday");
    }
}