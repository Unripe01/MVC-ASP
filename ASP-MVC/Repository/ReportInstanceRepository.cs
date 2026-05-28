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
    /// 帳票XMLスナップショットをユーザー・帳票種別単位で上書き保存する。
    /// </summary>
    public void Upsert(ReportInstance reportInstance)
    {
        var currentTime = DateTime.UtcNow;

        const string selectSql = @"
            SELECT Id
            FROM ReportInstances
            WHERE UserId = @UserId AND ReportType = @ReportType
            ORDER BY Id DESC
            LIMIT 1;";

        var existingId = _connection.ExecuteScalar<int?>(selectSql, new
        {
            reportInstance.UserId,
            reportInstance.ReportType
        });

        if (existingId is null)
        {
            const string insertSql = @"
                INSERT INTO ReportInstances (UserId, ReportType, XmlData, CreatedAt, UpdatedAt)
                VALUES (@UserId, @ReportType, @XmlData, @CreatedAt, @UpdatedAt);";

            _connection.Execute(insertSql, new
            {
                reportInstance.UserId,
                reportInstance.ReportType,
                reportInstance.XmlData,
                CreatedAt = reportInstance.CreatedAt == default ? currentTime : reportInstance.CreatedAt,
                UpdatedAt = currentTime
            });
            return;
        }

        const string updateSql = @"
            UPDATE ReportInstances
            SET XmlData = @XmlData,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id;";

        _connection.Execute(updateSql, new
        {
            Id = existingId.Value,
            reportInstance.XmlData,
            UpdatedAt = currentTime
        });

        const string deleteDuplicatesSql = @"
            DELETE FROM ReportInstances
            WHERE UserId = @UserId
              AND ReportType = @ReportType
              AND Id <> @KeepId;";

        _connection.Execute(deleteDuplicatesSql, new
        {
            reportInstance.UserId,
            reportInstance.ReportType,
            KeepId = existingId.Value
        });
    }

    /// <summary>
    /// ユーザーと帳票種別に対応する最新の保存済みXMLを取得する。
    /// </summary>
    public ReportInstance? GetLatest(int userId, string reportType)
    {
        const string sql = @"
            SELECT Id, UserId, ReportType, XmlData, CreatedAt, UpdatedAt
            FROM ReportInstances
            WHERE UserId = @UserId AND ReportType = @ReportType
            ORDER BY COALESCE(UpdatedAt, CreatedAt) DESC, Id DESC
            LIMIT 1;";

        return _connection.QuerySingleOrDefault<ReportInstance>(sql, new
        {
            UserId = userId,
            ReportType = reportType
        });
    }

    /// <summary>
    /// ユーザーに紐づく保存済み帳票スナップショットを取得する。
    /// </summary>
    public List<ReportInstance> GetByUserId(int userId)
    {
        const string sql = @"
            SELECT Id, UserId, ReportType, XmlData, CreatedAt, UpdatedAt
            FROM ReportInstances
            WHERE UserId = @UserId
            ORDER BY CreatedAt DESC;";

        return _connection.Query<ReportInstance>(sql, new { UserId = userId }).ToList();
    }
}