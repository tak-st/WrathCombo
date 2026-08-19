# 上流最新化のための調査

調査日: 2026-08-16 / 対象: [PunishXIV/WrathCombo](https://github.com/PunishXIV/WrathCombo)

このフォークの独自変更については [FORK.md](FORK.md) を参照。

---

## 結論（先に3行）

1. **日本語化（E）と HP閾値制御（C）は上流が公式に実装済み。移植不要どころか、捨てたほうが良い。**
2. **ビルド環境はローカルに既に揃っている**（.NET 10 SDK / Dalamud API 15）。ECommons を上流 pin に合わせれば通る見込み。
3. **MNK は上流で全面的に書き直された。差分の再適用は不可能。** 残す価値があるのは「上流が固定で決め打ちしている判断を、設定で切り替えられるようにする」部分だけ。

---

## 1. 上流の現在地

| 項目 | 値 |
| --- | --- |
| 最新リリース | **1.0.4.20**（2026-08-07） |
| `main` の先頭 | `93ccaff29`（2026-08-15）— リリースより33コミット先行 |
| 開発速度 | **約210コミット/月**（直近30日） |
| C#ファイル数 | 272（このフォーク: 184） |
| 我々の土台 `1.0.0.14` からの差分 | **419ファイル / +178,428 / −36,774 行** |

実質的に別物と考えたほうがよい規模。

---

## 2. ビルド環境の変化

| | このフォーク | 上流最新 |
| --- | --- | --- |
| TargetFramework | `net9.0-windows` | **`net10.0-windows`** |
| Dalamud API Level | 1.0.0.14 相当 | **15** |
| ImGui | `ImGui.NET` | **`Dalamud.Bindings.ImGui`**（+ ImPlot / ImGuizmo） |
| DalamudPackager | 12.0.0 | **15.0.0** |
| サブモジュール | ECommons, PunishLib | + **WrathCombo.API**（新規） |
| その他参照 | — | `System.Speech` 10.0.5 |

### ローカル環境は既に条件を満たしている

- インストール済み **.NET SDK: 9.0.300 と 10.0.100** → `net10.0-windows` はビルド可能
- インストール済み **Dalamud 15.0.3.1** = API Level 15 と一致
- `Dalamud.Bindings.ImGui.dll` も配置済み

つまり**現状ビルドが通らないのはサブモジュールが古いだけ**で、上流最新に切り替えて pin を合わせれば解消する見込み。

### 同期すべきサブモジュール pin（上流 `main` 時点）

| サブモジュール | commit | 備考 |
| --- | --- | --- |
| ECommons | `e76756e6` | `https://github.com/NightmareXIV/ECommons.git` |
| PunishLib | `f8b3e807` | `https://github.com/PunishXIV/PunishLib.git` |
| **WrathCombo.API** | `be5fd3a5` | **新規** `https://github.com/PunishXIV/WrathCombo.API.git` |

---

## 3. 独自変更の多くが上流に吸収されていた ★最重要

### 3-1. 日本語化（E）→ 完全に不要になった

上流に**公式のローカライズ機構**が入っている。

- `WrathCombo/Resources/Localization/` に resx 一式
- `CustomComboPresets.ja.resx` は **3,622エントリ**。モンクも完訳済み

  | キー | 訳 |
  | --- | --- |
  | `MNK_ST_AdvancedMode_Name` | アドバンストモード - 単体 |
  | `MNK_STUseBrotherhood_Name` | 桃園結義：攻撃オプション |
  | `MNK_STUseBrotherhood_Desc` | 桃園結義をローテーションに追加します。 |

- `Window/Text.cs` が `Svc.PluginInterface.UiLanguage` を見て**自動で言語を切り替え**、実行中の言語変更にも追従（`OnLanguageChanged`）
- 対応言語: de / fr / **ja** / ko / zh-Hans / zh-Hant ほか

**→ E は移植しない。** 上流に乗るだけで日本語UIになる。
`ImGui.InputInt($"###優先度{...}")` のウィジェットID汚染も自動的に消える。

### 3-2. HP閾値によるバフ制御（C）→ 上流が独自に同じ結論に到達

| | 実装 |
| --- | --- |
| このフォーク | `SubOption`（全コンテンツ/ボスのみ）を削除し、対象の最低HP%に置換 |
| 上流最新 | `MNK_ST_BHHPOption`（既定25）+ `MNK_ST_BHHPBossOption` を新設。RoF / RoW も同様。`BossHpThreshold(hpBossOption, hpOption, isBoss)` で解決 |

発想は同じ。ただし**上流はHP%とボス判定を併用**しており、HP一本にした我々より柔軟。

**→ C は移植しない。上流のほうが良い。**

### 3-3. AoE踏鳴（Cの一部）→ 上流に実装済み

`MNK_AoEUsePerfectBalance` / `MNK_AoE_PerfectBalanceHPThreshold` / `CanPerfectBalanceMaxChargeAoE()`

**→ 我々の `UsePerfectBalanceAoE()` は不要。**

### 3-4. 金剛の極意まわり（Dの一部）→ 上流に実装済み

`MNK_ST_UseRoE` / `MNK_ST_EarthsReply` / `CanEarthsReply()` が `GetPartyAvgHPPercent()` を使用。

**→ `MNK_AoE_RiddleOfEarth_Threshold` の発想は取り込まれている。**

### 3-5. ポーション → オプション化された

`MNK_Opener_Potion` + `DrawOpenerPotionChoice()`。開幕でのポーション使用がON/OFFできる。

**→ かつて `noPotion` ブランチで扱っていた問題は上流で解決済み。**

---

## 4. 依然としてフォーク独自のもの

上流のコードベース全体を検索し、**存在しないことを確認済み**:

| 独自オプション | 内容 |
| --- | --- |
| `MNK_ST_Fast_Phoenix` | 踏鳴を紅蓮の前／後どちらに置くか |
| `MNK_ST_FiresReply_Order` | 乾坤闘気弾と必殺技の順序（3段階） |
| `MNK_ST_Phoenix_Order` | 鳳凰の舞の型順序（5通り） |
| `MNK_ST_Many_PerfectBalance` | チャクラ状態 vs 踏鳴使用回数 |
| `MNK_ST_Brotherhood_ROFLastOnly` | 紅蓮効果中は桃園を使わない |
| `MNK_ST_Brotherhood_AdjustROF` | ずらした桃園を次の紅蓮に合わせ直す |
| 7秒バースト opener（SL7 / LL7）+ 踊り子自動検出 | PT構成でバーストタイミングを変える |
| PT平均HPとの差による内丹／ブラッドバス | 上流は `Role.CanSecondWind(自分のHP%)` のまま |

### ただし重要な但し書き

**上流は同じ問題領域を「設定なしの固定ロジック」として既に実装している。**

```text
ShouldUsePreRoFPerfectBalance()          ← Fast_Phoenix と同じ判断を内部で決め打ち
ShouldUsePostRoFLunarOddPerfectBalance()
IsEvenWindowApproaching()                 ← バースト窓合わせ
IsBurstHoldReleaseReady()
ShouldUsePBAfterBurstHolding()
IsRoFInPerfectBalanceWindow()
IsBrotherhoodInPerfectBalanceWindow()
```

つまり差は「**上流は答えを1つに決め打ち／我々は選択可能にしていた**」という点にある。

**→ 移植の本質は「上流の固定判断を、設定で切り替えられるようにする」こと。**
まず素の上流を使ってみて、実際に不満が出た項目だけ手を入れるのが合理的。

---

## 4-2. スキル使用の優先順（if の並び順）

設定値ではなく**コードの `if` の並び順**で決まるため、差分として持ち運べない項目。
`MNK_ST_AdvancedMode` の発動順を3者で比較した。

| | 土台 1.0.0.14 | このフォーク | 上流最新 |
| --- | --- | --- | --- |
| 最優先 | — | **乾坤（残2秒）→ 絶空拳 → 乾坤（遠距離）** | — |
| 前段 | 陰陽闘気 → 演武 → 開幕 | 陰陽闘気 → 演武 → 開幕 | 開幕 → 陰陽闘気 → 演武 |
| バフ | 紅蓮 | **桃園 → 紅蓮** | 踏鳴(burst hold) → **桃園 → 紅蓮** |
| oGCD | 桃園 → 疾風 → 踏鳴 → ヒール → 陰陽闘気斬 | 踏鳴 → **陰陽闘気斬 → 疾風** → ヒール | 踏鳴 → **疾風 → 陰陽闘気斬** → マントラ → 金剛 → 牽制 → ヒール → レッグスイープ |
| GCD | 必殺技 → 乾坤 → 絶空拳 | 必殺技 | 必殺技 → ForceSecondOpo → 乾坤 → 絶空拳 |

### 結論

| 変更 | 状態 |
| --- | --- |
| **桃園結義 → 紅蓮の極意** の順に入れ替え | **反映不要**。上流も独自に同じ順序へ変わった |
| **乾坤闘気弾／絶空拳を最優先に引き上げ** | **独自のまま**。上流は GCDブロック内（必殺技の後）に据え置き |
| **陰陽闘気斬 > 疾風の極意**（commit `chakra > ROW`） | **独自のまま**。上流は逆順（疾風 → 陰陽闘気斬） |

### 上流に優先度の設定機構はあるが、MNK は対象外

- `UserConfig.cs:847` に汎用の `DrawPriorityInput(UserIntArray, ...)` がある
- 使っているのは **AST / BLM / SCH / SGE / SMN / WHM のみ**（ヒーラー中心、ヒールや移動時アクションの選択順）
- `MNK_Config.cs` に `_Priority` は**一切ない** → モンクの発動順は `if` の並びで固定

**→「上流の作法に乗って設定化する」道は MNK には用意されていない。**

### 作業としての難易度

**「難しい」というより「別種の作業」**になる。cherry-pick は不可能だが、作業自体は土台のときより**簡単**。

上流はコードが整理されており、

- oGCDブロック / GCDブロックが明確に分離されている
- 条件が `CanRoF()` `CanFiresReply()` `CanBrotherhood()` のような述語関数に外出しされている

ため、**`if` を1つ移動するだけ**で済む。具体的には:

- `CanFiresReply()` / `CanWindsReply()` を呼ぶ `if` を関数上部へ移す
- 陰陽闘気斬の `if` を疾風の極意の前へ移す

### 設定で順序が変わるもの（`Order` 系）は完全に独自

`MNK_ST_FiresReply_Order`（3段階）/ `MNK_ST_Phoenix_Order`（5通り）/ `MNK_ST_Fast_Phoenix` は、
「並びを固定で変える」のではなく**設定値で分岐する**もの。上流に該当機構はない。

ただし上流は `ShouldUsePreRoFPerfectBalance()` 等で固定の答えを持っているため、
**まず素の上流を試し、実際に不満が出た項目だけ実装する**（§4 の但し書きと同じ）。

---

## 5. コードの移植可能性: 事実上ゼロ

MNK は上流で全面的に書き直されている。

| ファイル | 我々の土台 1.0.0.14 | 上流最新 |
| --- | --- | --- |
| `MNK.cs` | 666行 | **487行**（ロジックがHelperへ移動） |
| `MNK_Helper.cs` | 318行 | **892行**（約40個の小さな述語関数に分解） |
| `MNK_Config.cs` | 113行 | 227行 |

構造的な変更:

- `CustomComboPreset` enum → **`Preset` にリネーム**し、`WrathCombo.API.Enum`（別アセンブリ）へ移動
- opener 基盤の刷新 — `SkipSteps` / `PrepullDelays` / `SubstitutionSteps` / `AllowUpgradeSteps` / `Preset` プロパティ追加
- opener がレベル別に分割 — `Lvl90LL` / `Lvl100LL` / `Lvl90SL` / `Lvl100SL` / **`Lvl100BHFirst`**（桃園先行）
- MNK に新クラス — `MNK_Retarget_Thunderclap` / `MNK_PerfectBalanceProtection` / `MNK_Brotherhood_Riddle` / `MNK_BeastChakras`
- ロジックが `CanRoF()` `CanBrotherhood()` `CanFiresReply()` のような述語関数に分解された

**→ 差分の再適用（cherry-pick / merge）は不可能。「意図」を読み直して、上流の関数分解に合わせて書き直すしかない。**

---

## 6. モンク以外（F）— 未検証

MNK ほど詳細には確認していない。上流も書き換わっているため、**移植前に現物の比較が必要**。

| ジョブ | 分かっていること |
| --- | --- |
| SGE | 移動オプションが**優先度リスト方式に一般化**（`SGE_ST_DPS_Movement_Priority` + `CheckMovementConfigMeetsRequirements`）。我々のハードコード挿入は不要になっている可能性が高い。Psyche / フレグマが選択肢に含まれるか要確認 |
| WHM | ディヴァインベニゾンはヒール文脈で「対象に未付与なら使用」に変更。ミゼリは `!BloodLilyReady` 条件に。我々の「2スタック時に自動放出」「神速魔CDとリリータイマーで条件付け」とは別実装 — 要比較 |
| DNC | 未確認 |

---

## 6-2. 「上流に乗らず、今のフォークをそのまま動かす」は可能か

**結論: 実質不可能。** 硬い壁が3つある。

### 壁1: Dalamud が読み込みを拒否する

| | 値 |
| --- | --- |
| フォークの宣言 | `"DalamudApiLevel": 12` |
| インストール済み Dalamud | **15.0.3.1（API Level 15）** |
| 上流最新の宣言 | `"DalamudApiLevel": 15` |

API Level が一致しないプラグインは Dalamud が読み込まない。
数字を 15 に書き換えること自体は可能だが、それは壁を消すのではなく、
**壁の向こうにある破壊的変更を露出させるだけ**。

### 壁2: 参照している DLL が存在しない

csproj が `$(DalamudLibPath)` から参照している DLL の実在確認:

| 参照 | 現在の Dalamud |
| --- | --- |
| **ImGui.NET** | **無い**（`Dalamud.Bindings.ImGui.dll` に置換済み） |
| ImGuiScene / Lumina / Lumina.Excel | あり |
| InteropGenerator.Runtime / Newtonsoft.Json / FFXIVClientStructs | あり |

欠けているのは ImGui.NET だけ。ただし**参照名の差し替えでは済まない**:

- WrathCombo 本体で `using ImGuiNET` を持つファイル: **34**
- ECommons 側: **30**

名前空間・型・文字列の扱いが変わっているため、計 **64ファイル**の書き換えが必要。

### 壁3: ゲーム関数のシグネチャが16ヶ月分ずれている

バイト列でゲーム内関数を探す箇所が実際に稼働している:

| 場所 | 対象 |
| --- | --- |
| `Core/PluginAddressResolver.cs:16` | `IsActionIdReplaceable` |
| `Services/PartyTargetingService.cs:19` | `getGameObjectFromPronounID` |
| `Data/JobGaugeDebugging.cs:58,68` | ジョブゲージのアドレス |
| `Data/ActionWatching.cs` | `ReceiveActionEffect` / `UseAction` / `SendAction` の3フック |

これらはゲーム本体のパッチごとに壊れる。16ヶ月＝メジャーパッチ数回分ずれており、
一致しなければ `ScanText` が例外を投げて**起動時に落ちる**。

### 作業量の比較

| | そのまま動かす | 上流最新に乗る |
| --- | --- | --- |
| ECommons の更新 | 必要 | 必要（同じ） |
| ImGui 移行（64ファイル） | **自分でやる** | 上流が完了済み |
| シグネチャ6箇所の再取得 | **自分でやる** | 上流が完了済み |
| Dalamud API 12→15 の破壊的変更追従 | **自分でやる** | 上流が完了済み |
| FFXIVClientStructs 16ヶ月分の追従 | **自分でやる** | 上流が完了済み |
| MNK の独自変更 | 不要（既にある） | 29項目を再実装 |

「そのまま動かす」とは、**上流が16ヶ月かけてやった移植を一人でやり直すこと**に等しい。
MNK の29項目を書き直すほうが、どう見ても軽い。

### 良い知らせ

上流最新の opener が `MinOpenerLevel 90 / 100`、`MaxOpenerLevel 90 / 100` に収まっており、
**ゲームのレベル上限は 100 のまま**（新拡張は来ていない）。
モンクのアクション体系そのものは大きく変わっていないと見てよく、
移植時に「そもそも技が違う」という事態は起きにくい。

### もう一つの選択肢

**フォークを一旦離れ、公式配布の WrathCombo をそのまま入れる。**
作業ゼロで、日本語UI付きの動くモンクが手に入る。
[§4](#4-依然としてフォーク独自のもの) の但し書きのとおり、上流は独自変更の問題領域を
固定ロジックで既に実装しているため、**まずこれを試してから移植の要否を決める**のが最も合理的。

---

## 7. 最新化の手順（改訂版）

1. 現状を `origin/API` に push して退避
2. 上流最新から新ブランチを切る

   ```sh
   git switch -c mnk-custom upstream/main
   ```

3. サブモジュール3つを上流 pin に同期（**WrathCombo.API は新規追加**）

   ```sh
   git submodule sync && git submodule update --init --recursive
   ```

4. `dotnet build` で**素の上流がビルドできること**を先に確認
5. **素の上流を実際に使ってみる** ← ここが重要
   吸収された機能（HP閾値・AoE踏鳴・ポーション・日本語UI）が期待通りに動くか、
   固定判断のバースト制御に不満が出るかを、コードではなく実測で判断する
6. 不満が残った項目だけ、上流の関数分解に合わせて再実装
   （`CanBrotherhood()` などに条件を足す形になる）
7. 日本語UIには**何もしない**

### やらないこと

- `API` ブランチ経由での追従（役目を終えている）
- 独自変更の一括マージ／cherry-pick（構造が違いすぎる）
- 日本語化の再適用

---

## 8. 更新後の棚卸し（[FORK.md](FORK.md) の区分を改訂）

| 区分 | 対象 | 変更後の判断 |
| --- | --- | --- |
| **捨てる** | E 日本語化 | 上流の公式ローカライズへ移行 |
| **捨てる** | C HP閾値制御 / AoE踏鳴 | 上流実装のほうが柔軟 |
| **捨てる** | G csproj / Packager | 上流に吸収済み |
| **実測後に判断** | A バーストタイミング制御 | 上流が固定ロジックで実装済み。不満が出た項目のみ再実装 |
| **実測後に判断** | B 7秒バースト opener | 上流の opener 基盤（`PrepullDelays` 等）で代替できるか要検証 |
| **移植候補** | D PT平均HPによる自己回復 | 上流は自分のHP%のまま。独自性が残っている |
| **要比較** | F DNC / WHM / SGE | 上流の現行実装との比較が未了 |
