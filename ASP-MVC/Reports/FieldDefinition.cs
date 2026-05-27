using System.Collections;
using ASP_MVC.Entities;

namespace ASP_MVC.Reports;

/// <summary>
/// 帳票FieldのUI・マスター連携・逆反映可否を表す定義要素。
/// </summary>
public abstract class FieldDefinition
{
    protected FieldDefinition(string propertyPath)
    {
        PropertyPath = propertyPath;
        FieldId = propertyPath.Replace(".", "_");
        DisplayLabel = propertyPath;
        InputName = FieldId;
    }

    public string FieldId { get; }

    public string PropertyPath { get; }

    public string DisplayLabel { get; private set; }

    public string InputName { get; private set; }

    public bool IsFromMaster { get; private set; }

    public bool AllowReverseReflection { get; private set; }

    public bool IsCollection { get; private set; }

    public string? DialogUrl { get; private set; }

    public string? MasterSource { get; private set; }

    public Type? ResolverType { get; private set; }

    /// <summary>
    /// 画面に出すラベルを帳票定義側で指定する。
    /// </summary>
    public FieldDefinition Label(string label)
    {
        DisplayLabel = label;
        return this;
    }

    /// <summary>
    /// MVCのPOSTバインド名を帳票定義側で指定する。
    /// </summary>
    public FieldDefinition Input(string inputName)
    {
        InputName = inputName;
        return this;
    }

    /// <summary>
    /// このFieldがマスター取得対象であることを示す。
    /// </summary>
    public FieldDefinition FromMaster(string? masterSource = null)
    {
        IsFromMaster = true;
        MasterSource = masterSource ?? PropertyPath.Split('.')[0];
        return this;
    }

    /// <summary>
    /// このFieldがEntityへ逆反映できることを示す。
    /// </summary>
    public FieldDefinition AllowReverseReflect()
    {
        AllowReverseReflection = true;
        return this;
    }

    /// <summary>
    /// このFieldが1:N項目であることを示す。
    /// </summary>
    public FieldDefinition AsCollection()
    {
        IsCollection = true;
        return this;
    }

    /// <summary>
    /// マスター編集ダイアログを開くURLを定義する。
    /// </summary>
    public FieldDefinition OpenDialog(string dialogUrl)
    {
        DialogUrl = dialogUrl;
        return this;
    }

    /// <summary>
    /// 特殊な解決が必要なFieldにResolver型を紐づける。
    /// </summary>
    public FieldDefinition ResolveWith<TResolver>()
    {
        ResolverType = typeof(TResolver);
        return this;
    }

    /// <summary>
    /// 対象EntityからField値を取得する。
    /// </summary>
    public abstract object? ReadValue(object model);

    /// <summary>
    /// 入力UIで表示しやすい文字列値へ変換する。
    /// </summary>
    public string ReadTextValue(object model)
    {
        return FormatValue(ReadValue(model));
    }

    /// <summary>
    /// 1:N項目を入力UIで扱いやすい文字列配列へ変換する。
    /// </summary>
    public IReadOnlyList<string> ReadCollectionValues(object model)
    {
        var value = ReadValue(model);
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
                .Select(FormatValue)
                .Where(textValue => !string.IsNullOrWhiteSpace(textValue))
                .ToList();
        }

        return [FormatValue(value)];
    }

    private static string FormatValue(object? value)
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