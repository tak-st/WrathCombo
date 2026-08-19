# MNK 独自変更の完全な洗い出し

基準: 上流タグ `1.0.0.14`（`887cf641`, 2025-03-18） → 現在の `API` ブランチ先端（`4f471d24`）

関連: [FORK.md](FORK.md)（フォーク全体の棚卸し） / [UPSTREAM-SYNC.md](UPSTREAM-SYNC.md)（上流最新の調査）

---

## 調査方法

テキスト差分は「移動」を「削除＋追加」として表示するため、**位置だけ変えた箇所を書き換えと誤認する**。
そこで差分の通読に加えて、`if (条件) return アクション;` のガードを機械的に抽出し、
条件文とreturn式を正規化して土台と突き合わせ、**移動 / 条件変更 / 新規**を分類した。

分類スクリプト: `scratchpad/guards.py`

### 分類結果

| クラス | 土台 | フォーク | 完全同一 | 移動のみ | 移動+return変更 | 新規/条件変更 | 消滅/条件変更 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `MNK_ST_SimpleMode` | 20 | 22 | 11 | 1 | 3 | 6 | 5 |
| `MNK_ST_AdvancedMode` | 21 | 46 | **0** | 4 | 4 | 26 | 13 |
| `MNK_AOE_SimpleMode` | 20 | 20 | **18** | 0 | 0 | 2 | 2 |
| `MNK_AOE_AdvancedMode` | 20 | 34 | 1 | 4 | 1 | 28 | 14 |

読み取れること:

- **AoE シンプルは実質ほぼ無傷**（20個中18個が条件・位置とも完全同一。変更は2項目だけ）
- **ST アドバンストは条件が完全に残ったガードが1つもない**。「順番だけ変えた」のは4件で、しかも中身は Variant 系とヒールという些末な箇所
- したがって Advanced 2種は**実質的な書き直し**であり、「並べ替えただけ」ではない

### 「移動のみ」と判定された8件（＝対応不要）

| クラス | 移動 | 内容 |
| --- | --- | --- |
| ST_Adv | 4→9 | `Variant.Cure` |
| ST_Adv | 6→12 | `Variant.Rampart` |
| ST_Adv | 10→16 | `Role.CanSecondWind(閾値)` |
| ST_Adv | 11→17 | `Role.CanBloodBath(閾値)` |
| AoE_Adv | 3→5 | `Variant.Cure` |
| AoE_Adv | 5→8 | `Variant.Rampart` |
| AoE_Adv | 7→11 | 疾風の極意 |
| AoE_Adv | 16→31 | `HasEffect(Buffs.OpoOpoForm)` |

いずれも上流の位置のままで問題ない。**移植対象から除外してよい。**

---

## 凡例

| 記号 | 意味 |
| --- | --- |
| 🔴 **要移植** | 上流に無く、独自価値がある |
| 🟡 **実測後** | 上流が固定ロジックで別解を実装済み。使ってみて不満が出たら移植 |
| ⚪ **不要** | 上流が同等以上を実装済み、または対応不要 |
| 🐛 **バグ** | 移植する場合は修正が必要 |

### 2026-08-19 の再分類

当初 🟡 実測後 としていた23件のうち **15件を 🔴 要移植 に変更**した。

判断の原則は作者本人の指摘による:

> **土台から変更している箇所は、その方が良いという判断で変更しているはず。**

これは妥当なので、**意図的な挙動改善**および**既に 🔴 の項目が依存している項目**は
実測を待たずに要移植とした。移した15件:

`A4` `A5` `A6` `B5` `B7` `B16` `B17` `B18` `B20` `B23` `B29` `D2` `D5` `D10` `G9`

さらに `B8` `B9` も、`isWindBh` の意味が判明したことで**明確な改善**と確認できたため要移植とした（下記 B-3-2）。

最終的に **🟡 実測後 は 0件**となり、全83項目の判定が確定した。

**要するに「挙動をこうしたい」という判断は尊重し、「当時のコードでこう書くしかなかった」箇所だけ保留した。**

---

## A. `MNK_ST_SimpleMode`（シンプルモード・単体）

| # | 変更 | 上流最新 | 判断 |
| --- | --- | --- | --- |
| A1 | 内丹を `Role.CanSecondWind(25)` → `(PT平均HP% − 自分HP%) >= 25` | `Role.CanSecondWind(25)` のまま | 🔴 要移植 |
| A2 | ブラッドバスを同様に PT平均差 `>= 40` に | 同上 | 🔴 要移植 |
| A3 | `LevelChecked(SteeledMeditation)` → `LevelChecked(OriginalHook(...))`（LV連動修正） | 上流も `OriginalHook` 対応済み | ⚪ 不要 |
| A4 | **乾坤闘気弾を GCDブロック先頭（必殺技より前）へ移動**。`LevelChecked` と `!InMeleeRange()` 条件を削除 | 上流は GCDブロック内（必殺技の後）。`CanFiresReply()` に集約 | 🔴 要移植 |
| A5 | **絶空拳を同様に移動**。条件を `HasEffect(RiddleOfWind) && 残り<4秒` に変更（元は `!InMeleeRange() \|\| !HasEffect(PerfectBalance)`） | `CanWindsReply()` | 🔴 要移植 |
| A6 | 踏鳴ブロックの**後にも**乾坤/絶空拳の判定を追加（2箇所目） | 該当なし | 🔴 要移植 |
| A7 | `DragonKick` → `OriginalHook(DragonKick)` に統一（3箇所） | 上流も対応済み | ⚪ 不要 |

---

## B. `MNK_ST_AdvancedMode`（アドバンスト・単体）★本丸

### B-1. 設計思想レベルの変更

| # | 変更 | 上流最新 | 判断 |
| --- | --- | --- | --- |
| B1 | **対象アクションを4つに拡張** — `Bootshine/LeapingOpo` に加え `DragonKick` `TheForbiddenChakra` `Thunderclap` を割り当て可能に（`CustomComboPreset.cs` の `ReplaceSkill` も変更） | `OneButtonRotationChecker(actionID, SingleTargetDPS, Bootshine, LeapingOpo)` のみ。**多ボタン設計は無い** | 🔴 要移植 |
| B2 | `isPreserveMode`（`DragonKick` 起動時）— 連撃を温存する挙動に切り替え | 該当なし | 🔴 要移植 |
| B3 | `canBurst`（`DragonKick`/`TheForbiddenChakra` 以外）— バフを吐かない挙動に切り替え | 該当なし | 🔴 要移植 |
| B4 | **`Thunderclap`（羅刹衝）対応** — 距離>20 で到達秒数 `(距離−20)/6.13` を算出し、突進中の演武可否を判定 | `MNK_Retarget_Thunderclap`（マウスオーバー対象への**リターゲット**）。目的が全く異なる | 🔴 要移植 |

> B1〜B4 は「1ジョブに複数ボタンを割り当てて役割を分ける」という、このフォーク固有の設計。
> 上流には概念自体が存在しないため、**移植するなら最も大きな作業**になる。

### B-1-2. 各ボタンの役割（移植仕様）

