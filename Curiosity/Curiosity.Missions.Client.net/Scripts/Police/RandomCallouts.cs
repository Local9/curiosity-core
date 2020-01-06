using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using Curiosity.Missions.Client.net.MissionPeds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Curiosity.Missions.Client.net.Scripts.Mission;
using Curiosity.Missions.Client.net.Classes.PlayerClient;
using Curiosity.Missions.Client.net.Scripts.PedCreators;
using Curiosity.Shared.Client.net.Extensions;

namespace Curiosity.Missions.Client.net.Scripts.Police
{
    class RandomCallouts
    {
        static Blip _location;

        static public void Init()
        {
            RegisterCommand("createfight", new Action<int, List<object>, string>(DevCreateFight), false);
        }

        static void DevCreateFight(int playerHandle, List<object> arguments, string raw)
        {
            if (!ClientInformation.IsDeveloper()) return;
            CreateFight(false);
        }

        static public async void CreateFight(bool developer = false)
        {
            if (_location != null)
            {
                if (_location.Exists())
                    _location.Delete();
            }

            Vector3 pos = Game.PlayerPed.Position;
            Vector3 outpos = new Vector3();
            if (GetNthClosestVehicleNode(pos.X, pos.Y, pos.Z, Client.Random.Next(200, 300), ref outpos, 0, 0, 0))
            {

                if (developer)
                    outpos = Game.PlayerPed.GetOffsetPosition(new Vector3(0f, 5f, 0f));

                Array pedHashes = Enum.GetValues(typeof(PedHash));
                Model model1 = (PedHash)pedHashes.GetValue(Client.Random.Next(pedHashes.Length));
                Model model2 = (PedHash)pedHashes.GetValue(Client.Random.Next(pedHashes.Length));
                
                Ped suspect1Ped = await PedCreator.CreatePedAtLocation(model1, outpos + new Vector3(0f, -5f, 0f), 0);
                Ped suspect2Ped = await PedCreator.CreatePedAtLocation(model2, outpos + new Vector3(0f, 5f, 0f), 180);

                SetPedCombatAbility(suspect1Ped.Handle, 100);
                SetPedCombatAbility(suspect2Ped.Handle, 100);

                SetPedCombatAttributes(suspect1Ped.Handle, 5, true);
                SetPedCombatAttributes(suspect2Ped.Handle, 5, true);

                SetEntityCanBeDamagedByRelationshipGroup(suspect1Ped.Handle, false, Static.Relationships.Fighter2Relationship.Hash);
                SetEntityCanBeDamagedByRelationshipGroup(suspect2Ped.Handle, false, Static.Relationships.Fighter1Relationship.Hash);

                TaskPutPedDirectlyIntoMelee(suspect1Ped.Handle, suspect2Ped.Handle, 0f, 0f, 0f, false);
                TaskPutPedDirectlyIntoMelee(suspect2Ped.Handle, suspect1Ped.Handle, 0f, 0f, 0f, false);

                Blip location = World.CreateBlip(outpos);
                location.ShowRoute = true;

                while (Game.PlayerPed.Position.Distance(location.Position) >= 50f)
                {
                    await BaseScript.Delay(100);
                }

                InteractivePed suspect1 = InteractivePedCreator.Ped(suspect1Ped, relationshipGroup: Static.Relationships.Fighter1Relationship);
                InteractivePed suspect2 = InteractivePedCreator.Ped(suspect2Ped, relationshipGroup: Static.Relationships.Fighter2Relationship);

                if (location.Exists())
                    location.Delete();
            }
        }
    }
}
