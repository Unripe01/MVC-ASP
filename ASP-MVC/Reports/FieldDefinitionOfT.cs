namespace ASP_MVC.Reports;

/// <summary>
/// 型付きExpressionから値を読み出すFieldDefinition実装。
/// </summary>
public sealed class FieldDefinition<TModel, TValue> : FieldDefinition
{
    private readonly Func<TModel, TValue> _getter;

    public FieldDefinition(string propertyPath, Func<TModel, TValue> getter)
        : base(propertyPath)
    {
        _getter = getter;
    }

    /// <summary>
    /// 定義元の型にキャストできる場合だけ値を読み出す。
    /// </summary>
    public override object? ReadValue(object model)
    {
        return model is TModel typedModel ? _getter(typedModel) : null;
    }
}