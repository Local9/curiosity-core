namespace Curiosity.Systems.Library.Enums
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
        PatrolZoneUnknown,
        MustBeCloserToSubject,
        OutsideVehicle,
        SubjectNotFound,
        InsideVehicle,
        VehicleIsOwned,
        PurchaseSuccessful,
        PurchaseUnSuccessful,
        NotEnoughPoliceRep1000,
    };

    public static class ErrorMessage
    {
        public static string Get(CommonErrors errorType, string placeholderValue = null)
        {
            string outputMessage = "";
            string placeholder = placeholderValue != null ? " " + placeholderValue : "";
            switch (errorType)
            {
                case CommonErrors.NotEnoughPoliceRep1000:
                    outputMessage = "Not enough Police Rep. Require ~b~1000";
                    break;
                case CommonErrors.VehicleIsOwned:
                    outputMessage = "Vehicle is Owned by a Player.";
                    break;
                case CommonErrors.PurchaseSuccessful:
                    outputMessage = "Successful Purchase.";
                    break;
                case CommonErrors.PurchaseUnSuccessful:
                    outputMessage = "Unsuccessful Purchase.";
                    break;
                case CommonErrors.InsideVehicle:
                    outputMessage = "You need to be inside the vehicle.";
                    break;
                case CommonErrors.SubjectNotFound:
                    outputMessage = "Subject not found.";
                    break;
                case CommonErrors.OutsideVehicle:
                    outputMessage = "You need to be outside the vehicle.";
                    break;
                case CommonErrors.MustBeCloserToSubject:
                    outputMessage = "You need to be closer to the subject.";
                    break;
                case CommonErrors.PatrolZoneUnknown:
                    outputMessage = "Unknown Patrol Zone.";
                    break;
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
}
