using System.Xml.Serialization;

namespace ASP_MVC.Services;

/// <summary>
/// 帳票Snapshot用XMLのserialize / deserializeを担当するService。
/// </summary>
public class ReportXmlService
{
    /// <summary>
    /// 帳票投影対象のEntityグラフをXML文字列へ変換する。
    /// </summary>
    public string Serialize<TValue>(TValue value)
    {
        var serializer = new XmlSerializer(typeof(TValue));
        using var writer = new StringWriter();
        serializer.Serialize(writer, value);
        return writer.ToString();
    }

    /// <summary>
    /// 保存済みXML文字列を型付きオブジェクトへ戻す。
    /// </summary>
    public TValue? Deserialize<TValue>(string xmlData)
    {
        if (string.IsNullOrWhiteSpace(xmlData))
        {
            return default;
        }

        var serializer = new XmlSerializer(typeof(TValue));
        using var reader = new StringReader(xmlData);
        return (TValue?)serializer.Deserialize(reader);
    }
}