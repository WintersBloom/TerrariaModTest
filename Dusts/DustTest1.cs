using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;   // SpriteEffects / Texture2D 在这里
using Terraria;
using Terraria.ModLoader;

namespace OurMod.Dusts
{
    /// <summary>
    /// 自定义粒子：贴图 DustTest1.png 是一张 10×50 的图集，
    /// 竖向堆了 5 个 10×10 的粒子，种类之间随机挑。
    /// </summary>
    public class DustTest1 : ModDust
    {
        /// <summary>图集里一共几种粒子</summary>
        public const int VariantCount = 5;

        /// <summary>每一格的边长（正方形）</summary>
        public const int Cell = 10;

        public override void SetStaticDefaults()
        {
            // UpdateType = -1 表示不用原版任何粒子的更新逻辑，完全由下面的 Update 接管
            UpdateType = -1;
        }

        public override void OnSpawn(Dust dust)
        {
            // OnSpawn 在 Dust.NewDust 内部被调用，此时 position/velocity/color 已经由调用方设好
            dust.noGravity = false;    // 不受重力，悬浮着飘
            dust.noLight = false;     // 允许影响环境光照（要发光就靠它）
            dust.frame = new Rectangle(0, 0, Cell, Cell);   // 默认第 0 种，调用方会覆盖成随机值
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;      // 按速度移动
            dust.velocity *= 0.94f;              // 空气阻力，速度逐帧衰减
            dust.rotation += 0.08f;              // 缓慢自转
            dust.scale *= 0.985f;                // 每帧缩小一点点

            // 发光：颜色取自 dust.color，亮度随缩放一起衰减
            // ToVector3() 把 Color 的 0~255 通道转成 0~1 的浮点
            Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.8f * dust.scale);

            // 寿命：alpha 每帧 +6，从 0 涨到 255 大约 42 帧（0.7 秒）
            dust.alpha += 6;

            if (dust.alpha >= 255 || dust.scale < 0.15f)
            {
                dust.active = false;   // 标记为失效，下一秒会被 Main.dust 池回收
                return false;
            }

            return false;   // 返回 false = 不运行原版 Update，避免它把我们的运动覆盖掉
        }

        /// <summary>
        /// 自己画，因为默认绘制只认 10×30 的三帧布局，而我们这里是 5 帧。
        /// 返回 false 表示"我画完了，原版别再画一次"。
        /// </summary>
        public override bool PreDraw(Dust dust)
        {
            // 注意：Texture2D 在这个类里是 ModDust.Texture2D 属性（Asset&lt;Texture2D&gt;），
            // 所以用 var 接住它的 .Value，不要写成 "Texture2D tex = Texture2D.Value"
            var tex = Texture2D.Value;

            // dust.frame.Y 里存的是"第几种"（0、10、20、30、40），除回去得到索引
            int variant = dust.frame.Y / Cell;

            // 源矩形：整张图宽 10，高取第 variant 格
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
    }
}
