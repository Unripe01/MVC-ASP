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
    /// POST入力をマスター非依存の帳票用Entityグラフへ変換する。
    /// </summary>
    public User BuildReportUser(UserFavoriteReportPostViewModel input)
    {
        return new User
        {
            Id = input.UserId,
            CompanyId = input.CompanyId,
            UserName = input.UserName ?? "",
            Nationality = input.Nationality ?? "",
            Age = input.Age ?? "",
            BloodType = input.BloodType ?? "",
            Birthday = input.Birthday ?? "",
            Company = new Company
            {
                Id = input.CompanyId,
                CompanyName = input.CompanyName ?? ""
            },
            Favorites = BuildFavorites(input.UserId, input.FavoriteIds, input.FavoriteNames)
        };
    }

    /// <summary>
    /// 帳票入力値を保存せずにプレビュー用Entityグラフへ変換する。
    /// </summary>
    public User BuildTransientUser(UserFavoriteReportPostViewModel input)
    {
        return BuildReportUser(input);
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

    /// <summary>
    /// 選択された項目だけをマスターへ逆反映し、反映後のUserグラフを返す。
    /// </summary>
    public User ReverseReflect(User reportUser, IReadOnlySet<string> selectedFieldIds)
    {
        var currentMasterUser = reportUser.Id == 0 ? null : _userRepository.Get(reportUser.Id);
        var shouldReflectUserName = selectedFieldIds.Contains("UserName");
        var shouldReflectNationality = selectedFieldIds.Contains("Nationality");
        var shouldReflectAge = selectedFieldIds.Contains("Age");
        var shouldReflectBloodType = selectedFieldIds.Contains("BloodType");
        var shouldReflectBirthday = selectedFieldIds.Contains("Birthday");
        var shouldReflectCompany = selectedFieldIds.Contains("Company_CompanyName");
        var shouldReflectFavorites = selectedFieldIds.Contains("Favorites");

        if (!shouldReflectUserName && !shouldReflectNationality && !shouldReflectAge && !shouldReflectBloodType && !shouldReflectBirthday && !shouldReflectCompany && !shouldReflectFavorites)
        {
            return NormalizeReportUser(reportUser);
        }

        var userToSave = new User
        {
            Id = currentMasterUser?.Id ?? reportUser.Id,
            CompanyId = currentMasterUser?.CompanyId ?? reportUser.CompanyId,
            UserName = currentMasterUser?.UserName ?? reportUser.UserName,
            Nationality = currentMasterUser?.Nationality ?? reportUser.Nationality,
            Age = currentMasterUser?.Age ?? reportUser.Age,
            BloodType = currentMasterUser?.BloodType ?? reportUser.BloodType,
            Birthday = currentMasterUser?.Birthday ?? reportUser.Birthday
        };

        if (shouldReflectUserName)
        {
            userToSave.UserName = (reportUser.UserName ?? "").Trim();
        }

        if (shouldReflectCompany)
        {
            userToSave.CompanyId = reportUser.CompanyId;
        }

        if (shouldReflectNationality)
        {
            userToSave.Nationality = (reportUser.Nationality ?? "").Trim();
        }

        if (shouldReflectAge)
        {
            userToSave.Age = (reportUser.Age ?? "").Trim();
        }

        if (shouldReflectBloodType)
        {
            userToSave.BloodType = (reportUser.BloodType ?? "").Trim();
        }

        if (shouldReflectBirthday)
        {
            userToSave.Birthday = (reportUser.Birthday ?? "").Trim();
        }

        if (userToSave.Id == 0)
        {
            _userRepository.Add(userToSave);
        }
        else
        {
            _userRepository.Update(userToSave);
        }

        if (shouldReflectFavorites)
        {
            _favoriteRepository.ReplaceForUser(userToSave.Id, reportUser.Favorites.Select(favorite => new Favorite
            {
                UserId = userToSave.Id,
                FavoriteName = favorite.FavoriteName
            }));
        }

        return GetUserGraph(userToSave.Id) ?? NormalizeReportUser(reportUser);
    }

    /// <summary>
    /// XML deserialize後や空画面でも安全に扱えるUserグラフへ整形する。
    /// </summary>
    public static User NormalizeReportUser(User user)
    {
        user.Company ??= new Company();
        user.Favorites ??= [];
        return user;
    }

    private static List<Favorite> BuildFavorites(int userId, IEnumerable<string> favoriteNames)
    {
        return BuildFavorites(userId, Enumerable.Repeat(0, favoriteNames.Count()), favoriteNames);
    }

    private static List<Favorite> BuildFavorites(int userId, IEnumerable<int> favoriteIds, IEnumerable<string> favoriteNames)
    {
        var favoriteIdList = favoriteIds.ToList();
        var favoriteNameList = favoriteNames.ToList();

        return favoriteNameList
            .Select((favoriteName, index) => new Favorite
            {
                Id = index < favoriteIdList.Count ? favoriteIdList[index] : 0,
                UserId = userId,
                FavoriteName = (favoriteName ?? "").Trim()
            })
            .Where(favorite => !string.IsNullOrWhiteSpace(favorite.FavoriteName))
            .ToList();
    }
}