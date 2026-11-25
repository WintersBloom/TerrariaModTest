using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using OurMod.Common;

namespace OurMod.Content.Projectiles;

public class OurSwordBeam : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = 10;                      // 碰撞箱宽
        Projectile.height = 12;                     // 碰撞箱高
        Projectile.friendly = true;                 // 友方，能打怪
        Projectile.hostile = false;                 // 不是敌方
        Projectile.DamageType = DamageClass.Melee;  // 吃近战加成
        Projectile.penetrate = 1;                   // 穿透次数：1 = 打中 1 个敌人就消失；-1 = 无限穿透
        Projectile.timeLeft = 120;                  // 存活 120 tick = 2 秒（60 tick = 1 秒）
        Projectile.tileCollide = true;              // true = 撞到方块就消失；要穿墙填 false
        Projectile.ignoreWater = true;              // 水下不减速
        Projectile.light = 0.8f;                    // 照亮周围
    }

    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = 6;                  // 6 帧，纵向等分（贴图 10×72，每帧 10×12）
    }

    public override void AI()
    {
        // 朝向速度方向：贴图本身剑尖朝上，所以要先 +90°（PiOver2）才是"朝向速度"；
        // 再叠一个正弦摆动，MathF.Sin 让剑气飞行时略微左右晃
        Projectile.rotation = Projectile.velocity.ToRotation()
                              + MathHelper.PiOver2
                              + MathF.Sin(Main.GameUpdateCount * 0.15f) * 0.25f;

        // 帧动画：每 4 tick 换一帧
        if (++Projectile.frameCounter >= 4)
        {
            Projectile.frameCounter = 0;
            Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Type];
        }

        // 每 3 tick 撒一个金色火花（NewDustPerfect 返回 Dust 本体，可以直接改字段）
        if (Main.rand.NextBool(3))
        {
            Dust d = Dust.NewDustPerfect(
                Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                DustID.GoldFlame,
                Projectile.velocity * -0.1f,
                100, default, 1.1f);
            d.noGravity = true;
        }
    }

    // 撞到方块时调用，返回 true = 这个弹幕消失
    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.7f, PitchVariance = 0.15f }, Projectile.Center);
        DustBurst.Fan(Projectile.Center,-Projectile.velocity,5,90,Color.White);
        return true;
    }

    // 打中敌人时调用（1.4.4 的签名带 NPC.HitInfo）
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.7f, PitchVariance = 0.15f }, Projectile.Center);
        DustBurst.Fan(Projectile.Center,-Projectile.velocity,5,90,Color.White);
    }
}
