using ASP_MVC.Entities;
using ASP_MVC.Reports;
using ASP_MVC.ViewModels;

namespace ASP_MVC.Services;

/// <summary>
/// ReportDefinitionからRazor入力UI用のField ViewModelを生成するService。
/// </summary>
public class ReportRendererService
{
    /// <summary>
    /// 定義済みFieldを画面表示用の入力モデルへ変換する。
    /// </summary>
    public IReadOnlyList<ReportFieldInputViewModel> BuildFields(
        UserFavoriteReportDefinition definition,
        User user,
        IReadOnlyList<Company> companies)
    {
        return definition.Fields
            .Select(field => BuildField(field, user, companies))
            .ToList();
    }

    private static ReportFieldInputViewModel BuildField(
        FieldDefinition field,
        User user,
        IReadOnlyList<Company> companies)
    {
        return new ReportFieldInputViewModel
        {
            FieldId = field.FieldId,
            PropertyPath = field.PropertyPath,
            Label = field.DisplayLabel,
            InputName = field.InputName,
            Value = field.ReadTextValue(user),
            Values = field.ReadCollectionValues(user),
            Options = BuildOptions(field, user, companies),
            IsCollection = field.IsCollection,
            IsFromMaster = field.IsFromMaster,
            AllowReverseReflection = field.AllowReverseReflection,
            DialogUrl = field.DialogUrl
        };
    }

    private static IReadOnlyList<ReportSelectOptionViewModel> BuildOptions(
        FieldDefinition field,
        User user,
        IReadOnlyList<Company> companies)
    {
        if (field.MasterSource != "Company")
        {
            return [];
        }

        return companies.Select(company => new ReportSelectOptionViewModel
            {
                Value = company.Id.ToString(),
                Text = company.CompanyName,
                Selected = company.Id == user.CompanyId
            })
            .ToList();
    }
}