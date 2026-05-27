using ASP_MVC.Entities;

namespace ASP_MVC.Services;

/// <summary>
/// txtテンプレートの単純文字列置換とDocumentDownload出力を担当するService。
/// </summary>
public class TemplateRenderService
{
    private readonly IWebHostEnvironment _environment;

    public TemplateRenderService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    /// <summary>
    /// user_favorite.txtへEntity値を差し込み、プレビュー文字列を生成する。
    /// </summary>
    public string RenderUserFavorite(User user)
    {
        var templatePath = Path.Combine(_environment.ContentRootPath, "DocumentTemplates", "user_favorite.txt");
        var template = File.Exists(templatePath)
            ? File.ReadAllText(templatePath)
            : "企業：{{Company.CompanyName}}\nおなまえ：{{User.UserName}}\n好きなものリスト：{{Favorite.FavoriteName}}";

        return template
            .Replace("{{Company.CompanyName}}", user.Company?.CompanyName ?? "")
            .Replace("{{User.UserName}}", user.UserName)
            .Replace("{{Favorite.FavoriteName}}", string.Join("、", user.Favorites.Select(favorite => favorite.FavoriteName)));
    }

    /// <summary>
    /// txtテンプレート差し込み結果をDocumentDownloadへ保存する。
    /// </summary>
    public string WriteUserFavorite(User user)
    {
        var output = RenderUserFavorite(user);
        var outputDirectory = Path.Combine(_environment.ContentRootPath, "DocumentDownload");
        Directory.CreateDirectory(outputDirectory);

        var fileName = $"{Guid.NewGuid():N}-user-{user.Id}.txt";
        var outputPath = Path.Combine(outputDirectory, fileName);
        File.WriteAllText(outputPath, output);
        return fileName;
    }
}