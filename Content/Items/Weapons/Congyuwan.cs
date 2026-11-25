using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using OurMod.Content.Projectiles;
namespace OurMod.Content.Items.Weapons;

public class Congyuwan : ModItem
{

    public override void SetDefaults()
    {
        Item.width = 87;                        // width：碰撞箱宽（像素），等于贴图宽
        Item.height = 90;                       // height：碰撞箱高（像素）
        Item.damage = 20;                       // damage：基础伤害
        Item.DamageType = DamageClass.Melee;    // DamageClass.Melee：近战伤害类型
        Item.useStyle = ItemUseStyleID.Swing;   // Swing：挥舞动画（剑的标准挥法）
        Item.useTime = 20;                      // useTime：攻击间隔，单位 tick，20 tick = 1/3 秒
        Item.useAnimation = 20;                 // useAnimation：挥动动画时长，一般跟着 useTime
        Item.knockBack = 6f;                    // knockBack：击退力度
        Item.value = 10000;                     // value：价值，单位铜币（10000 = 1 银币）
        Item.rare = ItemRarityID.Blue;          // rare：稀有度，决定名字颜色
        Item.UseSound = SoundID.Item1;          // UseSound：挥动时的音效
        Item.autoReuse = true;                  // autoReuse：按住左键自动连续挥
        Item.maxStack = 1;                      // maxStack：堆叠上限，武器固定 1
        Item.scale = 1f;                        // scale：绘制缩放，1 = 原尺寸
    }
}
