using ASP_MVC.Entities;

namespace ASP_MVC.Repository;

/// <summary>
/// 帳票XMLスナップショットの保存と参照を担当するRepository契約。
/// </summary>
public interface IReportInstanceRepository
{
    void Add(ReportInstance reportInstance);

    List<ReportInstance> GetByUserId(int userId);
}