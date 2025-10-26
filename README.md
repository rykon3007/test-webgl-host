# # Git LFS 初期設定

## 🔧 初回セットアップ手順（Git LFS）

以下のコマンドで Git LFS を有効化し、必要なアセットを取得してください：

```
# Git LFS 初期化
git lfs install
# 追跡ルール（.gitattributes に依存）
git lfs pull
```

## .gitignoreを更新した場合
すでに Git にトラッキングされていた不要ファイルを除外対象にするには、以下のコマンドを実行してください

```
git rm -r --cached .
git add .
git commit -m "Apply updated .gitignore"
```
## .gitattributes を変更した場合
新しい拡張子をLFSで管理したい場合などで、.gitattributesの更新したときにそれを反映するには以下のコマンドを実行してください

```
git lfs track "*.新しい拡張子"
git add .gitattributes
git add --renormalize .
git commit -m "Update .gitattributes and re-track with LFS"
```