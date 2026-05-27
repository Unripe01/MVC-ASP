# 帳票定義中心PoC 技術仕様書

## 目的

このPoCは、特定帳票向け帳票システムを、画面・DB・JavaScript・Controllerに分散した実装から、Entityを起点にした帳票定義中心の構造へ移行できるかを検証するための実装ハーネスである。

主目的は、次の流れを小さく成立させること。

```text
Entity
  ↓
ReportDefinition
  ↓
UI / XML Snapshot / txt出力 / マスター連携 / 逆反映
```

このPoCは汎用フォームエンジンではない。帳票追加・項目追加・マスター連携変更のコストを下げるため、帳票ごとの差分を `Reports` 配下の定義へ寄せる。

---

## 現在の実装範囲

実装済みの帳票は `UserFavorite` 帳票である。

対象Entity:

- `Company`
- `User`
- `Favorite`
- `ReportInstance`

成立している機能:

- 帳票定義から入力Fieldを生成する
- 帳票定義の `Field(...).TemplateKey(...)` からtxt差し込みを実行する
- `Company` マスターを選択肢として表示する
- 帳票入力中に会社マスター編集ダイアログを開く
- 入力値から右側txtプレビューを更新する
- 入力値を `User` / `Favorite` へ逆反映する
- `User` EntityグラフをXML serializeして `ReportInstances` に保存する
- `DocumentTemplates/user_favorite.txt` を単純文字列置換して `DocumentDownload` に出力する

未実装または今後強化する領域:

- 帳票定義からValidationを生成する仕組み
- XML Snapshotからの復元UI
- ReportInstance履歴表示
- 複数帳票定義の登録・選択
- マスターダイアログ保存後のField単位再反映の精密化
- Company / User / Favorite の通常CRUD画面整備

---

## 技術スタック

必須技術:

- ASP.NET Core MVC
- Razor
- Dapper
- SQLite
- htmx
- Alpine.js

禁止または避けるもの:

- React / Vue / Blazor
- Entity Framework
- SPA化
- MediatR / CQRS
- 過剰なClean Architecture分割
- Generic万能Repository
- metadata DB化
- reflection中心設計
- `Dictionary<string, object>` 中心設計
- indexベースUI管理
- 帳票ごとの巨大case文

---

## 重要フォルダと責務

### `Entities/`

業務データそのものを置く。DB永続化対象であり、帳票Projectionの起点になるObject Graphを表す。

置かないもの:

- SQL
- Dapper呼び出し
- HTML
- Validation表示
- テンプレート置換
- XML serialize処理
- ファイル出力

現在の主なEntity:

- `Company`
- `User`
- `Favorite`
- `ReportInstance`

---

### `Repository/`

DBアクセスだけを担当する。SQLとDapper呼び出しはこの層に閉じ込める。

置かないもの:

- HTML生成
- 帳票定義
- 帳票UI生成
- XML serialize
- txt出力
- htmx / Alpine.js の制御

現在の主なRepository:

- `IUserRepository` / `UserRepository`
- `ICompanyRepository` / `CompanyRepository`
- `IFavoriteRepository` / `FavoriteRepository`
- `IReportInstanceRepository` / `ReportInstanceRepository`

---

### `Reports/`

帳票定義を書く場所。PoCの最重要フォルダ。

ここでは「帳票を書く」のではなく、「Entityを帳票へ投影する定義を書く」。

置くもの:

- `ReportDefinition<T>`
- `FieldDefinition`
- 帳票ごとの定義クラス
- ResolverやCustomRendererなど、特殊処理の入口を示す型

置かないもの:

- SQL
- Dapper呼び出し
- HTML
- htmx属性
- DB接続
- ファイル書き込み
- XML serialize実行

現在の主なクラス:

- `ReportDefinition<TModel>`
- `FieldDefinition`
- `FieldDefinition<TModel, TValue>`
- `ReportValueFormatter`
- `UserFavoriteReportDefinition`
- `FavoriteResolver`

帳票定義例:

```csharp
Template("user_favorite.txt");

Field(user => user.UserName)
    .Label("名前")
    .Input("UserName")
    .FromMaster()
  .AllowReverseReflect()
  .TemplateKey("User.UserName");

Field(user => user.Company!.CompanyName)
    .Label("会社")
    .Input("CompanyId")
    .FromMaster("Company")
  .OpenDialog("/Report/CompanyDialog")
  .TemplateKey("Company.CompanyName");

Field(user => user.Favorites)
    .Label("好きなもの")
    .Input("FavoriteNames")
    .AsCollection()
  .ResolveWith<FavoriteResolver>()
  .TemplateKey("Favorite.FavoriteName")
  .JoinWith("、");
```

標準的な帳票では、txtテンプレートのプレースホルダはField定義の `TemplateKey` に定義する。UI、マスター取得、逆反映、txt差し込みの定義を同じFieldチェーンで読めるようにする。`TemplateRenderService` に帳票固有の `RenderXxx` メソッドを追加しない。

---

### `Services/`

定義を回して業務処理を成立させる場所。

