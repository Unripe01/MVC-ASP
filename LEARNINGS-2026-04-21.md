# 学びメモ 2026-04-21

## 今日やったこと

`.NET MVC + Razor` のハンズオンを進めて、主にここを触った。

* STEP 5: POST処理
* STEP 6: DI（依存性注入）
* STEP 7: Dapper + SQLite でDB連携
* STEP 8: ルーティング
* STEP 9 相当: `Program.cs` が設定の中心だと確認

---

## 今日理解できたこと

### 1. GET と POST は役割が違う

* `GET` は画面を表示するためのもの
* `POST` はフォーム送信された値を受け取るためのもの
* 同じ `Create` でも、`GET /Home/Create` と `POST /Home/Create` は役割が違う

---

### 2. `return View();` は MVC の規約で動いている

* `return View();` は「今の action 名と同じ名前の View を探す」
* `HomeController` の `Create()` なら `Views/Home/Create.cshtml` が探される
* これは MVC のルールでつながっている

---

### 3. Model Binding は `name` 属性でつながる

* `<input name="Name" />` とすると `User.Name` に値が入る
* `type="name"` ではだめ
* MVC が見るのは `type` ではなく `name`

---

### 4. ViewBag より Model のほうが本番向き

* ViewBag は動的で気軽
* でもプロパティ名ミスでもコンパイルで気づけない
* Model は型があるので、ミスが見つけやすい
* 「なんとなく ViewBag」から少し抜けられた

---

### 5. DI は「自分で作らず、渡してもらう」

* Controller が `new UserRepository()` しない理由が分かった
* Controller は使うだけ、作るのは外側の仕事
* DI は疎結合にするための仕組み

---

### 6. interface は「できることの約束」

* `IUserRepository` は約束
* `UserRepository` はその実装
* Controller は `UserRepository` そのものではなく、`IUserRepository` に依存する
* これで差し替えやテストがしやすくなる

---

### 7. コンストラクタDIは1つにまとめる

* `ILogger<HomeController>` を追加したくなったとき、コンストラクタを別で増やす感覚だった
* でも実際は、必要な依存を1つのコンストラクタにまとめる

```csharp
public HomeController(
    IUserRepository userRepository,
    ILogger<HomeController> logger)
{
    _userRepository = userRepository;
    _logger = logger;
}
```

---

### 8. `AddScoped` / `AddTransient` / `AddSingleton` の違い

* `Transient`: 毎回新しい
* `Scoped`: 1リクエスト中は同じ
* `Singleton`: アプリ全体で同じ
* Webアプリでは `Scoped` をよく使う

---

### 9. `ILogger` は最初から使えるDIサービス

* `ILogger<T>` は ASP.NET Core 側が用意してくれている
* 自分で `Program.cs` に登録していなくても使えるものがある
* 自作の interface は自分で登録が必要

---

### 10. Dapper は「SQLは自分で書くけど、つらい部分を楽にする」

* ADO.NET 生書きだとかなり面倒
* Dapper は `Query<T>()` や `Execute()` でそのつらさを減らしてくれる
* `DbContext` は EF Core の考え方で、Dapper では `IDbConnection` が中心

---

### 11. SQLite は学習用にかなり便利

* MySQL や SQL Server を立てなくていい
* `app.db` というファイルで進められる
* Dapper の基本学習には十分

---

### 12. ルーティングは URL と Controller / Action をつなぐ

* `asp-action` は action 名
* `asp-controller` は controller 名
* `asp-route-id` のように route 値も渡せる
* `Program.cs` の `MapControllerRoute(...)` がその基本ルール

---

### 13. `[HttpGet]` がなくても GET で動くことがある

* 何も属性がなければ、ルーティングで当たった action として呼ばれる
* `[HttpGet]` は明示
* `[HttpPost]` は POST に絞るときに大事

---

### 14. `App_Start` や `RouteConfig` は古い構成だけど、本質は今と近い

* 現場のコードでは `App_Start` や `RouteConfig` があるかもしれない
* でも役割は今の `Program.cs` とかなり似ている
* 「場所と書き方が違うだけで、本質は近い」と分かった

---

## 今日の脳みそアウトプット

### 最初に引っかかったこと

* `return View();` がどの View を返すのか分からなかった
* `POST` のとき `Name` が `null` で混乱した
* `CreateController` を作らないことに違和感があった
* `DbContext` の話を聞いたけど、現場は Dapper なのでズレを感じた

---

### 途中で腹落ちしたこと

* MVC は「規約でつながる」
* Model Binding は `name` 属性が超大事
* interface は実装を隠すためにある
* Dapper は SQL を見失わずに済むのが良さそう
* 現場コードがちゃんと使えていない箇所も見え始めた

---

### いまの自分の理解

* MVC は「Controller が受けて View に渡す」
* View は Razor で C# と HTML が混ざる
* POST の流れは前よりかなり怖くなくなった
* DI は「必要なものを宣言して、外から渡してもらう」
* Dapper は「生ADO.NETのつらさを減らした SQL 主体のやり方」
* ルーティングは URL を見るとだいたい読める

---

### 現場コードを見るときの視点

今後は、既存コードを見たらこのへんを気にするとよさそう。

* Controller が責務を持ちすぎていないか
* `new` で直接依存を作っていないか
* Repository の責務が曖昧になっていないか
* SQL の置き場所がつらくなっていないか
* `ViewBag` が多すぎないか
* ルーティングのつながりが読めるか

---

## 今日の感想

今日はかなり理解が進んだ日だった。

「ただ動いた」ではなく、
「なんでそうなるのか」を何回も確認できたのが大きかった。

あと、既存コードがわりとちゃんと使えていないことにも気づけた。
これは嫌な意味ではなくて、基礎が見えてきたから違和感を言葉にできるようになった、ということだと思う。

---

## 次にやるとよさそうなこと

* STEP 7 の CRUD をもう少し進めて Update / Delete も触る
* `UserController` に分けるとどう見えるか試す
* 現場コードで `RouteConfig` や `App_Start` を見て、今の知識に対応づける
* Dapper の transaction を「いつ使うべきか」整理する
