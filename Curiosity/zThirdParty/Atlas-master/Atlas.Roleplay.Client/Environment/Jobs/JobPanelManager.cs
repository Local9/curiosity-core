using Atlas.Roleplay.Client.Extensions;
using Atlas.Roleplay.Client.Managers;
using Atlas.Roleplay.Library.Events;
using Atlas.Roleplay.Library.Models;
using CitizenFX.Core.Native;
using System;
using System.Linq;

namespace Atlas.Roleplay.Client.Environment.Jobs
{
    public class JobPanelManager : Manager<JobPanelManager>
    {
        public override void Begin()
        {
            EventSystem.Attach("job:employee:hired", new AsyncEventCallback(async metadata =>
            {
                var seed = metadata.Find<string>(0);
                var character = CharacterManager.GetModule().AvailableCharacters.First(self => self.Seed == seed);

                if (Cache.Character.Seed == character.Seed)
                {
                    Cache.Player.ShowNotification($"Du har blivit anställd hos {metadata.Find<string>(2)}!");
                }

                Enum.TryParse<Employment>(metadata.Find<string>(1), out var employment);

                character.Metadata.Employment = employment;
                character.Metadata.EmploymentRole = 0;

                await character.Save();

                return null;
            }));

            EventSystem.Attach("job:employee:kicked", new AsyncEventCallback(async metadata =>
            {
                var seed = metadata.Find<string>(0);
                var character = CharacterManager.GetModule().AvailableCharacters.First(self => self.Seed == seed);

                if (Cache.Character.Seed == character.Seed)
                {
                    Cache.Player.ShowNotification("Du blev sparkad ifrån ditt jobb!");
                }

                character.Metadata.Employment = Employment.Unemployed;
                character.Metadata.EmploymentRole = 0;

                await character.Save();

                return null;
            }));

            EventSystem.Attach("job:employment:update", new AsyncEventCallback(async metadata =>
            {
                var seed = metadata.Find<string>(0);
                var character = CharacterManager.GetModule().AvailableCharacters.First(self => self.Seed == seed);

                character.Metadata.EmploymentRole = metadata.Find<int>(1);

                await character.Save();

                return null;
            }));

            Atlas.AttachNuiHandler("CLOSE_JOB_PANEL", new EventCallback(metadata =>
            {
                API.SetNuiFocus(false, false);

                return null;
            }));

            Atlas.AttachNuiHandler("KICK_EMPLOYEE", new AsyncEventCallback(async metadata =>
            {
                var atlas = AtlasPlugin.Instance;
                var character = await EventSystem.Request<AtlasCharacter>("characters:fetchbyseed", metadata.Find<string>(0));

                if (character == null) return null;

                var user = await EventSystem.Request<AtlasUser>("user:fetch", character.Owner);

                if (user != null)
                {
                    user.Send("job:employee:kicked", character.Seed);
                }
                else
                {
                    character.Metadata.Employment = Employment.Unemployed;
                    character.Metadata.EmploymentRole = 0;

                    await character.Save();
                }

                return null;
            }));

            Atlas.AttachNuiHandler("EMPLOYEE_UPDATE", new AsyncEventCallback(async metadata =>
            {
                var atlas = AtlasPlugin.Instance;
                var character = await EventSystem.Request<AtlasCharacter>("characters:fetchbyseed", metadata.Find<string>(0));

                if (character == null) return null;

                var user = await EventSystem.Request<AtlasUser>("user:fetch", character.Owner);

                if (user != null)
                {
                    user.Send("job:employment:update", character.Seed, metadata.Find<int>(1));
                }
                else
                {
                    character.Metadata.EmploymentRole = metadata.Find<int>(1);

                    await character.Save();
                }

                return null;
            }));

            Atlas.AttachNuiHandler("EMPLOYEE_HIRE", new AsyncEventCallback(async metadata =>
            {
                var atlas = AtlasPlugin.Instance;
                var character =
                    await EventSystem.Request<AtlasCharacter>("characters:fetchbyssn", metadata.Find<string>(0));

                if (character == null) return null;

                var user = await EventSystem.Request<AtlasUser>("user:fetch", character.Owner);

                if (user != null)
                {
                    user.Send("job:employee:hire", character.Seed, metadata.Find<string>(1), metadata.Find<string>(2));
                }
                else
                {
                    Enum.TryParse<Employment>(metadata.Find<string>(1), out var employment);

                    character.Metadata.Employment = employment;
                    character.Metadata.EmploymentRole = 0;

                    await character.Save();
                }

                return null;
            }));
        }
    }
}