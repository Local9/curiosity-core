using Atlas.Roleplay.Client.Environment.Entities.Modules.Impl;
using Atlas.Roleplay.Client.Managers;
using Atlas.Roleplay.Library.Events;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using System.Threading.Tasks;

namespace Atlas.Roleplay.Client.Environment
{
    public class HandcuffManager : Manager<HandcuffManager>
    {
        public bool IsHandcuffed { get; set; }

        public override void Begin()
        {
            EventSystem.Attach("handcuff:toggle", new EventCallback(metadata =>
            {
                var state = metadata.Find<bool>(0);

                if (state == IsHandcuffed) return null;

                IsHandcuffed = state;

                if (IsHandcuffed) PutOn();
                else Remove();

                return null;
            }));

            EventSystem.Attach("handcuff:drag:toggle", new EventCallback(metadata =>
            {
                var ped = API.GetPlayerPed(-1);
                var source = API.GetPlayerPed(API.GetPlayerFromServerId(metadata.Find<int>(0)));

                if (!API.DoesEntityExist(source)) return null;

                if (API.IsEntityAttachedToEntity(ped, source))
                {
                    API.DetachEntity(ped, true, false);
                }
                else
                {
                    API.AttachEntityToEntity(ped, source, 11816, 0.54f, 0.54f, 0f, 0f, 0f, 0f, false, false, false, false, 2, true);
                }

                return null;
            }));
        }

        public void PutOn()
        {
            var ped = API.GetPlayerPed(-1);

            API.SetEnableHandcuffs(ped, true);
            API.SetCurrentPedWeapon(ped, (uint)API.GetHashKey("WEAPON_UNARMED"), true);
            API.DisablePlayerFiring(ped, true);

            var decors = new EntityDecorModule()
            {
                Id = ped
            };

            decors.Set("Player.IsHandcuffed", true);
        }

        public void Remove()
        {
            var ped = API.GetPlayerPed(-1);

            API.SetEnableHandcuffs(ped, false);
            API.DisablePlayerFiring(ped, false);
            API.ClearPedTasksImmediately(ped);

            var decors = new EntityDecorModule()
            {
                Id = ped
            };

            decors.Set("Player.IsHandcuffed", false);
        }

        [TickHandler(SessionWait = true)]
        private async Task OnTick()
        {
            if (IsHandcuffed)
            {
                var ped = API.GetPlayerPed(-1);

                if (API.DoesEntityExist(ped))
                {
                    if (!API.IsEntityPlayingAnim(ped, "mp_arresting", "idle", 3))
                    {
                        API.RequestAnimDict("mp_arresting");

                        while (!API.HasAnimDictLoaded("mp_arresting"))
                        {
                            await BaseScript.Delay(10);
                        }

                        API.TaskPlayAnim(ped, "mp_arresting", "idle", 8f, -8, -1,
                            (int)(AnimationFlags.Loop | AnimationFlags.UpperBodyOnly | AnimationFlags.AllowRotation),
                            0, false,
                            false,
                            false);
                    }
                }

                API.DisableControlAction(0, 1, true);
                API.DisableControlAction(0, 2, true);
                API.DisableControlAction(0, 24, true);
                API.DisableControlAction(0, 257, true);
                API.DisableControlAction(0, 25, true);
                API.DisableControlAction(0, 263, true);
                API.DisableControlAction(0, 59, true);
                API.DisableControlAction(0, 75, true);
                API.DisableControlAction(27, 75, true);

                await Task.FromResult(0);
            }
            else
            {
                await BaseScript.Delay(500);
            }
        }
    }
}