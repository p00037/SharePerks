# 管理者向け 商品ポイント・表示順一括編集 設計

## 背景

管理者が既存の優待商品の必要ポイントと表示順を、一覧形式でまとめて編集できる画面を追加する。商品名、商品コード、画像などの既存マスタ情報は対象外とし、変更範囲をポイントと表示順に限定する。

## 対象範囲

- 管理者画面に「ポイント・表示順 一括編集」ページを追加する。
- 既存商品の `RequiredPoints` と `DisplayOrder` のみ編集可能にする。
- 変更された行だけを一括保存する。
- 編集セルは `RequiredPoints` と `DisplayOrder` の2列だけとし、矢印キーで上下左右に移動できるようにする。
- DBスキーマ変更は行わない。

## UI

- ルートは `/reward-items/bulk-edit` とする。
- 管理者ナビに一括編集画面へのリンクを追加する。
- グリッドには商品コード、商品名、公開状態、必要ポイント、表示順を表示する。
- 商品コード、商品名、公開状態は読み取り専用とする。
- 必要ポイントと表示順は数値入力とする。
- 差分がない場合、保存ボタンは無効にする。
- 再読み込みボタンでAPIから再取得し、未保存の変更を破棄する。
- 保存成功後は再取得、または保存結果を反映して差分状態をクリアする。

## キーボード操作

- 対象は編集可能セルのみ。
- `ArrowLeft` / `ArrowRight` は同一行内の必要ポイントと表示順を移動する。
- `ArrowUp` / `ArrowDown` は同一列の前後行へ移動する。
- 範囲外へ移動するキー入力は無視する。
- 入力値変更中でも矢印キーでセル移動できることを優先する。

## API

- `PUT api/admin/items/bulk-order-points` を追加する。
- リクエストDTOは `RewardItemBulkUpdateRequestDto` と `RewardItemBulkUpdateRowDto` とする。
- 行DTOは `ItemId`, `RequiredPoints`, `DisplayOrder` を持つ。
- サーバー側は `RequiredPoints >= 1`、`DisplayOrder >= 0` を検証する。
- 対象 `ItemId` が存在しない場合は `ValidationProblem` を返す。
- 更新は Repository と UnitOfWork 経由で行い、`SaveChangesAsync()` は1回にまとめる。
- レスポンスは更新後の商品一覧を返す。

## テスト

- `Admin.UnitTests` に一括更新APIのテストを追加する。
- 正常系では複数商品の `RequiredPoints` と `DisplayOrder` が更新され、`SaveChangesAsync()` が1回呼ばれることを確認する。
- 不正値では `ValidationProblem` が返ることを確認する。
- 存在しない `ItemId` が含まれる場合に `ValidationProblem` が返ることを確認する。
- UIはビルドでコンパイル確認し、可能ならブラウザで矢印キー移動を手動確認する。
