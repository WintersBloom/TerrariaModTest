using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;   // SpriteEffects / Texture2D 在这里
using Terraria;
using Terraria.ModLoader;

namespace OurMod.Dusts;

/// <summary>
/// 「决心」粒子。贴图 DeterminationDust.png 是 10×40 的图集，
/// 竖向堆了 4 个 10×10 的粒子，生成时随机挑一种。
/// 表现：轻微向上飘、缓慢自转、越飘越淡、持续发光。
/// </summary>
public class DeterminationDust : ModDust
{
    /// <summary>图集里一共几种粒子</summary>
    public const int VariantCount = 4;

    /// <summary>每一格的边长（正方形）</summary>
    public const int Cell = 10;

    public override void SetStaticDefaults()
    {
        // UpdateType = -1：不用原版任何粒子的更新逻辑，完全由下面的 Update 接管
        UpdateType = -1;
    }

    public override void OnSpawn(Dust dust)
    {
        // OnSpawn 在 Dust.NewDust 内部被调用，此时 position/velocity/color 已由调用方设好
        dust.noGravity = true;     // 不受重力，悬浮飘散
        // noLight = false 表示"这颗粒子是允许发光的"（源码注释：If true, the dust will not emit light）
        // ⚠️ 它管的是"粒子自己发光"，不是"粒子受不受环境光照影响"，别和 Lighting.AddLight 搞混
        dust.noLight = false;
        dust.frame = new Rectangle(0, 0, Cell, Cell);   // 默认第 0 种，Spawn() 会改成随机的
    }

    public override bool Update(Dust dust)
    {
        // —— 运动 ——
        dust.position += dust.velocity;
        dust.velocity *= 0.96f;                 // 空气阻力，飘几下就慢下来
        dust.velocity.Y -= 0.008f;              // 一点点向上的浮力，像雾气往上飘

        // —— 旋转 ——
        dust.rotation += 0.035f;                // 每帧转一点点，肉眼可见的缓慢自转

        // —— 消散 ——
        dust.scale *= 0.99f;                    // 缓慢变小
        dust.alpha += 2;                        // 缓慢变透明（0→255 约 2 秒）

        // —— 发光 ——
        // ToVector3() 把 Color 的 0~255 通道转成 0~1 的浮点
        // 亮度跟着 scale 一起衰减，粒子越大越亮
        Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.9f * dust.scale);

        if (dust.alpha >= 255 || dust.scale < 0.2f)
        {
            dust.active = false;   // 标记失效，Main.dust 池会回收
            return false;
        }

        return false;   // 返回 false = 不运行原版 Update，避免它覆盖我们的运动
    }

    /// <summary>
    /// 自己画，因为默认绘制只认 10×30 的三帧布局，而这里是 4 帧。
    /// 返回 false 表示"我画完了，原版别再画一次"。
    /// </summary>
    public override bool PreDraw(Dust dust)
    {
        // Texture2D 在这里是 ModDust.Texture2D 属性（Asset&lt;Texture2D&gt;），
        // 所以用 var 接 .Value，不要写成 "Texture2D tex = Texture2D.Value"
        var tex = Texture2D.Value;

        // dust.frame.Y 里存的是"第几种"（0、10、20、30），除回去得到索引
        int variant = dust.frame.Y / Cell;

        Rectangle source = new Rectangle(0, variant * Cell, Cell, Cell);

        // 渐隐：alpha 越大越透明
        Color drawColor = dust.color * (1f - dust.alpha / 255f);

        Main.spriteBatch.Draw(
            tex,
            dust.position - Main.screenPosition,   // 世界坐标 → 屏幕坐标
            source,
            drawColor,
            dust.rotation,                          // 旋转角（弧度）
            new Vector2(Cell / 2f),                 // 以贴图中心为旋转轴
            dust.scale,
            SpriteEffects.None,
            0f);

        return false;
    }

    /// <summary>1 格 = 16 像素</summary>
    public const int PixelsPerTile = 16;

    /// <summary>默认散布半径：4 格 = 64 像素</summary>
    public const float DefaultRadius = 4 * PixelsPerTile;

    /// <summary>
    /// 在 <paramref name="center"/> 周围 <paramref name="radius"/> 像素内**随机取一个点，生成一颗**粒子。
    /// 位置、初速、变体、初始旋转全部随机。
    /// </summary>
    /// <param name="center">生成中心（世界坐标，通常是玩家或弹幕的中心）</param>
    /// <param name="radius">散布半径（像素）。1 格 = 16 像素，默认 4 格 = 64 像素</param>
    /// <param name="color">
    /// 染色，null 则用白色（保持贴图原色）。
    /// 注意 dust.color 是**乘法**染色：贴图是纯红的话，乘任何颜色都只会变暗红，
    /// 想真正调出金黄/青色，得把贴图重画成白色或浅灰。
    /// </param>
    /// <returns>生成出来的那颗 Dust，方便你接着改字段</returns>
    public static Dust SpawnOne(Vector2 center, float radius = DefaultRadius, Color? color = null)
    {
        // 默认白色 = 不改变贴图颜色；传了 color 就用传的
        Color tint = color ?? Color.White;

        // NextVector2Circular：在半径为参数值的圆内均匀取一个点
        Vector2 pos = center + Main.rand.NextVector2Circular(radius, radius);
        Vector2 vel = Main.rand.NextVector2Circular(0.6f, 0.6f);   // 很轻的初速，慢慢飘

        // NewDustPerfect 会调用 OnSpawn，所以 frame/rotation 必须在这之后改
        Dust d = Dust.NewDustPerfect(
            pos,
            ModContent.DustType<DeterminationDust>(),
            vel,
            0,
            tint,
            Main.rand.NextFloat(0.9f, 1.3f));   // 大小在一个范围内随机

        d.frame.Y = Main.rand.Next(VariantCount) * Cell;      // 随机挑一种
        d.rotation = Main.rand.NextFloat(MathHelper.TwoPi);    // 随机初始角度
        return d;
    }

    /// <summary>
    /// 在中心附近随机生成 <paramref name="count"/> 颗（内部就是循环调 SpawnOne）。
    /// </summary>
    public static void Spawn(Vector2 center, int count, float radius = DefaultRadius, Color? color = null)
    {
        for (int i = 0; i < count; i++)
            SpawnOne(center, radius, color);
    }
}
