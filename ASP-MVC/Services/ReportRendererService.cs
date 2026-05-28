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
    public IReadOnlyList<ReportFieldInputViewModel> BuildFields<TModel>(
        ReportDefinition<TModel> definition,
        object model,
        IReadOnlyList<Company> companies,
        IReadOnlySet<string>? selectedFieldIds = null)
    {
        var selectedFields = selectedFieldIds ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        return definition.Fields
            .Select(field => BuildField(field, model, companies, selectedFields))
            .ToList();
    }

    private static ReportFieldInputViewModel BuildField(
        FieldDefinition field,
        object model,
        IReadOnlyList<Company> companies,
        IReadOnlySet<string> selectedFieldIds)
    {
        return new ReportFieldInputViewModel
        {
            FieldId = field.FieldId,
            PropertyPath = field.PropertyPath,
            Label = field.DisplayLabel,
            InputName = field.InputName,
            Value = field.ReadTextValue(model!),
            HiddenValue = field.ReadTextValue(model!),
            Values = field.ReadCollectionValues(model!),
            ValueIds = BuildValueIds(field, model),
            Options = BuildOptions(field, model, companies),
            IsCollection = field.IsCollection,
            IsReportOnly = field.IsReportOnly,
            IsFromMaster = field.IsFromMaster,
            AllowReverseReflection = field.AllowReverseReflection,
            IsSelected = selectedFieldIds.Contains(field.FieldId),
            DialogUrl = field.DialogUrl
        };
    }

    private static IReadOnlyList<int> BuildValueIds(FieldDefinition field, object model)
    {
        if (!field.IsCollection || field.PropertyPath != "Favorites" || TryGetUser(model) is not { } user)
        {
            return [];
        }

        return user.Favorites.Select(favorite => favorite.Id).ToList();
    }

    private static IReadOnlyList<ReportSelectOptionViewModel> BuildOptions(
        FieldDefinition field,
        object model,
        IReadOnlyList<Company> companies)
    {
        if (field.MasterSource != "Company" || TryGetUser(model) is not { } user)
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

    private static User? TryGetUser(object model)
    {
        return model switch
        {
            User user => user,
            ReportDocument<User> document => document.Model,
            _ => null
        };
    }
}