using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.MissionManager.Client.Diagnostics;
using Curiosity.Systems.Library.Enums;
using System.Collections.Generic;

namespace Curiosity.MissionManager.Client.Interface
{
    public static class Notify
    {
        public static void Custom(string message, bool blink = true, bool saveToBrief = true, int bgColor = 2)
        {
            API.SetNotificationTextEntry("CELL_EMAIL_BCON"); // 10x ~a~
            foreach (string s in CitizenFX.Core.UI.Screen.StringToArray(message))
            {
                API.AddTextComponentSubstringPlayerName(s);
            }
            API.SetNotificationBackgroundColor(bgColor);
            API.DrawNotification(blink, saveToBrief);
        }

        public static void Alert(string message, bool blink = true, bool saveToBrief = true)
        {
            Custom("~y~~h~Alert~h~~s~: " + message, blink, saveToBrief, 8);
        }

        public static void Alert(CommonErrors errorMessage, bool blink = true, bool saveToBrief = true, string placeholderValue = null)
        {
            string message = ErrorMessage.Get(errorMessage, placeholderValue);
            Alert(message, blink, saveToBrief);
        }

        public static void Error(string message, bool blink = true, bool saveToBrief = true)
        {
            Custom("~r~~h~Error~h~~s~: " + message, blink, saveToBrief, 8);
            Logger.Error("[ERROR] " + message + "\n");
        }

        public static void Error(CommonErrors errorMessage, bool blink = true, bool saveToBrief = true, string placeholderValue = null)
        {
            string message = ErrorMessage.Get(errorMessage, placeholderValue);
            Error(message, blink, saveToBrief);
        }

        public static void Info(string message, bool blink = true, bool saveToBrief = true)
        {
            Custom("~b~~h~Info~h~~s~: " + message, blink, saveToBrief);
        }

        public static void Success(string message, bool blink = true, bool saveToBrief = true)
        {
            Custom("~g~~h~Success~h~~s~: " + message, blink, saveToBrief, 20);
        }

        public static void Impound(string subtitle, string message, bool blink = true, bool saveToBrief = true, int iconType = 1)
        {
            CustomImage("CHAR_PROPERTY_TOWING_IMPOUND", "CHAR_PROPERTY_TOWING_IMPOUND", message, "SA Impound", subtitle, saveToBrief, blink, iconType, 200);
        }

        public static void Dispatch(string subtitle, string message, bool blink = true, bool saveToBrief = true, int iconType = 1)
        {
            CustomImage("CHAR_CALL911", "CHAR_CALL911", message, "Dispatch", subtitle, saveToBrief, blink, iconType, 140);
        }

        public static void DispatchAI(string subtitle, string message, bool blink = true, bool saveToBrief = true, int iconType = 1)
        {
            CustomImage("CHAR_CALL911", "CHAR_CALL911", message, "~b~Dispatch A.I.", subtitle, saveToBrief, blink, iconType, 140);
        }

        public static void CustomImage(string textureDict, string textureName, string message, string title, string subtitle, bool saveToBrief, bool blink = false, int iconType = 0, int bgColor = 2)
        {
            ///
            /// iconTypes:  
            // 1 : Chat Box
            // 2 : Email
            // 3 : Add Friend Request
            // 4 : Nothing
            // 5 : Nothing
            // 6 : Nothing
            // 7 : Right Jumping Arrow
            // 8 : RP Icon
            // 9 : $ Icon
            ///

            API.BeginTextCommandThefeedPost("CELL_EMAIL_BCON"); // 10x ~a~
            foreach (string s in CitizenFX.Core.UI.Screen.StringToArray(message))
            {
                API.AddTextComponentSubstringPlayerName(s);
            }
            API.EndTextCommandThefeedPostMessagetext(textureName, textureDict, blink, iconType, title, subtitle);
            API.ThefeedNextPostBackgroundColor(bgColor);
            API.EndTextCommandThefeedPostTicker(false, saveToBrief);
        }
        
        /// <summary>
        /// Returns a notification ID, this is so that the notification can be removed manually.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="saveToBrief"></param>
        /// <param name="bgColor"></param>
        /// <param name="control"></param>
        /// <returns></returns>
        public static int CustomControl(string message, bool saveToBrief, int bgColor = 2, Control control = Control.Context)
        {
            API.BeginTextCommandThefeedPost("CELL_EMAIL_BCON"); // 10x ~a~
            foreach (string s in CitizenFX.Core.UI.Screen.StringToArray(message))
            {
                API.AddTextComponentSubstringPlayerName(s);
            }

            string controlToShow = "~INPUT_CONTEXT~";

            switch(control)
            {
                case Control.ContextSecondary:
                    controlToShow = "~INPUT_CONTEXT_SECONDARY~";
                    break;
            }

            int notificationId = API.EndTextCommandThefeedPostReplayInput(1, controlToShow, message);
            API.ThefeedNextPostBackgroundColor(bgColor);
            API.EndTextCommandThefeedPostTicker(false, saveToBrief);

            return notificationId;
        }
    }

