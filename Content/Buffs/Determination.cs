using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Localization;
using Terraria.DataStructures;
using OurMod.Dusts;
using OurMod.Players;
namespace OurMod.Content.Buffs;

public class Determination:ModBuff
{



    public override void SetStaticDefaults()
    {


        Main.buffNoTimeDisplay[Type] = true;

        // buffNoSave：true = 退出世界**不**保留，false = 保存。
        // 这里写 false，效果是"退出世界后这个 buff 还在"。
        Main.buffNoSave[Type] = false;

        // persistentBuff：true = 死亡后**保留**，false = 死亡时清掉。
        // 这里要"这次决心用完就没了"，所以 false。
        Main.persistentBuff[Type] = false;
    }

    /// <summary>
    /// 右键点 buff 图标时调用。返回 false = 玩家没法主动取消它。
    /// 文档原文：Return false to prevent the buff from being cancelled.
    /// </summary>
    public override bool RightClick(int buffIndex)
    {
        return false;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        var mp = player.GetModPlayer<DeterminationPlayer>();
        if (mp.Proved)
        {
            if (mp.determination_timer > 0)
            {
                mp.determination_timer--;
                if (mp.determination_timer % 12 == 0)
                {

                    DeterminationDust.SpawnOne(player.Center);
                }

            }
            else
            {
                mp.determination_timer = 600;
                if (player.Male)
                {
                    player.KillMe(PlayerDeathReason.ByCustomReason(NetworkText.FromLiteral($"{player.name} 证明了他的决心")),
                        9999, 0);
                }
                else
                {
                    player.KillMe(PlayerDeathReason.ByCustomReason(NetworkText.FromLiteral($"{player.name} 证明了她的决心")),
                        9999, 0);
                }
            }
        }
    }

    public override void Update(NPC npc, ref int buffIndex)
    {

    }



}
