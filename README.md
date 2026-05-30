# interactive-evacuation-route-simulation(話し合い型避難経路体験システム）
[RoboCup Rescue Simulation](https://www.robocup.or.jp/robocup-rescue/simulation/)を題材にした親子での体験を想定したコンテンツ
体験後の主な効果として防災意識の向上、避難経路計画の重要性の理解など

## 使用技術
![Unity](https://img.shields.io/badge/Unity-6000.0.47f1-blue?logo=unity)
![C#](https://img.shields.io/badge/C%23-Programming-239120?logo=csharp&logoColor=white)

## 特徴
- RRSのシミュレーションログを使用し，3Dで描画
- ゲームコントローラを使用した操作が可能
- setting.jsonからapp後の設定変更可能

## インストール方法
1. リポジトリをクローンします。
   ```sh
   git clone https://github.com/rrs-viewer-2025/viewer.git
   ```
2. Unity でプロジェクトを開きます。
3. 必要な依存関係がすべてインストールされていることを確認してください。

## 機能一覧

本リポジトリは RRS のシミュレーションログを3Dで描画し、ゲームコントローラで操作できます。詳細なスクリプト一覧は本文末の「詳細スクリプト一覧」にまとめています。

## 詳細スクリプト一覧

以下は `Assets/Scripts` フォルダの主要スクリプトと簡単な説明です。必要なら参照リンクや更に詳しい説明を追加します。

**ルート（主要スクリプト）**
- **AmbulanceteamLoader.cs**: 救急チームの読み込み・生成を行うローダー
- **BlockadeLoader.cs**: 封鎖（バリケード）オブジェクトの読み込み
- **BuildingLoader.cs**: 建物データの読み込み
- **ComplexBuildingGenerator.cs**: 複雑な建物の生成ロジック
- **GameData.cs**: シミュレーションのデータ構造・読み書き
- **Globaldata.cs**: 全体設定やグローバル変数の管理
- **MainPathDraw.cs**: メイン経路の描画処理
- **PlaneManager.cs**: 地面／平面関連の管理
- **RoadMesh.cs**: 道路メッシュの生成・管理
- **StepManager.cs / Timer.cs / StepDisplay.cs**: シミュレーションのステップ制御・表示

**フォルダ別（主なもの）**
- **Assets/Scripts/Civiian/**
   - **CivilianLoader.cs**: 市民（Civilian）の読み込み・生成
   - **CivilianAnimation.cs**: 市民アニメーション制御
   - **CivilianState.cs**: 市民の状態管理（行動状態など）

- **Assets/Scripts/controller/**
   - **LogitechDualActionHID.cs / LogitechDualActionInputReport.cs**: ゲームパッド（Logitech）入力処理

- **Assets/Scripts/Map/**
   - **MapLoader.cs**: マップデータの読み込み
   - **MapNameDisplay.cs**: マップ名やラベルの表示

- **Assets/Scripts/Play/**
   - **JoyconManager.cs / Joycon.cs / HIDapi.cs**: Joy-Con や HID デバイスの管理
   - **PlayerMove.cs / PlayerPosition.cs**: プレイヤーの移動・位置管理
   - **PlayerStartPosition.cs / PlayerTrail.cs**: 開始位置と軌跡表示

- **Assets/Scripts/SelectPathScene/**
   - **Manager.cs / PathDrawer.cs / Building.cs / Road.cs / Refuge.cs**: 経路選択シーンの管理、経路描画、建物・避難所表現
   - **MinimapCameraFitter.cs / Gizmo.cs**: ミニマップや描画補助ユーティリティ

- **Assets/Scripts/ResultScene/**
   - **ResultManager.cs / ResultPathDraw.cs / ResultRoad.cs**: 結果表示シーンの制御と描画

上記は主要スクリプトのサマリです。細かいファイルやユーティリティ（MultiObjectMover.cs、SetEntityID.cs、RefugeCamera.cs など）も多数あります。
特定のスクリプトの詳細説明やファイルへの直接リンクを追加希望であれば指示ください。

## コントリビューション
1. リポジトリをフォーク。
2. 新しいブランチを作成 (`feature-branch`)
3. 変更をコミット (`git commit -m '新機能追加'`)
4. ブランチをプッシュ (`git push origin feature-branch`)
5. プルリクエストを作成。

## ライセンス
このプロジェクトは MIT ライセンスの下で提供されています。詳細は [LICENSE](LICENSE) ファイルをご確認ください。

