using System.Data;
using ASP_MVC.Entities;
using Dapper;

namespace ASP_MVC.Repository;

/// <summary>
/// DapperでReportInstancesテーブルを読み書きするRepository。
/// </summary>
public class ReportInstanceRepository
{
    private readonly IDbConnection _connection;

    public ReportInstanceRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    /// <summary>
    /// 帳票XMLスナップショットを追加保存する。
    /// </summary>
    public void Add(ReportInstance reportInstance)
    {
        const string sql = @"
            INSERT INTO ReportInstances (UserId, ReportType, XmlData, CreatedAt)
            VALUES (@UserId, @ReportType, @XmlData, @CreatedAt);";

        _connection.Execute(sql, reportInstance);
    }

    /// <summary>
    /// ユーザーに紐づく保存済み帳票スナップショットを取得する。
    /// </summary>
    public List<ReportInstance> GetByUserId(int userId)
    {
        const string sql = @"
            SELECT Id, UserId, ReportType, XmlData, CreatedAt
            FROM ReportInstances
            WHERE UserId = @UserId
            ORDER BY CreatedAt DESC;";

        return _connection.Query<ReportInstance>(sql, new { UserId = userId }).ToList();
    }
}