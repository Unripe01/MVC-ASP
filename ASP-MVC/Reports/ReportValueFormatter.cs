using System.Collections;
using ASP_MVC.Entities;

namespace ASP_MVC.Reports;

/// <summary>
/// Field表示値とテンプレート差し込み値を共通の文字列表現へ変換する補助クラス。
/// </summary>
internal static class ReportValueFormatter
{
    /// <summary>
    /// 単一値またはcollection値をテンプレート差し込み用の文字列へ変換する。
    /// </summary>
    public static string Format(object? value, string collectionSeparator = "")
    {
        if (value is null)
        {
            return "";
        }

        if (value is string text)
        {
            return text;
        }

        if (value is IEnumerable values)
        {
            var textValues = values.Cast<object?>()
                .Select(FormatSingle)
                .Where(textValue => !string.IsNullOrWhiteSpace(textValue));

            return string.Join(collectionSeparator, textValues);
        }

        return FormatSingle(value);
    }

    /// <summary>
    /// 入力UIのcollection表示で扱いやすい文字列配列へ変換する。
    /// </summary>
    public static IReadOnlyList<string> FormatCollection(object? value)
    {
        if (value is null)
        {
            return [];
        }

        if (value is string text)
        {
            return [text];
        }

        if (value is IEnumerable values)
        {
            return values.Cast<object?>()
                .Select(FormatSingle)
                .Where(textValue => !string.IsNullOrWhiteSpace(textValue))
                .ToList();
        }

        return [FormatSingle(value)];
    }

    private static string FormatSingle(object? value)
    {
        return value switch
        {
            null => "",
            Company company => company.CompanyName,
            Favorite favorite => favorite.FavoriteName,
            _ => Convert.ToString(value) ?? ""
        };
    }
}