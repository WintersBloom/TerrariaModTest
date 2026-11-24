# 原版 ID 对照表

全部从本机 `tModLoader.dll` 里挖出来，不是网上抄的。

## 文件

| 文件 | 行数 | 对应常量类 | 说明 |
|---|---|---|---|
| `NPCID.csv` | 754 | `Terraria.ID.NPCID` | 全部敌人/城镇 NPC |
| `ItemID.csv` | 5504 | `Terraria.ID.ItemID` | 全部物品 |
| `ProjectileID.csv` | 1022 | `Terraria.ID.ProjectileID` | 全部弹幕 |
| `DustID.csv` | 377 | `Terraria.ID.DustID` | 全部粒子 |
| `TileID.csv` | 693 | `Terraria.ID.TileID` | 全部物块 |

格式统一为三列，UTF-8 BOM，Excel / VS Code / Rider 直接打开：

```csv
ID,Name,Chinese
3,Zombie,僵尸
757,TerraBlade,泰拉刃
```

- `ID` — 游戏内部整数，写进存档和网络包的就是它
- `Name` — 代码里用的常量名，即 `NPCID.Zombie`
- `Chinese` — 游戏内中文显示名，空 = 这个名字没有中文翻译

## 数据来源

1. **ID 与英文名**：用 `ilspycmd` 反编译 `C:\Program Files (x86)\Steam\steamapps\common\tModLoader\tModLoader.dll` 里的 `Terraria.ID.*` 类，抓出所有 `public const short/ushort 名字 = 数字;`
2. **中文名**：从同一个 dll 的**嵌入资源**里提取 `Terraria.Localization.Content.zh_Hans.{NPCs,Items,Projectiles}.json`，按常量名对上
3. 注意：`ilspycmd` 在 Windows 上输出的是 **UTF-16 LE**，用 `utf-8` 读会一行都抓不到

## 怎么用

### 代码里：id → 名字

```csharp
using Terraria.ID;

int id = npc.type;
string en = NPCID.Search.GetName(id);          // "Zombie"，找不到抛异常
string cn = Lang.GetNPCNameValue(id);          // "僵尸"
```

四个 ID 类都有 `Search`（`ReLogic.Reflection.IdDictionary`）：

```csharp
NPCID.Search.GetName(id)          NPCID.Search.GetId("Zombie")
ItemID.Search.GetName(id)         ItemID.Search.GetId("TerraBlade")
ProjectileID.Search.GetName(id)   ProjectileID.Search.GetId("SwordBeam")
DustID.Search.GetName(id)         DustID.Search.GetId("GoldFlame")
```

安全版：`TryGetName(int id, out string name)` / `TryGetId(string name, out int id)`。

### 代码里：名字 → id

```csharp
int zombieId = NPCID.Search.GetId("Zombie");   // 3
```

但更推荐直接用常量：

```csharp
if (npc.type == NPCID.Zombie) { ... }
```

常量在编译期就定死了，改不动、不会拼错，还能被 IDE 自动补全。`GetId` 留给解析配置文件、命令行参数这类运行时才知道名字的场合。

### 游戏内直接看

```csharp
Main.NewText($"{npc.type} = {NPCID.Search.GetName(npc.type)} ({Lang.GetNPCNameValue(npc.type)})", Color.Yellow);
```

## 几个坑

- **负 ID 是合法的**：`NPCID.csv` 开头一堆 `-66,-65,...`，比如 `BigHornetStingy = -65`，是原版用来标记同一物种不同外观变体的技巧，不是错误数据。
- **一个 ID 可能有多个名字**：本表按反编译顺序保留全部别名，所以同一个 ID 可能出现在多行。
- **`Chinese` 有空值**：共 224 条没有中文（NPC 60 / 弹幕 101 / 物品 63），都是较新的内容或本来就没翻的名字，不影响使用。
- **`DustID` / `TileID` 没有中文列**：原版没有为这两类单独提供 `zh_Hans` 文本文件。
- **mod 内容不在这几张表里**：mod 加的东西 ID 从 `NPCID.Count` 之后开始，名字用 `ModContent.GetModNPC(id).FullName`（格式 `"ModName/Name"`）。

## 重新生成

反编译产物在 `C:\Users\李林峯\Desktop\泰拉瑞亚模组开发\_tmp\`：

```powershell
cd "C:\Users\李林峯\Desktop\泰拉瑞亚模组开发\_tmp"
$dll = "C:\Program Files (x86)\Steam\steamapps\common\tModLoader\tModLoader.dll"
ilspycmd -t Terraria.ID.NPCID $dll > NPCID.cs
```

然后跑一遍提取脚本即可（脚本逻辑见上面的「数据来源」）。
