using System.Data;
using ASP_MVC.Entities;
using Dapper;

namespace ASP_MVC.Repository;

/// <summary>
/// DapperでUsersテーブルを読み書きするRepository。
/// </summary>
public class UserRepository
{
    private readonly IDbConnection _connection;

    public UserRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    /// <summary>
    /// 一覧表示用に会社名を含めてユーザーを取得する。
    /// </summary>
    public List<User> GetAll()
    {
        const string sql = @"
            SELECT
                u.Id,
                u.CompanyId,
                u.UserName,
                u.Nationality,
                u.Age,
                u.BloodType,
                u.Birthday,
                c.Id AS CompanyRecordId,
                c.CompanyName
            FROM Users u
            LEFT JOIN Companies c ON c.Id = u.CompanyId
            ORDER BY u.Id;";

        return _connection.Query<UserRow>(sql)
            .Select(MapUser)
            .ToList();
    }

    /// <summary>
    /// 指定IDのユーザーを会社情報込みで取得する。
    /// </summary>
    public User? Get(int id)
    {
        const string sql = @"
            SELECT
                u.Id,
                u.CompanyId,
                u.UserName,
                u.Nationality,
                u.Age,
                u.BloodType,
                u.Birthday,
                c.Id AS CompanyRecordId,
                c.CompanyName
            FROM Users u
            LEFT JOIN Companies c ON c.Id = u.CompanyId
            WHERE u.Id = @Id;";

        var row = _connection.QuerySingleOrDefault<UserRow>(sql, new { Id = id });
        return row is null ? null : MapUser(row);
    }

    /// <summary>
    /// ユーザーを新規登録する。
    /// </summary>
    public void Add(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var companyId = user.CompanyId == 0 ? GetDefaultCompanyId() : user.CompanyId;
        var userName = (user.UserName ?? "").Trim();

        const string sql = @"
            INSERT INTO Users (CompanyId, UserName, Nationality, Age, BloodType, Birthday)
            VALUES (@CompanyId, @UserName, @Nationality, @Age, @BloodType, @Birthday)
            RETURNING Id;";

        user.Id = _connection.ExecuteScalar<int>(sql, new
        {
            CompanyId = companyId,
            UserName = userName,
            Nationality = (user.Nationality ?? "").Trim(),
            Age = (user.Age ?? "").Trim(),
            BloodType = (user.BloodType ?? "").Trim(),
            Birthday = (user.Birthday ?? "").Trim()
        });
        user.CompanyId = companyId;
        user.UserName = userName;
    }

    /// <summary>
    /// ユーザーの基本項目を更新する。
    /// </summary>
    public void Update(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        const string sql = @"
            UPDATE Users
            SET CompanyId = @CompanyId,
                UserName = @UserName,
                Nationality = @Nationality,
                Age = @Age,
                BloodType = @BloodType,
                Birthday = @Birthday
            WHERE Id = @Id;";

        _connection.Execute(sql, new
        {
            user.Id,
            user.CompanyId,
            UserName = (user.UserName ?? "").Trim(),
            Nationality = (user.Nationality ?? "").Trim(),
            Age = (user.Age ?? "").Trim(),
            BloodType = (user.BloodType ?? "").Trim(),
            Birthday = (user.Birthday ?? "").Trim()
        });
    }

    /// <summary>
    /// 指定IDのユーザーを削除する。
    /// </summary>
    public void Delete(int id)
    {
        const string sql = "DELETE FROM Users WHERE Id = @Id;";
        _connection.Execute(sql, new { Id = id });
    }

    private int GetDefaultCompanyId()
    {
        var companyId = _connection.ExecuteScalar<int?>("SELECT Id FROM Companies ORDER BY Id LIMIT 1;");
        if (companyId is not null)
        {
            return companyId.Value;
        }

        return _connection.ExecuteScalar<int>(@"
            INSERT INTO Companies (CompanyName)
            VALUES (@CompanyName)
            RETURNING Id;", new { CompanyName = "サンプル企業" });
    }

    private static User MapUser(UserRow row)
    {
        return new User
        {
            Id = row.Id,
            CompanyId = row.CompanyId,
            UserName = row.UserName,
            Nationality = row.Nationality,
            Age = row.Age,
            BloodType = row.BloodType,
            Birthday = row.Birthday,
            Company = row.CompanyRecordId is null
                ? null
                : new Company
                {
                    Id = row.CompanyRecordId.Value,
                    CompanyName = row.CompanyName ?? ""
                }
        };
    }

    private sealed class UserRow
    {
        public int Id { get; set; }

        public int CompanyId { get; set; }

        public string UserName { get; set; } = "";

        public string Nationality { get; set; } = "";

        public string Age { get; set; } = "";

        public string BloodType { get; set; } = "";

        public string Birthday { get; set; } = "";

        public int? CompanyRecordId { get; set; }

        
        public string? CompanyName { get; set; }
    }
}