# Maxine AR SDK VRCFT モジュール

Maxine AR SDK の ExpressionApp から送られてくる UDP パケットをそのまま VRCFaceTracking に流し込み、Web カメラで取得した表情・頭部情報を OSC 経由で VRChat に橋渡しするためのモジュールです。

## 使い方
1. Maxine AR SDK の ExpressionApp をビルドし、デフォルト設定（127.0.0.1:9000）で起動します。
2. `VRCFT-MaxineAR` ディレクトリ内の C# ファイルを参照して `VRCFaceTracking.MaxineAR` モジュールをビルドし、生成された DLL を VRCFaceTracking の `CustomLibs` に配置します。
3. 必要に応じて実行ファイルと同じディレクトリに `MaxineARConfig.json` を置き、`host` や `port` を上書きします。

```json
{
  "Host": "127.0.0.1",
  "Port": 9000
}
```

## 実装メモ
- ExpressionApp から送られてくる 53 個の表情係数と頭部クォータニオン・位置をパースし、`UnifiedTracking` に直接マップしています。
- 目線は Maxine の `eyeLook*` 値を左右それぞれの水平方向・垂直方向ベクトルに変換、開閉度は `eyeBlink` と `eyeSquint` を合算して算出しています。
- 頭部の回転はクォータニオンからオイラー角に展開し、90 度で正規化した値を VRCFT に渡しています。
