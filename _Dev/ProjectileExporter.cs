using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace OurMod._Dev;

/// <summary>
/// 【临时开发工具】把原版「剑气 / 剑光」类弹幕的贴图导出成 PNG，供参考画风。
///
/// 为什么放在 PostUpdateEverything 而不是 PostSetupContent：
///   tML 的 mod 加载阶段跑在【后台线程】，任何 FNA3D 图形函数
///   （LoadProjectile / GetData / SaveAsPng）都会抛 ThreadStateException。
///   PostUpdateEverything 由主线程 Main.Update() 每 tick 调用，才能碰图形 API。
///
/// 导出完这个类就可以删掉（它只在第一次进世界时跑一遍）。
/// </summary>
public class ProjectileExporter : ModSystem
{
    // 只挑名字里带这些词的弹幕 —— 原版弹幕没有显示名，但 ProjectileID 的常量名有语义
    private static readonly string[] Keywords = { "Beam", "Blade", "Slash", "Sword", "Wave" };

    private bool _done;      // 已经成功跑过
    private int _attempts;   // 尝试次数（防止每 tick 无限重试）

    public override void PostUpdateEverything()
    {
        // Main.dedServ = 专用服务器（无图形），直接跳过
        if (_done || Main.dedServ || _attempts >= 2) return;
        _attempts++;
        _done = TryExport();
    }

    private bool TryExport()
    {
        string dir = Path.Combine(Main.SavePath, "VanillaProjectileExports");
        try
        {
            Directory.CreateDirectory(dir);
            int ok = 0, fail = 0;
            var errLog = new StringBuilder();

            // 反射 ProjectileID 里的所有 const short，按名字筛剑气类
            foreach (FieldInfo field in typeof(ProjectileID).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (!field.IsLiteral) continue;                          // 只要常量
                if (field.GetRawConstantValue() is not short id) continue; // ProjectileID 全是 short
                if (!Keywords.Any(k => field.Name.Contains(k, StringComparison.Ordinal))) continue;

                string file = Path.Combine(dir, $"Projectile_{id}_{field.Name}.png");

                // 已导出过就跳过（可重复运行，增量导出）
                if (File.Exists(file) && new FileInfo(file).Length > 0) { ok++; continue; }

                try
                {
                    Main.instance.LoadProjectile(id);                    // 确保贴图已加载进显存
                    Texture2D tex = TextureAssets.Projectile[id].Value;   // 取贴图
                    if (tex == null) { fail++; errLog.AppendLine($"{id} {field.Name}: texture is null"); continue; }
                    WritePng(tex, file);                                 // 手写 PNG（不依赖 SaveAsPng）
                    ok++;
                }
                catch (Exception ex)
                {
                    fail++;
                    errLog.AppendLine($"{id} {field.Name}: {ex.GetType().Name}: {ex.Message}");
                }
            }

            if (errLog.Length > 0) File.WriteAllText(Path.Combine(dir, "_error.txt"), errLog.ToString());
            Main.NewText($"[Exporter] 弹幕贴图导出完成 ok={ok} fail={fail} → {dir}", Color.Lime);
            return true;
        }
        catch (Exception ex)
        {
            try { File.WriteAllText(Path.Combine(dir, "_fatal.txt"), $"{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}"); } catch { }
            return false;
        }
    }

    /// <summary>
    /// 手写 PNG 编码：Texture2D.GetData 拿像素 → 逐行加 filter 字节 → zlib 压缩 → 拼 IHDR/IDAT/IEND。
    /// 不调 FNA 的 SaveAsPng，避免它内部依赖额外状态。
    /// </summary>
    private static void WritePng(Texture2D tex, string path)
    {
        int w = tex.Width, h = tex.Height;
        var data = new Color[w * h];
        tex.GetData(data);   // Color 是 XNA 的 RGBA 结构体

        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
        fs.Write(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, 0, 8);   // PNG 签名

        // ---- IHDR：宽、高、位深 8、颜色类型 6(RGBA)、压缩/过滤/隔行都为 0 ----
        using (var ihdr = new MemoryStream())
        {
            WriteBE(ihdr, w);
            WriteBE(ihdr, h);
            ihdr.WriteByte(8);
            ihdr.WriteByte(6);
            ihdr.WriteByte(0);
            ihdr.WriteByte(0);
            ihdr.WriteByte(0);
            WriteChunk(fs, "IHDR", ihdr.ToArray());
        }

        // ---- IDAT：每行开头 1 字节 filter(0=None)，然后 RGBA 四通道 ----
        using (var raw = new MemoryStream())
        {
            for (int y = 0; y < h; y++)
            {
                raw.WriteByte(0);
                for (int x = 0; x < w; x++)
                {
                    Color c = data[y * w + x];
                    raw.WriteByte(c.R);
                    raw.WriteByte(c.G);
                    raw.WriteByte(c.B);
                    raw.WriteByte(c.A);
                }
            }
            raw.Position = 0;
            using var comp = new MemoryStream();
            using (var z = new ZLibStream(comp, CompressionLevel.Optimal, leaveOpen: true))
            {
                raw.CopyTo(z);
            }
            WriteChunk(fs, "IDAT", comp.ToArray());
        }

        WriteChunk(fs, "IEND", Array.Empty<byte>());
    }

    /// <summary>大端写 int32（PNG 的长度和 CRC 都是大端）。</summary>
    private static void WriteBE(Stream s, int v)
    {
        s.WriteByte((byte)(v >> 24));
        s.WriteByte((byte)(v >> 16));
        s.WriteByte((byte)(v >> 8));
        s.WriteByte((byte)v);
    }

    /// <summary>写一个 PNG chunk：[长度4][类型4][数据][CRC4]。</summary>
    private static void WriteChunk(Stream s, string type, byte[] data)
    {
        WriteBE(s, data.Length);
        byte[] typeBytes = Encoding.ASCII.GetBytes(type);
        s.Write(typeBytes, 0, 4);
        s.Write(data, 0, data.Length);
        WriteBE(s, (int)Crc32(typeBytes, data));
    }

    /// <summary>PNG 用的 CRC32（多项式 0xEDB88320，反射版）。</summary>
    private static uint Crc32(byte[] a, byte[] b)
    {
        uint c = 0xFFFFFFFF;
        foreach (byte x in a.Concat(b))
        {
            c ^= x;
            for (int k = 0; k < 8; k++)
                c = (c & 1) != 0 ? (c >> 1) ^ 0xEDB88320 : c >> 1;
        }
        return ~c;
    }
}
