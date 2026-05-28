namespace ASP_MVC.ViewModels;

/// <summary>
/// CRUD画面で使う選択肢モデル。
/// </summary>
public class CrudSelectOptionViewModel
{
    public string Value { get; set; } = "";

    public string Text { get; set; } = "";

    public bool Selected { get; set; }
}
