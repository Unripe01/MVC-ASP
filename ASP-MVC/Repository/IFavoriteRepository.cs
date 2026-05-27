using ASP_MVC.Entities;

namespace ASP_MVC.Repository;

/// <summary>
/// 好きなものEntityの取得と保存だけを担当するRepository契約。
/// </summary>
public interface IFavoriteRepository
{
    List<Favorite> GetByUserId(int userId);

    void ReplaceForUser(int userId, IEnumerable<Favorite> favorites);
}