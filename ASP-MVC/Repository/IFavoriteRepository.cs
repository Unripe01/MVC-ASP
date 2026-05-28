using ASP_MVC.Entities;

namespace ASP_MVC.Repository;

/// <summary>
/// 好きなものEntityの取得と保存だけを担当するRepository契約。
/// </summary>
public interface IFavoriteRepository
{
    List<Favorite> GetAll();

    List<Favorite> GetByUserId(int userId);

    Favorite Add(Favorite favorite);

    void Update(Favorite favorite);

    void Delete(int id);

    void DeleteByUserId(int userId);

    void ReplaceForUser(int userId, IEnumerable<Favorite> favorites);
}