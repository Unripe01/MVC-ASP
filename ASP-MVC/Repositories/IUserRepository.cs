using ASP_MVC.Models;

namespace ASP_MVC.Repositories
{
    public interface IUserRepository
    {
        public List<User> GetAll();
    }
}
