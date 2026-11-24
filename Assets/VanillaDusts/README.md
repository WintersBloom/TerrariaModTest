# 原版粒子（Dust）贴图参考

## 文件

| 文件 | 说明 |
|---|---|
| `Dust.png` | 原版全部粒子的图集，**1000 × 120**，直接从游戏资源解出，未改一像素 |
| `Dust_x4_grid.png` | 上面那张放大 4 倍（4000 × 480）并叠了网格，方便看细节 |

## 图集布局（从源码推出来，不是猜的）

源码位置：`Terraria.Dust.NewDust` 里设置 `dust.frame` 的那几行：

```csharp
dust.frame.X = 10 * Type;
dust.frame.Y = 10 * Main.rand.Next(3);
int num4 = Type;
while (num4 >= 100)
{
    num4 -= 100;
    dust.frame.X -= 1000;
    dust.frame.Y += 30;
}
dust.frame.Width = 8;
dust.frame.Height = 8;
```

翻译成规则：

- **整张图分 4 个横向区块**，每块 1000 宽、30 高（红线分隔的就是区块边界）
- **每个区块装 100 种粒子**：区块 0 = type 0–99，区块 1 = type 100–199，区块 2 = type 200–299，区块 3 = type 300–376
- **每种粒子有 3 个变体**，纵向排列，每个占 **10 × 10** 像素的格子
- 但真正画的只有格子左上角的 **8 × 8**（`frame.Width/Height = 8`），右边 2 像素和下面 2 像素留空

所以第 `type` 种粒子的三个变体位置是：

```
block = type / 100          // 第几个 30 像素高的区块
col   = type % 100          // 区块内第几列
x     = 10 * col
y     = 30 * block + 10 * variant    // variant = 0 / 1 / 2
宽高  = 8 x 8
```

举例：`DustID.GoldFlame = 228` → block 2、col 28 → 三个变体在 `(280, 60)`、`(280, 70)`、`(280, 80)`，各取 8×8。

## 自己画粒子时的规格

`ModDust` 的贴图要求（`tModLoader.xml` 文档原文）：

> vertical alignment of three 10 x 10 pixel squares

即一张 **10 宽 × 30 高** 的 PNG，竖着三帧、每帧 10×10、实际画 8×8。文件名与类名一致，放在 `Dusts/` 目录下（命名空间也要对齐，例如 `OurMod.Dusts.OurDust1` → `Dusts/OurDust1.png`）。

## 常用 DustID 速查

| 名字 | ID | 名字 | ID |
|---|---|---|---|
| Dirt | 0 | Stone | 1 |
| Blood | 5 | Torch | 6 |
| Iron | 8 | Copper | 9 |
| Gold | 10 | Silver | 11 |
| HeartCrystal | 12 | MagicMirror | 15 |
| Corruption | 14 | Shadowflame | 27 |
| Smoke | 31 | Water | 33 |
| Ash | 36 | Snow | 76 |
| Ice | 80 | Frost | 92 |
| Crimson | 115 | Bee | 150 |
| Rain | 154 | TreasureSparkle | 204 |
| GoldFlame | 228 | Vortex | 229 |
| FlameBurst | 270 | SilverFlame | 279 |
| Firefly | 304 | SparkForLightDisc | 306 |
| ShimmerSpark | 309 | StarRoyale | 320 |

完整 377 个常量在 `Terraria.ID.DustID`。
