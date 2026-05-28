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
        IReadOnlyList<Company> companies,
        IReadOnlySet<string>? selectedFieldIds = null)
    {
        var selectedFields = selectedFieldIds ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        return definition.Fields
            .Select(field => BuildField(field, user, companies, selectedFields))
            .ToList();
    }

    private static ReportFieldInputViewModel BuildField(
        FieldDefinition field,
        User user,
        IReadOnlyList<Company> companies,
        IReadOnlySet<string> selectedFieldIds)
    {
        return new ReportFieldInputViewModel
        {
            FieldId = field.FieldId,
            PropertyPath = field.PropertyPath,
            Label = field.DisplayLabel,
            InputName = field.InputName,
            Value = field.ReadTextValue(user),
            HiddenValue = field.ReadTextValue(user),
            Values = field.ReadCollectionValues(user),
            ValueIds = BuildValueIds(field, user),
            Options = BuildOptions(field, user, companies),
            IsCollection = field.IsCollection,
            IsFromMaster = field.IsFromMaster,
            AllowReverseReflection = field.AllowReverseReflection,
            IsSelected = selectedFieldIds.Contains(field.FieldId),
            DialogUrl = field.DialogUrl
        };
    }

    private static IReadOnlyList<int> BuildValueIds(FieldDefinition field, User user)
    {
        if (!field.IsCollection || field.PropertyPath != "Favorites")
        {
            return [];
        }

        return user.Favorites.Select(favorite => favorite.Id).ToList();
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