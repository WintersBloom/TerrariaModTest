using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using OurMod.Content.Projectiles;

namespace OurMod.Content.Items.Weapons
{
	/// <summary>
	/// 仿原版"骑枪"（Jousting Lance，原版物品 ID 4788，动物学家出售）。
	/// 原版数据：60 近战伤害 / 击退 12 / 使用时间 24 / 射速 3.5 / 稀有度浅红 / 售价 1 金 20 银。
	/// 特点：按住会保持长枪前伸，伤害与击退随玩家移动速度提升（坐骑、冲刺、赫尔墨斯靴都加成）。
	/// 教学点：展示"伤害不等于武器面板"的武器 —— 在自己的弹幕里每帧改写 Projectile.damage。
	/// </summary>
	public class JoustingLance : ModItem
	{
		public override void SetDefaults()
		{
			Item.damage = 60;
			Item.DamageType = DamageClass.Melee;
			Item.knockBack = 12f;
			Item.width = 40;
			Item.height = 40;
			Item.useTime = 24;
			Item.useAnimation = 24;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.channel = true;          // 按住保持前伸
			Item.noMelee = true;
			Item.noUseGraphic = true;     // 由弹幕画长枪
			Item.autoReuse = true;
			Item.shoot = ModContent.ProjectileType<JoustingLanceProjectile>();
			Item.shootSpeed = 3.5f;
			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.sellPrice(gold: 1, silver: 20);
			Item.UseSound = SoundID.Item1;
		}

		public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "JoustingHint", "Build momentum to increase attack power"));
		}
	}
}
