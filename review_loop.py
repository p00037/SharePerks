import json
import shutil
import subprocess
import sys
from pathlib import Path

MAX_LOOPS = 3
PASS_SCORE = 85

REVIEW_SCHEMA = {
    "type": "object",
    "properties": {
        "approved": {"type": "boolean"},
        "score": {"type": "integer", "minimum": 0, "maximum": 100},
        "blocking_issues": {"type": "integer", "minimum": 0},
        "major_issues": {"type": "integer", "minimum": 0},
        "minor_issues": {"type": "integer", "minimum": 0},
        "summary": {"type": "string"},
        "must_fix": {
            "type": "array",
            "items": {"type": "string"}
        }
    },
    "required": [
        "approved",
        "score",
        "blocking_issues",
        "major_issues",
        "minor_issues",
        "summary",
        "must_fix"
    ],
    "additionalProperties": False
}


def find_codex_command() -> str:
    candidates = [
        "codex.cmd",
        "codex.exe",
        "codex",
        r"C:\Users\boru1\AppData\Roaming\npm\codex.cmd",
    ]

    for candidate in candidates:
        found = shutil.which(candidate)
        if found:
            return found

        candidate_path = Path(candidate)
        if candidate_path.exists():
            return str(candidate_path)

    raise FileNotFoundError(
        "codex コマンドが見つかりません。"
        "Codex CLI が未インストール、または PATH に入っていない可能性があります。"
    )


def run_cmd(cmd: list[str]) -> subprocess.CompletedProcess:
    try:
        return subprocess.run(
            cmd,
            text=True,
            capture_output=True,
            encoding="utf-8"
        )
    except FileNotFoundError as ex:
        raise FileNotFoundError(
            f"外部コマンドの起動に失敗しました: {cmd[0]}\n"
            "Codex CLI のコマンド解決に失敗しています。"
        ) from ex


def write_text(path: Path, text: str) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(text, encoding="utf-8")


def write_json(path: Path, data: dict) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(data, ensure_ascii=False, indent=2), encoding="utf-8")


def build_implement_prompt(task: str, must_fix: list[str] | None = None) -> str:
    must_fix_text = ""
    if must_fix:
        joined = "\n".join(f"- {item}" for item in must_fix)
        must_fix_text = f"""

前回レビューで必ず修正すべき点:
{joined}

上記の must_fix を優先して直してください。
指摘に関係ない大きなリファクタはしないでください。
"""

    return f"""
あなたは Implementer です。
このリポジトリの既存方針に従い、最小変更で対応してください。
必要なら関連ファイルを調査し、実装後は必要最小限の確認を行ってください。

今回の作業:
{task}
{must_fix_text}

完了したら、次の形式で簡潔に報告してください:
- 変更概要
- 変更した主なファイル
- 実行した確認内容
""".strip()


def build_review_prompt(task: str) -> str:
    return f"""
あなたは Reviewer です。
あなたの役割はレビューのみで、実装はしません。

次の観点で、現在の作業ツリーをレビューしてください。
- 要件を満たしているか
- 既存挙動を壊していないか
- Null/例外処理の問題がないか
- エラー表示やバリデーション表示が失われていないか
- テスト不足がないか
- 保守性が著しく悪化していないか

タスク:
{task}

重要:
- 軽微な好みの指摘より、正しさや安全性を優先してください。
- 必ず JSON Schema に従って返答してください。
- must_fix には次回の修正で必須のものだけを書いてください。
""".strip()


def codex_implement(prompt: str, log_path: Path) -> bool:
    codex_cmd = find_codex_command()
    cmd = [
        codex_cmd,
        "exec",
        "--full-auto",
        "--output-last-message",
        str(log_path),
        prompt
    ]
    result = run_cmd(cmd)

    print("=== IMPLEMENT COMMAND ===")
    print(" ".join(cmd))
    print("=== IMPLEMENT STDOUT ===")
    print(result.stdout)
    print("=== IMPLEMENT STDERR ===")
    print(result.stderr)

    return result.returncode == 0


def codex_review(prompt: str, schema_path: Path, output_path: Path) -> dict:
    codex_cmd = find_codex_command()
    cmd = [
        codex_cmd,
        "exec",
        "--output-schema",
        str(schema_path),
        "--output-last-message",
        str(output_path),
        prompt
    ]
    result = run_cmd(cmd)

    print("=== REVIEW COMMAND ===")
    print(" ".join(cmd))
    print("=== REVIEW STDOUT ===")
    print(result.stdout)
    print("=== REVIEW STDERR ===")
    print(result.stderr)

    if result.returncode != 0:
        raise RuntimeError("Reviewer run failed")

    if not output_path.exists():
        raise RuntimeError("Review output file was not created")

    return json.loads(output_path.read_text(encoding="utf-8"))


def is_pass(review: dict) -> bool:
    return (
        review.get("approved") is True
        and review.get("score", 0) >= PASS_SCORE
        and review.get("blocking_issues", 0) == 0
    )


def main() -> int:
    task = """
RewardItemsController の List メソッドと、その既存ユニットテストをレビューして修正してください。

対象:
- RewardItemsController.List()
- RewardItemsControllerTests の List 系テスト

やってほしいこと:
1. List メソッドの現在の仕様を読み取り、テスト観点が不足している箇所を洗い出す
2. 既存テストに実装との不整合があれば修正する
3. 必要であればテストケースを追加する
4. テストコードは MSTest + Moq の既存スタイルに合わせる
5. 変更理由を簡潔に説明する

特に確認してほしい点:
- コントローラ実装では _unitOfWork.RewardItems.ListAsync() をどのシグネチャで呼んでいるか
- 既存テストの Mock.Setup / Verify が実際の呼び出しと一致しているか
- 空リストだけでなく、通常の複数件データ返却ケースのテストが必要か
- null 戻り値テストを残すべきかどうか

期待する成果物:
- 修正後のテストコード全文
- 追加・修正した観点の箇条書き
- なぜその修正が必要かの説明
- コメントは日本語に変更
""".strip()

    base_dir = Path(".")
    logs_dir = base_dir / "logs"
    schema_path = logs_dir / "review_schema.json"
    write_json(schema_path, REVIEW_SCHEMA)

    must_fix: list[str] | None = None

    for i in range(1, MAX_LOOPS + 1):
        print(f"\n========== LOOP {i} ==========")

        implement_prompt = build_implement_prompt(task, must_fix)
        write_text(logs_dir / f"loop_{i}_implement_prompt.txt", implement_prompt)

        ok = codex_implement(
            implement_prompt,
            logs_dir / f"loop_{i}_implement_result.txt"
        )
        if not ok:
            print("Implementer failed.")
            return 1

        review_prompt = build_review_prompt(task)
        write_text(logs_dir / f"loop_{i}_review_prompt.txt", review_prompt)

        review = codex_review(
            review_prompt,
            schema_path,
            logs_dir / f"loop_{i}_review_result.json"
        )

        print("=== REVIEW JSON ===")
        print(json.dumps(review, ensure_ascii=False, indent=2))

        if is_pass(review):
            print("\nレビュー合格です。")
            return 0

        must_fix = review.get("must_fix", [])
        if not must_fix:
            print("\n不合格ですが must_fix が空です。ここで停止します。")
            return 2

    print("\n最大ループ回数に達しました。")
    return 3


if __name__ == "__main__":
    sys.exit(main())