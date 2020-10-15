using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared.net;
using Curiosity.Global.Shared.net.Entity;
using Curiosity.Missions.Client.net.Wrappers;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Curiosity.Missions.Client.net.Scripts.PedCreators
{
    class PedCreator
    {
        static public async Task<Ped> CreatePedAtLocation(Model model, Vector3 location, float heading, bool dropsWeaponsOnDeath = false)
        {
            API.RequestCollisionAtCoord(location.X, location.Y, location.Z);
            API.ClearAreaOfEverything(location.X, location.Y, location.Z, 1f, true, true, true, true);

            float posZ = location.Z;

            if (API.GetGroundZFor_3dCoord(location.X, location.Y, location.Z, ref posZ, false))
                location.Z = posZ;

            Ped spawnedPed = await World.CreatePed(model, location, heading);
            API.NetworkFadeInEntity(spawnedPed.Handle, true);

            API.SetPedFleeAttributes(spawnedPed.Handle, 0, false);
            spawnedPed.DropsWeaponsOnDeath = dropsWeaponsOnDeath;
            spawnedPed.IsPersistent = true;

            EntityEventWrapper entityEventWrapper = new EntityEventWrapper(spawnedPed);
            entityEventWrapper.Died += new EntityEventWrapper.OnDeathEvent(EventWrapperOnDied);
            entityEventWrapper.Disposed += new EntityEventWrapper.OnWrapperDisposedEvent(EventWrapperOnDisposed);

            return spawnedPed;
        }

        static private void EventWrapperOnDisposed(EntityEventWrapper sender, Entity entity)
        {

        }

        static private void EventWrapperOnDied(EntityEventWrapper sender, Entity entity)
        {
            Ped ped = entity as Ped;
            Ped killerPed = ped.GetKiller() as Ped;

            if (killerPed != null)
            {
                if (killerPed.IsPlayer)
                {
                    if (ped.GetRelationshipWithPed(killerPed) != Relationship.Hate)
                    {
                        Player p = new Player(API.NetworkGetPlayerIndexFromPed(killerPed.Handle));

                        SkillMessage skillMessage = new SkillMessage();
                        skillMessage.PlayerHandle = $"{p.ServerId}";
                        skillMessage.MissionPed = false;
                        skillMessage.Increase = false;

                        string json = JsonConvert.SerializeObject(skillMessage);

                        BaseScript.TriggerServerEvent("curiosity:Server:Missions:KilledPed", Encode.StringToBase64(json));
                    }
                }
            }

            Blip currentBlip = entity.AttachedBlip;
            if (currentBlip != null)
            {
                currentBlip.Delete();
            }
            entity.MarkAsNoLongerNeeded();
            sender.Dispose();
        }
    }
}
