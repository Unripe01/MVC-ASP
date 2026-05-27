# Editing Philosophy

編集時は、常に「変更量」ではなく「目的達成」を優先すること。

ただし、目的に応じて編集戦略を切り替える。

編集開始前に、
今回の編集が:

- Hotfix Mode
- Refactor Mode

のどちらかを自分で宣言してから作業すること。

---

# Editing Modes

## 1. Hotfix / Local Fix Mode

適用条件:
- バグ修正
- 緊急対応
- 小規模仕様変更
- レビュー済み構造への追従
- リスク最小化が必要

方針:
- 差分最小
- 既存構造維持
- 影響範囲最小
- 命名や責務は既存文化を尊重
- 不要なリファクタ禁止

---

## 2. Refactor / Re-Architecture Mode

適用条件:
- 教材
- onboarding
- 設計整理
- 可読性改善
- 長期保守改善
- 「理解しづらい」が問題になっているケース
- 局所修正で整合性が崩れるケース

方針:
- 全体整合性を優先
- 必要なら章構成・責務・命名を再設計
- 差分量を恐れない
- 「短いこと」より「理解できること」を優先
- 部分最適の継ぎ足しを避ける
- 学習導線・責務分離・将来保守を重視

---

# Important

既存ファイルを読んだだけで、
自動的に「最小変更モード」に入らないこと。

「既存構造を維持すること」が目的なのか、
「本質的に改善すること」が目的なのかを判断してから編集すること。

---

# Priority Order

以下の優先順位で判断する:

1. 本来の目的を達成しているか
2. 全体整合性が保たれているか
3. 将来保守しやすいか
4. 学習・理解しやすいか
5. 差分が小さいか

差分量は最下位。

---

# Documentation Policy

このPoCでは、コードとドキュメントを同じ設計成果物として扱うこと。

コード変更によって次のいずれかが変わる場合は、同じ作業で関連Markdownも更新する。

- フォルダ責務
- Entity構造
- DBテーブル構造
- ReportDefinition / FieldDefinition のAPI
- Service責務
- Controller endpoint
- htmx target / swap境界
- Alpine.jsが持つUI状態
- XML Snapshot仕様
- txtテンプレート仕様
- マスター取得 / 逆反映の流れ
- 新しい帳票追加手順

特に `ASP-MVC/Poc/TECHNICAL-SPEC.md` は、現在の実装仕様を表す一次資料として扱う。

`ASP-MVC/Poc/PoC.md`、`ASP-MVC/Poc/マスターダイアログ思想.md`、`ASP-MVC/Poc/アウトライン.md` は思想・背景・設計判断の資料として扱い、実装仕様が変わった場合は必要に応じて整合させる。

---

# Key PoC Folders

このPoCで重要なフォルダと責務は以下。

## `ASP-MVC/Entities/`

業務データそのもの。DB永続化対象。帳票Projectionの起点。

禁止:

- SQL
- Dapper呼び出し
- HTML
- Validation表示
- XML serialize
- txt出力
- ファイル書き込み

## `ASP-MVC/Repository/`

DBアクセス専用。DapperとSQLを書く場所。

禁止:

- HTML生成
- 帳票定義
- UI制御
- XML serialize
- txt出力

## `ASP-MVC/Reports/`

帳票定義を書く最重要フォルダ。

ここでは「帳票を書く」のではなく、「Entityを帳票へ投影する定義を書く」。

txtテンプレートのファイル名は `Template(...)`、プレースホルダは標準ケースでは対象Fieldの `.TemplateKey(...)` としてここに定義する。

禁止:

- SQL
- Dapper呼び出し
- HTML
- htmx属性
- DB接続
- ファイル書き込み

## `ASP-MVC/Services/`

ReportDefinitionを回して、UI生成、XML、txt差し込み、マスター取得、逆反映、Validationを成立させる場所。

`TemplateRenderService` は帳票固有の `RenderXxx` メソッドを増やさず、ReportDefinitionのレンダーキーを回す汎用処理として保つこと。

禁止:

- Razor HTML
- htmx属性
- Alpine.js状態
- SQL直書き

## `ASP-MVC/ViewModels/`

Razor表示とPOST入力のためのモデル。

禁止:

- Dapper
- SQL
- Repository呼び出し
- XML serialize
- txt出力

## `ASP-MVC/Views/Report/`

帳票PoCのRazor View / PartialView。

htmxは通信配線とPartial差し替えのみ、Alpine.jsはモーダル開閉と一時UI状態のみ担当する。

Razorでは Alpine.js の `@click` 省略記法を使わず、必ず `x-on:click` を使うこと。

## `ASP-MVC/DocumentTemplates/`

txtテンプレート置き場。

Mustache等は使わず、PoCでは単純文字列置換でよい。

## `ASP-MVC/DocumentDownload/`

txt出力結果の保存先。

実行時生成物のため、検証で作成された不要ファイルはコミット対象にしない。

## `ASP-MVC/Poc/`

PoCの思想、技術仕様、設計ログを置く。

コード変更で仕様が変わる場合は、このフォルダのMarkdownを確認し、必要なら更新する。

---

# Project File Policy

.NETプロジェクトでファイルを新規作成した場合は、対応する `.csproj` へ明示登録すること。

- `.cs`: `Compile`
- `.cshtml`: `Content`
- `.md`: `Content`
- txtテンプレート: `Content`

登録後は `dotnet build ASP-MVC/ASP-MVC.csproj` で確認する。