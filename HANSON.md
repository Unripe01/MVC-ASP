# .NET MVC + Razor ハンズオン学習プラン

## 目的

既存プロジェクトの理解に必要な以下の要素を、自分の手で最小構成から再現できる状態にする。

* MVC構造の理解
* RazorによるView描画
* DI（依存性注入）の基本理解
* Controller + [HttpPost] の流れ
* DB CRUDの基本実装
* App_Start / ルーティングの役割理解

---

## ゴール

以下を満たすシンプルなアプリを作る

* ユーザー一覧画面
* ユーザー登録画面（POST）
* ユーザー編集・削除
* DB連携あり
* DIでRepositoryを注入
* ルーティングが理解できる状態

---

## 技術スタック（想定）

* ASP.NET Core MVC
* Razor
* Dapper
* SQLite（ファイルDB。サーバーを立てなくてOK）

---

## STEP 0：プロジェクト作成

### やること

* Visual Studioで新規プロジェクト作成
* 「ASP.NET Core Web アプリ（Model-View-Controller）」を選択

### 確認ポイント

* Controllers / Models / Views フォルダ構成
* Program.cs の中身
* ルーティング設定の場所

---

## STEP 1：MVCの最小動作を確認

### やること

* HomeController を確認
* Index() を編集
* Viewに文字を出す

### 学ぶこと

* Controller → View の流れ
* return View() の意味

---

## STEP 2：Razorの理解

### やること

* Viewに以下を書く

```
@{
    ViewBag.Message = "Hello Razor";
}

<h1>@ViewBag.Message</h1>
```

### 学ぶこと

* @ の意味
* ViewBagの使い方
* C#とHTMLの混在

---

## STEP 3：Modelを作る

### やること

* Userクラスを作成

```
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```

### 学ぶこと

* Modelの役割
* データ構造とViewの関係

---

## STEP 4：一覧画面（GET）

### やること

* Controllerに一覧処理を書く

```
public ActionResult Index()
{
    var users = new List<User>
    {
        new User { Id = 1, Name = "Taro" },
        new User { Id = 2, Name = "Jiro" }
    };

    return View(users);
}
```

* Viewで表示

```
@model List<User>

@foreach (var user in Model)
{
    <p>@user.Name</p>
}
```

### 学ぶこと

* Modelの受け渡し
* strongly typed view

---

## STEP 4.5：ViewBag と Model の違いを体験する

### 目的

* ViewBag（動的）とModel（型付き）の違いを体感する
* 「なぜModelが推奨されるのか」を理解する

---

### ① ViewBag を使う（おさらい）

#### Controller

```csharp
public IActionResult Index()
{
    ViewBag.Message = "ViewBagのメッセージ";
    return View();
}
```

#### View（Index.cshtml）

```html
<h2>@ViewBag.Message</h2>
```

---

### 学びポイント

* 型がない（dynamic）
* プロパティ名ミスってもコンパイルエラーにならない
* 軽いデータ受け渡しには便利

---

### ② Model を使う（本命）

#### Modelクラス作成

📄 Models/User.cs

```csharp
public class User
{
    public string Name { get; set; }
}
```

---

#### Controller

```csharp
public IActionResult Index()
{
    var user = new User
    {
        Name = "タクマ"
    };

    return View(user);
}
```

---

#### View（Index.cshtml）

```html
@model User

<h2>@Model.Name</h2>
```

---

### 学びポイント

* 型がある（コンパイルチェックされる）
* IntelliSense効く
* 大規模開発では必須

---

### ③ わざとミスる（超重要）

#### Viewでこう書く

```html
<h2>@Model.Nam</h2>
```

👉 コンパイルエラーになる

---

#### ViewBagで同じことやる

```html
<h2>@ViewBag.Mesage</h2>
```

👉 エラーにならず、画面で null 表示

---

### 結論

| 項目    | ViewBag | Model |
| ----- | ------- | ----- |
| 型安全   | ❌       | ⭕     |
| 書きやすさ | ⭕       | △     |
| 本番向き  | ❌       | ⭕     |

---

### 使い分け

