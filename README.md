# XdUnityUI

![introduction](https://user-images.githubusercontent.com/20549024/75539439-8e0e9480-5a5d-11ea-8f92-520e6f7b0b3e.gif)

## 概要

- AdobeXD のアートボードを Unity 用 UI Prefab にコンバートします。
- ![GitHub](https://github.com/itouh2-i0plus/XdUnityUI)

## インストール

1. インストール
     - ダウンロードする場合
         1. https://github.com/itouh2-i0plus/XdUnityUI/releases
         1. 最新バージョンの 「▶Assets」をクリックし XdUnityUI.unitypackage をダウンロードします。
         1. XdUnityUI.unitypackage を Unity にインポートしてください。
         1. /Assets/I0plus/XdUnityUI フォルダが作成されます
         1. AdobeXDプラグインをインストール
            - /Assets/I0plus/XdUnityUI/ForAdobeXD/XdUnityUIExport.xdx をダブルクリックします。
     - Git リポジトリからクローンする場合
         1. Git リポジトリをクローン
                - https://github.com/itouh2-i0plus/XdUnityUI
                    - LFS を利用しています。Git クライアントによっては設定が必要となります。
         1. (クローンフォルダ)/UnityProject を Unity で開きます
                - Assets/I0plus/XdUnityUI 以下が、プラグインフォルダになっています
                - 現在 Unity2019.3、UniversalRenderPipeline のプロジェクトとなっています。
         1. AdobeXDプラグインをインストール
            - /Assets/I0plus/XdUnityUI/ForAdobeXD/XdUnityUIExport.xdx をダブルクリックします。

## クイックスタート

1. AdobeXD サンプルを 開く
     - /Assets/I0plus/XdUnityUI/ForAdobeXD/samples.xd にあります。

1. AdobeXD プラグイン起動
     1. アートボード TestButton 内、ルート直下のレイヤー(例えば yellow-button)を選択状態にします。 - 当プラグインは出力時にこの操作が必ず必要になります。 - 参考：[Edit Context rules · Adobe XD Plugin Reference](https://adobexdplatform.com/plugin-docs/reference/core/edit-context.html)
     1. プラグインメニューから、「XdUnityUI export plugin」をクリック、起動します。
     1. 「Folder」の項目が出力フォルダ先指定です。(クローンしたフォルダ)/UnityProject/I0plus/XdUnityUI/Import フォルダを選択。
     1. 「Export」をクリック。 - 出力時にエラーで止まるケースについて、当記事「問題が起こったとき」を参考にしてください。

1. Unity コンバート
     - Unity ウィンドウをアクティブにするとコンバートが開始されます。
     - 作成された Prefab は Assets/I0plus/CreatedPrefabs に配置されます。
     - 作成された UI 画像は Assets/I0plus/CreatedSprites に配置されます。
         - Slice 処理されています。
     - できた Prefab を Canvas 以下に配置します。

## ChangeLog

### [v0.3.2] - 2020-03-02

サンプル修正
README.md 修正・追記
XdPlugin/main.js コメント修正

### [v0.3.1] - 2020-03-02

TextMeshPro サンプル追加、説明修正
Button サンプル追加
Toggle サンプル追加
README.md 修正

### [v0.3] - 2020-03-01

- unitypackage の作成
- unitypackage からのインストール方法追記

## 動作条件

- Windows で開発しています。
    - Mac での動作確認は現在不十分です。
- Unity2019.3 で開発しています。
- AdobeXD は最新版でテストしています。
    - バージョン：27.1.12.4

## コンバートについて

### 概要

- AdobeXD レイヤー名に対して、コンバートルールが適応されます。
    - コンバートルールは CSS の記述によって定義されています。
    - json ファイルと画像ファイルが出力されます。
    - 出力画像は、指定が無い限りスライス処理されます。
- 出力ファイルを Unity プロジェクト、XdUnityUI/Import フォルダに書き込むことで Unity でのコンバート処理が行われます。
- 指定されたフォルダに Prefab と Sprite が出力されます。

### ルール説明

- AdobeXD レイヤー名で、Unity 上での機能が決まります。

#### image

- 例

    ```
    image
    window-image
    icon-image
    ```

- 説明
    - レイヤー、グループレイヤーに上記のような名前が付いていた場合、そのレイヤーと子レイヤーを合成した画像を生成し、Unity 上で Image コンポーネントが付与されます。
- 注意
    - 子レイヤーも画像としてしまうため、子レイヤーのコンバート処理はされません。

#### button

- 例
    ```
    button
    start-button
    back-button
    ```
- 説明
    - Unity 上で button コンポーネントが付与されます。
- 注意
    - 子レイヤーに image レイヤーが必要です。

#### text

- 例

    ```
    text
    title-text
    name-text
    ```

- 説明
    - テキストレイヤーに上記のような名前をつけることで Unity 上でも Text コンポーネントが 付与 されます。
- 注意
    - AdobeXD で使用したフォントが Unity プロジェクト内、Assets/I0plus/XdUnityUI/Fonts/以下、.ttf か.otf で存在する必要があります。
    - AdobeXD と Unity では、デザイン上の差異があります。(例：カーニング)

#### textmp

- 例

    ```
    textmp
    title-textmp
    name-textmp
    ```

- 説明
    - テキストレイヤーに上記のような名前をつけることで Unity 上でも TextMeshPro コンポーネントが 付与 されます。
- 注意
    - Project Settings > Player > Scripting Define SymbolsにTMP_PRESENTが必要です 
    - AdobeXD で使用したフォントが Unity プロジェクト内、Assets/I0plus/XdUnityUI/Fonts/以下、TextMeshPro フォントアセット が必要です
        - 例: AdobeXD で Open Sans フォント Regular を使用している場合
            - TextMeshPro フォント、ファイル名「Open Sans-Regular SDF.asset」を探します
    - AdobeXD と Unity では、デザイン上の差異があります。(例：カーニング)

- 追記予定
    - layout
    - viewport
    - scrollbar
    - toggle

## 問題が起こったとき

### AdobeXD プラグイン実行中

#### 画像の書き出しに失敗する

- 原因
    - AdobeXD 上の問題かもしれません。調査中です。
- 対策
    1. レイヤーを選択し画像出力の操作をする。
    2. 出力先に XdUnityUI/Import フォルダを選択すると、出力不可になっているか確認。
    3. フォルダを変えて画像出力。
    4. 再度 Import フォルダに出力する。
    5. 上記が成功した場合、プラグインからの出力も成功するようになります。

### Unity コンバート実行中

#### コンバート処理が実行されない

- 原因

    - 失敗後のファイルへの上書きでは、Unity 側がファイルの更新を検知できないため。

- 対応
    - XdUnityUI/Import 内の\_XdUnityUIImport、\_XdUnityUIImport.meta ファイル以外を削除する。
    - もう一度エキスポートする。


#### 文字(Text、TextMeshPro)を扱おうとするとコンバートに失敗する

- 原因
    - フォントが無い可能性があります。
- 対策
    - Console に探そうとして見つからなかったフォントファイル名が出力されます。
    - フォントファイルを場合によってはリネームして、XdUnityUI/Fonts ディレクトリ(\_XdUnityUIFonts ファイルがおいてあるディレクトリ)にコピーしてください。
    
#### TextMeshProのエラーが出力される
- 原因
    - Scripting Define SymbolsにTMP_PRESENTが無い
- 対策
    - Project Settings > Player > Scripting Define SymbolsにTMP_PRESENTを追記します。
        - TextMeshProパッケージをインストールしてもTMP_PRESENTが追記されない場合があるそうです。(v3.0?未確認)

### コンバート結果

#### レスポンシブパラメータが正確にコンバートされない

- 原因1
    - アートボードの「レスポンシブサイズを変更」がONになっていない
- 対策1
    - アートボードを選択し、「レイアウト」>「レスポンシブサイズを変更」をONにしてください。
- 原因2
    - AdobeXD プラグイン実行時、アートボードのサイズを変更しレイヤーのサイズの変化をみてレスポンシブパラメータを取得しています。その際、リピードグリッド内レイヤー等、サイズが変わらないものはレスポンシブパラメータが確定できません。
- 対策2
    - margin-fix プロパティをつかい、明示的に指定してください。
        - 例: start-button {margin-fix: t b l r}
    - AdobeXD Plugin API で、レスポンシブパラメータが取得できるようになりましたら対応します。

#### 背景のイメージコンポーネントがいらない
- 原因
    - アートボードの背景が設定してある
- 対策
    - アートボードを選択し、「アピアランス」>「塗り」のチェックを外してください。

## より使いこなすために

### オリジナル変換ルール

- 変換ルール CSS の編集
    - XDXファイルを変更する場合
        1. XdUnityUIExport.xdx を XdUnityUIExport.zip とリネーム
        1. 解凍し xd-unity.css ファイルを編集する
        1. 再び ZIP 圧縮、拡張子を xdx に変更
        1. プラグイン再インストール
    - AdobeXD 開発フォルダに展開する場合
        1. XdUnityUIExport.xdx を XdUnityUIExport.zip とリネーム
        1. 解凍しフォルダをAdobeXD開発フォルダ(プラグイン>開発版>開発フォルダーを表示)にコピーする
        1. xd-unity.cssを編集
        1. プラグイン再読み込み(プラグイン>開発版>再読み込み)
- 変換ルール CSS の説明
    - 追記予定
- アートボード毎の変換ルール
    - 追記予定

### コンバート時に自作コンポーネントを追加する

- 追記予定

### 9Slice

- 追記予定

## 今後進む方向について

- 目標
    - リリースまで AdobeXD で UI デザインできるようにする。
- メリット
    - デザイナの手に最終版をもたせる。
    - CCLibrary を使い、各種ツールとの連携ができる。
- 課題
    - コンバートでの Prefab 上書きによって、Prefab に対して行った作業(Unity 上で行ったコンポーネント追加、パラメータ調整)が消えてしまう
        - 対応策
            - コピーして使用する。
            - Prefab Variant で、追加作業の退避。
                - 名前の変更で作業が消えてしまう。
                - Variant ファイル内には作業分残っている模様(これを復帰できないか調査中)。
    - 同じ Sprite 画像が大量にできてしまう。
        - 対応中
            - Sprite 画像を比較して、同じであれば結合するツールを開発中。
    - 9Slice
        - 対応中
    - コンポーネントのステート
        - https://helpx.adobe.com/jp/xd/help/create-component-states.html
        - AdobeXD Plugin API で取得できるようになりましたら、対応します。
- その他
    - 調査中
        - UXML へコンバートできないか。
        - DOTS モード用 UI が作成できないか。

## 謝辞

- @kyubuns 様 (https://kyubuns.dev)
- Baum2 (https://github.com/kyubuns/Baum2)

### お世話になっております ありがとうございます
