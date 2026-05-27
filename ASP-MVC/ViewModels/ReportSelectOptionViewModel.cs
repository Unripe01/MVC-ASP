namespace ASP_MVC.ViewModels;

/// <summary>
/// マスター取得Fieldで使う選択肢ViewModel。
/// </summary>
public class ReportSelectOptionViewModel
{
    public string Value { get; set; } = "";

    public string Text { get; set; } = "";

    public bool Selected { get; set; }
}