フラグは `canBurst` と `isPreserveMode` の2つだけで、その組み合わせが4つのモードになる。
**「何のためのモードか」は作者本人の記述**（2026-08-19 に確認）。

| ボタン | `canBurst` | `isPreserveMode` | 目的 |
| --- | --- | --- | --- |
| `Bootshine` / `LeapingOpo` | ✓ | — | 通常 |
| `TheForbiddenChakra` | ✗ | — | **リソースが溢れないように消費しつつ、バーストはしない回し** |
| `DragonKick` | ✗ | ✓ | **ダメージが関係ない敵（演出中など、殴れるが殴る意味がない相手）向け。リソースが溢れても構わないので、バーストに向けて貯める回し** |
| `Thunderclap` | ✓ | — | **通常回しをしつつ、吹っ飛ばされた際に最速で接近する回し** |

#### `canBurst = false` が止めるもの（陰陽闘気斬・双竜脚 共通）

| 場所 | 挙動 |
| --- | --- |
| `MNK.cs:228` | 桃園結義のブロックごとスキップ |
| `MNK.cs:253` | 紅蓮の極意を撃たない |
| `MNK.cs:272` | 踏鳴を使わない |
| `MNK.cs:326-331` | **逆に必殺技はバフ待ちのゲートを外して即撃ち**（各条件に `\|\| !canBurst`） |

> **必殺技が即撃ちになるのは意図通り。**
> 必殺技は踏鳴を使わないと撃てないため、これらのモードではそもそも出番が無い。
> 万一なにかの間違いで踏鳴が入っている場合は、抱えずに吐かないともったいない。

#### `isPreserveMode = true` が追加で止めるもの（双竜脚のみ）

| 場所 | 挙動 |
| --- | --- |
| `MNK.cs:275` | 陰陽闘気斬を撃たない（＝チャクラを消費せず貯める） |
| `MNK.cs:289` | 疾風の極意を撃たない |
| `MNK.cs:342` / `MNK.cs:460` | トゥルーノースを使わない |
| `MNK.cs:312` / `MNK.cs:345-357` | **Fury を無視して、常にバフ/デバフ付与技を返す** |

| 型 | 通常 | 温存モード |
| --- | --- | --- |
| Opo-Opo | `Bootshine`（Fury消費） | **`DragonKick`** |
| Raptor | `TrueStrike` | **`TwinSnakes`** |
| Coeurl | `SnapPunch` | **`Demolish`** |

つまり Fury スタック・踏鳴チャージ・バフCD を次のバーストに残し、
チャクラの頭打ち分だけを捨てる。殴る意味がない相手なら、その損は許容できる。

#### `Thunderclap` の挙動

| 場所 | 挙動 |
| --- | --- |
| `MNK.cs:165` | 距離>20 なら到達時間を `(距離−20) / 6.13` 秒で算出 |
| `MNK.cs:167` | チャージあり・近接圏外・（ターゲット無し or 到達0.75秒未満）→ 素の羅刹衝を返して突進 |
| `MNK.cs:204` | 突進中、**到達までGCD分の余裕があるときだけ演武**を挟む |
| `MNK.cs:228` | 桃園結義を抑制。紅蓮・踏鳴は通常どおり撃つ |

#### 補足: 桃園結義だけが「通常ボタン専用」

`MNK.cs:228` のガードは展開すると通常ボタン限定になる。

```csharp
canBurst && originalActionId is not Thunderclap
// canBurst = not (DragonKick or TheForbiddenChakra) なので
// ⇒ originalActionId is Bootshine or LeapingOpo
```

つまり「羅刹衝が特別に桃園を抑制している」のではなく、
**桃園結義だけが通常ボタン専用**という一貫したルール。
紅蓮・踏鳴は `canBurst` 判定なので羅刹衝ボタンでも撃たれる。

**この guard が実際に効く場面は限定的。**
桃園結義のブロックには `MNK.cs:235` に `canMelee &&` が既にあるため、
突進中（近接圏外）は guard が無くても発動しない。
効くのは**着地して近接圏内に入った後も羅刹衝ボタンを押し続けている間**だけ。
「接近したら通常ボタンに戻す」運用を前提に、押しっぱなしでも桃園を溢さない作り。

> **なぜ桃園だけなのか — 作者本人も明確には記憶していない**（2026-08-19 時点）。
> 「接近時にボタンへ役割を付けるための変更だった気がする」とのこと。
>
> 推測としては、桃園結義はパーティバフで、紅蓮・疾風・踏鳴は自己バフである点が効いていそう。
> 接近直後に自分の判断で桃園を吐くとPTのバースト合わせから外れるが、
> 自己バフなら多少ずれても自分の損で済む。
> **パーティに影響するものだけ通常ボタンに限定した**、と読むのが自然。
> 移植時に同じ挙動を再現するかは、実際に使ってみて決めてよい部分。

### B-1-3. 移植前の決定事項（2026-08-19 確定）

| # | 論点 | 決定 |
| --- | --- | --- |
| 1 | 上流の `MNK_Retarget_Thunderclap` が `Thunderclap` を専有 | **Custom Action 方式を採るなら不要**。現行方式のまま移植する場合は `ConflictingCombos` を張る |
| 2 | 多ボタンをどう実現するか | **Custom Action を追加する方式を採用**（下記） |
| 3 | AutoRotation との関係 | **通常を押した扱いでよい**。追加作業なしで自動的にそうなる（後述） |
| 4 | 温存モードが方向指定ミスを出す | **問題なし**。仕様として許容 |
| 5 | `6.13` のマジックナンバー | **歩行速度**。`Thunderclap`（抜重歩法）の射程に入るまでの秒数を算出している。**定数化する** |
| 6 | 上流の新プリセットと役割が重なる | 必要なら `ConflictingCombos` を張る |

#### 採用方式: Custom Action を3つ追加する

現行のフォークは `DragonKick` / `TheForbiddenChakra` / `Thunderclap` の**実スキルを潰して**モードを表現しているが、
上流には合成アクションを生成する仕組み（`WrathCombo/Native/CustomActionManager.cs`）があるため、
**専用の Custom Action を作るほうが素直**。

実現可能性は確認済み:

- 上流は既に **9個の Custom Action** を生成している（4つの回し + `Items` + `Cease` + AutoOn/Off/Toggle）
- `Items.cs` はアイテム分を**動的に無限生成**している
- `CustomAction` はメモリ上に偽の Action 行を合成する汎用クラスで、個数制限は無い
- ID空間: `1_000_000`〜`1_000_007` が使用済み、`Items` が `2_000_000` 以上 → **`1_000_008` 以降が空いている**

**「通常」は既存の `SingleTargetDPS` を流用できるため、追加は3つで済む。**

| 追加するもの | 対応する現行ボタン |
| --- | --- |
| バースト抑制（`canBurst = false`） | `TheForbiddenChakra` |
| 温存（`+ isPreserveMode = true`） | `DragonKick` |
| 接近（桃園のみ保留 + 突進） | `Thunderclap` |

必要な変更箇所（すべて既存4つのコピーで済む定型作業）:

