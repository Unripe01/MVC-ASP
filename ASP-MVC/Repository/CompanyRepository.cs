using System.Data;
using ASP_MVC.Entities;
using Dapper;

namespace ASP_MVC.Repository;

/// <summary>
/// DapperでCompaniesテーブルを読み書きするRepository。
/// </summary>
public class CompanyRepository : ICompanyRepository
{
    private readonly IDbConnection _connection;

    public CompanyRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    /// <summary>
    /// 選択肢表示用に企業マスターを全件取得する。
    /// </summary>
    public List<Company> GetAll()
    {
        const string sql = "SELECT Id, CompanyName FROM Companies ORDER BY Id;";
        return _connection.Query<Company>(sql).ToList();
    }

    /// <summary>
    /// 指定IDの企業マスターを取得する。
    /// </summary>
    public Company? Get(int id)
    {
        const string sql = "SELECT Id, CompanyName FROM Companies WHERE Id = @Id;";
        return _connection.QuerySingleOrDefault<Company>(sql, new { Id = id });
    }

    /// <summary>
    /// 企業マスターを新規登録する。
    /// </summary>
    public Company Add(Company company)
    {
        const string sql = @"
            INSERT INTO Companies (CompanyName)
            VALUES (@CompanyName)
            RETURNING Id;";

        company.Id = _connection.ExecuteScalar<int>(sql, company);
        return company;
    }

    /// <summary>
    /// 企業マスターの名称を更新する。
    /// </summary>
    public void Update(Company company)
    {
        const string sql = @"
            UPDATE Companies
            SET CompanyName = @CompanyName
            WHERE Id = @Id;";

        _connection.Execute(sql, company);
    }

    /// <summary>
    /// 指定IDの企業マスターを削除する。
    /// </summary>
    public void Delete(int id)
    {
        const string sql = "DELETE FROM Companies WHERE Id = @Id;";
        _connection.Execute(sql, new { Id = id });
    }
}