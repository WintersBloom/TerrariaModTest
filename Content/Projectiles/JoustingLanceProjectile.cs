using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using OurMod.Content.Items.Weapons;

namespace OurMod.Content.Projectiles
{
	/// <summary>
	/// 骑枪本体：一个"枪尖判定"弹幕。
	/// 每帧根据玩家当前速度重算伤害与击退，速度越快越痛（原版公式见下方注释）。
	/// </summary>
	public class JoustingLanceProjectile : ModProjectile
	{
		/// <summary>原版速度表把"像素/帧"换算成"mph"的近似系数。</summary>
		private const float PixelsPerTickToMph = 5.114f;

		/// <summary>枪尖最大前伸距离（像素）。</summary>
		private const float MaxReach = 96f;

		public override string Texture => "One/Content/Items/example/JoustingLanceProjectile";

		private int Extend => (int)Projectile.ai[0];

		public override void SetDefaults()
		{
			Projectile.width = 28;
			Projectile.height = 28;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 7200;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 12;
		}

		public override void AI()
		{
			Player player = Main.player[Projectile.owner];

			// 第一帧把"基础伤害"存进 ai[1]，之后每帧都用它重新计算，避免反复叠加
			if (Projectile.ai[1] <= 0f)
			{
				Projectile.ai[1] = Projectile.damage;
			}

			// 玩家松手 / 死亡 / 换武器 → 收枪
			if (!player.active || player.dead || !player.channel || player.HeldItem.type != ModContent.ItemType<JoustingLance>())
			{
				Projectile.Kill();
				return;
			}

			// 原版机制：玩家受到伤害时会解除骑枪。
			// Player 上没有公开的"刚受击"标志位，标准做法是在受伤钩子里处理，例如：
			//   ModPlayer.PostHurt() 里找到属于该玩家的骑枪弹幕并 Kill()。
			// 为保持示例简单，这里只处理死亡/松手/换武器三种情况。

			// 枪尖方向：朝当前移速方向；站着不动就朝鼠标方向
			Vector2 dir = player.velocity.Length() > 0.5f
				? player.velocity.SafeNormalize(Vector2.UnitX)
				: Projectile.DirectionTo(Main.MouseWorld);

			// 前伸动画：10 帧伸展到位
			Projectile.ai[0] = MathHelper.Min(Extend + 1, 10);
			float reach = MaxReach * (Extend / 10f);

			Projectile.Center = player.MountedCenter + dir * reach;
			Projectile.velocity = Vector2.Zero;
			Projectile.rotation = dir.ToRotation() + MathHelper.PiOver4;

			// ---- 核心：速度加成公式（原版） ----
			// 伤害  = floor(基础伤害 × (mph × 22/875 + 0.1))
			// 击退  = mph × 武器击退 × 44/1575
			float mph = player.velocity.Length() * PixelsPerTickToMph;
			float damageMultiplier = mph * 22f / 875f + 0.1f;

			Projectile.damage = (int)(Projectile.ai[1] * MathHelper.Clamp(damageMultiplier, 0.1f, 8f));

			if (player.HeldItem.ModItem is JoustingLance)
			{
				Projectile.knockBack = mph * player.HeldItem.knockBack * 44f / 1575f;
			}

			// 速度越快，枪尖越亮
			Lighting.AddLight(Projectile.Center, 0.5f + mph * 0.02f, 0.4f, 0.8f);
		}
	}
}