| 場所 | 内容 |
| --- | --- |
| `Combos/PvE/ALL/ALL.cs` | ID を3つ追加（`1_000_008`〜） |
| `Native/CustomActionManager.cs:421` | `new CustomAction(...)` 3行 + `Manager.Register(...)` に追加 |
| `Native/CustomActionManager.cs` `CustomActionSettings` | bool 3つ |
| `Native/CustomActionManager.cs` `CustomActionType` | enum 3値 |
| `GetActionId` / `CustomActionEnabled` | case 追加 |
| `Window/Tabs/CustomActions.cs` | チェックボックス3つ |
| `Combos/PvE/MNK/MNK.cs` | 受け取って `canBurst` / `isPreserveMode` にマップ |
| `Resources/*.png` | アイコン3枚 |

**この方式の利点:**

- `Thunderclap` の衝突が消える（`ConflictingCombos` 自体が不要になる）
- `DragonKick` / `TheForbiddenChakra` が本来のスキルとして使えるようになる（現行方式は潰している）
- **AutoRotation は追加作業なしで「通常扱い」になる** — `GetTypeByAttribute` は `(IsAoE, IsHeal)` から型を引くため、mode 系の型は決して返らず通常経路が使われる
- 他ジョブへ流用できる（各ジョブの `Invoke` で同じ判定を書くだけ）

**注意点:**

上流の `CustomActionType` は `(IsAoE, IsHeal)` の 2×2 で、mode はそれに**直交する第3軸**。
AoE版も欲しくなると 3×2 = 6個追加になる。
個人フォークなら問題ないが、将来上流にPRするなら設計相談が必要な箇所。

### B-2. 優先順の変更

| # | 変更 | 上流最新 | 判断 |
| --- | --- | --- | --- |
| B5 | **乾坤/絶空拳を関数最上部（開幕判定より前）へ引き上げ**（3ブロック: 乾坤残2秒 / 絶空拳 / 乾坤遠距離） | GCDブロック内に据え置き | 🔴 要移植 |
| B6 | **桃園結義を紅蓮の極意より前へ移動** | **上流も同じ順序に変更済み** | ⚪ 不要 |
| B7 | oGCD順を 踏鳴 → 陰陽闘気斬 → 疾風 に変更（土台: 桃園 → 疾風 → 踏鳴 → ヒール → 陰陽闘気斬） | 踏鳴 → 疾風 → **陰陽闘気斬** の順 | 🔴 要移植 |
| B8 | 陰陽闘気（Meditation）判定を演武の後にも追加（計2箇所） | 1箇所のみ | 🔴 要移植 |

### B-3. 発動条件の変更

| # | 変更 | 上流最新 | 判断 |
| --- | --- | --- | --- |
| B9 | 陰陽闘気の条件を大幅変更（`IsOriginal(MasterfulBlitz)`・紅蓮/絶空/乾坤バフ条件を削除、`!HasEffect(Brotherhood)` を追加） | `CanMeditate()` に集約 | 🔴 要移植 |
| B10 | 演武（FormShift）を戦闘外限定から解放。非近接時も可、Raptor/Coeurl 型の禁止を解除、紅蓮残18秒以下、`isWindBh`/`readyBlitz`、Thunderclap 中は除外 | `CanFormshift()` は**土台と完全に同一**。戦闘中は一切撃たない | 🔴 要移植 |
| B11 | 桃園結義: `SubOption` 削除 → `MNK_ST_Brotherhood_HP`（対象最低HP%） | `MNK_ST_BHHPOption` + `MNK_ST_BHHPBossOption`（**HP%＋ボス判定の併用**） | ⚪ 不要（上流が上位互換） |
| B12 | 桃園結義に `ROFLastOnly`（紅蓮中は使わない）を追加 | 該当なし | 🔴 要移植 |
| B13 | 桃園結義に `AdjustROF`（ずらしたら次の紅蓮に合わせ直す）を追加 | 該当なし | 🔴 要移植 |
| B14 | 桃園結義を踏鳴スタック数と連動（`GetBuffStacks(PerfectBalance) <= 1` 等）、`Gauge.BlitzTimeRemaining >= 2000` | 通常窓は `IsEvenWindowApproaching()`、復帰時は `IsBurstHoldReleaseReady()` が `CanBrotherhood()` を止める。**発想は同じだが遅延量が1 GCD短い** | 🔴 要移植 |
| B15 | 紅蓮の極意: `SubOption` 削除 → `MNK_ST_RiddleOfFire_HP` | `MNK_ST_RoFHPOption` + `RoFHPBossOption` | ⚪ 不要 |
| B16 | 紅蓮の極意を**桃園CDと同期**（残54〜66秒 or 114秒以上）、`RemainingGCD <= 1` | `CanRoF()`。同期概念は `IsEvenWindowApproaching()` 等が担当 | 🔴 要移植 |
| B17 | oGCDブロックの入口を `CanWeave()` → `CanWeave() \|\| !HasBattleTarget()`（ターゲット無しでも発動） | `CanWeave()` のまま | 🔴 要移植 |
| B18 | 陰陽闘気斬に「桃園直後は撃たない」条件（`!JustUsed(Brotherhood, 122)`） | `CanUseChakra()` | 🔴 要移植 |
| B19 | 疾風の極意: `SubOption` 削除 → HP%、**距離10以内**、`isPreserveMode` 除外 | HP%は `MNK_ST_RoWHPOption` + Boss判定で同等。**`CanRoW()` に距離判定が無い** | 🔴 要移植（距離のみ。HP%部分は不要） |
| B20 | ComboHeals に `PlayerHealthPercentageHp() <= 99` ガード追加 | 無し | 🔴 要移植 |
| B21 | **金剛の返し（EarthsReply）を ST に追加**（残6秒未満＝**消えかけたら吐く**） | ST に存在はするが `CanEarthsReply()` は **PT平均HP ≤ 閾値 かつ PT の75%が範囲内**。**健康なら腐らせる** | 🔴 要移植（トリガのみ。ST への追加自体は不要） |
| B22 | FormlessFist の GCD 判定に `canMelee` と Blitz 残り時間条件を追加 | `ForcedOpoGCD()` / `ForceSecondOpo()` | 🔴 要移植 |

### B-3-2. `isWindBh` と「射程外GCDの明け渡し」（B5 / B8 / B9 は一体）

B5・B8・B9 は別々の変更に見えるが、**一つの仕組み**であり分割して移植できない。

#### `isWindBh` の意味

```csharp
bool isWindBh = HasEffect(Buffs.Brotherhood) &&
                JustUsed(RiddleOfWind, 15 + (20 - GetBuffRemainingTime(Buffs.Brotherhood)));
```

`20 - 残り` は桃園の**経過秒数**なので判定窓は `15 + 経過`。絶対位置に直すと

```text
桃園開始 + 経過 − (15 + 経過) = 桃園開始 − 15
```

経過に関わらず「**疾風を桃園開始の15秒前以降に撃った**」という固定条件になる。
絶空拳バフは30秒なので、その疾風由来の絶空拳は桃園開始+15秒までに消化しないと消える。

> **`isWindBh` = このバーストは絶空拳も捻じ込む必要がある = GCD に余裕がない**
> （2026-08-19、作者本人の説明により判明）

#### 3ブロックの動作

