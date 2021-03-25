using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
//using Newtonsoft.Json.Linq;
//using Newtonsoft.Json;

using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;


namespace fivem_taser
{
    public class Class1 : BaseScript
    {
        dynamic ESX;
        public Class1()
        {
            TriggerEvent("esx:getSharedObject", new object[] { new Action<dynamic>(esx => {
            ESX = esx;
            })});

            EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
            EventHandlers["better_taser:nui"] += new Action<string>(ShowNUI);
            EventHandlers["better_taser:cartridge"] += new Action<string>(Cartridge);
            EventHandlers["better_taser:batteries"] += new Action<string>(Batteries);
            EventHandlers["better_taser:destroy"] += new Action<string>(Destroy);
        }

        private async void OnClientResourceStart(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;

            //Used for debug
            /*API.RegisterCommand("nui_show", new Action<int, List<object>, string>((source, args, raw) =>
            {
                TriggerEvent("better_taser:nui");
            }), false);*/

            // Also used for debug
            /*API.RegisterCommand("spawntaserped", new Action<int, List<object>, string>(async(source, args, raw) =>
            {
                Vector3 pos = LocalPlayer.Character.Position;
                World.CreatePed(new Model(0x5E3DA4A4), pos);
                Ped[] peds = World.GetAllPeds();
                Ped ped = null;
                foreach (var item in peds)
                {
                    if (LocalPlayer.Character.Position.DistanceToSquared(item.Position) < 5f) ped = item;
                }
                await Delay(1000);
                ped.Weapons.Give(WeaponHash.StunGun, -1, true, true);
                ped.Task.ShootAt(LocalPlayer.Character, 1900, FiringPattern.Default);
                await Delay(15000);
                ped.Delete();
            }), false);*/

            Tick += Tased; Tick += Blockrunning; Tick += Ontheground;
            Tick += Shooting;
        }

