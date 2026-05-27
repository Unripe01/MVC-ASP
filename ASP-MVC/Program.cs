using Dapper;
using System.Data;
using ASP_MVC.Repository;
using ASP_MVC.Reports;
using ASP_MVC.Services;
using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=app.db";

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();
builder.Services.AddScoped<IReportInstanceRepository, ReportInstanceRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<UserFavoriteReportDefinition>();
builder.Services.AddScoped<ReportEngine>();
builder.Services.AddScoped<ReportRendererService>();
builder.Services.AddScoped<ReportXmlService>();
builder.Services.AddScoped<TemplateRenderService>();
builder.Services.AddScoped<UserService>();
// SQLiteを使うので、AddDbContext ではなく、AddScoped で DbConnection を登録する
builder.Services.AddScoped<IDbConnection>(_ =>
{
    return new SqliteConnection(connectionString);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// 初回用の処理 SQLiteのファイルが存在しない場合は作成する
using (var connection = new SqliteConnection(connectionString))
{
    connection.Open();
    EnsureDatabase(connection);
}


app.Run();

static void EnsureDatabase(IDbConnection connection)
{
    connection.Execute(@"
        CREATE TABLE IF NOT EXISTS Companies (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            CompanyName TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS Favorites (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            UserId INTEGER NOT NULL,
            FavoriteName TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS ReportInstances (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            UserId INTEGER NOT NULL,
            ReportType TEXT NOT NULL,
            XmlData TEXT NOT NULL,
            CreatedAt TEXT NOT NULL
        );
    ");

    EnsureUsersTable(connection);
    SeedDatabase(connection);
}

static void EnsureUsersTable(IDbConnection connection)
{
    var userColumns = connection.Query<string>("SELECT name FROM pragma_table_info('Users');")
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    if (userColumns.Count == 0)
    {
        connection.Execute(@"
            CREATE TABLE Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                CompanyId INTEGER NOT NULL,
                UserName TEXT NOT NULL
            );
        ");
        return;
    }

    if (userColumns.Contains("Name") || !userColumns.Contains("CompanyId") || !userColumns.Contains("UserName"))
    {
        RebuildUsersTable(connection, userColumns);
    }
}

static void RebuildUsersTable(IDbConnection connection, IReadOnlySet<string> userColumns)
{
    var companyIdExpression = userColumns.Contains("CompanyId")
        ? "CASE WHEN CompanyId IS NULL OR CompanyId = 0 THEN 1 ELSE CompanyId END"
        : "1";

    var userNameExpression = (userColumns.Contains("UserName"), userColumns.Contains("Name")) switch
    {
        (true, true) => "COALESCE(NULLIF(UserName, ''), Name, '')",
        (true, false) => "COALESCE(UserName, '')",
        (false, true) => "COALESCE(Name, '')",
        _ => "''"
    };

    connection.Execute("DROP TABLE IF EXISTS Users_Migrated;");
    connection.Execute(@"
        CREATE TABLE Users_Migrated (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            CompanyId INTEGER NOT NULL,
            UserName TEXT NOT NULL
        );
    ");

    connection.Execute($@"
        INSERT INTO Users_Migrated (Id, CompanyId, UserName)
        SELECT Id, {companyIdExpression}, {userNameExpression}
        FROM Users;
    ");

    connection.Execute("DROP TABLE Users;");
    connection.Execute("ALTER TABLE Users_Migrated RENAME TO Users;");
}

static void SeedDatabase(IDbConnection connection)
{
    var companyId = connection.ExecuteScalar<int?>("SELECT Id FROM Companies ORDER BY Id LIMIT 1;")
        ?? connection.ExecuteScalar<int>(@"
            INSERT INTO Companies (CompanyName)
            VALUES (@CompanyName)
            RETURNING Id;", new { CompanyName = "サンプル企業" });

    var userId = connection.ExecuteScalar<int?>("SELECT Id FROM Users ORDER BY Id LIMIT 1;")
        ?? connection.ExecuteScalar<int>(@"
            INSERT INTO Users (CompanyId, UserName)
            VALUES (@CompanyId, @UserName)
            RETURNING Id;", new { CompanyId = companyId, UserName = "山田 太郎" });

    var favoriteCount = connection.ExecuteScalar<int>(
        "SELECT COUNT(*) FROM Favorites WHERE UserId = @UserId;",
        new { UserId = userId });

    if (favoriteCount == 0)
    {
        connection.Execute(@"
            INSERT INTO Favorites (UserId, FavoriteName) VALUES (@UserId, @FirstFavoriteName);
            INSERT INTO Favorites (UserId, FavoriteName) VALUES (@UserId, @SecondFavoriteName);",
            new
            {
                UserId = userId,
                FirstFavoriteName = "読書",
                SecondFavoriteName = "散歩"
            });
    }
}
