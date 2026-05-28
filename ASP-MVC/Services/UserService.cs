using ASP_MVC.Entities;
using ASP_MVC.Repository;
using ASP_MVC.ViewModels;

namespace ASP_MVC.Services;

/// <summary>
/// Userを中心にCompany/FavoriteのEntityグラフを組み立てるService。
/// </summary>
public class UserService
{
    private readonly CompanyRepository _companyRepository;
    private readonly FavoriteRepository _favoriteRepository;
    private readonly UserRepository _userRepository;

    public UserService(
        CompanyRepository companyRepository,
        FavoriteRepository favoriteRepository,
        UserRepository userRepository)
    {
        _companyRepository = companyRepository;
        _favoriteRepository = favoriteRepository;
        _userRepository = userRepository;
    }

    /// <summary>
    /// 帳票Projectionに必要なCompanyとFavoriteを含めてUserを取得する。
    /// </summary>
    public User? GetUserGraph(int userId)
    {
        var user = _userRepository.Get(userId);
        if (user is null)
        {
            return null;
        }

        user.Favorites = _favoriteRepository.GetByUserId(user.Id);
        return user;
    }

    /// <summary>
    /// 帳票入力値を保存せずにプレビュー用Entityグラフへ変換する。
    /// </summary>
    public User BuildTransientUser(UserFavoriteReportPostViewModel input)
    {
        var company = _companyRepository.Get(input.CompanyId);

        return new User
        {
            Id = input.UserId,
            CompanyId = input.CompanyId,
            UserName = input.UserName ?? "",
            Company = company,
            Favorites = BuildFavorites(input.UserId, input.FavoriteNames)
        };
    }

    /// <summary>
    /// 帳票入力値をUser/Favoriteへ逆反映して保存する。
    /// </summary>
    public User SaveUserFavorite(UserFavoriteReportPostViewModel input)
    {
        var user = new User
        {
            Id = input.UserId,
            CompanyId = input.CompanyId,
            UserName = (input.UserName ?? "").Trim()
        };

        if (user.Id == 0)
        {
            _userRepository.Add(user);
        }
        else
        {
            _userRepository.Update(user);
        }

        _favoriteRepository.ReplaceForUser(user.Id, BuildFavorites(user.Id, input.FavoriteNames));
        return GetUserGraph(user.Id) ?? user;
    }

    private static List<Favorite> BuildFavorites(int userId, IEnumerable<string> favoriteNames)
    {
        return favoriteNames
            .Where(favoriteName => !string.IsNullOrWhiteSpace(favoriteName))
            .Select(favoriteName => new Favorite
            {
                UserId = userId,
                FavoriteName = favoriteName.Trim()
            })
            .ToList();
    }
}