using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.CasinoSystems.Client.Extensions;
using System.Threading.Tasks;

namespace Curiosity.CasinoSystems.Client.Scripts.InteriorScripts
{
    class LuckyWheel
    {
        static Plugin plugin;

        static int wheelObjectId;

        static Vector3 wheelPos = new Vector3(1111.052f, 229.8579f, -49.133f);

        static string luckyWheelAnimationDict;
		static string initialAnimation;

        public static void Init()
        {
            plugin = Plugin.GetInstance();

            plugin.RegisterTickHandler(OnLuckyWheelMessages);

            SetupAnimation();

			initialAnimation = AnimationSequence(0);

            CreateProp();
            RequestGraphics();
        }

        private static void SetupAnimation()
        {
            luckyWheelAnimationDict = GetAnimationDict();
            API.RequestAnimDict(luckyWheelAnimationDict);

        }

        static string GetAnimationDict()
        {
            if (Game.PlayerPed.IsFemale())
            {
                return "ANIM_CASINO_A@AMB@CASINO@GAMES@LUCKY7WHEEL@FEMALE";
            }
            return "ANIM_CASINO_A@AMB@CASINO@GAMES@LUCKY7WHEEL@MALE";
        }

        private static async Task OnLuckyWheelMessages()
        {
			// Helpers.DisplayHelp("CASINO_LUCK_WD", -1); // Try again later
			// Helpers.DisplayHelpWithNumber("CAS_MG_MEMB2", 20000, -1); // Try again later
			// Helpers.DisplayHelp("LUCKY_WHEEL_US", -1); // One per day
			// Helpers.DisplayHelp("LW_PLAY", -1); // Press E to Spin
			// Helpers.DisplayHelp("POD_TOO_MANY", -1); // Too many players near activity
			// Helpers.DisplayHelp("CAS_LW_REGL", -1); // Feature is not available for you
			// Helpers.IsTextHelpBeingDisplayed("LW_PLAY"); // One per day
			// Some stupid mini game?
			// Helpers.DisplayHelp("LUCK_W_SPIN_PC", -1); // Press WSAD to Spin
			// Helpers.DisplayHelp("LUCK_WHEEL_SPIN", -1); // Press WSAD to Spin

			if (API.IsEntityInAngledArea(Game.PlayerPed.Handle, 1110.995f, 228.9034f, -50.6408f, 1109.727f, 228.9352f, -48.3908f, 1.5f, false, true, 0))
            {
                Helpers.DisplayHelp("LUCK_WHEEL_SPIN", -1); // Press E to Spin

				if (Game.IsControlJustPressed(0, Control.Context))
					SpinWheel();
            }
        }

		static bool CanPlayLuckyWheel()
        {
			if (Game.PlayerPed.Exists())
            {
				if (!Game.PlayerPed.IsDead)
                {
					return true;
                }
            }
			return false;
        }

        static void SpinWheel()
        {
			int strength = Plugin.random.Next(3);
            string spinAnimation = GetRandomAnimation(strength);
            int playerPedId = Game.PlayerPed.Handle;
            API.SetEntityRotation(playerPedId, 0f, 0f, 0f, 2, true);
            API.PlayEntityAnim(playerPedId, spinAnimation, luckyWheelAnimationDict, 1000f, false, true, false, 0f, 2);
            API.ForceEntityAiAndAnimationUpdate(playerPedId);
        }

        private static async void CreateProp()
        {
            if (!API.DoesEntityExist(wheelObjectId))
            {
                Model model = (int)LuckyWheelHash();
                model.Request(1000);

                int loadCheck = 0;

                while(!model.IsLoaded)
                {
                    if (loadCheck > 10)
                        break;

                    await BaseScript.Delay(10);
                    loadCheck++;
                }

                if (model.IsLoaded)
                {
                    wheelObjectId = API.CreateObjectNoOffset(LuckyWheelHash(), wheelPos.X, wheelPos.Y, wheelPos.Z, false, false, true);
                    API.SetEntitySomething(wheelObjectId, true);
                    API.SetEntityCanBeDamaged(wheelObjectId, false);

                    model.MarkAsNoLongerNeeded();
                }
            }
        }

        public static void Dispose()
        {
			API.SetModelAsNoLongerNeeded(LuckyWheelHash());
			// StopPlayingAnimation();
			// StopSyncronisedScene();

			if (!string.IsNullOrEmpty(luckyWheelAnimationDict))
				API.RemoveAnimDict(luckyWheelAnimationDict);

			if (API.DoesEntityExist(wheelObjectId))
                API.DeleteEntity(ref wheelObjectId);

            plugin.DeregisterTickHandler(OnLuckyWheelMessages);
        }

