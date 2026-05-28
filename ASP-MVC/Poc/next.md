次に触るなら、優先度はこの順がよさそうです。

完了: Company / User / Favorite のCRUD画面を PoC責務境界に合わせて整える
完了: 帳票一覧から選択して帳票登録画面へ遷移する導線を追加する
完了: user_info 帳票を追加する
ReportDefinition のField定義からValidationも出せるようにする
ReportInstance の保存履歴表示とXML復元を追加する

帳票追加を Report クラス追加中心で成立させるための抽象化ステップ

1. ReportDefinition ごとの識別子・タイトル・対象モデル型を定義し、帳票一覧と起動導線を Report 定義から生成できるようにする
2. `UserFavoriteReportDefinition` 固有の `ReportEngine.UserFavorite*` 系メソッドをやめ、`ReportDefinition` を受ける汎用 Engine に寄せる
3. `GetUserGraph` のような帳票専用グラフ組み立てを Service に増やさないため、帳票ごとの入力/出力モデル組み立て責務を `ProjectionResolver` などの明示的な抽象に切り出す
4. 1:N や特殊項目は `FavoriteResolver` と同じ位置づけで、通常Fieldと違うことが分かる `Resolver` / `Fetcher` / `ReverseReflector` に逃がす
5. マスター取得と逆反映は Field 単位の metadata だけでなく、必要なら Field ごとの strategy を差し込める形にして、`働く場所` のような複数選択項目でも帳票ごとに Service を増やさず扱えるようにする
6. Report XML 復元時の POST バインド、UI生成、txtプレビュー、保存対象XML生成を `ReportDefinition` とその周辺抽象だけで回せるようにして、Controller が帳票名以外を知らなくて済む形へ寄せる
7. 帳票固有の ViewModel を増やすのではなく、共通の Report 画面 ViewModel に `ReportDefinition` 由来の metadata を集約する
8. 会社・好きなもののような既存項目だけでなく、今後の `働く場所` 複数選択のような新しい1:Nでも、追加実装が `ReportDefinition` と必要最小限の custom resolver で閉じるかを検証する

### いまの境界メモ

- `GetUserGraph` は「マスター駆動の帳票」にだけ使う。すべての帳票で毎回グラフ生成する前提にしない
- XML駆動の帳票は、保存済みXMLの deserialize を正として表示・出力する
- マスター取得対象か、逆反映対象かは `FieldDefinition` の metadata (`FromMaster` / `AllowReverseReflect`) で定義し、画面表示と処理可否をそこから決める
- 新しい帳票は、まず ReportDefinition を追加し、特殊処理があるときだけ Resolver / Fetcher / ReverseReflector を足す
- `働く場所` のような複数選択は、1:N を表現する Field と、必要最小限の特殊処理抽象を足す方針で扱う