* ViewBag：一時的・軽い表示
* Model：基本はこっち（本番）

---

### このステップのゴール

* 「なんとなくViewBag」から卒業する
* Modelの安心感を体感する
* 既存コードでViewBag見てもビビらなくなる


## STEP 5：POST処理（登録）

### やること

* GET（入力画面）

```
public ActionResult Create()
{
    return View();
}
```

* POST（登録処理）

```
[HttpPost]
public ActionResult Create(User user)
{
    // 保存処理（仮）
    return RedirectToAction("Index");
}
```

* Viewにフォームを書く

```
<form method="post">
    <input name="Name" />
    <button type="submit">登録</button>
</form>
```

### 学ぶこと

* [HttpPost] の意味
* Model Binding
* フォームとControllerの連携

---

## STEP 6：DI（依存性注入）

### やること

* Repositoryインターフェース作成

```
public interface IUserRepository
{
    List<User> GetAll();
    void Add(User user);
}
```

* 実装クラス作成

* ControllerでコンストラクタDI

```
private readonly IUserRepository _repo;

public UserController(IUserRepository repo)
{
    _repo = repo;
}
```

### 学ぶこと

* DIの目的（疎結合）
* テストしやすい構造
* 「Controllerは使うだけ、実装は外から渡す」の感覚

---

## STEP 7：DB連携（CRUD / Dapper + SQLite）

### やること

* NuGetで `Dapper` と `Microsoft.Data.Sqlite` を追加
* `app.db` のようなSQLiteファイルを使う
* `IDbConnection` または `SqliteConnection` をDI登録する
* `Users` テーブルを作る
* Repository内でSQLを書いてCRUD実装する

```
CREATE TABLE Users (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL
);
```

* 一覧取得

```
public List<User> GetAll()
{
    var sql = "SELECT Id, Name FROM Users";
    return _connection.Query<User>(sql).ToList();
}
```

* 登録

```
public void Add(User user)
{
    var sql = "INSERT INTO Users (Name) VALUES (@Name)";
    _connection.Execute(sql, user);
}
```

### 学ぶこと

* Dapperは「SQLを書く軽量ライブラリ」だと理解する
* `IDbConnection` がDB接続の入口だと分かる
* `Query<T>()` と `Execute()` の違い
* SQLの列名とModelのプロパティ名の対応
* Repositoryパターン

---

## STEP 8：ルーティング理解

### やること

* RouteConfig.cs を確認

```
routes.MapRoute(
    name: "Default",
    url: "{controller}/{action}/{id}",
    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
);
```

### 学ぶこと

* URLとControllerの対応関係
* ルーティングの仕組み

---

## STEP 9：App_Startの理解

### 見るべきファイル

* Program.cs
* appsettings.json
* launchSettings.json

### 学ぶこと

* ASP.NET Coreでは `Program.cs` が起動と設定の中心になる
* DI登録やミドルウェア設定の場所
* 接続文字列の置き場所

---

## 進め方（重要）

### NGな進め方

* AIに「全部作って」と依頼
* コピペして終わり

### 推奨

* 1ステップごとに手で書く
* わからない箇所だけAskモードで聞く
* 「なぜそうなるか」を必ず確認

---

## 最終到達イメージ

* Controller見たら処理の流れが読める
* View見たら何してるか理解できる
* POST処理が怖くなくなる
* DIの意図が分かる
* 既存コードを「読める」状態になる

---

## 補足

### DBを立てずに進める方法

MySQLやSQL Serverを別で立てなくても、SQLiteなら `.db` ファイル1つで進められる。

* 学習コストが低い
* 接続設定が軽い
* Dapperの基本（Query / Execute / パラメータ化SQL）を学ぶには十分

#### 接続文字列の例

```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=app.db"
}
```

#### 先にテーブルを作る方法

最初は「起動時にCREATE TABLEを1回流す」でも十分。
マイグレーションや本格的なDB管理は後回しでOK。

このハンズオンの目的は「正しい設計」ではなく
**既存プロジェクトを読み解ける状態になること**。

完璧な構成は目指さなくていい。
まずは「動く」「理解できる」を優先する。
