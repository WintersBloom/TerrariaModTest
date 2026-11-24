using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MyFirstMod.Systems
{
    /// <summary>
    /// 系统级钩子：每 tick 跑一次，用来证明 60Hz 主循环。
    /// </summary>
    public class MySystem : ModSystem
    {
        private int _ticksLogged;

        public override void PostUpdateEverything()
        {
            if (_ticksLogged >= 3)
                return;

            if (Main.GameUpdateCount % 60 == 0)
            {
                _ticksLogged++;
                Main.NewText($"[MyFirstMod] tick = {Main.GameUpdateCount}", Color.Cyan);
            }
        }
    }

    /// <summary>
    /// 玩家级钩子：玩家真正"进入世界"的那一刻触发。
    /// OnWorldLoad 太早了，那时聊天栏还没就绪，消息会被清掉，所以改用这个。
    /// </summary>
    public class MyPlayer : ModPlayer
    {
        public override void OnEnterWorld()
        {
            Main.NewText("[MyFirstMod] 玩家已进入世界，mod 生效！", Color.Yellow);
            Main.NewText($"[MyFirstMod] 进入世界时的 tick = {Main.GameUpdateCount}", Color.Yellow);
        }
    }
}
