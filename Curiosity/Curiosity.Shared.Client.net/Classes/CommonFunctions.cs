using CitizenFX.Core;
using CitizenFX.Core.Native;
using System.Drawing;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Shared.Client.net.Classes
{
    static public class CommonFunctions
    {
        private static string _currentScenario = "";

        /// <summary>
        /// I SEE YOU DECOMPILED THIS... YES, THIS ALL CAME FROM vMENU AND IN WHICH I GIVE GREAT THANKS FOR IT
        /// </summary>
        /// <param name="scenarioName"></param>

        public static void PlayScenario(string scenarioName)
        {
            // If there's currently no scenario playing, or the current scenario is not the same as the new scenario, then..
            if (_currentScenario == "" || _currentScenario != scenarioName)
            {
                // Set the current scenario.
                _currentScenario = scenarioName;
                // Clear all tasks to make sure the player is ready to play the scenario.
                ClearPedTasks(Game.PlayerPed.Handle);

                var canPlay = true;
                // Check if the player CAN play a scenario... 
                if (IsPedRunning(Game.PlayerPed.Handle))
                {
                    BaseScript.TriggerEvent("curiosity:Client:Notification:LifeV", 1, "Scenario Runner", string.Empty, "You can't start a scenario when you are running.", 2);
                    canPlay = false;
                }
                if (IsEntityDead(Game.PlayerPed.Handle))
                {
                    BaseScript.TriggerEvent("curiosity:Client:Notification:LifeV", 1, "Scenario Runner", string.Empty, "You can't start a scenario when you are dead.", 2);
                    canPlay = false;
                }
                if (IsPlayerInCutscene(Game.PlayerPed.Handle))
                {
                    BaseScript.TriggerEvent("curiosity:Client:Notification:LifeV", 1, "Scenario Runner", string.Empty, "You can't start a scenario when you are in a cutscene.", 2);
                    canPlay = false;
                }
                if (IsPedFalling(Game.PlayerPed.Handle))
                {
                    BaseScript.TriggerEvent("curiosity:Client:Notification:LifeV", 1, "Scenario Runner", string.Empty, "You can't start a scenario when you are falling.", 2);
                    canPlay = false;
                }
                if (IsPedRagdoll(Game.PlayerPed.Handle))
                {
                    BaseScript.TriggerEvent("curiosity:Client:Notification:LifeV", 1, "Scenario Runner", string.Empty, "You can't start a scenario when you are currently in a ragdoll state.", 2);
                    canPlay = false;
                }
                if (!IsPedOnFoot(Game.PlayerPed.Handle))
                {
                    BaseScript.TriggerEvent("curiosity:Client:Notification:LifeV", 1, "Scenario Runner", string.Empty, "You must be on foot to start a scenario.", 2);
                    canPlay = false;
                }
                if (NetworkIsInSpectatorMode())
                {
                    BaseScript.TriggerEvent("curiosity:Client:Notification:LifeV", 1, "Scenario Runner", string.Empty, "You can't start a scenario when you are currently spectating.", 2);
                    canPlay = false;
                }
                if (GetEntitySpeed(Game.PlayerPed.Handle) > 5.0f)
                {
                    BaseScript.TriggerEvent("curiosity:Client:Notification:LifeV", 1, "Scenario Runner", string.Empty, "You can't start a scenario when you are moving too fast.", 2);
                    canPlay = false;
                }

                if (canPlay)
                {
                    Function.Call(Hash.SET_SCENARIO_TYPE_ENABLED, scenarioName);
                    Function.Call(Hash.RESET_SCENARIO_TYPES_ENABLED);

                    // If the scenario is a "sit" scenario, then the scenario needs to be played at a specific location.
                    if (Classes.Data.PedScenarios.PositionBasedScenarios.Contains(scenarioName))
                    {
                        // Get the offset-position from the player. (0.5m behind the player, and 0.5m below the player seems fine for most scenarios)
                        var pos = GetOffsetFromEntityInWorldCoords(Game.PlayerPed.Handle, 0f, -0.5f, -0.5f);
                        var heading = GetEntityHeading(Game.PlayerPed.Handle);
                        // Play the scenario at the specified location.
                        // TaskStartScenarioAtPosition(Game.PlayerPed.Handle, scenarioName, pos.X, pos.Y, pos.Z, heading, -1, true, false);
                        API.TaskStartScenarioAtPosition(Game.PlayerPed.Handle, scenarioName, pos.X, pos.Y, pos.Z, Game.PlayerPed.Heading, -10, true, false);
                    }
                    // If it's not a sit scenario (or maybe it is, but using the above native causes other
                    // issues for some sit scenarios so those are not registered as "sit" scenarios), then play it at the current player's position.
                    else
                    {
                        Function.Call(Hash.TASK_START_SCENARIO_IN_PLACE, Game.PlayerPed.Handle, scenarioName, 0, true);
                        // TaskStartScenarioInPlace(Game.PlayerPed.Handle, scenarioName, 0, true);
                    }

                    Debug.WriteLine($"{scenarioName}");
                }
            }
            // If the new scenario is the same as the currently playing one, cancel the current scenario.
            else
            {
                _currentScenario = "";
                ClearPedTasks(Game.PlayerPed.Handle);
                ClearPedSecondaryTask(Game.PlayerPed.Handle);
            }

            // If the scenario name to play is called "forcestop" then clear the current scenario and force any tasks to be cleared.
            if (scenarioName == "forcestop")
            {
                _currentScenario = "";
                ClearPedTasks(Game.PlayerPed.Handle);
                ClearPedTasksImmediately(Game.PlayerPed.Handle);
            }

        }

        static public void DrawImage3D(string txdName, Vector3 pos, float width, float height, float rot, Color color)
        {
            float screenX = 0f;
            float screenY = 0f;
            if (World3dToScreen2d(pos.X, pos.Y, pos.Z, ref screenX, ref screenY))
            {
                Vector3 gameCam = GetGameplayCamCoord();
                float distCamPos = GetDistanceBetweenCoords(gameCam.X, gameCam.Y, gameCam.Z, pos.X, pos.Y, pos.Z, true);

                width = (1 / distCamPos) * width;
                height = (1 / distCamPos) * height;
                float fov = (1 / GetGameplayCamFov()) * 100;

                width = width * fov;
                height = height * fov;

                Vector3 playerPos = Game.PlayerPed.Position;
                RaycastResult raycastResult = World.Raycast(playerPos, pos, -1, IntersectOptions.Everything, Game.PlayerPed);
                if (raycastResult.HitEntity == null)
                {
                    DrawSprite(txdName, txdName, screenX, screenY, width, height, rot, color.R, color.G, color.B, color.A);
                }
            }
        }


    }
}
