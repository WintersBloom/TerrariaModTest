using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using OurMod.Projectiles;
namespace OurMod.Projectiles
{
    public class RadianceLightBall:ModProjectile
    {
        //基础设置
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;           // 只穿透一次
            Projectile.timeLeft = 300;          // 最大存在5秒
            Projectile.tileCollide = true;     // 可穿墙
            Projectile.aiStyle = 0;             // 关闭原版 AI
            Projectile.usesLocalNPCImmunity = true; // 开启局部无敌帧
            Projectile.localNPCHitCooldown = -1;    // 允许对同一个敌怪每帧都能造成伤害（由我们手动控制独立无敌帧）
        }
        public override void AI()
        {

            //运动逻辑
            Projectile.ai[0]++;
            Lighting.AddLight(Projectile.Center,0.85f,0.8f,0.7f);       //自带光（三个数字分别代表红、绿、蓝的亮度，数值范围0~1，现在是略带金光的白光）
            if(Projectile.ai[0]<=20){
                //前0.5s无操作
            }
            /*
            简述下列代码逻辑：
            1.有目标情况：
                有目标时，先判断目标是否还有效（状态与范围），若有效则保持目标直接进入步骤3，无效则切换为无目标状态进入步骤2。
            2.无目标情况：
                提取所有NPC，依次检查NPC状态与距离，符合的可攻击敌对目标且在距离内的就记录为待追踪敌怪并记录其距离，若有别的符合敌怪距离更短则更替，依此定位最符合的敌怪。
            3.开始追踪敌怪：
                先创建方向单位向量,然后原速度与加速度向量相加；检查是否达到最大速度，如果达到则把当前速度转化为单位向量，再乘上最大速度值。
            储存目标方法：
                ai数组不会每帧还原，Main.npc数组储存的NPC在数组中的位置不会变（即前面的怪死了后面的位置也不会前移），于是用ai[1]储存位置序号，每帧更新后再把目标NPC的指针赋值给target
            */
            else{
                float MaxDistance=960;     //最大追踪半径960像素（60格）（计算用的是欧几里得距离）
                if(Projectile.ai[1]==0f){
                    Projectile.ai[1]=-1f;       //ai[1]初始值为0，转为-1定为无目标状态
                }
                NPC target=null;                //定义初始无目标的指针
                //1.有目标情况
                if(Projectile.ai[1]!=-1f){
                    NPC tempTarget=Main.npc[(int)Projectile.ai[1]];      //赋值临时目标，判断有效后再赋值给target
                    //判断存活、非友好生物、非无敌状态、在距离内
                    if(tempTarget.active && !tempTarget.friendly && !tempTarget.dontTakeDamage && Vector2.Distance(Projectile.Center,tempTarget.Center)<=MaxDistance){
                        target=tempTarget;
                    }else{
                        Projectile.ai[1]=-1f;
                    }
                }
                //2.无目标情况
                if(Projectile.ai[1]==-1f){
                    float tempDistance=MaxDistance;
                    for(int i=0;i<Main.maxNPCs;i++){
                        NPC tempNPC=Main.npc[i];
                        if(tempNPC.active && !tempNPC.friendly && !tempNPC.dontTakeDamage && Vector2.Distance(Projectile.Center,tempNPC.Center)<=tempDistance){
                            target=tempNPC;
                            Projectile.ai[1]=i;
                            tempDistance=Vector2.Distance(Projectile.Center,tempNPC.Center);
                        }
                    }
                }
                //3.开始追踪
                if(Projectile.ai[1]==-1f){
                    //没有目标，什么都不做保持原样
                }else{
                    Vector2 direction=target.Center - Projectile.Center;    //创建方向向量
                    direction.Normalize();     //化为单位向量
                    Projectile.velocity+=direction*4f;      //每帧矢量加速：4像素/帧
                    if(Projectile.velocity.Length()>=40f){
                        Projectile.velocity=Vector2.Normalize(Projectile.velocity)*40f;    //限制最大速度为40像素/帧
                    }
                }
            }
            Projectile.rotation+=0.2f;       //始终逆时针自转，约0.5s/圈
        }

        //命中后逻辑
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            //产生爆炸：
            Vector2 explosionCenter=Projectile.Center;
            float explosionRadius=96f;     //半径96像素，即6格
            for(int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc=Main.npc[i];
                if(npc.active && !npc.friendly && !npc.dontTakeDamage && Vector2.Distance(explosionCenter, npc.Center) <= explosionRadius)
                {
                    //攻击方向：怪在爆心右边就向左推，在左边就向右推（确保不会被反向击退）
                    //HitDirection 指攻击的朝向，也就是怪在攻击的哪一边
                    int hitDirection=explosionCenter.X>npc.Center.X?-1:1;

                    //规范写法：用原版的计算入口生成 HitInfo，而不是手动 new 再填字段。
                    //CalculateHitInfo 内部会走 GetIncomingStrikeModifiers，自动完成：
                    //  1. 减去目标防御；2. 乘上目标的击退抗性 knockBackResist；
                    //  3. 对 knockBackResist==0 的目标（如训练假人 NPCID.TargetDummy）自动禁用击退；
                    //  4. 触发其他 mod 的 ModifyIncomingHit 钩子。
                    //手动 new HitInfo 会跳过以上全部计算，所以才会出现"击退假人"这种反常效果。
                    NPC.HitInfo hitInfo=npc.CalculateHitInfo(
                        Projectile.damage,      //基础伤害，内部会减去目标防御
                        hitDirection,           //击退方向
                        crit:false,             //不暴击
                        knockBack:6f,           //期望击退强度，目标抗性为 0 时会被自动清零
                        damageType:DamageClass.Magic    //伤害类型，参与减伤/增伤的判定
                    );
                    npc.StrikeNPC(hitInfo);
                }
            }
            //产生四散的白色粒子效果：
            for(int i = 0; i < 30; i++)
            {
                float rad=Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 direction=rad.ToRotationVector2();//生成0~2pi的随机弧度再转为方向向量
                float speed=Main.rand.NextFloat(1f,6f);
                Vector2 velocity=direction*speed;       //生成1~6随机速度并附上方向向量
                int DustIndex = Dust.NewDust(
                    Projectile.Center,      //中心
                    15,15,                  //生成范围的宽高
                    DustID.SilverCoin,      //白色粒子
                    velocity.X,velocity.Y,  //方向速度
                    100,                    //透明度
                    default,                //颜色覆盖(例：Color.white，因为这里本来就是白色就写defualt不改)
                    0.8f                    //粒子大小
                );
                Dust dust=Main.dust[DustIndex];
                dust.noGravity=true;        //无重力
                dust.noLight=true;          //不影响环境光照
                dust.fadeIn=1.2f;           //粒子膨胀最大值，达到后渐隐消失
            }
        }
    }
}
