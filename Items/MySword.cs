using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MyFirstMod.Projectiles;
namespace MyFirstMod.Items;

// MySword：第一把自定义武器
// 命名空间决定贴图路径：MyFirstMod.Items.MySword → Items/MySword.png
public class MySword : ModItem
{
    // SetDefaults：填模板数据，注册阶段只跑一次；之后每次 new Item(id) 都是克隆这份模板
    public override void SetDefaults()
    {
        Item.width = 48;                        // width：碰撞箱宽（像素），等于贴图宽
        Item.height = 48;                       // height：碰撞箱高（像素）
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
        Item.shoot      = ModContent.ProjectileType<MySwordBeam>();  // 声明：挥动时发射这个弹幕
        Item.shootSpeed = 6f;   // 弹幕初速度，单位 像素/tick

    }
}
