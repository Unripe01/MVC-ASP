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
    /// 帳票入力で受け取った1:N項目をユーザー単位で差し替える。
    /// </summary>
    public void ReplaceForUser(int userId, IEnumerable<Favorite> favorites)
    {
        _connection.Execute("DELETE FROM Favorites WHERE UserId = @UserId;", new { UserId = userId });

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