3つとも `(!InCombat() || !HasBattleTarget() || !InMeleeRange())` が前提なので、
これは**攻撃できない時間の埋め方**を決める処理。

| 順 | ブロック | 追加条件 |
| --- | --- | --- |
| ① | 陰陽闘気（`MNK.cs:193`） | `!HasEffect(Buffs.Brotherhood)` |
| ② | 演武（`MNK.cs:200`） | `(!isWindBh \|\| readyBlitz)` |
| ③ | 陰陽闘気（`MNK.cs:207`） | `(!isWindBh \|\| readyBlitz)` |

`isWindBh && !readyBlitz`（タイトなバースト、必殺技も抱えていない）のとき:

- ① は `!HasEffect(Brotherhood)` で塞がる（`isWindBh` は桃園中が前提）
- ② ③ も塞がる

**3つとも通らず素通りする。** その先には関数最上部に引き上げられた
乾坤闘気弾・絶空拳のブロック（B5）があり、これらは `GetTargetDistance() <= 20` / `<= 10` で
**近接圏外でも撃てる**。

> **射程外の1 GCD を、タイトなバースト中は陰陽闘気や演武に使わず、絶空拳・乾坤闘気弾に明け渡す。**

`readyBlitz`（必殺技を抱えている）が例外になるのは、その GCD が既に確保されているため
余った時間を演武・陰陽闘気に回してよいから。

#### それ以外のときの優先順位

| 状態 | 挙動 |
| --- | --- |
| 桃園が乗っていない | ① が通る → **陰陽闘気が演武より優先** |
| 桃園が乗っている（タイトでない） | ① が塞がり ② が先 → **演武が優先**、通らなければ ③ で陰陽闘気 |

桃園中は味方の行動でもチャクラが溜まるため、陰陽闘気に GCD を使う必要が薄い。
Formless Fist を確保しておくほうが得、という判断。

#### B9 が削除した条件

土台と上流の `CanMeditate()` は同一で、次の**粗いガード**を持つ。

```csharp
Chakra < 5 && IsOriginal(MasterfulBlitz) &&
!HasStatusEffect(Buffs.RiddleOfFire) &&
!HasStatusEffect(Buffs.WindsRumination) &&
!HasStatusEffect(Buffs.FiresRumination)
```

「絶空拳や乾坤を抱えていたら陰陽闘気を撃つな」という一律の禁止。
フォークはこれを削除し（B9）、`isWindBh` による**精密な判定**に置き換えた。

**B8 と B9 は「粗いガードを精密なガードに差し替えた」一つの改善。**

#### 移植上の注意

- **B5 単独では意味を成さない。** 射程技を最上部に置いても、B8/B9 が道を空けなければ陰陽闘気・演武に GCD を取られる
- **B8 単独でも意味を成さない。** 道を空けた先に射程技が無ければ素通りするだけ
- 上流の `CanFormshift()` は `!InCombat()` **限定**で、戦闘中は一切演武を撃たない。
  フォークは非近接なら戦闘中でも撃つため、この緩和（B10 の一部）も必要になる可能性がある

### B-3-3. 演武の条件緩和の内訳（B10）

上流の `CanFormshift()` は**土台と一字一句同一**で、`!InCombat()` 限定。
つまり戦闘に入ったら二度と演武を撃たない。フォークの変更は5点。

| # | 変更 | 位置づけ |
| --- | --- | --- |
| 1 | `!InCombat()` → `(!InCombat() \|\| !HasBattleTarget() \|\| !InMeleeRange())` | **戦闘中でも射程外なら撃つ**。B5/B8/B9 と同じ「射程外GCDを無駄にしない」設計 |
| 2 | `!HasEffect(RaptorForm)` / `!HasEffect(CoeurlForm)` を**削除** | B10 固有の改善（下記） |
| 3 | `(!isWindBh \|\| readyBlitz)` | B8/B9 の仕組みそのもの |
| 4 | 紅蓮バフ中は残り18秒以下のみ | **不具合対応と思われる**（下記） |
| 5 | `(actionID is not Thunderclap \|\| remainingSec >= GCD - 0.25)` | B4 の一部 |

#### #2 — Raptor / Coeurl 型でも演武を撃つ

| 状態 | 土台・上流 | フォーク |
| --- | --- | --- |
| Opo型 | 撃たない | 撃たない |
| **Raptor型** | 撃たない | **撃つ** |
| **Coeurl型** | 撃たない | **撃つ** |

Raptor / Coeurl 型で止まった状態から復帰すると `Raptor → Coeurl → Opo` と回さないと
Opo に戻れない。**演武で Formless Fist を得ておけば復帰1発目から Opo を撃てる。**

逆に Opo 型なら既に Opo から始められるので演武を撃つ意味がない。
だから Opo 型だけ禁止を残してある——理屈が通っている。

#### #4 — 紅蓮発動直後の約2秒だけ演武を止める

```csharp
!LevelChecked(RiddleOfFire) || !HasEffect(Buffs.RiddleOfFire) ||
GetBuffRemainingTime(Buffs.RiddleOfFire) <= 18
```

紅蓮は20秒なので、**バフ残り18秒超＝発動から約2秒以内**のときだけ演武を止める窓。
その外（バースト中ずっと）では演武を許可しているので「バースト中は演武禁止」ではない。

> **意図は作者本人も明確には記憶していないが、「何か問題が発生してこの条件を入れた」との証言**
> （2026-08-19）。害のある条件ではないため、そのまま移植する方針。

### B-3-4. `OriginalHook` の一斉適用は4ボタン化の副作用対策（B31 ほか）

フォークが各所で `Bootshine` → `OriginalHook(Bootshine)`、`DragonKick` → `OriginalHook(DragonKick)`
のように包んでいるのは、**4ボタン化（B1）で壊れる自動アップグレードを塞ぐため**
（2026-08-19、作者本人の指摘）。

#### 仕組み

FFXIV はホットバーの**スロットに置かれた元アクション**を基準に上位技へ自動変換する。

| スロットの元アクション | プラグインが返す値 | 結果 |
| --- | --- | --- |
| `Bootshine` | `Bootshine` | ゲームが `LeapingOpo` へ自動変換 ✓ |
| **`DragonKick`** | `Bootshine` | **自動変換の経路に乗らない** ✗ |

上流は `Bootshine` / `LeapingOpo` の2ボタンだけなので、どちらのスロットでも自動変換が効く。
**この問題は4ボタン化に固有**であり、`OriginalHook()` で明示的に現在の上位形へ解決するのが対策。

#### 影響範囲

`OriginalHook` 化は開幕ブロック（B31）だけでなく、
`MNK.cs` / `MNK_Helper.cs` の広範囲に及ぶ（A3・A7・E2 として個別に記録した箇所を含む）。

> **A3・A7・E2 を「上流も対応済みだから ⚪ 不要」と判定したが、
> 4ボタン化を移植する場合は話が変わる。**
> 上流の `OpoFormGCD()` などは2ボタン前提で書かれているため、
> Custom Action 方式（B-1-3）で新しいボタンを足すなら、
> **返り値を `OriginalHook()` で包む必要があるか改めて確認すること。**

#### 現状の opener では no-op

