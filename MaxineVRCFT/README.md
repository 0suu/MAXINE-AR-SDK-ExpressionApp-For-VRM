# VRCFaceTracking Maxine AR SDK モジュール

NVIDIA Maxine AR SDK の ExpressionApp 互換の UDP 出力を受け取り、VRCFaceTracking 向けの UnifiedExpressions に変換するモジュールです。Webcam でキャプチャした表情と頭部姿勢をそのまま VRChat へ転送できます。

## 使い方
1. `MaxineConfig.json` で受信するホスト・ポートを設定します（デフォルトは `127.0.0.1:9000`）。
2. Maxine AR SDK の ExpressionApp で UDP 出力を有効にし、上記ポートへ送信します。
3. ビルドした `VRCFaceTracking.Maxine` モジュールを `AppData/Roaming/VRCFaceTracking/CustomLibs` へ配置します。
4. VRCFaceTracking を起動すると自動的に Web カメラの表情が読み込まれ、VRChat の OSC として反映されます。

## 実装メモ
- ExpressionApp から送られる 60 個の float 配列をそのまま UnifiedExpressions にマッピングしています。
- 受信時の欠損やフォーマットエラーはログへ警告を出し、最新フレームのみをスレッドセーフに保持します。
- 頭部回転はクォータニオンをオイラー角へ変換し、`±90°` を `[-1, 1]` に正規化して `UnifiedTracking.Data.Head` に渡しています。
