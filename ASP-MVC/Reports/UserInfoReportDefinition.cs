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
            summary: "ユーザー基本情報を保存・出力する帳票");

        Template("user_info.txt");

        Field(user => user.UserName)
            .FromMaster()
            .Label("名前")
            .Input("UserName")
            .AllowReverseReflect()
            .TemplateKey("User.UserName");

        Field(user => user.Nationality)
            .FromMaster()
            .Label("国籍")
            .Input("Nationality")
            .AllowReverseReflect()
            .TemplateKey("User.Nationality");

        Field(user => user.Company!.CompanyName)
            .FromMaster("Company")
            .Label("企業")
            .Input("CompanyId")
            .AllowReverseReflect()
            .TemplateKey("Company.CompanyName");

        Field(user => user.Age)
            .FromMaster()
            .Label("年齢")
            .Input("Age")
            .AllowReverseReflect()
            .TemplateKey("User.Age");

        Field(user => user.BloodType)
            .FromMaster()
            .Label("血液型")
            .Input("BloodType")
            .TemplateKey("User.BloodType");

        Field(user => user.Birthday)
            .FromMaster()
            .Label("生年月日")
            .Input("Birthday")
            .TemplateKey("User.Birthday");

        Field("EntryDate")
            .Label("入国日")
            .TemplateKey("入国日");
    }
}