なお現行の opener リスト（`SL` / `LL` / `SL7` / `LL7`）は収録アクションが全て最終形
（`LeapingOpo` `TwinSnakes` `Demolish` `TheForbiddenChakra` `ElixirBurst` `RisingPhoenix` 等）で、
`MinOpenerLevel` も 100 固定のため、**B31 単体では実効的な差は出ない**。
ただし4ボタン化に伴う防御的措置として意味があるので、B1 とセットで移植する。

### B-4. 必殺技・踏鳴・チャクラ

| # | 変更 | 上流最新 | 判断 |
| --- | --- | --- | --- |
| B23 | **必殺技の条件を全面書き換え** — `InMasterfulRange()` を廃し、Blitz残り時間・桃園CD・`Fast_Phoenix`・`ROFLastOnly` を絡めた複合条件に | `CanMasterfulBlitz()` / `ShouldSpendMasterfulBlitz()` | 🔴 要移植 |
| B24 | 踏鳴中のアクションを `OpoOpoAction`/`RaptorAction`/`CoeurlAction` の変数に集約。**非近接時は範囲技へフォールバック**（壊神衝/四面脚/地烈脚） | `DoPerfectBalanceCombo()` に集約。範囲フォールバックは無し | 🔴 要移植 |
| B25 | チャクラ数による分岐を新設（`>=2` で該当型、2種が1個ずつなら残り1種） | **スロット位置で決め打ち**。想定外の並びだと**魔天絶技を作る**（下記 B-4-2） | 🔴 要移植 |
| B26 | Open Lunar 条件に `Many_PerfectBalance` と `compareNextBurstTime(0, 20)` を導入 | 該当なし | 🔴 要移植 |
| B27 | Open Lunar の `JustUsed` 判定に `ElixirField` を追加（`ElixirBurst` のみだった） | — | 🔴 要移植 |
| B28 | **Open Solar に `Phoenix_Order` 5通りの分岐を新設**（`1→2→3` / `2→3→1` / `3→2→1` / 直前の型依存 / 現在方向依存） | 該当なし。上流は固定順 | 🔴 要移植 |
| B29 | 乾坤/絶空拳の「通常位置」の条件も書き換え（`FiresReply_Order`、距離20/10、桃園・紅蓮の残り時間） | `CanFiresReply()` / `CanWindsReply()` | 🔴 要移植 |
| B30 | **トゥルーノースの使用条件を厳格化** — チャージ2以上、または1以上かつ（Solar/両開き/次バーストに間に合う）、または紅蓮/桃園中 | `MNK_ManualTN`（手動チャージ数指定） | 🔴 要移植 |
| B31 | 開幕ブロックで `TheForbiddenChakra` → `OriginalHook(...)`、`return actionID` → `return OriginalHook(actionID)` | 上流は素の `actionID` を返す（2ボタンなのでゲーム側の自動アップグレードが効く） | 🔴 要移植（**B1 とセット**） |

### B-4-3. `Phoenix_Order` の対応表と、実際に使っていた設定（B28）

UIラベルの `1` `2` `3` は第1〜第3の型。`else` 節（既定値 `0`）が
`Opo → Raptor → Coeurl` を返すことから確定できる。

| 番号 | 型 | アクション |
| --- | --- | --- |
| 1 | Opo-Opo | `OpoOpoAction` |
| 2 | Raptor | `RaptorAction` |
| 3 | Coeurl | `CoeurlAction` |

| 設定値 | ラベル | 実際の順序 |
| --- | --- | --- |
| 0（既定） | 1→2→3 | Opo → Raptor → Coeurl |
| 1 | 2→3→1 | Raptor → Coeurl → Opo |
| **2** | 3→2→1 | **Coeurl → Raptor → Opo** ← 上流の固定順と一致 |
| 3 | 直前の型依存 | Raptor型GCDの直後なら Coeurl を先に。それ以外は Raptor → Coeurl → Opo |
| **4** | 現在方向依存 | **方向指定が取れていれば Coeurl を先に**。それ以外は Raptor → Opo → Coeurl |

#### 上流は「3→2→1」固定

```csharp
if (Gauge.BeastChakra[0] is BeastChakra.None) { actionID = CoeurlFormGCD(); return true; }
if (Gauge.BeastChakra[1] is BeastChakra.None) { actionID = RaptorFormGCD(); return true; }
if (Gauge.BeastChakra[2] is BeastChakra.None) { actionID = OpoFormGCD();    return true; }
```

**フォークの既定値（0）とは完全に逆順。**

#### 実際に使っていたのは設定値 4（2026-08-19、作者本人）

```csharp
if (CoeurlChakra == 0 && positionCheck(actionID, false))
    return CoeurlAction;                      // 今その場で方向指定が取れる → Coeurl を消化
else
{
    if (RaptorChakra == 0) return RaptorAction;
    if (OpoOpoChakra == 0) return OpoOpoAction;
    if (CoeurlChakra == 0) return CoeurlAction;   // 取れないなら Coeurl を最後へ回す
}
```

`positionCheck(actionID, false)` は `WeaveOnly = false` なので
「トゥルーノースを待てるか」を外し、**今この瞬間に方向指定が成立するか**だけを見る。
`Gauge.CoeurlFury` で破砕拳（背面）と崩拳（側面）を撃ち分けるところまで見ている。

> **狙い: 立ち位置が合っているうちに方向指定技を消化し、合っていなければ最後に回して
> 位置を直す時間を稼ぐ。**

#### 移植への影響

**上流の固定順（Coeurl 先頭）は、設定値 4 とは真逆の最悪ケース。**
立ち位置に関係なく、踏鳴の1発目で必ず方向指定技を要求される。

| 依存 | 状態 |
| --- | --- |
| `positionCheck()`（F6） | 🔴 要移植。設定値 4 はこれ無しでは成立しない |
| B24（`CoeurlAction` の変数化） | 🔴 要移植。撃ち分けの受け皿 |

**B28 は選択肢 4 を含めて全面的に移植が必要。** 順序を差し替えるだけでは足りない。

### B-4-2. チャクラ数分岐は「安全網」だった（B25）

当初「方式が違うだけで同等」と判定したが**誤り**。上流のほうが脆い。

#### 上流はスロット位置で決め打ちしている

```csharp
// Open Solar
if (Gauge.BeastChakra[0] is BeastChakra.None) { actionID = CoeurlFormGCD(); return true; }
if (Gauge.BeastChakra[1] is BeastChakra.None) { actionID = RaptorFormGCD(); return true; }
if (Gauge.BeastChakra[2] is BeastChakra.None) { actionID = OpoFormGCD();    return true; }
```

**空きスロットの位置**だけを見ており、既に入っているチャクラの種類を見ていない。

#### 崩れた状態を通すと

| 状態 | 上流 | フォーク |
| --- | --- | --- |
| `[Opo, Opo, None]` | Opo → Lunar ✓ | Opo → Lunar ✓ |
| `[Coeurl, Coeurl, None]` | Opo → **魔天絶技** ✗ | Coeurl → Lunar ✓ |
| `[Raptor, Raptor, None]` | Opo → **魔天絶技** ✗ | Raptor → Lunar ✓ |
| `[Opo, Coeurl, None]` | Opo → **魔天絶技** ✗ | Raptor → Solar ✓ |
| `[Opo, Raptor, None]` | Opo → **魔天絶技** ✗ | Coeurl → Solar ✓ |

