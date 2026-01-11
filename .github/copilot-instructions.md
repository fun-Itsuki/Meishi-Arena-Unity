<!-- Meishi-Arena-Unity: Copilot instructions for coding agents -->
# コーディングエージェント向け要点

目的: このファイルはリポジトリ固有の構造・ワークフロー・コーディング慣習を短くまとめ、AIエージェントがすぐに生産的になれるようにする。

- **プロジェクト種別**: Unity プロジェクト（`Assets/`, `ProjectSettings/`, `Packages/` を持つ）。エディタバージョンは `ProjectSettings/ProjectVersion.txt` を参照する。
- **主要構成**:
  - `Assets/Scripts/` : MonoBehaviour ベースの C# スクリプト群。シーン単位の「Manager」クラス（例: `TitleSceneManager.cs`, `ResultManager.cs`）が多い。
  - `Assets/InputSystem_Actions.inputactions` : Unity Input System のアクション定義（キーバインドやアクション名のソース）。必要に応じてこのファイルを編集/参照して入力処理を変更する。
  - `Assets/Scenes/` と `Assets/Prefabs/` : シーン構成・プレハブはこれらを参照して変更する。
  - `ProjectSettings/` : ビルド/エディタ設定の真のソース。エディタ互換性や URP 等の設定はここを確認する。

- **命名/設計規約（発見済み）**:
  - シーン制御ロジックは `*Manager`（例: `ScoreManager`, `BattleUIManager`）でまとめられている。
  - UI / シーンの遷移ロジックは専用の MonoBehaviour に置かれている（例: `MoveToCardScene.cs`）。
  - 物理的なプレハブ操作やカードロジックは `Card*` 系ファイルにまとまる（例: `CardBattleLogic.cs`, `CardScrollController.cs`）。

- **典型的な変更箇所の例**:
  - 入力の追加/変更 → `Assets/InputSystem_Actions.inputactions` を更新、対応するスクリプトで `InputAction` を参照。
  - シーン遷移やUI更新 → `Assets/Scripts/*Manager.cs` を編集。
  - アセット移動時は `.meta` を維持し、GUID が保持されるようにする（Unity のアセット管理要件）。

- **ビルド / 実行 / デバッグ**:
  - エディタで確認する：Unity Hub で `ProjectSettings/ProjectVersion.txt` のバージョンに合うエディタを使って開く。
  - コマンドラインビルド（一般例）：
    ```bash
    Unity -batchmode -nographics -projectPath "<path-to-repo>" -buildTarget <platform> -executeMethod <BuildClass.Method> -quit
    ```
    ※このリポジトリにはカスタムビルドスクリプトは検出されなかったため、具体的な `-executeMethod` は現場で確認してください。
  - デバッグ: `Assembly-CSharp.csproj` / `.slnx` が生成されるため、Visual Studio / Rider でソリューションを開き、通常の Unity デバッグを行う。

- **テスト**:
  - リポジトリ内に自動テストのプロジェクトは見つかっていません。ユニットテストの追加は `Assets/Tests/`（Unity Test Framework）を採用すること。

- **外部依存と統合ポイント**:
  - Unity Input System（`Assets/InputSystem_Actions.inputactions`）
  - URP 関連設定（`ProjectSettings/URPProjectSettings.asset`）
  - TextMesh Pro（アセットメタファイルから使用の痕跡あり）

- **エージェント向け作業方針（具体的）**:
  1. 変更前に `ProjectSettings/ProjectVersion.txt` を確認し、エディタ互換性を保つ。
  2. スクリプト修正は `Assets/Scripts/` 内で行い、クラス名とファイル名を一致させる（既存規約に従う）。
  3. シーンやプレハブを変更する場合は `.meta` を含めて管理し、GUID を壊さない。
  4. 入力関連を変更する場合は `Assets/InputSystem_Actions.inputactions` と対応スクリプトの両方を更新する。
  5. 大きな変更（シーン構成やビルド設定）は事前に user に確認を求める。

- **参照ファイル（作業の起点）**:
  - `Assets/Scripts/`（スクリプト群）
  - `Assets/InputSystem_Actions.inputactions`（入力定義）
  - `ProjectSettings/ProjectVersion.txt`（エディタ推奨バージョン）

疑問点や補足があれば教えてください。必要なら、このファイルを拡張して「よく使う編集パターン」「よくあるバグの原因」などを追加します。
