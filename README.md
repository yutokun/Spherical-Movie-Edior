# Spherical Movie Editor

180 / 360 度動画に立体物を追加するための Unity プロジェクトです。Unity Editor 自体を動画エディタとして扱います。アプリケーションを生成するものではありません。

## 注意

出来具合はベータ版といったところです。

一応使えますが、まだ人に勧められる完成度ではありません。しかし1からやるよりかはだいぶ楽…みたいな制作進度です。

## こういう時に使いたい（あるいは動機）

- 全天球動画を撮ったけど、タイトルや字幕が入れられなくて困った。編集ソフトなんか持ってねえよ…
- ましてやステレオスコピック（立体視の）動画は対応してるソフトも少ない…あるいは高価
- 立体の動画に後から雪を降らせたりしたい…したくない？
- Unity でできたらいいのにな……せや！

## 提供する機能

- 180 / 360 / モノスコピック / ステレオスコピックの動画を簡単な設定でレンダリング
- 動画の再生時間で Timeline をコントロールすることによる正確な編集（Timeline 上での作業を想定しています）
- Unity 上のフレームレートに依存せず、元のビデオの全フレームを確実に出力

## 提供しない機能

- 動画自体のカット・クロスフェード・色変更など。あくまで立体物を追加するためだけのものです。

## 欲しい機能

[Issues](https://github.com/yutokun/Spherical-Movie-Editor/issues) へ

## 下準備

動画を出力するには、ffmpeg の PATH を通す必要があります。（ライセンスには注意して下さい）

VR180 の場合は、Google の [VR180 Creator](https://arvr.google.com/vr180/apps/) で編集用に動画を変換して下さい。

## 使い方（簡易版・WIP）

SampleScene の Video Player に動画をセットします。

Assets/Timeline/Video Composition タイムライン上にオブジェクトを実装します。

VR でのプレビューは、プレイモードでできるのではないでしょうか（未検証・視点の 3DoF 固定は未実装なのでベンダの SDK で）

Movie/Export... メニューから、Capture and Encode を押すと、プロジェクトフォルダ内に全てのフレームを画像として出力します。ffmpeg が使用できる場合、これをエンコードし、指定のファイル名でデスクトップに保存します。

![ExportWindow](doc/ExportWindow.png)

## 使い方（詳細・WIP）

### 動画の用意

立体物を追加したい動画を Unity にインポートします。Scenes/MovieEditor を開き、SphericalMovieEditor の Clip にアタッチして下さい。

![SMEInspector](doc/SMEInspector.png)

| 名称      | 機能                                                         |
| --------- | ------------------------------------------------------------ |
| Clip      | 立体物を追加したい動画をセットします                         |
| Use Proxy | 低解像度のプロキシ動画を作成し、プレビュー再生やタイムラインでのスクラブ時に使用します。エクスポート時はオリジナルの動画が自動的に使用されます。 |

### 立体物の追加

Timeline/Video Composition タイムラインに動画を追加します。

### 出力の方法

Movie メニューから Export... を選択すると次のウインドウが現れます。

![Menu](doc/Menu.png)

![ExportWindow](doc/ExportWindow.png)

| 名称                | 機能                                                         |
| ------------------- | ------------------------------------------------------------ |
| **Image Settings**  |                                                              |
| Height              | 出力される動画の縦のサイズをピクセル数で指定します。         |
| Width               | 出力される動画の横のサイズをピクセル数で指定します。         |
| Map Size            | 内部でフレームをレンダリングする際の解像度を指定します。これを大きめにすることでアーティファクトの少ない画像が得られる可能性がありますが、出力速度が大きく低下します。 |
| Render Stereo       | ステレオスコピックの動画を生成します。                       |
| Stereo Separation   | 瞳孔間距離を指定します。                                     |
| **Encode Settings** |                                                              |
| Codec               | コーデックを指定します。NVENC を使用するには、対応する GPU が必要です。 |
| CRF                 | 画質を指定します。小さくするほど画質が上がり、大きくするほど画質が下がります。H.264 では 23、H.265 では 28 が標準です。NVENC では無視されます。 |
| File Name           | 出力するファイルの名前を指定します。拡張子は自動で付加されます。 |
| Capture and Encode  | 動画の出力を開始します。                                     |
| Encode Only         | 既にキャプチャされている連番画像とオリジナルの動画ファイルを使って動画をエンコードします。コーデックや CRF のみを変えて試したい時に、キャプチャし直さなくて良いため高速です。 |

## Licenses

### Unity Recorder

[copyright © 2019 Unity Technologies ApS](https://docs.unity3d.com/Packages/com.unity.recorder@2.2/license/LICENSE.html)

### UniTask

[Copyright (c) 2019 Yoshifumi Kawai / Cysharp, Inc.](https://github.com/Cysharp/UniTask/blob/master/LICENSE)

### ffmpeg

Currently not included in this repository, but you **MUST** check when you use patented codec.

[FFmpeg License and Legal Considerations](http://ffmpeg.org/legal.html)