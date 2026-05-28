using ASP_MVC.Entities;
using ASP_MVC.Reports;
using ASP_MVC.ViewModels;

namespace ASP_MVC.Services;

/// <summary>
/// UserInfo帳票の処理を共通Workflow契約で提供する。
/// </summary>
public class UserInfoReportWorkflow : IReportWorkflow
{
    private static readonly StringComparer FieldComparer = StringComparer.OrdinalIgnoreCase;

    private readonly ReportDocumentService _reportDocumentService;
    private readonly UserService _userService;
    private readonly UserInfoReportDefinition _definition;

    public UserInfoReportWorkflow(
        ReportDocumentService reportDocumentService,
        UserService userService,
        UserInfoReportDefinition definition)
    {
        _reportDocumentService = reportDocumentService;
        _userService = userService;
        _definition = definition;
    }

    public string ReportKey => _definition.ReportKey;

    public string ReportTitle => _definition.DisplayTitle;

    public string? Summary => _definition.Summary;

    public ReportWorkspaceViewModel Open(int userId)
    {
        var snapshot = _reportDocumentService.GetLatestSnapshot(userId, ReportKey);
        var document = RestoreDocument(snapshot?.XmlData, userId);

        return BuildWorkspace(document, savedXmlData: snapshot?.XmlData ?? "");
    }

    public ReportWorkspaceViewModel Save(UserFavoriteReportPostViewModel input)
    {
        var currentSnapshot = _reportDocumentService.GetLatestSnapshot(input.UserId, ReportKey);
        var existingDocument = currentSnapshot is null
            ? null
            : RestoreDocument(currentSnapshot.XmlData, input.UserId);
        var document = BuildDocument(input, existingDocument);

        var savedXmlData = _reportDocumentService.SaveSnapshot(ReportKey, input.UserId, document);
        return BuildWorkspace(document, message: "XMLを保存しました", savedXmlData: savedXmlData);
    }

    public string Preview(UserFavoriteReportPostViewModel input)
    {
        var document = BuildDocument(input);
        return _reportDocumentService.RenderPreview(_definition, document);
    }

    public ReportWorkspaceViewModel FetchMaster(UserFavoriteReportPostViewModel input)
    {
        var selectedFieldIds = ToSelectedFieldSet(input.SelectedFieldIds);
        var allowedFieldIds = ToAllowedFieldSet(field => field.IsFromMaster);
        selectedFieldIds.IntersectWith(allowedFieldIds);

        var document = BuildDocument(input);
        var masterUser = _userService.GetUserGraph(input.UserId) ?? document.Model ?? CreateEmptyUser(input.UserId);
        var snapshot = _reportDocumentService.GetLatestSnapshot(input.UserId, ReportKey);

        if (selectedFieldIds.Count == 0)
        {
            return BuildWorkspace(
                document,
                message: "マスター取得対象を選択してください",
                selectedFieldIds: input.SelectedFieldIds,
                savedXmlData: snapshot?.XmlData ?? "");
        }

        ApplyMasterValues(document.Model!, masterUser, selectedFieldIds);

        return BuildWorkspace(
            document,
            message: "選択項目をマスターから取得しました",
            selectedFieldIds: input.SelectedFieldIds,
            savedXmlData: snapshot?.XmlData ?? "");
    }

    public ReportWorkspaceViewModel ReverseReflect(UserFavoriteReportPostViewModel input)
    {
        var selectedFieldIds = ToSelectedFieldSet(input.SelectedFieldIds);
        var allowedFieldIds = ToAllowedFieldSet(field => field.AllowReverseReflection);
        selectedFieldIds.IntersectWith(allowedFieldIds);

        var document = BuildDocument(input);
        var user = document.Model ?? CreateEmptyUser(input.UserId);
        var snapshot = _reportDocumentService.GetLatestSnapshot(input.UserId, ReportKey);

        if (selectedFieldIds.Count == 0)
        {
            return BuildWorkspace(
                document,
                message: "逆反映対象を選択してください",
                selectedFieldIds: input.SelectedFieldIds,
                savedXmlData: snapshot?.XmlData ?? "");
        }

        document.Model = UserService.NormalizeReportUser(_userService.ReverseReflect(user, selectedFieldIds));

        return BuildWorkspace(
            document,
            message: "選択項目をマスターへ逆反映しました",
            selectedFieldIds: input.SelectedFieldIds,
            savedXmlData: snapshot?.XmlData ?? "");
    }

    public ReportWorkspaceViewModel Export(int userId)
    {
        var snapshot = _reportDocumentService.GetLatestSnapshot(userId, ReportKey)
            ?? throw new InvalidOperationException("出力対象の保存済み帳票が見つかりません。");

        var document = RestoreDocument(snapshot.XmlData, userId);
        var user = document.Model ?? CreateEmptyUser(userId);
        var fileName = $"{Guid.NewGuid():N}-user-info-{user.Id}.txt";
        _reportDocumentService.Write(_definition, document, fileName);

        return BuildWorkspace(
            document,
            message: $"{fileName} を出力しました",
            savedXmlData: snapshot.XmlData);
    }

