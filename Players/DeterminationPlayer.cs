using System;                   // Math 等
using Microsoft.Xna.Framework;  // Color
using Terraria;                 // Main、Player
using Terraria.DataStructures;  // PlayerDeathReason
using Terraria.ID;              // BuffID
using Terraria.ModLoader;       // ModPlayer、ModContent
using OurMod.Content.Buffs;     // Determination
namespace OurMod.Players;

public class DeterminationPlayer: ModPlayer

{
    public bool Proved;
    public int determination_timer=600;
    private static readonly int[] VanillaDebuffs =
    {
        BuffID.Poisoned,      BuffID.PotionSickness, BuffID.Darkness,
        BuffID.Cursed,        BuffID.OnFire,         BuffID.Bleeding,
        BuffID.Confused,      BuffID.Slow,           BuffID.Weak,
        BuffID.Silenced,      BuffID.BrokenArmor,    BuffID.Horrified,
        BuffID.TheTongue,     BuffID.CursedInferno,  BuffID.Frostburn,
        BuffID.Chilled,       BuffID.Frozen,         BuffID.Burning,
        BuffID.Suffocation,   BuffID.Ichor,          BuffID.Venom,
        BuffID.Blackout,      BuffID.ChaosState,     BuffID.ManaSickness,
        BuffID.Stinky,        BuffID.Slimed,         BuffID.Electrified,
        BuffID.MoonLeech,     BuffID.Rabies,         BuffID.Webbed,
        BuffID.ShadowFlame,   BuffID.Stoned,         BuffID.Dazed,
        BuffID.Obstructed,    BuffID.VortexDebuff,   BuffID.Daybreak,
        BuffID.WindPushed,    BuffID.WitheredArmor,  BuffID.WitheredWeapon,
        BuffID.OgreSpit,      BuffID.NoBuilding,     BuffID.Oiled,
        BuffID.OnFire3,       BuffID.Frostburn2,
        BuffID.NeutralHunger, BuffID.Hunger,         BuffID.Starving,
        BuffID.BloodButcherer,
    };
    public override void OnRespawn()

    {
        base.OnRespawn();

            Proved = false;

    }


    public override bool PreKill(double damage, int hitDirection, bool pvp,
        ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
    {
        if (Player.HasBuff(ModContent.BuffType<Determination>()) && !Proved)
        {
            playSound = false;
            genGore = false;
            Proved = true;
            foreach (int t in VanillaDebuffs)
                Player.ClearBuff(t);
            Player.statLife = 1;
            if (Player.Male) Main.NewText($"{Player.name}的决心使他坚持了下来!", Color.LawnGreen);
            else Main.NewText($"{Player.name}的决心使她坚持了下来!", Color.LawnGreen);

                return false;
        }
        return true;
    }

}
