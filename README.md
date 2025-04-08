# setogura
瀬戸蔵に出展用のゲーム制作リポジトリです。
このリポジトリには、ゲーム内のさまざまなエンティティを管理するためのスクリプトが含まれています。

## 特徴
- 警察、消防、救急、避難所などのゲームエンティティをロード・管理。
- ゲーム内の市民の数をカウント。
- タイトル画面のボタン操作を処理。

## インストール方法
1. リポジトリをクローンします。
   ```sh
   git clone https://github.com/yourusername/yourrepository.git
   ```
2. Unity でプロジェクトを開きます。
3. 必要な依存関係がすべてインストールされていることを確認してください。

## 使用方法
- `Scripts` フォルダ内のスクリプトは、ゲーム内のさまざまな要素を管理するために使用されます。
- **BuildingLoader.cs**: 建物のロードと管理を担当。
- **PoliceforceLoader.cs**: 警察ユニットを管理。
- **FirebrigadeLoader.cs**: 消防隊ユニットを管理。
- **AmbulanceteamLoader.cs**: 救急隊ユニットを管理。
- **RefugeLoader.cs**: 避難所のロードと追跡を担当。
- **CitizenCounter.cs**: 市民の数をカウント。
- **SetEntityIDtoCivilian.cs**: 市民に一意のIDを割り当てる。
- **Title_button.cs**: タイトル画面のボタン操作を処理。

## コントリビューション
1. リポジトリをフォーク。
2. 新しいブランチを作成 (`feature-branch`)
3. 変更をコミット (`git commit -m '新機能追加'`)
4. ブランチをプッシュ (`git push origin feature-branch`)
5. プルリクエストを作成。

## ライセンス
このプロジェクトは MIT ライセンスの下で提供されています。詳細は [LICENSE](LICENSE) ファイルをご確認ください。

