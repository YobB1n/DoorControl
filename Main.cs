using System;
using System.Linq;  //for checking strings
using System.Net;
using System.Threading;
using LSPD_First_Response.Mod.API;
using Rage;

namespace DoorControl
{
    public class Main : Plugin
    {
        //UPDATE CHECKER
        private Version NewVersion = new Version();
        private Version curVersion = new Version("1.1.0.0");  //DON'T FORGET TO CHANGE THIS, MATTHEW!!!

        public static bool UpToDate;
        public override void Initialize()
        {
            Functions.OnOnDutyStateChanged += OnOnDutyStateChangedHandler;
            Game.LogTrivial("DOORCONTROL: DoorControl " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + " by YobB1n has been loaded.");   //Returns Callout Version
            try
            {
                Thread FetchVersionThread = new Thread(() =>
                {
                    using (WebClient client = new WebClient())
                    {
                        try
                        {
                            string s = client.DownloadString("https://www.lcpdfr.com/applications/downloadsng/interface/api.php?do=checkForUpdates&fileId=31024&textOnly=1"); //CHANGE LATER

                            NewVersion = new Version(s);
                        }
                        catch (Exception) { Game.LogTrivial("DOORCONTROL: Cannot Connect to Plugin Info Page. Aborting Update Checks."); }
                    }
                });
                FetchVersionThread.Start();
                while (FetchVersionThread.ThreadState != System.Threading.ThreadState.Stopped)  //if we have a thread to check the update. Otherwise go straight to catch blocks
                {
                    GameFiber.Yield();
                }
                // compare the versions  
                if (curVersion.CompareTo(NewVersion) < 0)
                {
                    Game.LogTrivial("DOORCONTROL: Update Available for Door Control. Installed Version " + curVersion + "New Version " + NewVersion);
                    UpToDate = false;
                }
                else
                {
                    UpToDate = true;
                }
            }
            catch (System.Threading.ThreadAbortException)
            {
                Game.LogTrivial("DOORCONTROL: Error while checking Door Control for updates. System.ThreadAbortException.");
            }
            catch (Exception)
            {
                Game.LogTrivial("DOORCONTROL: Error while checking Door Control for updates. Some Other Exception.");
            }
            Game.LogTrivial("==========DOORCONTROL INFORMATION==========");
            Game.LogTrivial("Door Control by YobB1n");
            Game.LogTrivial("Version 1.1.0.0");
            if(UpToDate == true)
            {
                Game.LogTrivial("Door Control is Up-To-Date.");
            }
            else
            {
                Game.LogTrivial("Door Control is NOT Up-To-Date.");
            }
            if (Config.INIFile.Exists())
            {
                Game.LogTrivial("Door Control Config is Installed by User.");
            }
            else
            {
                Game.LogTrivial("Door Control Config is NOT Installed by User.");
            }
            Game.LogTrivial("Please Join My Discord Server to Report Bugs/Improvements: https://discord.gg/Wj522qa5mT. Enjoy!");
            Game.LogTrivial("==========DOORCONTROL INFORMATION==========");
            Game.DisplayNotification("~b~DoorControl ~y~v." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + "~w~ by ~b~YobB1n ~g~Loaded Successfully.");   //Returns Callout Version
            if (UpToDate == true)
            {
                Game.DisplayNotification("You are on the ~g~Latest Version~w~ of ~b~Door Control.");
            }
            else
            {
                Game.LogTrivial("DOORCONTROL: Update Available for Door Control. Installed Version " + curVersion + "New Version " + NewVersion);
                Game.DisplayNotification("It is ~y~Strongly Recommended~w~ to~g~ Update~b~ Door Control. ~w~Playing on an Old Version ~r~May Cause Issues!");
            }
            GameFiber.StartNew(delegate {
                if (Menu.MenuInitialized == false)
                {
                    Menu.createMenu();
                    while (true)
                    {
                        GameFiber.Yield();
                        if (Game.IsKeyDown(Config.MenuKey) || Game.IsControllerButtonDown(Config.ControllerMenuKey))
                        {
                            Menu.mainMenu.Visible = !Menu.mainMenu.Visible;
                        }
                        Menu._menuPool.ProcessMenus();
                    }
                }
            });
        }
        public override void Finally()
        {
            Game.LogTrivial("DOORCONTROL: Door Control has been cleaned up.");
        }
        private static void OnOnDutyStateChangedHandler(bool OnDuty)
        {
        }
    }
}
