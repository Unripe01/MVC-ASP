using ASP_MVC.Models;

namespace ASP_MVC.Repositories
{
    public interface IUserRepository
    {
        List<User> GetAll();
        void Add(User user);
    }
}
