using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Missions.Client.net.MissionPeds;
using Curiosity.Missions.Client.net.Scripts.Interactions.VehicleInteractions;
using Curiosity.Missions.Client.net.Static;
using Curiosity.Shared.Client.net.Enums.Patrol;
using System.Collections.Generic;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Missions.Client.net.Scripts.Interactions.PedInteractions
{
    class Social
    {
        static public async void InteractionHello(Ped ped)
        {
            await Helpers.Animations.LoadAnimation("gestures@m@sitting@generic@casual");

            if (Client.speechType == SpeechType.NORMAL)
            {
                string voiceName = "s_f_y_cop_01_white_full_01";
                if (ped.Gender == Gender.Male)
                {
                    voiceName = "s_m_y_cop_01_white_full_01";
                }
                List<string> hello = new List<string>() { "GENERIC_HI", "KIFFLOM_GREET" };
                PlayAmbientSpeechWithVoice(ped.Handle, hello[Client.Random.Next(hello.Count)], voiceName, "SPEECH_PARAMS_FORCE_SHOUTED", false);
                Game.PlayerPed.Task.PlayAnimation("gestures@m@standing@casual", "gesture_hello", 8.0f, -1, (AnimationFlags)49);
            }
            else
            {
                string voiceName = "s_f_y_cop_01_white_full_01";
                if (ped.Gender == Gender.Male)
                {
                    voiceName = "s_m_y_cop_01_white_full_01";
                }
                PlayAmbientSpeechWithVoice(ped.Handle, "GENERIC_INSULT_HIGH", voiceName, "SPEECH_PARAMS_FORCE_SHOUTED", false);
                Game.PlayerPed.Task.PlayAnimation("gestures@m@standing@casual", "gesture_what_hard", 8.0f, -1, (AnimationFlags)49);
            }
            await Client.Delay(1000);
            Game.PlayerPed.Task.ClearAll();
        }

        internal static async void InteractionPresentIdentification(InteractivePed interactivePed)
        {
            string officerSubtitle = string.Empty;
            if (Client.speechType == SpeechType.NORMAL)
            {
                officerSubtitle = DataClasses.Police.LinesOfSpeech.OfficerNormalQuotes[Client.Random.Next(DataClasses.Police.LinesOfSpeech.OfficerNormalQuotes.Count)];
            }
            else
            {
                officerSubtitle = DataClasses.Police.LinesOfSpeech.OfficerAggresiveQuotes[Client.Random.Next(DataClasses.Police.LinesOfSpeech.OfficerAggresiveQuotes.Count)];
            }
            Screen.ShowSubtitle($"~o~Officer: ~w~{officerSubtitle}");
            await Client.Delay(2000);
            if (interactivePed.HasLostId)
            {
                Screen.ShowSubtitle($"~b~Driver: ~w~Sorry officer, I don't have it on me...");
            }
            else
            {
                if (interactivePed.Attitude < 50)
                {
                    string driverResponse = DataClasses.Police.LinesOfSpeech.DriverResponseNormalIdentity[Client.Random.Next(DataClasses.Police.LinesOfSpeech.DriverResponseNormalIdentity.Count)];
                    Screen.ShowSubtitle($"~b~Driver: ~w~{driverResponse}");
                }
                else if (interactivePed.Attitude >= 50 && interactivePed.Attitude < 80)
                {
                    string driverResponse = DataClasses.Police.LinesOfSpeech.DriverResponseRushedIdentity[Client.Random.Next(DataClasses.Police.LinesOfSpeech.DriverResponseRushedIdentity.Count)];
                    Screen.ShowSubtitle($"~b~Driver: ~w~{driverResponse}");
                }
                else if (interactivePed.Attitude >= 80)
                {
                    if (Client.Random.Next(10) > 8)
                    {
                        if (interactivePed.Ped.IsInVehicle())
                        {
                            interactivePed.Ped.Weapons.Give(WeaponHash.StunGun, 1, true, true);
                            interactivePed.SetRelationship(Relationships.HostileRelationship);
                            interactivePed.Ped.Task.FightAgainstHatedTargets(20f, 3000);
                            await BaseScript.Delay(2000);
                            interactivePed.IsWanted = true;
                            interactivePed.RanFromPolice = true;

                            TrafficStopInteractions.TrafficStopVehicleFlee(interactivePed.Ped.CurrentVehicle, interactivePed.Ped);

                            string driverResponse = DataClasses.Police.LinesOfSpeech.DriverResponsePissedIdentity[Client.Random.Next(DataClasses.Police.LinesOfSpeech.DriverResponsePissedIdentity.Count)];
                            Screen.ShowSubtitle($"~b~Driver: ~w~{driverResponse}");
                        }
                    }
                    else
                    {
                        string driverResponse = DataClasses.Police.LinesOfSpeech.DriverResponseAngeredIdentity[Client.Random.Next(DataClasses.Police.LinesOfSpeech.DriverResponseAngeredIdentity.Count)];
                        Screen.ShowSubtitle($"~b~Driver: ~w~{driverResponse}");
                    }
                }

                Wrappers.Helpers.ShowNotification("Driver's ID", string.Empty, $"~w~Name: ~y~{interactivePed.Name}\n~w~DOB: ~y~{interactivePed.DateOfBirth}");

                Client.TriggerEvent("curiosity:interaction:idRequesed", interactivePed.Handle);
            }
        }

        static public void InteractionDrunk(InteractivePed interactivePed)
        {
            Wrappers.Helpers.ShowOfficerSubtitle("Have you had anything to drink today?");
            List<string> response;
            if (interactivePed.IsUnderTheInfluence)
            {
                response = new List<string>() { "*Burp*", "What's a drink?", "No.", "You'll never catch me alive!", "Never", "Nope, i don't drink Ossifer", "Maybe?", "Just a few." };
            }
            else
            {
                response = new List<string>() { "No, sir", "I dont drink.", "Nope.", "No.", "Only 1.", "Yes... a water and 2 orange juices." };
            }
            Wrappers.Helpers.ShowSuspectSubtitle(response[Client.Random.Next(response.Count)]);
        }

        static public void InteractionDrug(InteractivePed interactivePed)
        {
            Wrappers.Helpers.ShowOfficerSubtitle("Have you consumed any drugs recently?");
            List<string> response;
            if (interactivePed.IsUsingCannabis || interactivePed.IsUsingCocaine)
            {
                response = new List<string>() { "What is life?", "Who is me?", "NoOOOooo.", "Is that a UNICORN?!", "If I've done the what?", "WHAT DRUGS? I DONT KNOW KNOW ANYTHING ABOUT DRUGS.", "What's a drug?" };
            }
            else
            {
                response = new List<string>() { "No, sir", "I don't do that stuff.", "Nope.", "No.", "Nah" };
            }
            Wrappers.Helpers.ShowSuspectSubtitle(response[Client.Random.Next(response.Count)]);
        }

        static public void InteractionIllegal(InteractivePed interactivePed)
        {
            bool isInVehicle = interactivePed.Ped.IsInVehicle();
            string checkScript = isInVehicle ? "in the vehicle" : "I might find on you";
            Wrappers.Helpers.ShowOfficerSubtitle($"Is there anything illegal {checkScript}?");
            List<string> response = new List<string>() { "No, sir", "Not that I know of.", "Nope.", "No.", "Maybe? But most probably not.", "I sure hope not" };
            if (isInVehicle)
            {
                response = new List<string>() { "No, sir", "Not that I know of.", "Nope.", "No.", "Apart from the 13 dead hookers in the back.. No.", "Maybe? But most probably not.", "I sure hope not" };
            }
            Wrappers.Helpers.ShowSuspectSubtitle(response[Client.Random.Next(response.Count)]);
        }

        static public void InteractionSearch(InteractivePed interactivePed)
        {
            Wrappers.Helpers.ShowOfficerSubtitle("Would you mind if I search you?");
            List<string> response;
            if (!interactivePed.IsAllowedToBeSearched)
            {
                response = new List<string>() { "I'd prefer you not to...", "I'll have to pass on that", };
            }
            else
            {
                response = new List<string>() { "Go ahead", "Shes all yours", "I'd prefer you not to", "I don't have anything to hide, go for it.", "Uuuh... Y- No..", "Go ahead. For the record its not my car", "Yeah, why not.." };
            }
            Wrappers.Helpers.ShowSuspectSubtitle(response[Client.Random.Next(response.Count)]);
        }

        //static public void InteractionSearchVehicle()
        //{
        //    Helpers.ShowOfficerSubtitle("Would you mind if i search your vehicle?");
        //    List<string> response;
        //    if (CanSearchVehicle)
        //    {
        //        response = new List<string>() { "I'd prefer you not to...", "I'll have to pass on that", "Uuuh... Y- No..", "Go ahead. For the record its not my car", "Yeah, why not.." };
        //    }
        //    else
        //    {
        //        response = new List<string>() { "Go ahead", "Shes all yours", "I'd prefer you not to", "I don't have anything to hide, go for it." };
        //    }
        //    Helpers.ShowDriverSubtitle(response[Client.Random.Next(response.Count)]);
        //}
    }
}