        static void RequestGraphics()
        {
            API.RequestStreamedTextureDict("CasinoUI_Lucky_Wheel", false);
        }

        static uint LuckyWheelHash()
        {
            return (uint)API.GetHashKey("vw_prop_vw_luckywheel_02a");
        }

		static bool IsPlayingAnimation(int anim)
        {
			string animation;

			if (CanPlayLuckyWheel())
            {
				animation = AnimationSequence(anim);
				if (!string.IsNullOrEmpty(animation))
                {
					return API.IsEntityPlayingAnim(Game.PlayerPed.Handle, luckyWheelAnimationDict, animation, 3);
                }
            }
			return false;
        }

		static int GetAnimationSequenceFromName(string animStr)
        {
			int animationSquenceNumber;

			animationSquenceNumber = -1;
			if (animStr == "Enter_to_ArmRaisedIDLE")
			{
				animationSquenceNumber = 0;
			}
			else if (animStr == "ArmRaisedIDLE")
			{
				animationSquenceNumber = 1;
			}
			else if (animStr == "ArmRaisedIDLE_to_SpinReadyIDLE")
			{
				animationSquenceNumber = 2;
			}
			else if (animStr == "SpinReadyIDLE")
			{
				animationSquenceNumber = 3;
			}
			else if (animStr == "SpinStart_ArmRaisedIDLE_to_BaseIDLE")
			{
				animationSquenceNumber = 4;
			}
			else if (animStr == "spinreadyidle_to_spinningidle_low" || animStr == "ArmRaisedIDLE_to_SpinningIDLE_Low")
			{
				animationSquenceNumber = 5;
			}
			else if (animStr == "spinreadyidle_to_spinningidle_med" || animStr == "ArmRaisedIDLE_to_SpinningIDLE_Med")
			{
				animationSquenceNumber = 6;
			}
			else if (animStr == "spinreadyidle_to_spinningidle_high" || animStr == "ArmRaisedIDLE_to_SpinningIDLE_High")
			{
				animationSquenceNumber = 7;
			}
			else if (animStr == "SpinningIDLE_Low")
			{
				animationSquenceNumber = 8;
			}
			else if (animStr == "SpinningIDLE_Med")
			{
				animationSquenceNumber = 9;
			}
			else if (animStr == "SpinningIDLE_High")
			{
				animationSquenceNumber = 10;
			}
			else if (animStr == "Win")
			{
				animationSquenceNumber = 11;
			}
			else if (animStr == "Win_Big")
			{
				animationSquenceNumber = 12;
			}
			else if (animStr == "Win_Huge")
			{
				animationSquenceNumber = 13;
			}
			else if (animStr == "Exit_to_Standing")
			{
				animationSquenceNumber = 14;
			}
			else if (animStr == "SpinReadyIDLE_to_ArmRaisedIDLE")
			{
				animationSquenceNumber = 15;
			}
			return animationSquenceNumber;
		}

		static string AnimationSequence(int anim)
		{
			string animation = string.Empty;
			bool isFemale = Game.PlayerPed.IsFemale();

			switch (anim)
			{
				case 0:
					animation = "Enter_to_ArmRaisedIDLE";
					break;
				case 1:
					animation = "ArmRaisedIDLE";
					break;
				case 2:
					animation = "ArmRaisedIDLE_to_SpinReadyIDLE";
					break;
				case 3:
					animation = "SpinReadyIDLE";
					break;
				case 4:
					animation = "SpinStart_ArmRaisedIDLE_to_BaseIDLE";
					break;
				case 5:
					if (isFemale)
					{
						animation = "spinreadyidle_to_spinningidle_low";
					}
					else
					{
						animation = "ArmRaisedIDLE_to_SpinningIDLE_Low";
					}
					break;
				case 6:
					if (isFemale)
					{
						animation = "spinreadyidle_to_spinningidle_med";
					}
					else
					{
						animation = "ArmRaisedIDLE_to_SpinningIDLE_Med";
					}
					break;
				case 7:
					if (isFemale) // gender?
					{
						animation = "spinreadyidle_to_spinningidle_high";
					}
					else
					{
						animation = "ArmRaisedIDLE_to_SpinningIDLE_High";
					}
					break;
				case 8:
					animation = "SpinningIDLE_Low";
					break;
				case 9:
					animation = "SpinningIDLE_Medium";
					break;
				case 10:
					animation = "SpinningIDLE_High";
					break;
				case 11:
					animation = "Win";
					break;
				case 12:
					animation = "Win_Big";
					break;
				case 13:
					animation = "Win_Huge";
					break;
				case 14:
					animation = "Exit_to_Standing";
					break;
				case 15:
					animation = "SpinReadyIDLE_to_ArmRaisedIDLE";
					break;
			}
			return animation;
		}