置くもの:

- 帳票定義からViewModelを生成する処理
- Entityグラフの組み立て
- 入力値の逆反映
- XML serialize / deserialize
- ReportDefinitionのレンダーキーに基づくtxtテンプレート差し込み
- Resolver呼び出し
- Validationの実行
- 出力ファイル生成

置かないもの:

- Razor HTML
- htmx属性
- Alpine.js状態
- DBアクセスSQLの直書き

現在の主なService:

- `ReportEngine`
- `ReportRendererService`
- `ReportXmlService`
- `TemplateRenderService`
- `UserService`

---

### `ViewModels/`

画面とHTTP POSTのためのモデルを置く。

置くもの:

- Razor表示用モデル
- htmx部分更新用モデル
- POST入力受信用モデル
- Select optionなどの画面補助モデル

置かないもの:

- SQL
- Dapper呼び出し
- Repository呼び出し
- XML serialize実行
- txt出力

現在の主なViewModel:

- `UserFavoriteReportViewModel`
- `UserFavoriteReportPostViewModel`
- `ReportFieldInputViewModel`
- `ReportSelectOptionViewModel`
- `ReportPreviewViewModel`
- `CompanyDialogViewModel`

---

### `Controllers/`

HTTP入口を担当する。

置くもの:

- GET / POST action
- Service呼び出し
- View / PartialView の返却
- htmx向けレスポンスヘッダー

置かないもの:

- SQL
- 帳票定義の詳細分岐
- txtテンプレート置換
- XML serialize実装
- DB保存の詳細ロジック

現在の主なController:

- `ReportController`
- `HomeController`

`HomeController` は旧ハンズオン由来の入口であり、PoCの主役ではない。帳票PoCの主入口は `ReportController` とする。

---

### `Views/Report/`

帳票PoCのRazor Viewを置く。

置くもの:

- 帳票入力画面
- htmx部分更新用PartialView
- txtプレビューPartialView
- マスター編集ダイアログPartialView

置かないもの:

- 業務判定
- DBアクセス
- Entity保存処理
- Validationの最終判断
- JavaScriptによる業務状態管理

現在の主なView:

- `UserFavorite.cshtml`
- `_ReportWorkspace.cshtml`
- `_ReportPreview.cshtml`
- `_CompanyDialog.cshtml`

RazorでAlpine.jsを書く場合、`@click` はRazorのC#として解釈されるため使わない。必ず `x-on:click` を使う。

---

### `DocumentTemplates/`

txtテンプレートを置く。

現在のテンプレート:

- `user_favorite.txt`

テンプレート置換はMustache等を使わず、単純文字列置換で行う。

ただし置換キーと値の取得方法は `TemplateRenderService` へ直書きせず、`Reports/` のField定義に `TemplateKey` として定義する。

例:

```csharp
Template("user_favorite.txt");
Field(user => user.UserName).TemplateKey("User.UserName");
Field(user => user.Company!.CompanyName).TemplateKey("Company.CompanyName");
Field(user => user.Favorites).TemplateKey("Favorite.FavoriteName").JoinWith("、");
```

---

### `DocumentDownload/`

txt出力結果を保存する。

ファイル名はUUIDとUserIdを含める。

```text
{uuid}-user-{userId}.txt
```

実行時生成物のため、検証で作ったファイルは必要に応じて削除する。

---

### `Poc/`

PoCの思想・仕様・検討ログを置く。

現在の主な文書:

- `PoC.md`
- `アウトライン.md`
- `マスターダイアログ思想.md`
- `TECHNICAL-SPEC.md`

コード変更で責務境界、画面仕様、保存仕様、フォルダ責務が変わる場合は、このフォルダの文書も同じ作業で更新する。

---

## データモデル

### `Companies`

企業マスター。

```text
Id INTEGER PRIMARY KEY AUTOINCREMENT
CompanyName TEXT NOT NULL
```

### `Users`

帳票Projectionの起点になるユーザー。

```text
Id INTEGER PRIMARY KEY AUTOINCREMENT
CompanyId INTEGER NOT NULL
UserName TEXT NOT NULL
```

旧ハンズオン由来の `Name` カラムが存在する場合は、起動時初期化で `Users` テーブルを新スキーマへ再構築し、`Name` の値を `UserName` へ移行する。

### `Favorites`

User 1:N Favoriteを検証するための明細。

```text
Id INTEGER PRIMARY KEY AUTOINCREMENT
UserId INTEGER NOT NULL
FavoriteName TEXT NOT NULL
```

### `ReportInstances`

帳票XML Snapshot保存先。

```text
Id INTEGER PRIMARY KEY AUTOINCREMENT
UserId INTEGER NOT NULL
ReportType TEXT NOT NULL
XmlData TEXT NOT NULL
CreatedAt TEXT NOT NULL
```

---

## 主要処理フロー

### 初期表示

