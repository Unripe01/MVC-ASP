namespace ASP_MVC.Reports;

/// <summary>
/// Entityプロパティに紐づかず、帳票XMLだけに保存されるField定義。
/// </summary>
public sealed class ReportOnlyFieldDefinition : FieldDefinition
{
    public ReportOnlyFieldDefinition(string fieldKey)
        : base(fieldKey)
    {
        IsReportOnly = true;
        InputName = BuildReportValueInputName(fieldKey);
    }

    /// <summary>
    /// 帳票専用入力値はReportDocumentからField keyで読み出す。
    /// </summary>
    public override object? ReadValue(object model)
    {
        return model is IReportFieldValueDocument document
            ? document.GetReportFieldValue(PropertyPath)
            : "";
    }

    /// <summary>
    /// MVCのDictionary bindingに合わせたPOST名へ変換する。
    /// </summary>
    public override FieldDefinition Input(string inputName)
    {
        InputName = BuildReportValueInputName(inputName);
        return this;
    }

    private static string BuildReportValueInputName(string key)
    {
        return $"ReportValues[{key}]";
    }
}