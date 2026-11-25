using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using OurMod.Content.Projectiles;
namespace OurMod.Content.Items.Weapons
{
    public class RadianceStaff:ModItem
    {
        public override void SetDefaults()
        {
			Item.damage = 240;                          //基础伤害
			Item.DamageType = DamageClass.Magic;        //伤害类型
            Item.crit=40;   //额外暴击率（加上基础的4%，无配装状态下是44%暴击率）
			Item.mana = 12;                             //法力消耗
			Item.knockBack = 9;                         //击退力
			Item.width = 60;
			Item.height = 60;
			Item.useTime = 12;
			Item.useAnimation = 12;                     //动画时间（通常与使用时间相同）
			Item.useStyle = ItemUseStyleID.Shoot;       //使用方式为射弹类型
			Item.shoot = ModContent.ProjectileType<RadianceLightBall>();    //指定射弹
			Item.shootSpeed = 4;                        //初始速度设为4，之后加速写在弹幕的AI文件里
			Item.rare = ItemRarityID.Red;
			Item.value = Item.sellPrice(gold: 99);
            Item.UseSound = SoundID.Item20;
			Item.noMelee = true;                        //禁止近战伤害
			Item.autoReuse = true;                      //允许自动使用
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.LunarBar, 12);   //12个夜明锭
            recipe.AddTile(TileID.WorkBenches);             //工作台
            recipe.Register();
        }
    }
}