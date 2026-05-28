namespace ASP_MVC.ViewModels;

/// <summary>
/// 帳票Field定義から生成される入力UI用ViewModel。
/// </summary>
public class ReportFieldInputViewModel
{
    public string FieldId { get; set; } = "";

    public string PropertyPath { get; set; } = "";

    public string Label { get; set; } = "";

    public string InputName { get; set; } = "";

    public string Value { get; set; } = "";

    public string HiddenValue { get; set; } = "";

    public IReadOnlyList<string> Values { get; set; } = [];

    public IReadOnlyList<int> ValueIds { get; set; } = [];

    public IReadOnlyList<ReportSelectOptionViewModel> Options { get; set; } = [];

    public bool IsCollection { get; set; }

    public bool IsReportOnly { get; set; }

    public bool IsFromMaster { get; set; }

    public bool AllowReverseReflection { get; set; }

    public bool IsSelected { get; set; }

    public string? DialogUrl { get; set; }
}