using System.Linq.Expressions;

namespace ASP_MVC.Reports;

/// <summary>
/// Entityを帳票へ投影するField定義の基底クラス。
/// </summary>
public abstract class ReportDefinition<TModel> : IReportDefinitionMetadata
{
    private readonly List<FieldDefinition> _fields = [];

    protected ReportDefinition()
    {
        Configure();
    }

    public IReadOnlyList<FieldDefinition> Fields => _fields;

    public string ReportKey { get; private set; } = "";

    public string DisplayTitle { get; private set; } = "";

    public string? Summary { get; private set; }

    public IEnumerable<FieldDefinition> TemplateFields => _fields.Where(definitionField => definitionField.HasTemplateKey);

    public string TemplateFileName { get; private set; } = "";

    /// <summary>
    /// 派生クラスが帳票のField構成を定義する。
    /// </summary>
    protected abstract void Configure();

    /// <summary>
    /// この帳票定義が使用するtxtテンプレートファイル名を指定する。
    /// </summary>
    protected void Template(string templateFileName)
    {
        TemplateFileName = templateFileName;
    }

    /// <summary>
    /// 帳票一覧や画面タイトルに使うメタデータを定義する。
    /// </summary>
    protected void DefineReport(string reportKey, string displayTitle, string? summary = null)
    {
        ReportKey = reportKey;
        DisplayTitle = displayTitle;
        Summary = summary;
    }

    /// <summary>
    /// 型付きExpressionからPropertyPathと値取得関数を登録する。
    /// </summary>
    protected FieldDefinition Field<TValue>(Expression<Func<TModel, TValue>> expression)
    {
        var propertyPath = GetPropertyPath(expression.Body);
        var field = new FieldDefinition<TModel, TValue>(propertyPath, expression.Compile());
        _fields.Add(field);
        return field;
    }

    private static string GetPropertyPath(Expression expression)
    {
        var members = new Stack<string>();
        var currentExpression = RemoveConvertExpression(expression);

        while (currentExpression is MemberExpression memberExpression)
        {
            members.Push(memberExpression.Member.Name);
            currentExpression = RemoveConvertExpression(memberExpression.Expression);
        }

        if (members.Count == 0)
        {
            throw new InvalidOperationException("Fieldにはプロパティ参照を指定してください。");
        }

        return string.Join(".", members);
    }

    private static Expression? RemoveConvertExpression(Expression? expression)
    {
        return expression is UnaryExpression unaryExpression
            ? unaryExpression.Operand
            : expression;
    }
}