        bool tased = false; bool blockrunning = false; bool ontheground = false; bool onetime = false; int bwcounter = 4; DateTime resetbwcounter;
        private async Task Tased()
        {
            if (!onetime) { await Delay(15000); Function.Call(Hash.SET_PED_MIN_GROUND_TIME_FOR_STUNGUN, LocalPlayer.Character, 5000); }
            onetime = true;
            if (LocalPlayer.Character.IsBeingStunned)
            {
                if (tased) return;
                tased = true;
                if (resetbwcounter < DateTime.Now) bwcounter = 4;
                resetbwcounter = DateTime.Now.AddMinutes(5);
                int timerng = 15 /*new Random().Next(15, 30)*/;
                int lucky = new Random().Next(1, 100);
                if (lucky < 6)
                {
                    LocalPlayer.Character.Task.ClearAllImmediately();
                }
                else
                {
                    await Delay(8100);
                }
                if (lucky > 6)
                {
                    bwcounter--;
                    if (bwcounter == 1)
                    {
                        ontheground = true;
                        Tick += Recovering;
                        while (true)
                        {
                            recovering++;
                            await Delay(1000);
                            if (stoprecovering) { stoprecovering = false; break; }
                        }
                        ontheground = false;
                        LocalPlayer.Character.CancelRagdoll();
                        LocalPlayer.Character.Task.ClearAllImmediately();
                    }
                    else if (bwcounter == 0)
                    {
                        LocalPlayer.Character.ApplyDamage(2000);
                        Screen.ShowNotification("Zbyt wiele razy dostałeś ~y~taserem ~s~i nie wytrzymałeś tego...");
                        bwcounter = 4;
                    }
                    else
                    {
                        int rng = new Random().Next(1, 100);
                        if (rng < 40)
                        {
                            Screen.ShowNotification("~r~Zostałeś sparaliżowany, ciężko ci wstać...");
                            ontheground = true;
                            LocalPlayer.Character.Ragdoll(timerng * 1000, RagdollType.Normal);
                            await Delay(timerng * 1000);
                            ontheground = false;
                            LocalPlayer.Character.Ragdoll(100, RagdollType.Normal);
                        }
                        else if (rng > 40 && rng <= 95)
                        {
                            Screen.ShowNotification("~r~Zostałeś sparaliżowany, nie masz siły biegać...");
                            blockrunning = true;
                            Function.Call(Hash.SET_PED_MOVEMENT_CLIPSET, LocalPlayer.Character.Handle, "move_injured_generic", 1f);
                            await Delay(timerng * 1000);
                            Function.Call(Hash.RESET_PED_MOVEMENT_CLIPSET, LocalPlayer.Character.Handle, 5f);
                            blockrunning = false;
                            API.DisableControlAction(2, 21, false);
                            API.DisableControlAction(2, 22, false);
                        }
                    }
                }
                tased = false;
            }
        }
        private async Task Blockrunning()
        {
            if (blockrunning)
            {
                API.DisableControlAction(2, 21, true);
                API.DisableControlAction(2, 22, true);
                API.DisableControlAction(2, 24, true);
                API.DisableControlAction(2, 25, true);
                API.DisableControlAction(2, 140, true);
            }
        }
        private async Task Ontheground()
        {
            if (ontheground)
            {
                LocalPlayer.Character.Ragdoll(-1, RagdollType.Normal);
            }
        }
        bool loaded = false;
        int battery_charges = 4;
        private async Task Shooting()
        {
            if (LocalPlayer.Character.Weapons.Current.Hash == WeaponHash.StunGun)
            {
                if (!loaded || battery_charges < 1) 
                { 
                    API.DisableControlAction(2, 24, true);
                    API.DisableControlAction(2, 257, true);
                    API.DisableControlAction(2, 69, true);
                    API.DisableControlAction(2, 92, true);
                    API.DisableControlAction(2, 142, true);

                    if(API.IsControlJustPressed(2, 25))
                    {
                        Screen.ShowNotification("~y~Taser~s~ nie posiada kartridżu i jest ~r~~h~niezaładowany/rozładowany(baterie) ~s~!");
                    }
                }
                if (LocalPlayer.Character.IsShooting) 
                {
                    battery_charges--;
                    loaded = false;
                    TriggerServerEvent("better_taser:cartridge:server", LocalPlayer.ServerId, "remove");
                    if(battery_charges == 0) 
                    { 
                        Screen.ShowNotification("~b~Wystrzeliłeś ~y~kartridż~b~, baterie się ~r~rozładowały~b~ !");
                        TriggerEvent("better_taser:nui:lua:cartridge", "cartridgelow");
                        TriggerEvent("better_taser:nui:lua:batteries", "batterieslow");
                    }
                    else if(battery_charges > 0) 
                    { 
                        Screen.ShowNotification("~r~Wystrzeliłeś kartridż !");
                        TriggerEvent("better_taser:nui:lua:cartridge", "cartridgelow");
                    }
                }
            }
        }
        private async Task SoundAlert()
        {
            for (int i = 0; i < 2; i++)
            {
                await Delay(1000);
                Function.Call(Hash.PLAY_SOUND, -1, "CHARACTER_CHANGE_CHARACTER_01_MASTER", 0, 0, 0, 0);
            }
        }

        private async void ShowNUI(string display)
        {
            if (LocalPlayer.Character.Weapons.Current.Hash != WeaponHash.StunGun)
            {
                Screen.ShowNotification("~r~Nie trzymasz ~y~tasera ~r~!");
                return;
            }
            TriggerEvent("better_taser:nui:lua");
            /*API.SendNuiMessage(JsonConvert.SerializeObject(new
            {
                type = "ui",
                display = display
            }));*/
        }
        private async void Cartridge(string action)
        {

            if (action == "check") {
                if (!loaded) { TriggerServerEvent("better_taser:cartridge:server", LocalPlayer.ServerId, "check"); }
                else { Screen.ShowNotification("~y~Taser~s~ posiada załadowany kartridż ~s~!"); }
            }
            if (action == "load")
            {
                //LocalPlayer.Character.Task.PlayAnimation("cover@weapon@reloads@pistol@pistol", "reload_low_left");
                Function.Call(Hash.TASK_PLAY_ANIM, LocalPlayer.Character.Handle, "weapons@first_person@aim_rng@generic@pistol@flare_gun@str", "reload_aim", 8f, 8f, 1000, 49, 0, 0, 0, 0);
                await Delay(1000);
                Function.Call(Hash.TASK_PLAY_ANIM, LocalPlayer.Character.Handle, "weapons@first_person@aim_rng@generic@pistol@ap_pistol@str", "reload_aim", 8f, 8f, 3000, 49, 0, 0, 0, 0);
                await Delay(3200);
                loaded = true;
                Screen.ShowNotification("Załadowałeś ~y~taser ~s~kartridżem~s~!");
                TriggerEvent("better_taser:nui:lua:cartridge", "cartridgeloaded");
            }
        }
        bool stoploading = false;
        private async void Batteries(string action)
        {
            //LocalPlayer.Character.Task.PlayAnimation("weapons@first_person@aim_rng@generic@pistol@flare_gun@str", "reload_aim");
            Function.Call(Hash.TASK_PLAY_ANIM, LocalPlayer.Character.Handle, "weapons@first_person@aim_rng@generic@pistol@flare_gun@str", "reload_aim", 8f, 8f,3000, 49, 0, 0, 0, 0);
            Screen.ShowNotification("Ładujesz ~g~baterie !");
            Tick += LoadingTaser;
            while (true)
            {
                percent++;
                await Delay(1000);
                if (battery_charges == 4) break;
                if (stoploading) { stoploading = false; break; }
            }
        }

