using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using CitizenFX.Core;

namespace fivem_taser_server
{
    public class Class1 : BaseScript
    {
        dynamic ESX;
        public Class1()
        {
            TriggerEvent("esx:getSharedObject", new object[] { new Action<dynamic>(esx => {
            ESX = esx;
            })});

            EventHandlers["better_taser:cartridge:server"] += new Action<Player, int, string>(Cartridge);
        }
        //DB_helper DB = new DB_helper();

        private async void Cartridge([FromSource] Player player, int playerid, string action)
        {
            if (player != (Player)Players[playerid]) player.Drop("Are you cheating ?");
            var xPlayer = ESX.GetPlayerFromId(playerid);
            var item = xPlayer.getInventoryItem("cartridge");
            if (action == "check")
            {
                if (item.count > 0) player.TriggerEvent("better_taser:cartridge", "load");
            }
            else if (action == "remove")
            {
                xPlayer.removeInventoryItem("cartridge", 1);
            }
        }
    }
}