    public static class Subtitle
    {
        public static void Custom(string message, int duration = 2500, bool drawImmediately = true)
        {
            API.BeginTextCommandPrint("CELL_EMAIL_BCON");
            foreach (string s in CitizenFX.Core.UI.Screen.StringToArray(message))
            {
                API.AddTextComponentSubstringPlayerName(s);
            }
            API.EndTextCommandPrint(duration, drawImmediately);
        }

        public static void Alert(string message, int duration = 2500, bool drawImmediately = true, string prefix = null)
        {
            Custom((prefix != null ? "~y~" + prefix + " ~s~" : "~y~") + message, duration, drawImmediately);
        }

        public static void Error(string message, int duration = 2500, bool drawImmediately = true, string prefix = null)
        {
            Custom((prefix != null ? "~r~" + prefix + " ~s~" : "~r~") + message, duration, drawImmediately);
        }

        public static void Info(string message, int duration = 2500, bool drawImmediately = true, string prefix = null)
        {
            Custom((prefix != null ? "~b~" + prefix + " ~s~" : "~b~") + message, duration, drawImmediately);
        }

        public static void Success(string message, int duration = 2500, bool drawImmediately = true, string prefix = null)
        {
            Custom((prefix != null ? "~g~" + prefix + " ~s~" : "~g~") + message, duration, drawImmediately);
        }
    }

    public static class HelpMessage
    {


        public enum Label
        {
            EXIT_INTERIOR_HELP_MESSAGE,
            ENTER_INTERIOR_HELP_MESSAGE,
            MISSION_CLERK_SPEAK_WITH,
            MISSION_CLERK_RESPONSE_SUSPECT_RAN,
            MISSION_PARKING_METER_CONTEXT,
            MISSION_STOP_VEHICLE,
            MISSION_VEHICLE_TICKET_DRIVER,
            MISSION_VEHICLE_STOPPED,
            TRAFFIC_STOP_INITIATE,
            MENU_OPEN
        }

        public static Dictionary<Label, KeyValuePair<string, string>> HelpTextLabels = new Dictionary<Label, KeyValuePair<string, string>>()
        {
            [Label.EXIT_INTERIOR_HELP_MESSAGE] = new KeyValuePair<string, string>("EXIT_INTERIOR_HELP_MESSAGE", "Press ~INPUT_CONTEXT~ to exit the building."),
            [Label.ENTER_INTERIOR_HELP_MESSAGE] = new KeyValuePair<string, string>("ENTER_INTERIOR_HELP_MESSAGE", "Press ~INPUT_CONTEXT~ to enter the building."),
            [Label.MISSION_CLERK_SPEAK_WITH] = new KeyValuePair<string, string>("MISSION_CLERK_SPEAK_WITH", "Press ~INPUT_CONTEXT~ to speak with the ~b~Store Clerk~w~."),
            [Label.MISSION_CLERK_RESPONSE_SUSPECT_RAN] = new KeyValuePair<string, string>("MISSION_CLERK_RESPONSE_SUSPECT_RAN", $"The perp has just ran off, he's not far away."),
            [Label.MISSION_PARKING_METER_CONTEXT] = new KeyValuePair<string, string>("MISSION_PARKING_METER_CONTEXT", $"Press ~INPUT_CONTEXT~ to ticket the ~b~Vehicle~w~."),
            [Label.MISSION_STOP_VEHICLE] = new KeyValuePair<string, string>("MISSION_STOP_VEHICLE", $"Press ~INPUT_CONTEXT~ to stop the ~b~Vehicle~w~."),
            [Label.MISSION_VEHICLE_TICKET_DRIVER] = new KeyValuePair<string, string>("MISSION_VEHICLE_TICKET_DRIVER", $"Press ~INPUT_CONTEXT~ to ticket the ~b~Driver~w~."),
            [Label.MISSION_VEHICLE_STOPPED] = new KeyValuePair<string, string>("MISSION_VEHICLE_STOPPED", $"Leave your vehicle and walk up to the drivers window."),
            [Label.TRAFFIC_STOP_INITIATE] = new KeyValuePair<string, string>("TRAFFIC_STOP_INITIATE", $"Press ~INPUT_CONTEXT~ to pull over the vehicle in front.~n~Press ~INPUT_COVER~ to ignore the vehicle."),
            [Label.MENU_OPEN] = new KeyValuePair<string, string>("MENU_OPEN", $"~w~Press ~b~~h~F1~h~ ~w~to open menu."),
        };



        public static void Custom(string message) => Custom(message, 6000, true);
        public static void Custom(string message, int duration) => Custom(message, duration, true);
        public static void Custom(string message, int duration, bool sound)
        {
            string[] array = CommonFunctions.StringToArray(message);
            if (API.IsHelpMessageBeingDisplayed())
            {
                API.ClearAllHelpMessages();
            }
            API.BeginTextCommandDisplayHelp("CELL_EMAIL_BCON");
            foreach (string s in array)
            {
                API.AddTextComponentSubstringPlayerName(s);
            }
            API.EndTextCommandDisplayHelp(0, false, sound, duration);
        }

        public static void CustomLooped(Label label)
        {
            if (API.GetLabelText(HelpTextLabels[label].Key) == "NULL")
            {
                API.AddTextEntry(HelpTextLabels[label].Key, HelpTextLabels[label].Value);
            }

            API.DisplayHelpTextThisFrame(HelpTextLabels[label].Key, false);
        }
    }
}