        Vector2 World3DToScreen2d(Vector3 pos)
        {
            var x2dp = new OutputArgument();
            var y2dp = new OutputArgument();
            Function.Call<bool>(Hash.GET_SCREEN_COORD_FROM_WORLD_COORD, pos.X, pos.Y, pos.Z, x2dp, y2dp);
            return new Vector2(x2dp.GetResult<float>(), y2dp.GetResult<float>());
        }
        int percent = 0;
        async Task LoadingTaser()
        {
            if (battery_charges < 1 || true)
            {
                Vector3 pos_ = LocalPlayer.Character.Position;
                var pos = World3DToScreen2d(pos_);
                Function.Call(Hash.SET_TEXT_SCALE, 0.0, 0.30f);
                Function.Call(Hash.SET_TEXT_FONT, 0);
                Function.Call(Hash.SET_TEXT_PROPORTIONAL, 1);
                Function.Call(Hash.SET_TEXT_COLOUR, 255, 255, 255, 255);
                Function.Call(Hash.SET_TEXT_DROPSHADOW, 0, 0, 0, 0, 255);
                Function.Call(Hash.SET_TEXT_EDGE, 2, 0, 0, 0, 150);
                Function.Call(Hash.SET_TEXT_DROPSHADOW);
                Function.Call(Hash.SET_TEXT_OUTLINE);
                Function.Call(Hash._SET_TEXT_ENTRY, "STRING");
                Function.Call(Hash.SET_TEXT_CENTRE, 1);
                double text = percent / 1.8;
                text = Math.Floor(text);
                Function.Call(Hash._ADD_TEXT_COMPONENT_STRING, $"~w~Ładowanie ~y~tasera~w~: ~p~{text}%");
                Function.Call(Hash._DRAW_TEXT, pos.X, pos.Y);

                if(LocalPlayer.Character.Health < 100)
                {
                    stoploading = true;
                    percent = 0;
                    Tick -= LoadingTaser;
                }
                if (percent > 179)
                {
                    Screen.ShowNotification("~y~Taser ~s~jest ~g~~h~naładowany ~s~!");
                    battery_charges = 4;
                    percent = 0;
                    Tick -= LoadingTaser;
                    TriggerEvent("better_taser:nui:lua:batteries", "batteriesloaded");
                }
            }
        }
        private async void Destroy(string action)
        {

        }

        int recovering = 0; bool stoprecovering = false;
        private async Task Recovering()
        {
            Vector3 pos_ = LocalPlayer.Character.Position;
            var pos = World3DToScreen2d(pos_);
            Function.Call(Hash.SET_TEXT_SCALE, 0.0, 0.30f);
            Function.Call(Hash.SET_TEXT_FONT, 0);
            Function.Call(Hash.SET_TEXT_PROPORTIONAL, 1);
            Function.Call(Hash.SET_TEXT_COLOUR, 255, 255, 255, 255);
            Function.Call(Hash.SET_TEXT_DROPSHADOW, 0, 0, 0, 0, 255);
            Function.Call(Hash.SET_TEXT_EDGE, 2, 0, 0, 0, 150);
            Function.Call(Hash.SET_TEXT_DROPSHADOW);
            Function.Call(Hash.SET_TEXT_OUTLINE);
            Function.Call(Hash._SET_TEXT_ENTRY, "STRING");
            Function.Call(Hash.SET_TEXT_CENTRE, 1);
            Function.Call(Hash._ADD_TEXT_COMPONENT_STRING, $"~w~Powoli wstajesz z ziemi ~p~{recovering}%");
            Function.Call(Hash._DRAW_TEXT, pos.X, pos.Y);

            if(recovering == 100) { Tick -= Recovering; stoprecovering = true; recovering = 0; }
        }
    }
}
