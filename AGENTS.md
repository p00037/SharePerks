# AGENTS.md (SharePerks)

このリポジトリに対して作業するエージェント（Codex等）は、本ドキュメントのルールに従うこと。

---

## 1. 言語 / 出力方針（必須）
- 会話・設計意図・レビューコメント・プルリクエスト本文・コミットメッセージは **日本語** で記述する。
- 実装言語は **C# / .NET（ASP.NET Core + EF Core）** を前提とする。
- コードの命名は英語（型/メソッド: PascalCase、ローカル/引数: camelCase）を基本とする。
- ユーザー向け文言（画面ラベル、バリデーション、例外メッセージ等）は **日本語** を優先する（既存方針/要件があればそれに従う）。

---

## 2. ソリューション前提（このプロジェクト固有）
- アプリは **株主向け（User）** と **管理者向け（Admin）** があり、UI/Server プロジェクトは分離されている前提で作業する。
- 認証/認可は ASP.NET Core Identity を利用し、ロールは `Admin` / `User` を基本とする（既存定数・既存シード処理を優先）。
- ログは Serilog を利用し、既存のログ出力先・プロパティ方針を踏襲する。

---

## 3. データアクセス方針（必須）: Repository + UnitOfWork
### 3.1 原則
- **DbContext をアプリ層・UI層から直接参照しない。**
- DBアクセスは **Repository パターン** と **UnitOfWork パターン** を通す。
- 参照系/更新系ともに「アプリケーションサービス（UseCase）」→「Repository」→「EF Core」の方向に依存する。
- トランザクション境界は原則として **UnitOfWork** が担う。

### 3.2 Repository のルール
- Repository は集約/エンティティ単位で作成し、用途が曖昧な “汎用Repository乱用” は避ける。
- 取得は `GetByIdAsync` / `FindAsync` / `ListAsync` など意図が分かる名前を使う。
- `IQueryable` を外へ返さない（クエリが呼び出し元へ漏れるのを防ぐ）。
- 追加・更新・削除は `Add` / `Update` / `Remove` を用意し、永続化は UnitOfWork の `SaveChangesAsync` で行う。

### 3.3 UnitOfWork のルール
- 1つのユースケース（1リクエスト）で必要な Repository を UnitOfWork から取得して使う。
- 永続化は `await unitOfWork.SaveChangesAsync()` を原則 1回に集約する。
- 複数操作をまとめる必要がある場合は、UnitOfWork 側でトランザクションを扱う（既存実装に合わせる）。

### 3.4 例（方針の意図）
- ✅ OK: `RewardItemService` → `IUnitOfWork.RewardItems.Add(...)` → `SaveChangesAsync()`
- ❌ NG: `RewardItemService` が `DbContext.RewardItems` を直接触る
- ❌ NG: Repository が `IQueryable` を返して呼び出し側で `Where` を重ねる

---

## 4. バリデーション / エラー返却（このプロジェクト固有）
- 入力検証は「クライアント側（UI）＋サーバー側（API）」の二段構えを基本とする。
- サーバー側の検証エラーは **ProblemDetails / ValidationProblemDetails** 形式を優先し、
  クライアントではフィールド単位のエラー表示ができる形にする（既存の `RewardItemValidationException` 等があれば踏襲）。
- “DBを使う検証（重複チェック等）” は入力途中で毎回呼ばず、原則「登録（Submit）時」にまとめて行う（既存方針に従う）。

---

## 5. コーディング規約（C#）
- `#nullable enable` 前提で null 安全に書く。
- async は原則 `Task` / `Task<T>` を返し、I/Oは async を優先する。
- 例外は “本当に異常” に限定し、入力不備は検証結果として返す（上記方針）。
- 既存の DTO / Entity / 命名 / フォルダ構成に合わせる（新規導入は最小限）。

---

## 6. プルリクエストの書き方（必須テンプレ / 日本語）
PR本文は必ず以下のテンプレで作成する（見出しを省略しない）。

### Summary
- 変更の目的を1〜3行で要約

### Changes
- 主要な変更点を箇条書き（3〜7個）

### Screenshots / UI
- UI変更がある場合のみ添付（なければ「なし」）

### Testing
- 実行した確認内容（例: `dotnet test`、手動確認手順）
- 実行できない場合は理由と代替確認を書く

### Notes / Risks
- 影響範囲、マイグレーション、設定変更、互換性、既知の課題

---

## 7. 作業の進め方（重要）
- 既存コードの設計・例外設計・検証方針・ログ方針を最優先で踏襲する。
- 1PRは機能単位で分割し、無関係なリファクタは混ぜない。
- DBスキーマ変更が必要なら migration を追加し、Notes に影響と適用手順を書く。

## 8. マイグレーション運用（このプロジェクト固有 / 必須）
- DB マイグレーションの適用は **手動で実行** する運用とする。
- そのためエージェントは以下を **勝手に変更しない**：
  - 既存の migration ファイル（`Migrations/` 配下など）
  - migration 実行設定（起動時自動適用、HostedServiceでの適用、CIでの自動適用 等）
  - migration 用のスクリプト/コマンド、実行手順を自動化するコード
- スキーマ変更が必要な場合は **変更内容だけ実装**し、PR の `Notes / Risks` に以下を記載する：
  - 変更理由（どの機能のためか）
  - 追加/変更されるテーブル・カラム・インデックスの概要
  - 「マイグレーション作成・適用は手動で行う」旨（手順があれば簡潔に）