上流が正しく動くのは**プラグインが最初から最後まで制御した場合だけ**。
スロットを順に埋める前提なので、途中で手動入力が挟まる、
踏鳴開始時に既にチャクラが乗っている、といった状況で破綻する。

#### フォークの3行は復旧機構

```csharp
if (OpoOpoChakra >= 2) return OpoOpoAction;      // 2つ同種 → 3つ揃えて Lunar
if (RaptorChakra >= 2) return RaptorAction;
if (CoeurlChakra >= 2) return CoeurlAction;

if (OpoOpoChakra >= 1 && RaptorChakra >= 1) return CoeurlAction;   // 2種1つずつ → 残りを埋めて Solar
if (OpoOpoChakra >= 1 && CoeurlChakra >= 1) return RaptorAction;
if (CoeurlChakra >= 1 && RaptorChakra >= 1) return OpoOpoAction;
```

Lunar/Solar の計画ロジック**より前**に置かれており、
**どんな状態から入っても必ず有効なブリッツに着地させる安全網**として働く。
チャクラが 0〜1 個のときだけ通常の計画ロジックへ落ちる。

> **Open Lunar 側は上流も安全。** 常に `OpoFormGCD()` を返すため 3 つ揃えば必ず Lunar。
> 脆いのは Open Solar 側だけ。

---

## C. `MNK_AOE_SimpleMode`（シンプルモード・範囲）

| # | 変更 | 上流最新 | 判断 |
| --- | --- | --- | --- |
| C1 | 内丹を PT平均差 `>= 25` に | `Role.CanSecondWind(25)` | 🔴 要移植 |
| C2 | ブラッドバスを PT平均差 `>= 40` に | 同上 | 🔴 要移植 |

**これだけ。** 20個のガードのうち18個が条件・位置とも完全に同一。

---

## D. `MNK_AOE_AdvancedMode`（アドバンスト・範囲）

| # | 変更 | 上流最新 | 判断 |
| --- | --- | --- | --- |
| D1 | `raptorAction` / `coeurlAction` / `maxPowerSkill` を変数化（レベルと近接可否でフォールバック） | 同等の集約はあるが、**D6/D11/D12 の受け皿**として必要 | 🔴 要移植（依存） |
| D2 | 乾坤/絶空拳を関数最上部に追加（ST の B5 と同思想） | GCDブロック内 | 🔴 要移植 |
| D3 | 陰陽闘気/演武の条件を `!InCombat()` のみに簡素化 | 上流 `CanMeditate(true)` / `CanFormshift()` は削除された条件を全て持つ。**ST の B8/B9/B10 に相当する置き換えが AoE には入っていない** | 🔴 要移植（**新規実装**。下記 D-2） |
| D4 | **桃園結義を oGCDブロックの外（紅蓮より前）へ移動** | 上流も桃園→紅蓮の順 | ⚪ 不要 |
| D5 | 紅蓮の極意に桃園CD同期条件（54〜66秒 / 114秒以上）を追加 | 該当なし | 🔴 要移植 |
| D6 | **`UsePerfectBalanceAoE()` を新設**して oGCD 先頭へ | **F8 と同一のもの**。上流 `CanPerfectBalanceMaxChargeAoE()` に桃園同期もダウンタイム対応も無い | 🔴 要移植 |
| D7 | 万象闘気圏を `OriginalHook(InspiritedMeditation)` → **`OriginalHook(Enlightenment)` に修正**（土台はチャクラ蓄積技を返していた＝上流のバグ） | 上流も修正済み | ⚪ 不要 |
| D8 | **金剛の極意 / 金剛の返しを追加**（`MNK_AoE_RiddleOfEarth_Threshold`、PT平均HPで判断） | **B21 と同じ**「消えかけたら吐く」ルール（`残り < 6`）が上流に無い | 🔴 要移植 |
| D9 | FormlessFist の GCD 判定を追加 | **B22 と同じ**。`canMelee` が無く `BothNadisOpen` が射程判定を単独で担う（幻影闘舞は3m以内＋対象指定が必要） | 🔴 要移植 |
| D10 | 必殺技の条件を書き換え（ST の B23 の簡略版） | `CanMasterfulBlitz(true)` | 🔴 要移植 |
| D11 | 踏鳴中のチャクラ選択を全面書き換え（ST の B24〜B25 と同思想） | **B25 と同じ**。上流のスロット決め打ちは崩れた並びから魔天絶技を作る | 🔴 要移植 |
| D12 | Open Lunar に `ElixirField` 追加 + `compareNextBurstTime` | 該当なし | 🔴 要移植 |
| D13 | 通常ローテを `raptorAction` / `coeurlAction` に置換 | 単なる置換ではなく **Fury 判定と近接フォールバックが入る**。D1 と一体 | 🔴 要移植 |

### D-2. AoE への新規実装が必要な箇所（D3）

D3 だけは**フォークにも存在しない**ため、移植ではなく新規実装になる。

ST 側は B8/B9/B10 で「粗いガードを精密なガードに差し替える」改修を行った。

- 土台/上流の `CanMeditate()` は `!紅蓮 && !絶空拳バフ && !乾坤バフ` という一律の禁止
- ST 版はこれを削除し、`isWindBh`（＝そのバーストは GCD に余裕が無い）による判定に置換
- あわせて射程外でも演武・陰陽闘気を撃てるようにし、タイトなバースト中は
  それらを止めて絶空拳・乾坤に GCD を明け渡す（B-3-2 参照）

**AoE 側は条件の削除だけが行われ、置き換えが入っていない。**

> **作者本人の判断（2026-08-19）: 「手つかずの可能性がある。今回追加して、何か問題があれば外す」**

したがって AoE には ST と同じ仕組みを新たに組む。フォークの AoE コードをそのまま
持ってくると上流の粗いガードより弱い状態になるため、**コピー元が無い点に注意**。

実測での確認が特に必要な項目。AoE では絶空拳・乾坤を捻じ込む必要が薄い可能性があり、
その場合は上流の `CanMeditate(true)` のままでも困らない。

---

## E. その他のコンボクラス

| # | 変更 | 上流最新 | 判断 |
| --- | --- | --- | --- |
| E1 | `MNK_Riddle_Brotherhood` の条件を「桃園CD明け **or** 桃園CD < 紅蓮CD+3」に変更（土台: `ActionReady(Brotherhood) && IsOnCooldown(RiddleOfFire)`） | `MNK_Brotherhood_Riddle` + `MNK_BH_RoF`（どちらを置き換えるか選択可） | ⚪ 不要（上流が上位互換） |
| E2 | `MNK_BeastChakras` の `DragonKick` → `OriginalHook(DragonKick)` | 対応済み | ⚪ 不要 |

---

## F. `MNK_Helper.cs`