		static string GetRandomAnimation(int strenght)
		{
			string anim = "null";
			bool isFemale = Game.PlayerPed.IsFemale();

			int randomAnimation = Plugin.random.Next(20);

			switch (strenght)
			{
				case 0:
					if (isFemale) // female
					{
						switch (randomAnimation)
						{
							case 0:
								return "spinningwheel_low_effort_01_wheel";
							case 1:
								return "spinningwheel_low_effort_02_wheel";
							case 2:
								return "spinningwheel_low_effort_03_wheel";
							case 3:
								return "spinningwheel_low_effort_04_wheel";
							case 4:
								return "spinningwheel_low_effort_05_wheel";
							case 5:
								return "spinningwheel_low_effort_06_wheel";
							case 6:
								return "spinningwheel_low_effort_07_wheel";
							case 7:
								return "spinningwheel_low_effort_08_wheel";
							case 8:
								return "spinningwheel_low_effort_09_wheel";
							case 9:
								return "spinningwheel_low_effort_10_wheel";
							case 10:
								return "spinningwheel_low_effort_11_wheel";
							case 11:
								return "spinningwheel_low_effort_12_wheel";
							case 12:
								return "spinningwheel_low_effort_13_wheel";
							case 13:
								return "spinningwheel_low_effort_14_wheel";
							case 14:
								return "spinningwheel_low_effort_15_wheel";
							case 15:
								return "spinningwheel_low_effort_16_wheel";
							case 16:
								return "spinningwheel_low_effort_17_wheel";
							case 17:
								return "spinningwheel_low_effort_18_wheel";
							case 18:
								return "spinningwheel_low_effort_19_wheel";
							case 19:
								return "spinningwheel_low_effort_20_wheel";
						}
					}
					else
					{
						switch (randomAnimation)
						{
							case 0:
								return "spinningwheel_low_effort_01";
							case 1:
								return "spinningwheel_low_effort_02";
							case 2:
								return "spinningwheel_low_effort_03";
							case 3:
								return "spinningwheel_low_effort_04";
							case 4:
								return "spinningwheel_low_effort_05";
							case 5:
								return "spinningwheel_low_effort_06";
							case 6:
								return "spinningwheel_low_effort_07";
							case 7:
								return "spinningwheel_low_effort_08";
							case 8:
								return "spinningwheel_low_effort_09";
							case 9:
								return "spinningwheel_low_effort_10";
							case 10:
								return "spinningwheel_low_effort_11";
							case 11:
								return "spinningwheel_low_effort_12";
							case 12:
								return "spinningwheel_low_effort_13";
							case 13:
								return "spinningwheel_low_effort_14";
							case 14:
								return "spinningwheel_low_effort_15";
							case 15:
								return "spinningwheel_low_effort_16";
							case 16:
								return "spinningwheel_low_effort_17";
							case 17:
								return "spinningwheel_low_effort_18";
							case 18:
								return "spinningwheel_low_effort_19";
							case 19:
								return "spinningwheel_low_effort_20";
						}
					}
					break;

				case 1:
					if (isFemale) // female
					{
						switch (randomAnimation)
						{
							case 0:
								return "spinningwheel_med_effort_20_wheel";
							case 1:
								return "spinningwheel_med_effort_01_wheel";
							case 2:
								return "spinningwheel_med_effort_02_wheel";
							case 3:
								return "spinningwheel_med_effort_03_wheel";
							case 4:
								return "spinningwheel_med_effort_04_wheel";
							case 5:
								return "spinningwheel_med_effort_05_wheel";
							case 6:
								return "spinningwheel_med_effort_06_wheel";
							case 7:
								return "spinningwheel_med_effort_07_wheel";
							case 8:
								return "spinningwheel_med_effort_08_wheel";
							case 9:
								return "spinningwheel_med_effort_09_wheel";
							case 10:
								return "spinningwheel_med_effort_10_wheel";
							case 11:
								return "spinningwheel_med_effort_11_wheel";
							case 12:
								return "spinningwheel_med_effort_12_wheel";
							case 13:
								return "spinningwheel_med_effort_13_wheel";
							case 14:
								return "spinningwheel_med_effort_14_wheel";
							case 15:
								return "spinningwheel_med_effort_15_wheel";
							case 16:
								return "spinningwheel_med_effort_16_wheel";
							case 17:
								return "spinningwheel_med_effort_17_wheel";
							case 18:
								return "spinningwheel_med_effort_18_wheel";
							case 19:
								return "spinningwheel_med_effort_19_wheel";
						}
					}
					else
					{
						switch (randomAnimation)
						{
							case 0:
								return "spinningwheel_med_effort_01";
							case 1:
								return "spinningwheel_med_effort_02";
							case 2:
								return "spinningwheel_med_effort_03";
							case 3:
								return "spinningwheel_med_effort_04";
							case 4:
								return "spinningwheel_med_effort_05";
							case 5:
								return "spinningwheel_med_effort_06";
							case 6:
								return "spinningwheel_med_effort_07";
							case 7:
								return "spinningwheel_med_effort_08";
							case 8:
								return "spinningwheel_med_effort_09";
							case 9:
								return "spinningwheel_med_effort_10";
							case 10:
								return "spinningwheel_med_effort_11";
							case 11:
								return "spinningwheel_med_effort_12";
							case 12:
								return "spinningwheel_med_effort_13";
							case 13:
								return "spinningwheel_med_effort_14";
							case 14:
								return "spinningwheel_med_effort_15";
							case 15:
								return "spinningwheel_med_effort_16";
							case 16:
								return "spinningwheel_med_effort_17";
							case 17:
								return "spinningwheel_med_effort_18";
							case 18:
								return "spinningwheel_med_effort_19";
							case 19:
								return "spinningwheel_med_effort_20";
						}
					}
					break;

				case 2:
					if (isFemale) // female
					{
						switch (randomAnimation)
						{
							case 0:
								return "spinningwheel_high_effort_01_wheel";
							case 1:
								return "spinningwheel_high_effort_02_wheel";
							case 2:
								return "spinningwheel_high_effort_03_wheel";
							case 3:
								return "spinningwheel_high_effort_04_wheel";
							case 4:
								return "spinningwheel_high_effort_05_wheel";
							case 5:
								return "spinningwheel_high_effort_06_wheel";
							case 6:
								return "spinningwheel_high_effort_07_wheel";
							case 7:
								return "spinningwheel_high_effort_08_wheel";
							case 8:
								return "spinningwheel_high_effort_09_wheel";
							case 9:
								return "spinningwheel_high_effort_10_wheel";
							case 10:
								return "spinningwheel_high_effort_11_wheel";
							case 11:
								return "spinningwheel_high_effort_12_wheel";
							case 12:
								return "spinningwheel_high_effort_13_wheel";
							case 13:
								return "spinningwheel_high_effort_14_wheel";
							case 14:
								return "spinningwheel_high_effort_15_wheel";
							case 15:
								return "spinningwheel_high_effort_16_wheel";
							case 16:
								return "spinningwheel_high_effort_17_wheel";
							case 17:
								return "spinningwheel_high_effort_18_wheel";
							case 18:
								return "spinningwheel_high_effort_19_wheel";
							case 19:
								return "spinningwheel_high_effort_20_wheel";
						}
					}
					else
					{
						switch (randomAnimation)
						{
							case 0:
								return "spinningwheel_high_effort_01";
							case 1:
								return "spinningwheel_high_effort_02";
							case 2:
								return "spinningwheel_high_effort_03";
							case 3:
								return "spinningwheel_high_effort_04";
							case 4:
								return "spinningwheel_high_effort_05";
							case 5:
								return "spinningwheel_high_effort_06";
							case 6:
								return "spinningwheel_high_effort_07";
							case 7:
								return "spinningwheel_high_effort_08";
							case 8:
								return "spinningwheel_high_effort_09";
							case 9:
								return "spinningwheel_high_effort_10";
							case 10:
								return "spinningwheel_high_effort_11";
							case 11:
								return "spinningwheel_high_effort_12";
							case 12:
								return "spinningwheel_high_effort_13";
							case 13:
								return "spinningwheel_high_effort_14";
							case 14:
								return "spinningwheel_high_effort_15";
							case 15:
								return "spinningwheel_high_effort_16";
							case 16:
								return "spinningwheel_high_effort_17";
							case 17:
								return "spinningwheel_high_effort_18";
							case 18:
								return "spinningwheel_high_effort_19";
							case 19:
								return "spinningwheel_high_effort_20";
						}
					}
					break;
			}

			return anim;
		}
	}
}
