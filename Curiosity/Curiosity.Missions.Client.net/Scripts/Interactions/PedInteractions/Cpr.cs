using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Missions.Client.net.MissionPeds;
using System;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Missions.Client.net.Scripts.Interactions.PedInteractions
{
    class Cpr
    {
        static Client client = Client.GetInstance();

        private static TaskSequence _introAndIdleMedic;
        private static TaskSequence _introAndIdleVictim;
        private static TaskSequence _pumpAndIdleMedic;
        private static TaskSequence _pumpAndSuccessMedic;
        private static TaskSequence _pumpAndSuccessVictim;
        private static TaskSequence _failMedic;
        private static TaskSequence _failVictim;

        private static bool _performingCpr;
        private static bool _hasSucceeded;
        private static bool _cprFinished;
        private static bool _loaded = false;

        private static int _seconds;
        private static int _cprStart;
        private static int _chance;
        private static int _cprEnd;

        private static DateTime _lastTime = DateTime.Now;

        private static Ped _victim;

        public static void Init()
        {
            client.DeregisterTickHandler(OnTask);

            client.RegisterTickHandler(OnTask);

            if (Classes.PlayerClient.ClientInformation.IsDeveloper())
            {
                Debug.WriteLine("Initiated CPR -> Init");
                //var l = -6539555060529535670L;
                //Debug.WriteLine($"{l.ToString("X")}");
            }
        }
        public static void Dispose()
        {
            client.DeregisterTickHandler(OnTask);

            Client.TriggerEvent("curiosity:interaction:cpr", _victim.Handle, false);

            if (!_hasSucceeded)
            {
                Client.TriggerEvent("curiosity:interaction:cpr:failed", _victim.Handle);
            }

            if (Classes.PlayerClient.ClientInformation.IsDeveloper())
            {
                Debug.WriteLine("Initiated CPR -> Dispose");
            }

            _cprStart = _seconds;
            NpcHandler.IsPerformingCpr = false;
            _cprFinished = false;
            _hasSucceeded = false;
            _chance = _seconds - _cprStart + 4;
        }

        public static void InteractionCPR(InteractivePed interactivePed)
        {
            Client.TriggerEvent("curiosity:interaction:cpr", interactivePed.Handle, true);

            _chance = _seconds - _cprStart + 4;
            // START
            _victim = interactivePed;
            NpcHandler.IsPerformingCpr = true;
            Game.PlayerPed.SetNoCollision(interactivePed, false);
            interactivePed.SetNoCollision(Game.PlayerPed, false);
            Function.Call((Hash)8195582117541601333L, new InputArgument[] { _victim.Handle });
            _victim.Health = 200;
            Vector3 vector3 = new Vector3((float)Math.Cos((double)Game.PlayerPed.Heading * 0.0174532923847437 + 70), (float)Math.Sin((double)Game.PlayerPed.Heading * 0.0174532923847437 + 70), 0f);
            Game.PlayerPed.Task.ClearAll();
            _victim.Task.ClearAllImmediately();
            _victim.Position = (((Game.PlayerPed.Position + (Game.PlayerPed.ForwardVector * 1.1f)) + (vector3 * -0.1f)) - new Vector3(0f, 0f, 1f));
            _victim.Heading = (Game.PlayerPed.Heading + 75f);
            _victim.IsPositionFrozen = true;
            Game.PlayerPed.Task.PerformSequence(_introAndIdleMedic);
            _victim.Task.PerformSequence(_introAndIdleVictim);

            if (Classes.PlayerClient.ClientInformation.IsDeveloper())
            {
                Debug.WriteLine("Initiated CPR");
            }
        }

        static async Task OnTask()
        {
            if (!_loaded)
            {
                _pumpAndIdleMedic = new TaskSequence();
                await _pumpAndIdleMedic.AddTask.PlayAnimation("mini@cpr@char_a@cpr_str", "cpr_pumpchest", 8f, -8f, 1000, AnimationFlags.Loop, 8f);
                await _pumpAndIdleMedic.AddTask.PlayAnimation("mini@cpr@char_a@cpr_def", "cpr_pumpchest_idle", 8f, -8f, 100000, AnimationFlags.Loop, 8f);
                _pumpAndIdleMedic.Close();
                _pumpAndSuccessMedic = new TaskSequence();
                await _pumpAndSuccessMedic.AddTask.PlayAnimation("mini@cpr@char_a@cpr_str", "cpr_pumpchest", 8f, -8f, 1000, AnimationFlags.Loop, 8f);
                await _pumpAndSuccessMedic.AddTask.PlayAnimation("mini@cpr@char_a@cpr_str", "cpr_success", 8f, -8f, 28000, AnimationFlags.Loop, 8f);
                _pumpAndSuccessMedic.Close();
                _pumpAndSuccessVictim = new TaskSequence();
                await _pumpAndSuccessVictim.AddTask.PlayAnimation("mini@cpr@char_b@cpr_str", "cpr_pumpchest", 8f, -8f, 1000, AnimationFlags.Loop, 8f);
                await _pumpAndSuccessVictim.AddTask.PlayAnimation("mini@cpr@char_b@cpr_str", "cpr_success", 8f, -8f, 28000, AnimationFlags.Loop, 8f);
                _pumpAndSuccessVictim.Close();
                _failMedic = new TaskSequence();
                await _failMedic.AddTask.PlayAnimation("mini@cpr@char_a@cpr_str", "cpr_fail", 8f, -8f, 20000, AnimationFlags.Loop, 8f);
                _failMedic.Close();
                _failVictim = new TaskSequence();
                await _failVictim.AddTask.PlayAnimation("mini@cpr@char_b@cpr_str", "cpr_fail", 8f, -8f, 20000, AnimationFlags.Loop, 8f);
                _failVictim.Close();
                _introAndIdleMedic = new TaskSequence();
                await _introAndIdleMedic.AddTask.PlayAnimation("mini@cpr@char_a@cpr_def", "cpr_intro", 8f, -8f, 15000, AnimationFlags.Loop, 8f);
                await _introAndIdleMedic.AddTask.PlayAnimation("mini@cpr@char_a@cpr_def", "cpr_pumpchest_idle", 8f, -8f, 100000, AnimationFlags.Loop, 8f);
                _introAndIdleMedic.Close();
                _introAndIdleVictim = new TaskSequence();
                await _introAndIdleVictim.AddTask.PlayAnimation("mini@cpr@char_b@cpr_def", "cpr_intro", 8f, -8f, 15000, AnimationFlags.Loop, 8f);
                await _introAndIdleVictim.AddTask.PlayAnimation("mini@cpr@char_b@cpr_def", "cpr_pumpchest_idle", 8f, -8f, 100000, AnimationFlags.Loop, 8f);
                _introAndIdleVictim.Close();
                _loaded = true;
            }

            if (!HasAnimDictLoaded("mini@cpr@char_a@cpr_str"))
                RequestAnimDict("mini@cpr@char_a@cpr_str");

            if (!HasAnimDictLoaded("mini@cpr@char_b@cpr_str"))
                RequestAnimDict("mini@cpr@char_b@cpr_str");

            if (!HasAnimDictLoaded("mini@cpr@char_a@cpr_def"))
                RequestAnimDict("mini@cpr@char_a@cpr_def");

            if (!HasAnimDictLoaded("mini@cpr@char_b@cpr_def"))
                RequestAnimDict("mini@cpr@char_b@cpr_def");

            if (_lastTime.Second != DateTime.Now.Second)
            {
                _seconds++;
                _lastTime = DateTime.Now;
            }

            if (!_hasSucceeded && !_cprFinished)
            {
                string helpText = $"You have got {60 - (_seconds - _cprStart)} seconds to try to revive the person before they die. Your chances of revival are ";
                helpText += ((1f / (float)_chance * 100f) <= 1 ? "very slim" : string.Concat((1f / (float)_chance * 100f), "%"));
                helpText += ".~n~Press ~INPUT_JUMP~ to CPR the person.~n~Press ~INPUT_FRONTEND_CANCEL~ to leave.";
                // Start CPR
                Screen.DisplayHelpTextThisFrame(helpText);
            }

            // CANCEL
            if (Game.IsControlJustReleased(0, Control.FrontendCancel) && !_hasSucceeded && !_cprFinished)
            {
                NpcHandler.IsPerformingCpr = false;
                Game.PlayerPed.SetNoCollision(_victim, true);
                _victim.SetNoCollision(Game.PlayerPed, true);
                _hasSucceeded = false;
                _victim.IsPositionFrozen = false;
                _victim.Kill();
                Game.PlayerPed.Task.ClearAll();
                _victim.Task.ClearAll();
                _cprEnd = _seconds;
                Dispose();
                return;
            }

            // PERFORM CPR
            if (Game.PlayerPed.TaskSequenceProgress != -1 || !_cprFinished)
            {
                if (Game.IsControlJustPressed(0, Control.Jump) && !_hasSucceeded && Game.PlayerPed.TaskSequenceProgress == 1 && !_cprFinished)
                {
                    _hasSucceeded = Client.Random.Next(_chance) == 0;
                    if (!_hasSucceeded)
                    {
                        Game.PlayerPed.Task.PerformSequence(_pumpAndIdleMedic);
                    }
                    else if (_hasSucceeded)
                    {
                        Game.PlayerPed.Task.PerformSequence(_pumpAndSuccessMedic);
                        _victim.Task.PerformSequence(_pumpAndSuccessVictim);
                        _cprFinished = true;
                    }
                }
                if (_seconds - _cprStart > 60 && !_cprFinished)
                {
                    _victim.IsPositionFrozen = false;
                    _victim.Kill();
                    _cprFinished = true;
                    Game.PlayerPed.Task.PerformSequence(_failMedic);
                    _victim.Task.PerformSequence(_failVictim);
                    await Client.Delay(1000);
                    Dispose();
                }
                return;
            }

            NpcHandler.IsPerformingCpr = false;
            Game.PlayerPed.SetNoCollision(_victim, true);
            _victim.SetNoCollision(Game.PlayerPed, true);

            if (!_hasSucceeded)
            {
                Screen.ShowNotification("~r~The person has died.");
            }
            else
            {
                _victim.Resurrect();
            }
            _hasSucceeded = false;
            _victim.IsPositionFrozen = false;
            _cprEnd = _seconds;

            Dispose();
        }
    }
}
