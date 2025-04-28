# viewer
viewer制作リポジトリです。
このリポジトリにはRRS-ActionViewerについてのソースが記載されています。

## 使用技術
![Unity](https://img.shields.io/badge/Unity-2022.3.13f1-blue?logo=unity)
![C#](https://img.shields.io/badge/C%23-Programming-239120?logo=csharp&logoColor=white)

## 特徴
- RRSのシミュレーションログを使用し，3Dで描画
- キーボード，マット，joy-conを使用した操作が可能
- setting.jsonからapp後の設定変更可能

## インストール方法
1. リポジトリをクローンします。
   ```sh
   git clone https://github.com/yourusername/yourrepository.git
   ```
2. Unity でプロジェクトを開きます。
3. 必要な依存関係がすべてインストールされていることを確認してください。

## 機能一覧

### ゲーム基本管理
- `Setting.cs` : 設定ファイル読み込み
- `StepManager.cs` : ステップ進行管理
- `StepDisplay.cs` : ステップ数表示
- `Title_button.cs` : タイトル画面ボタン制御

### マップ・オブジェクトロード
- `BuildingLoader.cs` : 建物データロード
- `CivilianLoader.cs` : 市民データロード
- `BlockadeLoader.cs` : 障害物ロード
- `AmbulanceteamLoader.cs` : 救急隊ロード
- `FirebrigadeLoader.cs` : 消防隊ロード
- `PoliceforceLoader.cs` : 警察隊ロード
- `RefugeLoader.cs` : 避難所ロード
- `MapLoader.cs` : マップ全体ロード
- `SetEntityID.cs` : エンティティID付与
- `CitizenCounter.cs` : 市民数カウント

### キャラクター操作・アニメーション
- `PlayerMove.cs` : プレイヤー移動
- `CivilianAnimation.cs` : 市民アニメーション
- `CivilianState.cs` : 市民ステータス管理
- `MultiObjectMover.cs` : 複数オブジェクト操作

### カメラ制御
- `CameraController.cs` : メインカメラ操作
- `OverviewCamera.cs` : 俯瞰カメラ操作
- `Minimap.cs` : ミニマップ表示
- `MapNameDisplay.cs` : マップ名UI表示

### 外部デバイス連携
- `JoyconManager.cs` : Joy-Con管理
- `Joycon.cs` : Joy-Con操作
- `MatAction.cs` : マット入力制御
- `MicController.cs` : マイク入力取得
- `HIDapi.cs` : HID通信処理

### その他システム
- `ComplexBuildingGenerator.cs` : 複雑な建物生成
- `mesh.cs` : メッシュ制御
- `RoadMesh.cs` : 道路メッシュ生成
- `URN.cs` : URN定義

## コントリビューション
1. リポジトリをフォーク。
2. 新しいブランチを作成 (`feature-branch`)
3. 変更をコミット (`git commit -m '新機能追加'`)
4. ブランチをプッシュ (`git push origin feature-branch`)
5. プルリクエストを作成。

## ライセンス
このプロジェクトは MIT ライセンスの下で提供されています。詳細は [LICENSE](LICENSE) ファイルをご確認ください。

