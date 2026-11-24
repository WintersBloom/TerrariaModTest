using Microsoft.Xna.Framework;
using OurMod.Dusts;
using Terraria;
using Terraria.ModLoader;

namespace OurMod.Common
{
    /// <summary>
    /// 粒子爆发工具：把一组粒子按扇形朝某个方向喷出去。
    /// </summary>
    public static class DustBurst
    {
        /// <summary>
        /// 扇形喷射。
        /// </summary>
        /// <param name="origin">喷射起点（世界坐标）</param>
        /// <param name="direction">扇形中心朝向，不用归一化也行</param>
        /// <param name="count">生成几个</param>
        /// <param name="spreadDegrees">扇形总张角（度）。90 就是左右各偏 45 度</param>
        /// <param name="color">粒子颜色，同时决定发光颜色</param>
        /// <param name="speedMin">速度下限</param>
        /// <param name="speedMax">速度上限</param>
        public static void Fan(Vector2 origin, Vector2 direction, int count, float spreadDegrees, Color color,
            float speedMin = 3f, float speedMax = 7f)
        {
            int dustType = ModContent.DustType<DustTest1>();

            // 防止传进来零向量导致后续旋转全是 NaN
            direction = direction.SafeNormalize(Vector2.UnitX);

            for (int i = 0; i < count; i++)
            {
                // NextFloat() 返回 0~1，减 0.5 变成 -0.5~0.5，再乘总张角 = 范围内随机偏移
                float offset = MathHelper.ToRadians(spreadDegrees) * (Main.rand.NextFloat() - 0.5f);
                Vector2 vel = direction.RotatedBy(offset) * Main.rand.NextFloat(speedMin, speedMax);

                // NewDustPerfect 会依次设置 position/type/velocity/color/scale，然后触发 OnSpawn
                Dust d = Dust.NewDustPerfect(origin, dustType, vel, 0, color, Main.rand.NextFloat(0.8f, 1.4f));

                // 随机挑图集里的一种形状（必须在 NewDustPerfect 之后设，否则会被 OnSpawn 覆盖）
                d.frame.Y = Main.rand.Next(DustTest1.VariantCount) * DustTest1.Cell;
            }
        }
    }
}
