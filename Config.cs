using Rage;
using System.Windows.Forms;

namespace DoorControl
{
    internal static class Config
    {
        public static readonly InitializationFile INIFile = new InitializationFile(@"Plugins\LSPDFR\DoorControl.ini");

        public static readonly bool LoadOnStart = INIFile.ReadBoolean("Startup", "Enable on Startup", true);
        public static readonly bool PursuitOnlyOnStart = INIFile.ReadBoolean("Startup", "Enable Door Control Only for Pursuits/Pullovers on Startup", false);
        public static readonly bool TrafficControlOnStart = INIFile.ReadBoolean("Startup", "Stop Traffic During Pursuits on Startup", true);

        public static readonly Keys VehicleExitKey = INIFile.ReadEnum<Keys>("Keys", "Vehicle Exit Key", Keys.F);
        public static readonly Keys MenuKey = INIFile.ReadEnum<Keys>("Keys", "Menu Key", Keys.RControlKey);

        public static readonly ControllerButtons ControllerVehicleExitKey = INIFile.ReadEnum<ControllerButtons>("Controller Keys", "Controller Vehicle Exit Key", ControllerButtons.Y);
        public static readonly ControllerButtons ControllerMenuKey = INIFile.ReadEnum<ControllerButtons>("Controller Keys", "Controller Menu Key", ControllerButtons.None);

        public static readonly string FavouritePoliceVehicle = INIFile.ReadString("Miscellaneous", "Favourite Police Vehicle", "police");
        public static readonly int TrafficControlRadius = INIFile.ReadInt32("Miscellaneous", "Traffic Control Radius", 50);
        public static readonly bool EndTrafficControl = INIFile.ReadBoolean("Miscellaneous", "Automatically Remove Traffic Control Upon Re-Entering Vehicle", false);
        public static readonly bool VehicleOptions = INIFile.ReadBoolean("Miscellaneous", "Enable Police Vehicle Options in Menu", true);
    }
}
