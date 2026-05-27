using ASP_MVC.Entities;

namespace ASP_MVC.Repository;

/// <summary>
/// ユーザーEntityの取得と保存だけを担当するRepository契約。
/// </summary>
public interface IUserRepository
{
    List<User> GetAll();

    User? Get(int id);

    void Add(User user);

    void Update(User user);
}