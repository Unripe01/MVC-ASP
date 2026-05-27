# AI実装用ハーネス設計 / 指示書（PoC固定版）

## 目的

本PoCは、
* 汎用フォームエンジン開発ではない
* 特定帳票向け帳票システム再設計
* 帳票定義中心アーキテクチャ
* XML Snapshot保存
* txtテンプレート差し込み
* マスター取得 / 逆反映
* MVC + htmx + Alpine.js 構成
* 変更頻度が高い帳票修正の変更コストを下げたい。定義中心へ寄せたい

を、小規模・最小構成で検証するための実装ハーネスである。

---

# 最重要思想

このPoCの主役は：

```text id="wjlwm1"
Entity
```

である。

帳票は：

```text id="xjlwm2"
Entity を帳票へ投影する定義
```

として扱う。

---

# このPoCで検証したいこと

1. 帳票定義中心でUIを生成できるか
2. 帳票定義からマスター取得できるか
3. 帳票定義から逆反映できるか
4. XML serialize / deserialize が成立するか
5. 1:N を帳票定義で扱えるか
6. txtテンプレ差し込みが成立するか
7. 「帳票を定義する」開発体験になるか

---

# 開始地点

既存 HANSON.md 実装済プロジェクトを開始地点とする。 

UI構成は HANSON2.md の思想に従う。 

---

# 使用技術

## 必須

* ASP.NET Core MVC
* Razor
* Dapper
* SQLite
* htmx
* Alpine.js

---

# 禁止

* React
* Vue
* Blazor
* EntityFramework
* SPA化
* MediatR
* CQRS
* 過剰DI
* CleanArchitecture過剰分割
* Generic万能Repository
* metadata DB化
* reflection地獄
* everything generic

---

# ディレクトリ構成

```text id="jlwm3"
Controllers/
Views/

Entities/
ViewModels/

Repository/
Reports/
Services/

DocumentTemplates/
DocumentDownload/
```

---

# 各ディレクトリ責務

## /Entities

業務データそのもの。

DB永続化対象。

Dapper の SELECT / INSERT 対象。

---

# Entityルール

禁止：

* SQL
* Dapper
* HTML
* Validation
* 業務ロジック
* ファイル出力
* テンプレート置換

---

# Entity例

```csharp id="jlwm4"
namespace WebApplication1.Entities;

public class User
{
    public int Id { get; set; }

    public int CompanyId { get; set; }

    public string UserName { get; set; } = "";

    public Company? Company { get; set; }

    public List<Favorite> Favorites { get; set; } = [];
}
```

---

## /ViewModels

画面専用。

Razorへ渡すためのモデル。

DB保存対象ではない。

---

# ViewModelルール

禁止：

* Dapper
* SQL
* Repository
* XML serialize

---

# ViewModel例

```csharp id="jlwm5"
public class UserEditViewModel
{
    public User User { get; set; }

    public List<Company> Companies { get; set; } = [];
}
```

---

## /Repository

DBアクセス責務のみ。

Dapperを書く場所。

---

# Repositoryルール

Repository は：

* SELECT
* INSERT
* UPDATE
* DELETE

のみ担当。

---

# Repository禁止事項

禁止：

* HTML生成
* 帳票生成
* XML serialize
* txt出力
* UI制御

---

# Repository例

```csharp id="jlwm6"
public interface IUserRepository
{
    User? Get(int id);

    List<User> GetAll();

    void Save(User user);
}
```

---

## /Reports

帳票定義を書く場所。
PoCの主役。

---

# 最重要思想

```text id="jlwm7"
帳票を書く
ではなく

帳票定義を書く
```

ことで：

* UI
* XML
* txt差し込み
* マスター取得
* 逆反映

を成立させる。

---

# ReportDefinition思想

帳票定義は：

```text id="jlwm8"
帳票定義
=
UI定義
=
マスター連携定義
```

として扱う。

---

# ReportDefinition例

```csharp id="jlwm9"
public class UserFavoriteReportDefinition : ReportDefinition<User>
{
    protected override void Configure()
    {
        Field(x => x.UserName)
            .Label("名前")
            .FromMaster()
            .AllowReverseReflect();

        Field(x => x.Company!.CompanyName)
            .Label("会社")
            .FromMaster()
            .OpenDialog("/Company/Edit");

        Field(x => x.Favorites)
            .Label("好きなもの")
            .AsCollection()
            .ResolveWith<FavoriteResolver>();
    }
}
```

