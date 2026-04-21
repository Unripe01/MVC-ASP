using ASP_MVC.Models;

namespace ASP_MVC.Repositories
{
    public class UserRepository : IUserRepository
    {
        public List<User> GetAll()
        {
            return
            [
                new() { Id = 1, Name = "太郎" },
                new() { Id = 2, Name = "次郎" }
            ];
        }

    }
}
