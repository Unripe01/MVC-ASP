using ASP_MVC.Entities;
using ASP_MVC.Repository;
using ASP_MVC.ViewModels;

namespace ASP_MVC.Services;

/// <summary>
/// Company / User / Favorite の通常CRUD画面を組み立てるService。
/// </summary>
public class MasterCrudService
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly IUserRepository _userRepository;

    public MasterCrudService(
        ICompanyRepository companyRepository,
        IFavoriteRepository favoriteRepository,
        IUserRepository userRepository)
    {
        _companyRepository = companyRepository;
        _favoriteRepository = favoriteRepository;
        _userRepository = userRepository;
    }

    /// <summary>
    /// Company CRUD画面を表示用モデルへ組み立てる。
    /// </summary>
    public CrudCompanyPageViewModel BuildCompanyPage(int? editId = null, CrudCompanyInputViewModel? draft = null, string? message = null)
    {
        var companies = _companyRepository.GetAll();
        var input = draft ?? BuildCompanyInput(editId);

        return new CrudCompanyPageViewModel
        {
            Companies = companies,
            Input = input,
            Message = message
        };
    }

    /// <summary>
    /// Companyの追加または更新を実行する。
    /// </summary>
    public string SaveCompany(CrudCompanyInputViewModel input)
    {
        var normalizedName = (input.CompanyName ?? "").Trim();

        if (input.Id == 0)
        {
            _companyRepository.Add(new Company { CompanyName = normalizedName });
            return "会社を登録しました。";
        }

        _companyRepository.Update(new Company
        {
            Id = input.Id,
            CompanyName = normalizedName
        });

        return "会社を更新しました。";
    }

    /// <summary>
    /// Company削除を実行する。紐づくUserがある場合は削除しない。
    /// </summary>
    public string DeleteCompany(int id)
    {
        var hasLinkedUsers = _userRepository.GetAll().Any(user => user.CompanyId == id);
        if (hasLinkedUsers)
        {
            return "この会社を参照するユーザーがいるため削除できません。";
        }

        _companyRepository.Delete(id);
        return "会社を削除しました。";
    }

    /// <summary>
    /// User CRUD画面を表示用モデルへ組み立てる。
    /// </summary>
    public CrudUserPageViewModel BuildUserPage(int? editId = null, CrudUserInputViewModel? draft = null, string? message = null)
    {
        var users = _userRepository.GetAll();
        var companies = _companyRepository.GetAll();
        var input = draft ?? BuildUserInput(editId, companies);

        return new CrudUserPageViewModel
        {
            Users = users,
            Companies = BuildCompanyOptions(companies, input.CompanyId),
            Input = input,
            Message = message
        };
    }

    /// <summary>
    /// Userの追加または更新を実行する。
    /// </summary>
    public string SaveUser(CrudUserInputViewModel input)
    {
        var normalizedName = (input.UserName ?? "").Trim();

        var user = new User
        {
            Id = input.Id,
            CompanyId = input.CompanyId,
            UserName = normalizedName
        };

        if (input.Id == 0)
        {
            _userRepository.Add(user);
            return "ユーザーを登録しました。";
        }

        _userRepository.Update(user);
        return "ユーザーを更新しました。";
    }

    /// <summary>
    /// User削除を実行し、関連Favoriteも同時に削除する。
    /// </summary>
    public string DeleteUser(int id)
    {
        _favoriteRepository.DeleteByUserId(id);
        _userRepository.Delete(id);
        return "ユーザーを削除しました。";
    }

    /// <summary>
    /// Favorite CRUD画面を表示用モデルへ組み立てる。
    /// </summary>
    public CrudFavoritePageViewModel BuildFavoritePage(int? editId = null, CrudFavoriteInputViewModel? draft = null, string? message = null)
    {
        var users = _userRepository.GetAll();
        var favorites = _favoriteRepository.GetAll();
        var input = draft ?? BuildFavoriteInput(editId, favorites, users);
        var userMap = users.ToDictionary(user => user.Id, user => user.UserName);

        return new CrudFavoritePageViewModel
        {
            Favorites = favorites
                .Select(favorite => new CrudFavoriteListItemViewModel
                {
                    Id = favorite.Id,
                    UserId = favorite.UserId,
                    FavoriteName = favorite.FavoriteName,
                    UserName = userMap.TryGetValue(favorite.UserId, out var userName) ? userName : "(不明なユーザー)"
                })
                .ToList(),
            Users = BuildUserOptions(users, input.UserId),
            Input = input,
            Message = message
        };
    }

    /// <summary>
    /// Favoriteの追加または更新を実行する。
    /// </summary>
    public string SaveFavorite(CrudFavoriteInputViewModel input)
    {
        var normalizedName = (input.FavoriteName ?? "").Trim();

        var favorite = new Favorite
        {
            Id = input.Id,
            UserId = input.UserId,
            FavoriteName = normalizedName
        };

        if (input.Id == 0)
        {
            _favoriteRepository.Add(favorite);
            return "好きなものを登録しました。";
        }

        _favoriteRepository.Update(favorite);
        return "好きなものを更新しました。";
    }

    /// <summary>
    /// Favorite削除を実行する。
    /// </summary>
    public string DeleteFavorite(int id)
    {
        _favoriteRepository.Delete(id);
        return "好きなものを削除しました。";
    }

    private CrudCompanyInputViewModel BuildCompanyInput(int? editId)
    {
        if (editId is null)
        {
            return new CrudCompanyInputViewModel();
        }

        var company = _companyRepository.Get(editId.Value);
        if (company is null)
        {
            return new CrudCompanyInputViewModel();
        }

        return new CrudCompanyInputViewModel
        {
            Id = company.Id,
            CompanyName = company.CompanyName
        };
    }

    private CrudUserInputViewModel BuildUserInput(int? editId, IReadOnlyList<Company> companies)
    {
        if (editId is null)
        {
            return new CrudUserInputViewModel
            {
                CompanyId = companies.FirstOrDefault()?.Id ?? 0
            };
        }

        var user = _userRepository.Get(editId.Value);
        if (user is null)
        {
            return new CrudUserInputViewModel
            {
                CompanyId = companies.FirstOrDefault()?.Id ?? 0
            };
        }

        return new CrudUserInputViewModel
        {
            Id = user.Id,
            CompanyId = user.CompanyId,
            UserName = user.UserName
        };
    }

    private CrudFavoriteInputViewModel BuildFavoriteInput(
        int? editId,
        IReadOnlyList<Favorite> favorites,
        IReadOnlyList<User> users)
    {
        if (editId is null)
        {
            return new CrudFavoriteInputViewModel
            {
                UserId = users.FirstOrDefault()?.Id ?? 0
            };
        }

        var favorite = favorites.FirstOrDefault(item => item.Id == editId.Value);
        if (favorite is null)
        {
            return new CrudFavoriteInputViewModel
            {
                UserId = users.FirstOrDefault()?.Id ?? 0
            };
        }

        return new CrudFavoriteInputViewModel
        {
            Id = favorite.Id,
            UserId = favorite.UserId,
            FavoriteName = favorite.FavoriteName
        };
    }

    private static IReadOnlyList<CrudSelectOptionViewModel> BuildCompanyOptions(
        IEnumerable<Company> companies,
        int selectedCompanyId)
    {
        return companies
            .Select(company => new CrudSelectOptionViewModel
            {
                Value = company.Id.ToString(),
                Text = company.CompanyName,
                Selected = company.Id == selectedCompanyId
            })
            .ToList();
    }

    private static IReadOnlyList<CrudSelectOptionViewModel> BuildUserOptions(
        IEnumerable<User> users,
        int selectedUserId)
    {
        return users
            .Select(user => new CrudSelectOptionViewModel
            {
                Value = user.Id.ToString(),
                Text = user.UserName,
                Selected = user.Id == selectedUserId
            })
            .ToList();
    }
}
