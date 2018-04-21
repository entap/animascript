# AnimaScript
## 概要
ゲームやマルチメディア用のスクリプトエンジンの基盤。
IfとかJumpとかの基本命令以外、何も実装していないがゆえに、様々なゲームにバインドできます。
そのうち、色んな機能を入れるかもしれない。

https://entap.github.io/animascript/index.html

## DLLのダウンロード(.NET framework 3.5)
https://entap.github.io/animascript/Expr.dll
https://entap.github.io/animascript/AnimaScript.dll

## 文法
### コメント

    // コメント
    /*
    複数行コメント
    複数行コメント
    複数行コメント
    */

### 命令

    [tagname param1="value1" param2="value2"]

### ラベル

    :labelName

### メッセージ

テキストをそのまま書くとメッセージ（キャラのセリフ等）としてコンパイルされます。

    あいうえおあいうえお

### 実装済みの命令

条件分岐

    [if exp="条件"]
    [elif exp="条件"]
    [elif exp="条件"]
    [else]
    [endif]

演算

    [set var="varname" exp="1+2"]

移動

    [jump to="移動先ラベル名"]

