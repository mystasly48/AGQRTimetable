# AGQRTimetable

超!A&G+ の番組表をスクレイピングして、いろいろ取得しやすくした（つもり）。

## Installation

1. AGQRTimetable プロジェクトをビルド
1. `AGQRTimetable.dll` を自身のプロジェクトに参照追加
1. `Newtonsoft.Json` と `HtmlAgilityPack` を NuGet から追加

## Usage

AGQRコンストラクタで番組表をスクレイピングします。

```csharp
AGQR agqr = new AGQR();
```

#### 今週の全番組表を取得

月曜日から日曜日までの順番で並んでいます。

```csharp
foreach (var day in agqr.All) {
  Console.WriteLine(day.Date);
  foreach (var item in day.Programs) {
    Console.WriteLine(item.Title);
  }
}
```

#### 今日の番組表を取得

午前6時から次の日の午前6時までの番組表を取得します。

```csharp
foreach (var item in agqr.Today.Programs) {
  Console.WriteLine(item.Title);
}
```

#### 今放送中の番組を取得

```csharp
Console.WriteLine(agqr.Now.Title);
```

#### いつまでの番組表を利用できるのか取得

取得した日（午前0時から午前5時の間の場合には前日）から7日間の番組表を利用できます。

```csharp
Console.WriteLine(agqr.ExpiryDateTime);
```

#### 番組表が古くなっているか取得

取得してから7日間が経過したことと等しいです。

```csharp
Console.WriteLine(agqr.IsExpired);
```

#### 番組表の取得日時（更新日時）を取得

AGQRクラスのコンストラクタ実行時、または Refresh() メソッド実行時の時間です。

```csharp
Console.WriteLine(agqr.UpdatedDateTime);
```

#### JSON形式の文字列を取得（ワンライン）

今週の全番組表をJSON形式にシリアライズし、改行や空白で**フォーマットされていない1行の文字列**を取得します。

```csharp
Console.WriteLine(agqr.JsonSimple);
```

#### JSON形式の文字列を取得（複数ライン）

今週の全番組表をJSON形式にシリアライズし、 **改行や空白でフォーマットされた文字列** を取得します。

2020年3月21日に取得した番組表のJSONは[コレです](https://github.com/mystasly48/AGQRTimetable/blob/master/20200321.json)。

```csharp
Console.WriteLine(agqr.JsonFormatted);
```

#### 番組表を再取得

毎日午前5時に再取得することをおすすめします。

```csharp
agqr.Refresh();
```

#### 番組表的な日付を取得

6時から29時（5時）を同日時として、その日付の6時を返します。

##### 例

- 3月26日6時 の場合、3月26日6時 を返します。
- 3月26日12時 の場合、3月26日6時 を返します。
- 3月27日0時 の場合、3月26日6時 を返します。
- 3月27日5時 の場合、3月26日6時 を返します。

```csharp
Console.WriteLine(AGQR.GetSpecializedDate(DateTime.Now));
```
