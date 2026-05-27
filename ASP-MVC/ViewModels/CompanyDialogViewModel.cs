namespace ASP_MVC.ViewModels;

/// <summary>
/// 帳票入力中に企業マスターを編集するダイアログViewModel。
/// </summary>
public class CompanyDialogViewModel
{
    public int UserId { get; set; }

    public int CompanyId { get; set; }

    public string CompanyName { get; set; } = "";
}