    private ReportWorkspaceViewModel BuildWorkspace(
        ReportDocument<User> document,
        string? message = null,
        IEnumerable<string>? selectedFieldIds = null,
        string? savedXmlData = null)
    {
        return _reportDocumentService.BuildWorkspace(
            _definition,
            document,
            reportKey: ReportKey,
            reportTitle: ReportTitle,
            message: message,
            selectedFieldIds: selectedFieldIds,
            savedXmlData: savedXmlData);
    }

    private static ReportDocument<User> BuildDocument(
        UserFavoriteReportPostViewModel input,
        ReportDocument<User>? existingDocument = null)
    {
        var document = existingDocument ?? ReportDocument<User>.FromModel(CreateEmptyUser(input.UserId));
        document.Model = UserService.NormalizeReportUser(BuildInfoUser(input));
        document.Values = MergeReportValues(document.Values, BuildReportValues(input));
        return document;
    }

    private ReportDocument<User> RestoreDocument(string? xmlData, int userId)
    {
        if (string.IsNullOrWhiteSpace(xmlData))
        {
            return ReportDocument<User>.FromModel(CreateEmptyUser(userId));
        }

        var document = TryDeserializeDocument(xmlData);
        if (document?.Model is not null)
        {
            document.Model = UserService.NormalizeReportUser(document.Model);
            document.Values ??= [];
            return document;
        }

        var legacyUser = _reportDocumentService.Deserialize<User>(xmlData) ?? CreateEmptyUser(userId);
        return ReportDocument<User>.FromModel(UserService.NormalizeReportUser(legacyUser));
    }

    private ReportDocument<User>? TryDeserializeDocument(string xmlData)
    {
        try
        {
            return _reportDocumentService.Deserialize<ReportDocument<User>>(xmlData);
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private static User BuildInfoUser(UserFavoriteReportPostViewModel input)
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
            Favorites = []
        };
    }

    private static User CreateEmptyUser(int userId)
    {
        return new User
        {
            Id = userId,
            Company = new Company(),
            Favorites = []
        };
    }

    private static IReadOnlyList<ReportFieldValue> BuildReportValues(UserFavoriteReportPostViewModel input)
    {
        return input.ReportValues
            .Where(value => !string.IsNullOrWhiteSpace(value.Key))
            .Select(value => new ReportFieldValue
            {
                Key = value.Key,
                Value = value.Value ?? ""
            })
            .ToList();
    }

    private static List<ReportFieldValue> MergeReportValues(
        IEnumerable<ReportFieldValue>? existingValues,
        IReadOnlyList<ReportFieldValue> incomingValues)
    {
        var merged = (existingValues ?? [])
            .Where(value => !string.IsNullOrWhiteSpace(value.Key))
            .ToDictionary(value => value.Key, value => value.Value ?? "", StringComparer.OrdinalIgnoreCase);

        foreach (var incoming in incomingValues)
        {
            if (string.IsNullOrWhiteSpace(incoming.Key))
            {
                continue;
            }

            merged[incoming.Key] = incoming.Value ?? "";
        }

        return merged
            .Select(pair => new ReportFieldValue
            {
                Key = pair.Key,
                Value = pair.Value
            })
            .ToList();
    }

    private static void ApplyMasterValues(User target, User masterUser, IReadOnlySet<string> selectedFieldIds)
    {
        if (selectedFieldIds.Contains("UserName"))
        {
            target.UserName = masterUser.UserName;
        }

        if (selectedFieldIds.Contains("Nationality"))
        {
            target.Nationality = masterUser.Nationality;
        }

        if (selectedFieldIds.Contains("Age"))
        {
            target.Age = masterUser.Age;
        }

        if (selectedFieldIds.Contains("BloodType"))
        {
            target.BloodType = masterUser.BloodType;
        }

        if (selectedFieldIds.Contains("Birthday"))
        {
            target.Birthday = masterUser.Birthday;
        }

        if (selectedFieldIds.Contains("Company_CompanyName"))
        {
            target.CompanyId = masterUser.CompanyId;
            target.Company = masterUser.Company ?? new Company();
        }
    }

    private static HashSet<string> ToSelectedFieldSet(IEnumerable<string>? selectedFieldIds)
    {
        return selectedFieldIds is null
            ? new HashSet<string>(FieldComparer)
            : new HashSet<string>(selectedFieldIds.Where(value => !string.IsNullOrWhiteSpace(value)), FieldComparer);
    }

    private HashSet<string> ToAllowedFieldSet(Func<FieldDefinition, bool> predicate)
    {
        return _definition.Fields
            .Where(predicate)
            .Select(field => field.FieldId)
            .ToHashSet(FieldComparer);
    }
}