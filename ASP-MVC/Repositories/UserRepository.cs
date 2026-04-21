using System.Data;
using ASP_MVC.Models;
using Dapper;

namespace ASP_MVC.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnection _connection;
        public UserRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public List<User> GetAll()
        {

            //dapperを使わずにSQLを実行する例
            // var users = new List<User>();
            // var command = _connection.CreateCommand();
            // command.CommandText = "SELECT Id, Name FROM Users";
            // _connection.Open();
            // using (var reader = command.ExecuteReader())
            // {
            //     while (reader.Read())
            //     {
            //         users.Add(new User
            //         {
            //             Id = reader.GetInt32(0),
            //             Name = reader.GetString(1)
            //         });
            //     }
            // }
            // _connection.Close();
            // return users;
            
            //dapperを使ってSQLを実行する例
            var sql = "SELECT Id, Name FROM Users";
            return _connection.Query<User>(sql).ToList();
        }

        public void Add(User user)
        {
            var sql = "INSERT INTO Users (Name) VALUES (@Name)";
            _connection.Execute(sql, user);
        }
    }
}
