using ASP_MVC.Entities;

namespace ASP_MVC.Repository;

/// <summary>
/// 企業マスターの取得と保存だけを担当するRepository契約。
/// </summary>
public interface ICompanyRepository
{
    List<Company> GetAll();

    Company? Get(int id);

    Company Add(Company company);

    void Update(Company company);
}