```text
GET /Report/UserFavorite/{id}
  ↓
ReportController.UserFavorite
  ↓
ReportEngine.BuildUserFavoriteReport
  ↓
UserService.GetUserGraph
  ↓
ReportRendererService.BuildFields
  ↓
TemplateRenderService.Render
  ↓
ReportDefinition.TemplateFields
  ↓
ReportXmlService.Serialize
  ↓
Views/Report/UserFavorite.cshtml
```

### プレビュー更新

```text
入力Field change
  ↓ htmx
POST /Report/PreviewUserFavorite
  ↓
ReportEngine.RenderPreview
  ↓
TemplateRenderService.Render
  ↓
ReportDefinition.TemplateFields
  ↓
Views/Report/_ReportPreview.cshtml
  ↓
#previewRegion 差し替え
```

### XML保存と逆反映

```text
XML保存ボタン
  ↓ htmx
POST /Report/SaveUserFavorite
  ↓
ReportEngine.SaveUserFavoriteReport
  ↓
UserService.SaveUserFavorite
  ↓
UserRepository.Update / Add
FavoriteRepository.ReplaceForUser
  ↓
ReportXmlService.Serialize
  ↓
ReportInstanceRepository.Add
  ↓
Views/Report/_ReportWorkspace.cshtml
  ↓
#reportWorkspace 差し替え
```

### マスター編集ダイアログ

```text
会社Fieldの編集ボタン
  ↓ htmx
GET /Report/CompanyDialog
  ↓
Views/Report/_CompanyDialog.cshtml
  ↓
Alpine.jsでモーダル表示
  ↓
POST /Report/SaveCompanyDialog
  ↓
CompanyRepository.Update / Add
  ↓
HX-Trigger: report-modal-close
  ↓
#reportWorkspace 差し替え
```

### txt出力

```text
txt出力ボタン
  ↓ htmx
POST /Report/ExportUserFavorite
  ↓
ReportEngine.ExportUserFavoriteText
  ↓
TemplateRenderService.Write
  ↓
ReportDefinition.TemplateFields
  ↓
DocumentDownload/{uuid}-user-{userId}.txt
```

---

## htmx / Alpine.js の責務境界

### htmx

担当するもの:

- 部分更新通信
- `hx-target` へのPartialView差し替え
- プレビュー更新
- 保存後のワークスペース更新
- ダイアログHTML取得

担当しないもの:

- 業務状態保持
- Validation判定
- Entity保存
- Field index管理

### Alpine.js

担当するもの:

- モーダル開閉
- Collection入力行の一時的な追加・削除
- UI上の一時状態

担当しないもの:

- 業務データの正本管理
- Validation結果保持
- Repository呼び出し
- マスター整合性判定

---

## 新しい帳票を追加する手順

1. `Entities/` に必要なEntityまたは既存Entityの関連を追加する。
2. 必要なDBアクセスを `Repository/` に追加する。
3. `Reports/` に `ReportDefinition<T>` 派生クラスを追加し、Field定義を書く。
4. 同じ帳票定義に `Template("file_name.txt")` を追加する。
5. txtテンプレートのプレースホルダは対象Fieldへ `.TemplateKey("Placeholder.Name")` として追加する。
6. collection差し込みが必要な場合は `.JoinWith("、")` のように連結文字を定義する。
7. 必要ならResolverやCustomRendererを `Reports/` または専用フォルダへ追加する。
8. `ViewModels/` に画面表示・POST受信用モデルを追加する。
9. `Services/` にEntityグラフ構築、逆反映、XML、txt出力の処理を追加する。
10. `Controllers/` にHTTP入口を追加する。
11. `Views/Report/` にViewまたはPartialViewを追加する。
12. `DocumentTemplates/` にtxtテンプレートを追加する。
13. `.csproj` に新規 `.cs` は `Compile`、新規 `.cshtml` / `.md` / テンプレートは `Content` として明示登録する。
14. `Poc/TECHNICAL-SPEC.md` と必要なPoC文書を更新する。
15. `dotnet build ASP-MVC/ASP-MVC.csproj` を実行する。

---

## ドキュメント更新ルール

コード変更で次のいずれかが変わる場合は、同じ作業でドキュメントも更新する。

- フォルダ責務
- Entity構造
- テーブル構造
- ReportDefinitionのField API
- FieldDefinitionのTemplateKey / JoinWith API
- Service責務
- htmx target / endpoint
- Alpine.jsが持つUI状態
- XML保存形式
- txtテンプレート仕様
- マスター取得 / 逆反映の流れ
- 新しい帳票追加手順

特に `Reports/`、`Services/`、`Views/Report/`、`Program.cs` の変更は仕様に影響しやすいため、必ず `Poc/TECHNICAL-SPEC.md` を確認する。

---

## 動作確認

ビルド確認:

```bash
dotnet build ASP-MVC/ASP-MVC.csproj
```

開発サーバー起動:

```bash
dotnet run --project ASP-MVC/ASP-MVC.csproj --launch-profile http
```

確認URL:

```text
http://localhost:5144/Report/UserFavorite/1
```

httpプロファイルでは `Failed to determine the https port for redirect.` が表示される場合がある。PoC画面がHTTPで200応答する限り、この警告自体は今回の検証を阻害しない。