using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace OurMod.Projectiles;
public class OurSwordBeam :ModProjectile
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

    // 撞到方块时调用，返回 true = 这个弹幕消失
    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        // SoundEngine.PlaySound 在指定位置播一个音效；
        // with { Volume = ..., PitchVariance = ... } 是 C# 的 record with 表达式，用来改副本的音量/音调抖动
        SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.6f, PitchVariance = 0.15f }, Projectile.Center);
        return true;
    }

    // 打中敌人时调用（1.4.4 的签名带 NPC.HitInfo）
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        SoundEngine.PlaySound(SoundID.NPCHit1 with { Volume = 0.5f, PitchVariance = 0.1f }, target.Center);
    }
}
