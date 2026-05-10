# Reward Item Bulk Points Order Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 管理者が既存商品の必要ポイントと表示順だけを一覧で編集し、変更行のみ一括保存できる画面を追加する。

**Architecture:** サーバーは専用の一括更新APIを追加し、Repository + UnitOfWork 経由で更新する。クライアントは専用ページを追加し、初期値との差分だけをAPIに送る。矢印キー移動は編集可能な2列の入力要素だけを対象にする。

**Tech Stack:** ASP.NET Core MVC、Blazor WebAssembly、MudBlazor、MSTest、Moq

---

## File Map

- `Shared/Dtos/RewardItemBulkUpdateRequestDto.cs`: 一括更新リクエストDTO。
- `SharePerks/Admin/Controllers/RewardItemsController.cs`: 一括更新APIを追加。
- `SharePerks/Admin.UnitTests/Controllers/RewardItemsControllerTests.cs`: APIテストを追加。
- `SharePerks/Admin.Client/Models/RewardItemBulkEditRow.cs`: UI用の編集行モデル。
- `SharePerks/Admin.Client/Pages/RewardItems/RewardItemBulkEdit.razor`: 一括編集画面。
- `SharePerks/Admin.Client/Pages/RewardItems/RewardItemBulkEdit.razor.cs`: 画面ロジック、差分検出、保存処理。
- `SharePerks/Admin.Client/Services/Api/Interface/IRewardItemApiClient.cs`: 一括更新メソッドを追加。
- `SharePerks/Admin.Client/Services/Api/RewardItemApiClient.cs`: 一括更新API呼び出しを追加。
- `SharePerks/Admin.Client/Layout/NavMenu.razor`: 管理者ナビにリンク追加。

## Tasks

### Task 1: API DTO と正常系テスト

- [ ] `Admin.UnitTests` に、一括更新APIが複数商品の `RequiredPoints` と `DisplayOrder` を更新し、`SaveChangesAsync` を1回呼ぶテストを追加する。
- [ ] テストを実行し、未実装のため失敗することを確認する。
- [ ] `Shared/Dtos/RewardItemBulkUpdateRequestDto.cs` を追加する。
- [ ] `RewardItemsController` に `PUT api/admin/items/bulk-order-points` を追加する。
- [ ] テストを実行し、成功することを確認する。

### Task 2: API バリデーションテスト

- [ ] 不正値のテストを追加する。`RequiredPoints < 1` または `DisplayOrder < 0` で `ValidationProblem` を返す。
- [ ] 存在しない `ItemId` のテストを追加する。対象商品がなければ `ValidationProblem` を返す。
- [ ] テストを実行し、未実装分が失敗することを確認する。
- [ ] `RewardItemsController` に検証処理を追加する。
- [ ] テストを実行し、成功することを確認する。

### Task 3: クライアントAPI

- [ ] `IRewardItemApiClient` に一括更新メソッドを追加する。
- [ ] `RewardItemApiClient` に `PutJsonAsync` 相当の呼び出しを追加する。既存基底にJSON PUTがなければ `HttpClient.PutAsJsonAsync` を使い、既存のエラー処理パターンに合わせる。
- [ ] `Admin.Client` をビルドしてコンパイル確認する。

### Task 4: 一括編集画面

- [ ] `RewardItemBulkEditRow` を追加し、初期値と現在値、変更有無を持たせる。
- [ ] `RewardItemBulkEdit.razor` / `.razor.cs` を追加する。
- [ ] 商品一覧を読み込み、編集可能列は必要ポイントと表示順だけにする。
- [ ] 変更行だけを `RewardItemBulkUpdateRequestDto` に変換して保存する。
- [ ] 保存成功後に再読み込みし、差分状態をクリアする。

### Task 5: 矢印キー移動とナビ

- [ ] 編集セルに安定した `id` を付与する。
- [ ] `ArrowLeft` / `ArrowRight` / `ArrowUp` / `ArrowDown` で編集可能セル間を移動する。
- [ ] JSは使わず、Blazor 側で移動先IDを組み立てて `ElementReference.FocusAsync()` または必要最小限のJSを使う。既存でJSパターンがなければ `ElementReference.FocusAsync()` を優先する。
- [ ] `NavMenu.razor` にリンクを追加する。

### Task 6: Verification

- [ ] `dotnet test SharePerks/Admin.UnitTests/Admin.UnitTests.csproj -v minimal` を実行する。
- [ ] `dotnet build SharePerks/Admin.Client/Admin.Client.csproj -v minimal` を実行する。
- [ ] `dotnet build SharePerks.slnx -v minimal` を実行する。
- [ ] `dotnet test SharePerks.slnx -v minimal --no-restore` を実行する。