| # | 変更 | 上流最新 | 判断 |
| --- | --- | --- | --- |
| F1 | `Nadi` / `BeastChakra` の enum リネーム追従（`LUNAR`→`Lunar` 等） | 上流の現行名 | ⚪ 不要（API追従） |
| F2 | `DetermineCoreAbility` に `isPreserveMode` / `canMelee` を導入、非近接時のフォールバック追加 | `DoBasicCombo()` | 🔴 要移植（B2と一体） |
| F3 | `DetermineCoreAbility` のトゥルーノースに **`CanWeave()` 条件追加** | 上流 `DoBasicCombo()` に**ウィーブ判定が無い**（代わりに `trueNorthCharges` でチャージ温存指定） | 🔴 要移植 |
| F4 | **`compareCooldownTime(a1, a2, needTime)` 新設** — 2アクションのリキャストを猶予込みで比較 | 該当なし | 🔴 要移植 |
| F5 | **`compareNextBurstTime(action, needTime)` 新設** — 次のバースト窓に間に合うか。桃園と紅蓮のズレ（54〜66秒）で基準を切り替え、`AdjustROF` 連動 | `IsEvenWindowApproaching()` 等が別解 | 🔴 要移植（中核） |
| F6 | **`positionCheck(actionId, weaveOnly)` 新設** — 方向指定の可否判定を関数化 | 同等の判定は各所に分散 | 🔴 要移植（B28で使用） |
| F7 | **`UsePerfectBalance()` を全面書き換え** — 奇数窓/偶数窓/低レベル/追加/ターゲット無し の5分岐。`Fast_Phoenix`・`Many_PerfectBalance`・`ROFLastOnly`・`AdjustROF`・`FiresReply_Order` 連動 | `CanPerfectBalance()` + `ShouldUsePreRoFPerfectBalance()` / `ShouldUseSecondPerfectBalance()` 等に分解済み。**上流も同等以上に細かいが、思想が違う**（下記 F-2） | 🔴 要移植 |
| F8 | **`UsePerfectBalanceAoE(maxPowerSkill)` 新設** — ST版と同じ5分岐（桃園CD同期・`compareNextBurstTime`・ダウンタイム） | `CanPerfectBalanceMaxChargeAoE()` は**チャージ最大かつ紅蓮窓外**を見るだけ。桃園との同期は無い | 🔴 要移植 |
| F9 | **`Opener()` に自動選択を追加** — `MNK_SelectedOpener == 4` でPT内の踊り子を検出し `LL7`/`LL` を切替 | 3種の固定選択（DoubleLunar / SolarLunar / BrotherhoodFirst）。PT構成の検出は無し | 🔴 要移植 |
| F10 | **`MNKOpenerLogicSL7` / `MNKOpenerLogicLL7` を新設**（各20ステップの7秒バースト版） | レベル別5種（`Lvl90LL`/`Lvl100LL`/`Lvl90SL`/`Lvl100SL`/`Lvl100BHFirst`）。7秒版は無し | 🔴 要移植 |
| F11 | Buffs に `EarthsResolve = 1180` / `EarthsRumination = 3841` を追加 | `EarthsRumination` は上流にもあり。`EarthsResolve` は**上流に無いがフォークでも未使用**（定義のみ） | ⚪ 不要 |

---

### F-2. 踏鳴を押す際の「コンボ進捗」の扱い（F7 の詳細）

F7 のうち、移植時に**最も見落としやすい差分**。判定の中身ではなく**入口のガード**の話。

#### 上流: Opo GCD 直後を全分岐で強制

`CanPerfectBalance()` の前提ガードに入っており、例外が無い。

```csharp
if (... || !JustUsedOpoGCD(GCD, onAoE))
    return false;

private static bool JustUsedOpoGCD(float window, bool onAoE = false) =>
    onAoE ? ... : JustUsed(OriginalHook(Bootshine), window) || JustUsed(DragonKick, window);
```

**必ず Opo 型 GCD の直後1 GCD 以内**でしか踏鳴を押さない。
唯一の緩和は低レベル分岐の `JustUsedOpoGCD(GCD * 3, onAoE)` のみ。
復帰時の `ShouldUsePBAfterBurstHolding()` も同じガードを持つ。

#### フォーク: 分岐ごとに許容幅が違い、偶数窓は迂回できる

| 分岐 | コンボ進捗の条件 |
| --- | --- |
| 奇数窓 | `JustUsed(Bootshine/DragonKick, GCD)` — 1 GCD 以内 |
| **偶数窓** | `JustUsed(Bootshine/DragonKick, GCD * 2)` — **2 GCD 以内** |
| 低レベル | `JustUsed(Bootshine, GCD) \|\| JustUsed(DragonKick)` — 既定幅 |
| 追加 | `JustUsed(..., GCD)` — 1 GCD 以内 |
| ターゲット無し | **Opo 判定そのものが無い** |

さらに偶数窓のトリガは5つの **OR** で、後ろ3つは Opo 直後かどうかを見ていない。

```csharp
JustUsed(Bootshine, GCD * 2) || JustUsed(DragonKick, GCD * 2) ||
GetCooldownRemainingTime(Brotherhood) < (GCD * 1 + RemainingGCD) ||      // ← Opo判定を迂回
GetBuffRemainingTime(Buffs.WindsRumination) >= (GCD * 3 + RemainingGCD) || // ← 迂回
GetCooldownRemainingTime(RiddleOfWind) < (17.35 + GCD * 2 + RemainingGCD)  // ← 迂回
```

**桃園がもうすぐ空く／絶空拳バフが残っている／疾風が近い**のいずれかなら、
コンボのどの位置からでも踏鳴に入る。

#### 思想の差

| | 優先するもの |
| --- | --- |
| 上流 | **コンボ効率**。Opo 直後でなければバーストが迫っていても踏鳴を遅らせる |
| フォーク | **バースト同期**。窓に確実に収めるためコンボ位置を捨てる |

踏鳴中は型を無視して撃てるので、直前に Opo を消費しておけば踏鳴の3 GCD を
最も価値の高い構成に充てられる——上流はこれを厳格に守る設計。

フォークの緩和は B14（桃園を2 GCD 待たせる）と同じく
**「バースト同期 > コンボ効率」という一貫した思想**によるもの。

#### 判定: 🔴 要移植

特に `GetCooldownRemainingTime(Brotherhood) < (GCD * 1 + RemainingGCD)` は、
**桃園があと1 GCD で空くならコンボ位置を無視して踏鳴に入る**という強い指定で、
上流はこれを持たない。ここで1 GCD ずれる。

#### 上流に無い制御（F7 の中で移植必須の部分）

| # | 制御 | 目的 |
| --- | --- | --- |
| 1 | 偶数窓の Opo 迂回トリガ3つ | バースト窓に踏鳴を確実に収める |
| 2 | `RiddleOfWind` CD からの逆算（`17.35 + GCD * 2`） | 絶空拳を踏鳴窓に収める |
| 3 | `WindsRumination` バフ残量で踏鳴を起動 | 同上 |
| 4 | ターゲット無し分岐 | ダウンタイムブリッツ |

2・3 はどちらも「絶空拳を踏鳴窓に確実に収める」ための調整。
上流は絶空拳を `CanWindsReply()` で独立に処理しており、踏鳴のタイミングには反映していない。

---

## G. `MNK_Config.cs`

