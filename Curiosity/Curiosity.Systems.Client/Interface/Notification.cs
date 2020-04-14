using CitizenFX.Core.Native;
using Curiosity.Systems.Client.Diagnostics;
using System.Collections.Generic;
using System.Diagnostics;

namespace Curiosity.Systems.Client.Interface
{
    public enum CommonErrors
    {
        NoVehicle,
        NeedToBeTheDriver,
        UnknownError,
        NotAllowed,
        InvalidModel,
        InvalidInput,
        InvalidSaveName,
        SaveNameAlreadyExists,
        CouldNotLoadSave,
        CouldNotLoad,
        PlayerNotFound,
        PedNotFound,
        WalkingStyleNotForMale,
        WalkingStyleNotForFemale,
        RightAlignedNotSupported,
    };

    public static class ErrorMessage
    {
        public static string Get(CommonErrors errorType, string placeholderValue = null)
        {
            string outputMessage = "";
            string placeholder = placeholderValue != null ? " " + placeholderValue : "";
            switch (errorType)
            {
                case CommonErrors.NeedToBeTheDriver:
                    outputMessage = "You need to be the driver of this vehicle.";
                    break;
                case CommonErrors.NoVehicle:
                    outputMessage = $"You need to be inside a vehicle{placeholder}.";
                    break;
                case CommonErrors.NotAllowed:
                    outputMessage = $"You are not allowed to{placeholder}, sorry.";
                    break;
                case CommonErrors.InvalidModel:
                    outputMessage = $"This model~r~{placeholder} ~s~could not be found, are you sure it's valid?";
                    break;
                case CommonErrors.InvalidInput:
                    outputMessage = $"The input~r~{placeholder} ~s~is invalid or you cancelled the action, please try again.";
                    break;
                case CommonErrors.InvalidSaveName:
                    outputMessage = $"Saving failed because the provided save name~r~{placeholder} ~s~is invalid.";
                    break;
                case CommonErrors.SaveNameAlreadyExists:
                    outputMessage = $"Saving failed because the provided save name~r~{placeholder} ~s~already exists.";
                    break;
                case CommonErrors.CouldNotLoadSave:
                    outputMessage = $"Loading of~r~{placeholder} ~s~failed! Is the saves file corrupt?";
                    break;
                case CommonErrors.CouldNotLoad:
                    outputMessage = $"Could not load~r~{placeholder}~s~, sorry!";
                    break;
                case CommonErrors.PedNotFound:
                    outputMessage = $"The specified ped could not be found.{placeholder}";
                    break;
                case CommonErrors.PlayerNotFound:
                    outputMessage = $"The specified player could not be found.{placeholder}";
                    break;
                case CommonErrors.WalkingStyleNotForMale:
                    outputMessage = $"This walking style is not available for male peds.{placeholder}";
                    break;
                case CommonErrors.WalkingStyleNotForFemale:
                    outputMessage = $"This walking style is not available for female peds.{placeholder}";
                    break;
                case CommonErrors.RightAlignedNotSupported:
                    outputMessage = $"Right aligned menus are not supported for ultra wide aspect ratios.{placeholder}";
                    break;

                case CommonErrors.UnknownError:
                default:
                    outputMessage = $"An unknown error occurred, sorry!{placeholder}";
                    break;
            }
            return outputMessage;
        }
    }

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

        public static void CustomImage(string textureDict, string textureName, string message, string title, string subtitle, bool saveToBrief, int iconType = 0, int bgColor = 0)
        {
            API.SetNotificationTextEntry("CELL_EMAIL_BCON"); // 10x ~a~
            foreach (string s in CitizenFX.Core.UI.Screen.StringToArray(message))
            {
                API.AddTextComponentSubstringPlayerName(s);
            }
            API.SetNotificationMessage(textureName, textureDict, false, iconType, title, subtitle);
            API.SetNotificationBackgroundColor(bgColor);
            API.DrawNotification(false, saveToBrief);
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
            EXIT_INTERIOR_HELP_MESSAGE
        }

        private static Dictionary<Label, KeyValuePair<string, string>> labels = new Dictionary<Label, KeyValuePair<string, string>>()
        {
            [Label.EXIT_INTERIOR_HELP_MESSAGE] = new KeyValuePair<string, string>("EXIT_INTERIOR_HELP_MESSAGE", "Press ~INPUT_CONTEXT~ to exit the building.")
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
            if (API.GetLabelText(labels[label].Key) == "NULL")
            {
                API.AddTextEntry(labels[label].Key, labels[label].Value);
            }
            API.DisplayHelpTextThisFrame(labels[label].Key, true);
        }
    }
}
