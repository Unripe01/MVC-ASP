namespace ASP_MVC.Reports;

/// <summary>
/// 帳票XMLに保存するEntity本体と帳票専用入力値をまとめるスナップショット。
/// </summary>
public class ReportDocument<TModel> : IReportFieldValueDocument
{
    public TModel? Model { get; set; }

    public List<ReportFieldValue> Values { get; set; } = [];

    public static ReportDocument<TModel> FromModel(TModel model, IEnumerable<ReportFieldValue>? values = null)
    {
        return new ReportDocument<TModel>
        {
            Model = model,
            Values = values?.ToList() ?? []
        };
    }

    /// <summary>
    /// Field keyに対応する帳票専用値を返す。未入力の項目は空文字として扱う。
    /// </summary>
    public string GetReportFieldValue(string key)
    {
        return Values.FirstOrDefault(value => string.Equals(value.Key, key, StringComparison.OrdinalIgnoreCase))?.Value ?? "";
    }
}