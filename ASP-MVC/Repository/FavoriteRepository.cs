using System.Data;
using ASP_MVC.Entities;
using Dapper;

namespace ASP_MVC.Repository;

/// <summary>
/// DapperでFavoritesテーブルを読み書きするRepository。
/// </summary>
public class FavoriteRepository : IFavoriteRepository
{
    private readonly IDbConnection _connection;

    public FavoriteRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    /// <summary>
    /// すべての好きなものをユーザー名付きで取得する。
    /// </summary>
    public List<Favorite> GetAll()
    {
        const string sql = @"
            SELECT f.Id, f.UserId, f.FavoriteName
            FROM Favorites f
            ORDER BY f.Id;";

        return _connection.Query<Favorite>(sql).ToList();
    }

    /// <summary>
    /// ユーザーに紐づく好きなものを取得する。
    /// </summary>
    public List<Favorite> GetByUserId(int userId)
    {
        const string sql = @"
            SELECT Id, UserId, FavoriteName
            FROM Favorites
            WHERE UserId = @UserId
            ORDER BY Id;";

        return _connection.Query<Favorite>(sql, new { UserId = userId }).ToList();
    }

    /// <summary>
    /// 好きなものを新規登録する。
    /// </summary>
    public Favorite Add(Favorite favorite)
    {
        const string sql = @"
            INSERT INTO Favorites (UserId, FavoriteName)
            VALUES (@UserId, @FavoriteName)
            RETURNING Id;";

        favorite.Id = _connection.ExecuteScalar<int>(sql, new
        {
            favorite.UserId,
            FavoriteName = (favorite.FavoriteName ?? "").Trim()
        });

        return favorite;
    }

    /// <summary>
    /// 好きなものを更新する。
    /// </summary>
    public void Update(Favorite favorite)
    {
        const string sql = @"
            UPDATE Favorites
            SET UserId = @UserId,
                FavoriteName = @FavoriteName
            WHERE Id = @Id;";

        _connection.Execute(sql, new
        {
            favorite.Id,
            favorite.UserId,
            FavoriteName = (favorite.FavoriteName ?? "").Trim()
        });
    }

    /// <summary>
    /// 指定IDの好きなものを削除する。
    /// </summary>
    public void Delete(int id)
    {
        _connection.Execute("DELETE FROM Favorites WHERE Id = @Id;", new { Id = id });
    }

    /// <summary>
    /// 指定ユーザーに紐づく好きなものをすべて削除する。
    /// </summary>
    public void DeleteByUserId(int userId)
    {
        _connection.Execute("DELETE FROM Favorites WHERE UserId = @UserId;", new { UserId = userId });
    }

    /// <summary>
    /// 帳票入力で受け取った1:N項目をユーザー単位で差し替える。
    /// </summary>
    public void ReplaceForUser(int userId, IEnumerable<Favorite> favorites)
    {
        DeleteByUserId(userId);

        const string sql = @"
            INSERT INTO Favorites (UserId, FavoriteName)
            VALUES (@UserId, @FavoriteName);";

        foreach (var favorite in favorites.Where(favorite => !string.IsNullOrWhiteSpace(favorite.FavoriteName)))
        {
            _connection.Execute(sql, new
            {
                UserId = userId,
                FavoriteName = favorite.FavoriteName.Trim()
            });
        }
    }
}