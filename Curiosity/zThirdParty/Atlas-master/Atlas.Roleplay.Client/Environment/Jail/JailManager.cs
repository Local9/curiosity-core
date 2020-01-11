using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using Atlas.Roleplay.Client.Environment.Jobs;
using Atlas.Roleplay.Client.Environment.Jobs.Police;
using Atlas.Roleplay.Client.Extensions;
using Atlas.Roleplay.Client.Interface;
using Atlas.Roleplay.Client.Interface.Impl;
using Atlas.Roleplay.Client.Managers;
using Atlas.Roleplay.Library.LawEnforcement;
using Atlas.Roleplay.Library.Models;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Atlas.Roleplay.Client.Environment.Jail
{
    public class JailManager : Manager<JailManager>
    {
        public int ZoneBlip { get; set; }
        public JailCase CaseBuilder { get; set; }

        public Dictionary<JailSecurity, Position> Positions { get; } = new Dictionary<JailSecurity, Position>
        {
            [JailSecurity.Maxiumum] = new Position(1642.683f, 2573.736f, 45.56486f, 175.6137f),
            [JailSecurity.Middle] = new Position(),
            [JailSecurity.Minimum] = new Position(1774.496f, 2552.372f, 45.76497f, 87.74078f)
        };

        public Position ReleasePosition { get; } = new Position(1849.07f, 2600.969f, 45.61405f, 265.0127f);
        public Position JailPosition { get; } = new Position();
        public JailCase LastJailCase { get; set; }
        
        public override async void Begin()
        {
            await Session.Loading();

            var character = Cache.Character;

            if (character.Metadata.ActiveJailCase == null) return;

            SetActive(character.Metadata.ActiveJailCase);

            Atlas.AttachTickHandler(OnTick);
            
            var marker = new Marker(JailPosition)
            {
                Message = "Tryck ~INPUT_CONTEXT~ för att fängsla personen.",
                Scale = 3f,
                Condition = self => character.Metadata.Employment == Employment.Police
            };

            marker.Callback += OpenJailMenu;
            marker.Show();
        }

        public void OpenJailMenu()
        {
            var job = JobManager.GetModule().GetJob<PoliceJob>();

            CaseBuilder = new JailCase();
            
            new Menu($"{job.Label} | Fängsla person")
            {
               Items = new List<MenuItem>
               {
                   new MenuItem("security", "Säkerhet")
                   {
                       Profile = new MenuProfileSlider
                       {
                           Translations = CaseBuilder.GetSecurityLevels()
                       }
                   },
                   new MenuItem("pursue_case", "Fänsla")
               },
               Callback = (menu, item, operation) =>
               {
                   if (operation.Type != MenuOperationType.Select) return;
               }
            }.Commit();
        }

        public async void Jail(JailCase jailCase)
        {
            API.DoScreenFadeOut(0);

            var character = Cache.Character;

            Chat.SendGlobalMessage("Fängelse",
                $"{character.Fullname} Har blivit fängslad i {jailCase.GetDate(jailCase.IssuedAt)} med {jailCase.GetSecurity().ToLower()} säkerhet:",
                Color.FromArgb(255, 0, 0));
            Chat.SendGlobalMessage($"- {jailCase.Crime}", Color.FromArgb(255, 255, 255));

            await Cache.Entity.Teleport(Positions[jailCase.JailSecurity]);

            SetActive(jailCase);

            jailCase.Commit(character);
            LastJailCase = jailCase;

            Atlas.AttachTickHandler(OnTick);

            await BaseScript.Delay(3000);

            API.DoScreenFadeIn(5000);
        }

        public async void Unjail(JailCase jailCase)
        {
            Atlas.DetachTickHandler(OnTick);

            if (jailCase.HasEscaped) return;

            API.DoScreenFadeOut(0);

            await Cache.Entity.Teleport(ReleasePosition);
            await BaseScript.Delay(3000);

            API.DoScreenFadeIn(5000);

            LastJailCase = null;
        }

        public void SetActive(JailCase jailCase)
        {
            var position = Positions[jailCase.JailSecurity].AsVector();
            var temp = ZoneBlip;

            API.RemoveBlip(ref temp);

            var blip = API.AddBlipForRadius(position.X, position.Y, position.Z, 300f);

            API.SetBlipColour(blip, 6);
            API.SetBlipAlpha(blip, 75);

            ZoneBlip = blip;
        }

        public void SetInactive(JailCase jailCase)
        {
            var temp = ZoneBlip;

            API.RemoveBlip(ref temp);

            ZoneBlip = 0;
        }

        private async Task OnTick()
        {
            var character = Cache.Character;
            var jailCase = character.Metadata.ActiveJailCase ?? LastJailCase;

            if (jailCase != null)
            {
                if (!jailCase.IsActive)
                {
                    Unjail(jailCase);
                }
                else
                {
                    if (Cache.Entity.Position.Distance(Positions[jailCase.JailSecurity]) >
                        300f)
                    {
                        SetInactive(jailCase);

                        jailCase.HasEscaped = true;
                        jailCase.Commit(character);

                        Cache.Player.ShowNotification(
                            "Du har lyckats rymma ifrån fängelset, dra här ifrån innan du blir hittad!");
                    }
                    else
                    {
                        SetActive(jailCase);

                        Cache.Player.ShowNotification(
                            jailCase.HasEscaped
                                ? $"Du blev hittad utav vakterna/poliserna och kommer nu sitta i {jailCase.GetDate()}"
                                : $"Vakter: Du har {jailCase.GetDate()} kvar i fängelset!");

                        jailCase.HasEscaped = false;
                        jailCase.Commit(character);
                    }
                }
            }

            await BaseScript.Delay(30000);
        }
    }
}