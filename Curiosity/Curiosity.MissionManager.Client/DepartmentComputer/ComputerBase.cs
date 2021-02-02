using CitizenFX.Core;
using Curiosity.MissionManager.Client.Managers;
using Curiosity.MissionManager.Client.Menu;
using Curiosity.Systems.Library.Entity;
using NativeUI.PauseMenu;
using System;
using System.Collections.Generic;

namespace Curiosity.MissionManager.Client.DepartmentComputer
{
    public class ComputerBase : Manager<ComputerBase>
    {
        public static ComputerBase Instance;

        TabView menuContainer = new TabView("LSPD National Computer", "To Protect and to Serve");

		TabTextItem tabWelcome = new TabTextItem("Home", "~b~LSPD National Computer", "Here you'll find additional information.");

		static Dictionary<string, string> emptyDictionary = new Dictionary<string, string>();

		TabItemSimpleList tabIdentifcation = new TabItemSimpleList("Personal Identification", emptyDictionary);
		TabItemSimpleList tabRegistration = new TabItemSimpleList("Vehicle Registration", emptyDictionary);
		TabItemSimpleList tabCourtOutcomes = new TabItemSimpleList("Courts", emptyDictionary);


		// Setup, on Open, get information from server ...
		/*
		 * Wants
		 * Registration
		 * 
		 * 
		 * */

		public override async void Begin()
		{
			Instance = this;

			menuContainer.SideStringTop = $"Officer: {Game.Player.Name}";
			menuContainer.SideStringMiddle = string.Empty;
			menuContainer.SideStringBottom = $"All Services Operational";

			tabWelcome.Text = "Here you'll find additional information." +
				"~n~* ~g~NOTE~s~: If you exit and the screen is blurred, reopen the computer and exit using the Back button shown in the bottom right.";

			TabSubmenuItem nationalComputer = new TabSubmenuItem("LSPD:NC", new List<TabItem>()
			{
				tabIdentifcation,
				tabRegistration,
			});

			menuContainer.AddTab(tabWelcome);
			menuContainer.AddTab(nationalComputer);

			while (MenuManager._MenuPool == null)
			{
				await BaseScript.Delay(100);
			}

			MenuManager._MenuPool.AddPauseMenu(menuContainer);
		}

        public void Open()
        {
            tabWelcome.Active = true;
            tabWelcome.Focused = true;
            tabWelcome.Visible = true;
			tabWelcome.FadeInWhenFocused = true;

			tabRegistration.FadeInWhenFocused = true;
			tabIdentifcation.FadeInWhenFocused = true;

			if (!Mission.isOnMission)
            {
				NotOnCallout();
			}
			else
            {
				SetupComputer();
            }

			// Must have ID
			// if traffic stop, can get vehicle information and owner

			menuContainer.Visible = true;
        }

        private void SetupComputer()
        {
			MissionData missionData = Mission.currentMissionData;

			Dictionary<string, string> identification = new Dictionary<string, string>();
			Dictionary<string, string> registration = new Dictionary<string, string>();

			try
			{
				int suspects = 0;

				foreach (KeyValuePair<int, MissionDataPed> kvp in missionData.NetworkPeds)
				{
					MissionDataPed pedData = kvp.Value;

					suspects++;

					identification[$"#{suspects} ~g~REQUEST"] = "";

					if (pedData.IsIdentified)
					{
						identification[$"#{suspects} ~b~Name"] = pedData.FullName;
						identification[$"#{suspects} ~b~Date Of Birth"] = pedData.DateOfBirth.ToString($"yyyy-MM-dd");
						identification[$"#{suspects} ~b~Gender"] = pedData.Gender == 0 ? "Male" : "Female";
						identification[$"#{suspects} ~b~Carry License"] = pedData.HasCarryLicense ? "~g~Valid" : "~o~Unknown/Invalid";

						if (pedData.HasBeenBreathalysed)
                        {
							identification[$"#{suspects} ~b~Blood Alcohol Level"] = pedData.BloodAlcoholLimit >= 8 ? $"~o~0.{pedData.BloodAlcoholLimit:00}" : $"~g~0.{pedData.BloodAlcoholLimit:00}";
						}

						if (pedData.Wants.Count > 0)
						{
							int wants = 0;
							pedData.Wants.ForEach(s =>
							{
								wants++;
								identification[$"#{suspects} ~r~Wanted #{wants}"] = s;
							});
						}

						if (pedData.Items.Count > 0)
                        {
							List<string> items = new List<string>();
							int itemCount = 0;
							foreach(KeyValuePair<string, bool> itm in pedData.Items)
                            {
								itemCount++;
								items.Add(itm.Value ? $"~r~{itm.Key}~w~" : $"~g~{itm.Key}~w~");
                            }
							string itmLst = string.Join(", ", items);
							identification[$"#{suspects} ~b~Items [{itemCount}]"] = itmLst;
						}
					}
					else
					{
						identification[$"#{suspects} ~o~Error"] = "No Name Entered";
					}

					identification[$"#{suspects} ~g~REQUEST END"] = "";
				}

				if (suspects == 0)
				{
					identification[$"~o~Error"] = "No Name Entered";
				}

				int registrations = 0;

				foreach (KeyValuePair<int, MissionDataVehicle> kvp in missionData.NetworkVehicles)
				{
					MissionDataVehicle vehData = kvp.Value;
					registrations++;
					registration[$"#{registrations} ~g~REQUEST"] = "";

					if (vehData.RecordedLicensePlate)
					{
						registration[$"#{registrations} ~b~Plate"] = vehData.LicensePlate;
						registration[$"#{registrations} ~b~Make"] = vehData.DisplayName;
						registration[$"#{registrations} ~b~Color"] = (vehData.PrimaryColor == vehData.SecondaryColor) ? vehData.PrimaryColor : $"{vehData.PrimaryColor} / {vehData.SecondaryColor}";
						registration[$"#{registrations} ~b~Registered To"] = vehData.OwnerName;
						registration[$"#{registrations} ~b~Insurance"] = vehData.InsuranceValid ? "~g~Valid" : "~r~Invalid";

						if (vehData.Stolen)
							registration[$"#{registrations} ~b~Stolen"] = "~r~Reported Stolen";

						if (vehData.Items.Count > 0)
						{
							List<string> items = new List<string>();
							int itemCount = 0;
							foreach (KeyValuePair<string, bool> itm in vehData.Items)
							{
								itemCount++;
								items.Add(itm.Value ? $"~r~{itm.Key}~w~" : $"~g~{itm.Key}~w~");
							}
							string itmLst = string.Join(", ", items);
							identification[$"#{suspects} ~b~Items [{itemCount}]"] = itmLst;
						}
					}
					else
					{
						registration[$"#{registrations} ~o~Error"] = "No License Plate Entered";
					}

					registration[$"#{registrations} ~g~REQUEST END"] = "";
				}

				if (registrations == 0)
				{
					registration[$"~o~Error"] = "No License Plate Entered";
				}
			}
			catch (Exception ex)
			{
				identification[$"~o~Error"] = "System Error";
				registration[$"~o~Error"] = "System Error";
			}

			tabIdentifcation.Dictionary = identification;
			tabRegistration.Dictionary = registration;
		}

        void NotOnCallout()
        {
			tabIdentifcation.Dictionary = new Dictionary<string, string>()
			{
				["~b~Currently not on a callout"] = ""
			};

			tabRegistration.Dictionary = new Dictionary<string, string>()
			{
				["~b~Currently not on a callout"] = ""
			};
		}
    }
}