| # | 変更 | 上流最新 | 判断 |
| --- | --- | --- | --- |
| G1 | `MNK_ST_Brotherhood_SubOption` / `RiddleOfFire_SubOption` / `RiddleOfWind_SubOption` を**削除** | 上流も `SubOption` を廃止済み | ⚪ 不要 |
| G2 | 代わりに `MNK_ST_Brotherhood_HP` / `RiddleOfFire_HP` / `RiddleOfWind_HP` を新設（既定1） | `MNK_ST_BHHPOption`(25) + `BHHPBossOption` 等 | ⚪ 不要 |
| G3 | `MNK_AoE_RiddleOfFire_HP` 既定値 5 → **0** | — | ⚪ 不要（好みの初期値） |
| G4 | `MNK_SelectedOpener` 既定値 0 → **4（自動）** | 既定0 | 🔴 要移植（F9と一体） |
| G5 | `MNK_ST_Brotherhood_ROFLastOnly`（bool）新設 | 該当なし | 🔴 要移植 |
| G6 | `MNK_ST_Brotherhood_AdjustROF`（bool）新設 | 該当なし | 🔴 要移植 |
| G7 | `MNK_ST_FiresReply_Order`（3択）新設 | 該当なし | 🔴 要移植 |
| G8 | `MNK_ST_Phoenix_Order`（5択）新設 | 該当なし | 🔴 要移植 |
| G9 | `MNK_ST_Fast_Phoenix`（2択）新設 | 該当なし（固定ロジック） | 🔴 要移植 |
| G10 | `MNK_ST_Many_PerfectBalance`（2択）新設 | 該当なし | 🔴 要移植 |
| G11 | `MNK_AoE_RiddleOfEarth_Threshold` 新設（既定90） | `MNK_ST_EarthsReplyHPThreshold` 等が存在 | ⚪ 不要 |
| G12 | UI テキストを全面日本語化 | 公式ローカライズ（`ja.resx` 3,622エントリ） | ⚪ 不要 |

---

## H. `CustomComboPreset.cs`（MNK 関連）

| # | 変更 | 上流最新 | 判断 |
| --- | --- | --- | --- |
| H1 | `MNK_ST_AdvancedMode` の `ReplaceSkill` に `DragonKick` / `Thunderclap` を追加 | `Bootshine` / `LeapingOpo` のみ | ⚪ **不要**（B-1-3 で Custom Action 方式を採用したため消滅） |
| H2 | `MNK_STUseFiresReply` の親を `MNK_STUseBuffs` → **`MNK_STUseROF`** | 上流は**土台と同じ** `MNK_ST_AdvancedMode` 直下。紅蓮をOFFにしても乾坤が飛ぶ | 🔴 要移植 |
| H3 | `MNK_STUseWindsReply` の親を `MNK_STUseBuffs` → **`MNK_STUseROW`** | 同上。疾風をOFFにしても絶空拳が飛ぶ | 🔴 要移植 |
| H4 | `MNK_PerfectBalance` から `ConflictingCombos(MNK_PerfectBalanceProtection)` を削除 | 上流は**両方向**に宣言。強制は一方向なので、片方削除は**操作順で結果が変わる不安定な状態**を作る | ⚪ **不要**（消し忘れと判断） |
| H5 | MNK 全プリセットの名称・説明を日本語化 | 公式ローカライズ | ⚪ 不要 |

---

## I. 移植時に修正すべきバグ

| # | 場所 | 内容 |
| --- | --- | --- |
| 🐛1 | [MNK.cs:711](WrathCombo/Combos/PvE/MNK/MNK.cs#L711) | AoE ComboHeals で、ブラッドバスの条件が成立したときに **`return Role.SecondWind;`** している。`Role.Bloodbath` が正しい。**AoEでブラッドバスが永久に発動しない** |
| 🐛2 | [MNK.cs:731](WrathCombo/Combos/PvE/MNK/MNK.cs#L731) | `MNK_AOE_AdvancedMode` の必殺技判定が **ST用の `MNK_STUseMasterfulBlitz`** を見ている。`MNK_AoEUseMasterfulBlitz`(9040) が正しい。**AoE側の必殺技オプションが効かず、ST側の設定に引きずられる** |
| 🐛3 | [MNK_Helper.cs:100](WrathCombo/Combos/PvE/MNK/MNK_Helper.cs#L100) | `UsePerfectBalance()` に**空の `if` ブロック**（`AdjustROF` 時に何もしない意図だが可読性が低い） |
| 🐛4 | [MNK_Helper.cs:163](WrathCombo/Combos/PvE/MNK/MNK_Helper.cs#L163) | `UsePerfectBalance()` の奇数窓判定に**大きなコメントアウト塊**が残存。`Many_PerfectBalance` の条件が無効化されたまま。設定が効いていない可能性 |
| 🐛5 | [MNK_Helper.cs:1](WrathCombo/Combos/PvE/MNK/MNK_Helper.cs#L1) | `using ECommons.Logging;` がデバッグ出力のコメントアウトのみのために残っている |

---

## まとめ

全 **83項目**（うち MNK.cs が 52、Helper 11、Config 12、Preset 5、その他 2）。

| 判断 | 件数 | 主な内容 |
| --- | --- | --- |
| 🔴 **要移植** | **64件** | 多ボタン設計（B1〜B4）、`compareNextBurstTime`（F5）、**桃園と踏鳴の連動（B14）**、**踏鳴のタイミング制御（F7 / F-2）**、`Phoenix_Order`（B28）、7秒opener（F9/F10）、PT平均ヒール（A1/A2/C1/C2）、トゥルーノース制御（B30） |
| 🟡 **実測後** | 0件 | 上流が固定ロジックで別解を持つもの。素の上流を使って不満が出た項目だけ |
| ⚪ **不要** | 19件 | 上流が同等以上を実装済み、または移動のみ・API追従 |
| 🐛 **バグ** | 5件 | うち2件は挙動に実害あり |

セクション別の項目数: A(STシンプル) 7 / **B(STアドバンスト) 31** / C(AoEシンプル) 2 / D(AoEアドバンスト) 13 / E(その他コンボ) 2 / F(Helper) 11 / G(Config) 12 / H(Preset) 5

### 移植の核

**29件の要移植のうち、独立して価値があるのは次の4つ。**

1. **多ボタン設計（B1〜B4, F2, H1）** — 温存モード / バフ抑制モード / 羅刹衝。作業量は最大だが、上流に概念が無く代替もない
2. **`compareNextBurstTime()`（F5）** — 桃園と紅蓮のズレを見てバースト窓を推定する中核ロジック。他の多くがこれに依存
3. **開幕の自動選択と7秒版（F9, F10, G4）** — PT構成でバーストタイミングを変える
4. **PT平均HPによるヒール（A1, A2, C1, C2）** — 4箇所とも同じ1行の置換で済む、最も安上がりな移植

### 最初に手を付けるべきもの

**C1 / C2（AoEシンプルの2項目）** — AoE シンプルモードは20個中18個が完全同一で、変更はこの2つだけ。
上流最新でも同じ1行の置換で済む可能性が高く、**移植手順の検証台として最適**。
