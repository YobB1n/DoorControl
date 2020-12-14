using System;
using LSPD_First_Response.Mod.API;
using Rage;
using RAGENativeUI;
using RAGENativeUI.Elements;

namespace DoorControl
{
    class Menu
    {
        public static UIMenu mainMenu;
        public static MenuPool _menuPool;

        public static bool EnableDoorControl;
        public static bool PlayerHasLastVehicle = false;
        public static bool MenuInitialized = false;

        public static uint SpeedZone;
        public static Blip SpeedZoneBlip;
        public static Vector3 SpeedZonePosition;

        public static void createMenu()
        {
            MenuInitialized = true;
            Player player = Game.LocalPlayer;

            _menuPool = new MenuPool();
            mainMenu = new UIMenu("Door Control", "~y~v1.1.0.0~w~ by ~b~YobB1n~w~");
            _menuPool.Add(mainMenu);
            mainMenu.MouseControlsEnabled = false;
            mainMenu.AllowCameraMovement = true;
            var AlwaysEnableDoorControl = new UIMenuCheckboxItem("~b~Enable Door Control", Config.LoadOnStart, "The Door Will ~b~Remain Open~w~ After Exiting a Police Vehicle.");
            mainMenu.AddItem(AlwaysEnableDoorControl);

            var EnableDoorcontrolPursuit = new UIMenuCheckboxItem("~b~Door Control On Pursuit/Pullover Only", Config.PursuitOnlyOnStart, "The Door Will Remain Open After Exiting a Police Vehicle ~b~Only During a Pursuit/Traffic Stop. ~w~Door Control Must Be Ebabled.");
            mainMenu.AddItem(EnableDoorcontrolPursuit);

            var TrafficControl = new UIMenuCheckboxItem("~b~Stop Traffic During Pursuits", Config.TrafficControlOnStart, "After Exiting a Police Vehicle During a Pursuit, ~b~Traffic Will Be Automatically Stopped ~w~Within " + Config.TrafficControlRadius + " Meters of Your Current Position. Door Control Must Be Enabled.");
            mainMenu.AddItem(TrafficControl);

            var AddTrafficZone = new UIMenuItem("~r~Stop Traffic", "Stops All Traffic Within " + Config.TrafficControlRadius + " Metres. ~r~This Includes Backup/Responding Units.");
            mainMenu.AddItem(AddTrafficZone);

            var RemoveTrafficZone = new UIMenuItem("~g~Resume Traffic", "Allows Traffic to ~g~Move Once More.");
            mainMenu.AddItem(RemoveTrafficZone);

            AddTrafficZone.Activated += (menu, item) =>
            {
                if (!SpeedZoneBlip.Exists() && !player.Character.IsInAnyVehicle(false))
                {
                    World.RemoveSpeedZone(SpeedZone);
                    Game.LogTrivial("DOORCONTROL: STOPPED TRAFFIC");
                    Game.DisplayNotification("Traffic Has Been ~y~Stopped.");
                    SpeedZone = World.AddSpeedZone(player.Character.Position, Config.TrafficControlRadius, 0);
                    SpeedZoneBlip = new Blip(player.Character.Position, Config.TrafficControlRadius);
                    SpeedZoneBlip.Color = System.Drawing.Color.Yellow;
                    SpeedZoneBlip.Alpha = 0.5f;
                    SpeedZonePosition = player.Character.Position;
                }
                else if (SpeedZoneBlip.Exists())
                {
                    Game.DisplayNotification("Please ~b~Remove~w~ the Previous ~y~Traffic Zone~w~ First.");
                }
                else if (player.Character.IsInAnyVehicle(false))
                {
                    Game.DisplayNotification("Cannot Stop Traffic While ~b~Currently in a Vehicle.");
                }
                GameFiber.StartNew(delegate
                {
                    while (player.Character.DistanceTo(SpeedZonePosition) <= 75 && SpeedZoneBlip.Exists()) { GameFiber.Wait(0); }
                    if (SpeedZoneBlip.Exists())
                    {
                        World.RemoveSpeedZone(SpeedZone);
                        if (SpeedZoneBlip.Exists()) { SpeedZoneBlip.Delete(); }
                        Game.LogTrivial("DOORCONTROL: RESUMED TRAFFIC");
                        Game.DisplayNotification("Traffic Has ~y~Resumed.");
                    }
                    GameFiber.Yield();
                }
                );
            };
            RemoveTrafficZone.Activated += (menu, item) =>
            {
                if (SpeedZoneBlip.Exists())
                {
                    World.RemoveSpeedZone(SpeedZone);
                    if (SpeedZoneBlip.Exists()) { SpeedZoneBlip.Delete(); }
                    Game.LogTrivial("DOORCONTROL: RESUMED TRAFFIC");
                    Game.DisplayNotification("Traffic Has ~y~Resumed.");
                }
            };

            if (Config.VehicleOptions)
            {
                var OpenDoor = new UIMenuItem("~y~Open Police Vehicle Door", "~y~Opens the Door ~w~of the Current/Last Police Vehicle. Vehicle Must be Stopped.");
                mainMenu.AddItem(OpenDoor);
                var CloseDoor = new UIMenuItem("~y~Close Police Vehicle Door", "~y~Closes the Door ~w~of the Current/Last Police Vehicle.");
                mainMenu.AddItem(CloseDoor);
                var RepairVehicle = new UIMenuItem("~y~Repair Current Police Vehicle");
                mainMenu.AddItem(RepairVehicle);
                var CleanVehicle = new UIMenuItem("~y~Clean Current Police Vehicle");
                mainMenu.AddItem(CleanVehicle);
                var TeleportToLastVehicle = new UIMenuItem("~o~Teleport to Last Police Vehicle");
                mainMenu.AddItem(TeleportToLastVehicle);
                var SpawnPoliceVehicle = new UIMenuItem("~o~Spawn Favourite Police Vehicle", "You Can Change Which Vehicle to Spawn in the ~o~.ini Settings.");
                mainMenu.AddItem(SpawnPoliceVehicle);

                //VEHICLE OPTIONS BUTTON ACTIONS
                OpenDoor.Activated += (menu, item) =>
                {
                    if (player.Character.IsInAnyPoliceVehicle && player.Character.CurrentVehicle.Speed > 0)
                    {
                        Game.DisplayNotification("Vehicle must be ~y~Stopped~w~ to Open the Door.");
                        //Game.LogTrivial("DOORCONTROL: Player's Vehicle is Still Moving. Aborting Door Open.");
                    }
                    else if (player.Character.LastVehicle.Exists() && !player.Character.IsInAnyVehicle(false))
                    {
                        if (player.Character.LastVehicle.IsPoliceVehicle)
                        {
                            if (player.Character.LastVehicle.HasBone("door_dside_f") && player.Character.LastVehicle.Doors[0].IsValid())
                            {
                                player.Character.LastVehicle.Doors[0].Open(false);
                            }
                        }
                        else
                        {
                            Game.DisplayNotification("Last Vehicle Must be a ~b~Police Vehicle~w~ to Open the Door.");
                        }
                        //Game.LogTrivial("DOORCONTROL: Opened Player Vehicle Door.");
                    }
                    else if (player.Character.IsInAnyPoliceVehicle)
                    {
                        if (player.Character.CurrentVehicle.HasBone("door_dside_f") && player.Character.CurrentVehicle.Doors[0].IsValid())
                        {
                            player.Character.CurrentVehicle.Doors[0].Open(false);
                        }
                        //Game.LogTrivial("DOORCONTROL: Opened Player Vehicle Door.");
                    }
                    else
                    {
                        Game.DisplayNotification("Last Vehicle Must be a ~b~Police Vehicle~w~ to Open the Door.");
                    }
                };
                CloseDoor.Activated += (menu, item) =>
                {
                    if (player.Character.IsInAnyPoliceVehicle)
                    {
                        if (player.Character.CurrentVehicle.HasBone("door_dside_f") && player.Character.CurrentVehicle.Doors[0].IsValid())
                        {
                            if (player.Character.CurrentVehicle.Doors[0].IsOpen)
                            {
                                player.Character.CurrentVehicle.Doors[0].Close(false);
                            }
                            else
                            {
                                Game.DisplayNotification("Door Must be ~y~Open~w~ to Close the Door.");
                                //Game.LogTrivial("DOORCONTROL: Door is Not Open. Aborting Door Close.");
                            }
                        }
                        //Game.LogTrivial("DOORCONTROL: Closed Player Vehicle Door.");
                    }
                    else if (player.Character.LastVehicle.Exists() && !player.Character.IsInAnyVehicle(false))
                    {
                        if (player.Character.LastVehicle.IsPoliceVehicle)
                        {
                            if (player.Character.LastVehicle.Doors[0].IsOpen)
                            {
                                player.Character.LastVehicle.Doors[0].Close(false);
                            }
                            else
                            {
                                Game.DisplayNotification("Door Must be ~y~Open~w~ to Close.");
                                //Game.LogTrivial("DOORCONTROL: Door is Not Open. Aborting Door Close.");
                            }
                        }
                        else
                        {
                            Game.DisplayNotification("Last Vehicle Must be a ~b~Police Vehicle~w~ to Close the Door.");
                        }
                        //Game.LogTrivial("DOORCONTROL: Opened Player Vehicle Door.");
                    }
                    else
                    {
                        Game.DisplayNotification("Last Vehicle Must be a ~b~Police Vehicle~w~ to Close the Door.");
                        //Game.LogTrivial("DOORCONTROL: Last Vehicle is not a Police Vehicle. Aborting Door Close.");
                    }
                };
                RepairVehicle.Activated += (menu, item) =>
                {
                    if (!player.Character.IsInAnyPoliceVehicle)
                    {
                        Game.DisplayNotification("Must be in a ~b~Police Vehicle~w~ to Repair the Vehicle.");
                        //Game.LogTrivial("DOORCONTROL: Player is Not in a Police Vehicle. Aborting Repair.");
                    }
                    else
                    {
                        if (player.Character.CurrentVehicle.IsValid())
                        {
                            player.Character.CurrentVehicle.Repair();
                        }
                        //Game.LogTrivial("DOORCONTROL: Repaired Current Vehicle.");
                    }
                };
                CleanVehicle.Activated += (menu, item) =>
                {
                    if (!player.Character.IsInAnyPoliceVehicle)
                    {
                        Game.DisplayNotification("Must be in a ~b~Police Vehicle~w~ to Clean the Vehicle.");
                        //Game.LogTrivial("DOORCONTROL: Player is Not in a Police Vehicle. Aborting Clean.");
                    }
                    else
                    {
                        if (player.Character.CurrentVehicle.IsValid())
                        {
                            player.Character.CurrentVehicle.DirtLevel = 0;
                        }
                        //Game.LogTrivial("DOORCONTROL: Cleaned Current Vehicle.");
                    }
                };
                TeleportToLastVehicle.Activated += (menu, item) =>
                {
                    if (PlayerHasLastVehicle == true)
                    {
                        if (player.Character.IsInAnyVehicle(false))
                        {
                            Game.DisplayNotification("Cannot Teleport while ~b~Inside~w~ a Vehicle.");
                            //Game.LogTrivial("DOORCONTROL: Player is in a Police Vehicle. Aborting Teleport.");
                        }
                        else if (player.Character.LastVehicle.Exists() && !player.Character.IsInAnyVehicle(false))
                        {
                            if (player.Character.LastVehicle.HasDriver)
                            {
                                Game.DisplayNotification("Last ~b~Police Vehicle~w~ is Occupied.");
                            }
                            else if (!player.Character.LastVehicle.IsPoliceVehicle)
                            {
                                Game.DisplayNotification("Last Vehicle is Not a ~b~Police Vehicle.");
                            }
                            else
                            {
                                try
                                {
                                    player.Character.WarpIntoVehicle(player.Character.LastVehicle, -1);
                                    //Game.LogTrivial("DOORCONTROL: Warped Player into Last Vehicle Driver Seat.");
                                }
                                catch (System.NullReferenceException)
                                {
                                    Game.DisplayNotification("Last ~b~Police Vehicle~w~ is Not Valid.");
                                    //Game.LogTrivial("DOORCONTROL: Last Police Vehicle is Not Valid. Aborting Teleport");
                                    player.Character.ClearLastVehicle();
                                }
                            }
                        }
                        else
                        {
                            Game.DisplayNotification("Last ~b~Police Vehicle~w~ is Not Valid.");
                            //Game.LogTrivial("DOORCONTROL: Last Police Vehicle is Not Valid. Aborting Teleport.");
                        }
                    }
                    else
                    {
                        Game.DisplayNotification("Please ~g~Enter~w~ a ~b~Police Vehicle~w~ While ~y~Door Control~w~ is Loaded First.");
                    }
                };
                SpawnPoliceVehicle.Activated += (menu, item) =>
                {
                    if (player.Character.IsInAnyPoliceVehicle)
                    {
                        Game.DisplayNotification("Cannot Spawn Vehicle while Inside a ~b~Police Vehicle.");
                        //Game.LogTrivial("DOORCONTROL: Player is in a Police Vehicle. Aborting Vehicle Spawn.");
                    }
                    else
                    {
                        new Vehicle(Config.FavouritePoliceVehicle, player.Character.GetOffsetPositionFront(5));
                        //Game.LogTrivial("DOORCONTROL: Spawned New Police Vehicle.");
                    }
                };
            }
            GameFiber.StartNew(delegate
            {
                while (true)
                {
                    while (AlwaysEnableDoorControl.Checked)
                    {
                        EnableDoorControl = true;
                        if (player.Character.IsInAnyPoliceVehicle)
                        {
                            PlayerHasLastVehicle = true;
                            if (player.Character.CurrentVehicle.HasBone("door_dside_f") && player.Character.CurrentVehicle.Doors[0].IsValid())
                            {
                                try
                                {
                                    //Game.LogTrivial("DOORCONTROL: Player's Vehicle Door Valid, Started Fiber.");
                                    if (!EnableDoorcontrolPursuit.Checked)
                                    {
                                        while (!Game.IsKeyDown(Config.VehicleExitKey) && !Game.IsControllerButtonDown(Config.ControllerVehicleExitKey))
                                        {
                                            GameFiber.Wait(0);
                                        }
                                        player.Character.Tasks.LeaveVehicle(player.Character.CurrentVehicle, LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion();
                                        //Game.LogTrivial("DOORCONTROL: Opened Door");
                                        if (TrafficControl.Checked)
                                        {
                                            if (Functions.GetActivePursuit() != null && !SpeedZoneBlip.Exists())
                                            {
                                                Game.LogTrivial("DOORCONTROL: AUTOMATICALLY STOPPED TRAFFIC, PLAYER IN PURSUIT");
                                                Game.DisplayNotification("Traffic Has Been Automatically ~y~Stopped.");
                                                SpeedZone = World.AddSpeedZone(player.Character.Position, Config.TrafficControlRadius, 0);
                                                SpeedZoneBlip = new Blip(player.Character.Position, Config.TrafficControlRadius);
                                                SpeedZoneBlip.Color = System.Drawing.Color.Yellow;
                                                SpeedZoneBlip.Alpha = 0.5f;
                                                if (Config.EndTrafficControl)
                                                {
                                                    while (!player.Character.IsInAnyPoliceVehicle) { GameFiber.Wait(0); }
                                                    if (SpeedZoneBlip.Exists())
                                                    {
                                                        World.RemoveSpeedZone(SpeedZone);
                                                        SpeedZoneBlip.Delete();
                                                        Game.LogTrivial("DOORCONTROL: AUTOMATICALLY RESUMED TRAFFIC, PLAYER ENTERED VEHICLE");
                                                        Game.DisplayNotification("Traffic Has Been Automatically ~y~Resumed.");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        while (!Game.IsKeyDown(Config.VehicleExitKey) && !Game.IsControllerButtonDown(Config.ControllerVehicleExitKey))
                                        {
                                            GameFiber.Wait(0);
                                        }
                                        if (player.Character.IsInCombat || Functions.IsPlayerPerformingPullover() || Functions.GetActivePursuit() != null)
                                        {
                                            player.Character.Tasks.LeaveVehicle(player.Character.CurrentVehicle, LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion();
                                        }
                                        if (TrafficControl.Checked)
                                        {
                                            if (Functions.GetActivePursuit() != null && !SpeedZoneBlip.Exists())
                                            {
                                                Game.LogTrivial("DOORCONTROL: AUTOMATICALLY STOPPED TRAFFIC, PLAYER IN PURSUIT");
                                                Game.DisplayNotification("Traffic Has Been Automatically ~y~Stopped.");
                                                SpeedZone = World.AddSpeedZone(player.Character.Position, Config.TrafficControlRadius, 0);
                                                SpeedZoneBlip = new Blip(player.Character.Position, Config.TrafficControlRadius);
                                                SpeedZoneBlip.Color = System.Drawing.Color.Yellow;
                                                SpeedZoneBlip.Alpha = 0.5f;
                                                if (Config.EndTrafficControl)
                                                {
                                                    while (!player.Character.IsInAnyPoliceVehicle) { GameFiber.Wait(0); }
                                                    if (SpeedZoneBlip.Exists())
                                                    {
                                                        World.RemoveSpeedZone(SpeedZone);
                                                        SpeedZoneBlip.Delete();
                                                        Game.LogTrivial("DOORCONTROL: AUTOMATICALLY RESUMED TRAFFIC, PLAYER ENTERED VEHICLE");
                                                        Game.DisplayNotification("Traffic Has Been Automatically ~y~Resumed.");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                    //Game.DisplayNotification("~y~Door Control~w~ has Crashed. Please Send Me Your ~b~Log File.");
                                    Game.LogTrivial("DOORCONTROL: Detected Crash. Please Send me Your Log File.");
                                }
                            }
                        }
                        GameFiber.Yield();
                    }
                    GameFiber.Yield();
                }
            }
            );
            GameFiber.StartNew(delegate
            {
                while (true)
                {
                    if (!AlwaysEnableDoorControl.Checked)
                    {
                        EnableDoorcontrolPursuit.Checked = false;
                        EnableDoorControl = false;
                        TrafficControl.Checked = false;
                    }
                    GameFiber.Yield();
                }
            }
            );
        }
    }
}
