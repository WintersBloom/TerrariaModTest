using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace MyFirstMod.Projectiles;
public class MySwordBeam :ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = 10;           // 碰撞箱宽
        Projectile.height = 12;          // 碰撞箱高
        Projectile.friendly  = true;     // 友方，能打怪
        Projectile.hostile   = false;    // 不是敌方
        Projectile.DamageType = DamageClass.Melee;  // 吃近战加成
        Projectile.penetrate = 1;        // 穿透次数：1 = 打中 1 个敌人就消失；-1 = 无限穿透
        Projectile.timeLeft  = 120;      // 存活 120 tick = 2 秒（60 tick = 1 秒）
        Projectile.tileCollide = true;   // true = 撞到方块就消失；要穿墙填 false
        Projectile.ignoreWater = true;   // 水下不减速
        Projectile.light = 0.8f;         // 照亮周围
    }
    public override void AI()
    {
        Projectile.rotation = Projectile.velocity.ToRotation();   // 贴图朝速度方向

        // 帧动画：每 4 tick 换一帧
        if (++Projectile.frameCounter >= 4)
        {
            Projectile.frameCounter = 0;
            Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Type];
        }
    }
    public override void SetStaticDefaults()
{
    Main.projFrames[Type] = 6;   // 6 帧，纵向等分
}
}