---

# ReportDefinitionルール

禁止：

* SQL
* Dapper
* HTML
* htmx
* DB接続
* ファイル書き込み

---

# 画面思想
[マスターダイアログ思想](マスターダイアログ思想.md) に従う。


# /Services

業務ロジック中心。

定義をぐるぐる回す場所。

---

# Service責務

ここで行う：

* UI生成
* XML serialize
* XML deserialize
* txt差し込み
* マスター取得
* 逆反映
* Validation
* Resolver呼び出し

---

# Service例

```text id="jlwm10"
ReportRendererService
ReportXmlService
TemplateRenderService
ReportEngine
UserService
```

---

# PoC対象テーブル

## Company

```csharp id="jlwm11"
class Company
{
    int Id
    string CompanyName
}
```

---

## User

```csharp id="jlwm12"
class User
{
    int Id
    int CompanyId
    string UserName
}
```

---

## Favorite

```csharp id="jlwm13"
class Favorite
{
    int Id
    int UserId
    string FavoriteName
}
```

---

# 関係

```text id="jlwm14"
Company 1 : N User
User 1 : N Favorite
```

---

# 1:N 設計方針

## NG

```text id="jlwm15"
Favorite1
Favorite2
Favorite3
```

---

## OK

```csharp id="jlwm16"
Field(x => x.Favorites)
```

---

# Resolver方針

1:N や特殊項目は Resolver へ逃がす。

---

# 例

```csharp id="jlwm17"
Field(x => x.Favorites)
    .ResolveWith<FavoriteResolver>()
```

---

# 特殊処理思想

## 重要

「特殊処理ゼロ」は目指さない。

---

# 方針

通常：

```text id="jlwm18"
定義だけ
```

特殊：

```text id="jlwm19"
Resolver
CustomValidator
CustomRenderer
```

---

# txtテンプレート仕様

## 保存場所

```text id="jlwm20"
/DocumentTemplates
```

---

# テンプレ例

```text id="jlwm21"
user_favorite.txt
```

内容：

```text id="jlwm22"
企業：{{Company.CompanyName}}
おなまえ：{{User.UserName}}
好きなものリスト：{{Favorite.FavoriteName}}
```

---

# 出力仕様

## 出力先

```text id="jlwm23"
/DocumentDownload
```

---

# ファイル名

UUID + userId を含める。

例：

```text id="jlwm24"
a8f3-user-1.txt
```

---

# 出力方式

Mustache等は禁止。

単純文字列置換でよい。

---

# XML保存仕様

## 目的

帳票専用テーブル増殖を避ける。

---

# テーブル

```text id="jlwm25"
ReportInstance
```

---

# カラム

```text id="jlwm26"
Id
UserId
ReportType
XmlData
CreatedAt
```

---

# XML方針

帳票クラスそのものを serialize する。

---

# UI思想

## 技術

* Razor
* htmx
* Alpine.js

---

# UI責務

## サーバー

* 業務ルール
* Validation
* 権限制御
* DB整合性

---

## htmx

* 部分更新
* 通信配線
* PartialView差し替え

---

## Alpine.js

* モーダル開閉
* UI状態
* collection追加削除
* 表示切替

---

# Alpine.js禁止事項

禁止：

* 業務状態保持
* Validation保持
* Repository呼び出し

---

# 目指す構造

```text id="jlwm29"
帳票定義
↓
UI生成
```

---

# 絶対禁止事項

禁止：

* DataTable主導
* JS主導業務ルール
* Dictionary<string, object> 中心設計
* indexベースUI
* case文帳票分岐地獄

---

# 実装優先順位

## STEP1

CRUD完成

* Company
* User
* Favorite

---

## STEP2

Repository分離

---

## STEP3

ReportDefinition基盤

---

## STEP4

UI自動生成

---

## STEP5

XML serialize

---

## STEP6

txtテンプレ出力

---

## STEP7

マスター取得

---

## STEP8

逆反映

---

# 最終ゴール

以下の体験を成立させること：

```text id="jlwm30"
帳票クラスを追加
↓
Field定義を書く
↓
txtテンプレ置く
↓
UI/保存/出力が動く
```

---

# 最終思想

このPoCは、

```text id="jlwm31"
帳票入力システム
```

ではなく、

```text id="jlwm32"
業務データ(Entity)
↓
帳票Projection
```

という構造へ移行できるかを検証するPoCである。
