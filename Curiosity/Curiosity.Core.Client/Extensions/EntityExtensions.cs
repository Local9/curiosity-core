using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Client.Diagnostics;
using System;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Extensions
{
    public static class EntityExtensions
    {
        /// <summary>
        /// Sets the alpha of an entity.
        /// </summary>
        /// <param name="entity">The entity to change the alpha.</param>
        /// <param name="alpha">The alpha value to set.</param>
        public static void SetAlpha(this Entity entity, int alpha)
        {
            // If the alpha is 255, reset the alpha
            if (alpha == 255)
            {
                API.ResetEntityAlpha(entity.Handle);
            }
            // Otherwise, set it as usual
            else
            {
                API.SetEntityAlpha(entity.Handle, alpha, 0);
            }
        }

        public async static Task FadeOut(this Prop prop, bool slow = true)
        {
            await Fade(prop, false, slow);
        }

        public async static Task FadeIn(this Prop prop, bool slow = true)
        {
            await Fade(prop, true, slow);
        }

        public async static Task Fade(this Prop prop, bool fadeIn, bool fadeOutNormal = false, bool slow = true)
        {
            if (fadeIn)
            {
                API.NetworkFadeInEntity(prop.Handle, slow);
            }
            else
            {
                API.NetworkFadeOutEntity(prop.Handle, fadeOutNormal, slow);
            }

            while (API.NetworkIsEntityFading(prop.Handle))
            {
                await BaseScript.Delay(10);
            }
        }

        public async static Task FadeOut(this Entity entity, bool slow = true)
        {
            await Fade(entity, false, slow);
        }

        public async static Task FadeIn(this Entity entity, bool slow = true)
        {
            await Fade(entity, true, slow);
        }

        public async static Task Fade(this Entity entity, bool fadeIn, bool fadeOutNormal = false, bool slow = true)
        {
            if (fadeIn)
            {
                API.NetworkFadeInEntity(entity.Handle, slow);
            }
            else
            {
                API.NetworkFadeOutEntity(entity.Handle, fadeOutNormal, slow);
            }

            while (API.NetworkIsEntityFading(entity.Handle))
            {
                await BaseScript.Delay(10);
            }
        }

        public static Entity GetEntityInFront(this Entity entity, float distance = 5f, float radius = 1.5f)
        {
            try
            {
                RaycastResult raycast = World.RaycastCapsule(entity.Position, entity.GetOffsetPosition(new Vector3(0f, distance, 0f)), radius, (IntersectOptions)10, entity);
                if (raycast.DitHitEntity)
                {
                    return raycast.HitEntity;
                }
            }
            catch (Exception ex)
            {
                Logger.Debug($"GetEntityInFront -> {ex}");
            }
            return default(Vehicle);
        }

        public static async void Dispose(this Entity entity, bool detatch = false)
        {
            Logger.Debug($"Removing Entity from the World");
            if (entity == null) return;
            if (!entity.Exists()) return;

            int handle = entity.Handle;

            NetworkRequestControlOfEntity(handle);
            entity.IsCollisionEnabled = false;
            entity.Opacity = 0;

            int attempts = 0;

            while (!NetworkHasControlOfEntity(handle) && attempts < 100)
            {
                NetworkRequestControlOfEntity(handle);
                await BaseScript.Delay(10);
                attempts++;
            }

            if (detatch && entity.IsAttached()) DetachEntity(handle, false, false);
            SetEntityAsMissionEntity(handle, true, true);
            SetEntityAsNoLongerNeeded(ref handle);
            entity.Delete();

            if (entity.Exists()) entity.Position = new Vector3(0f, 0f, 0f);
        }
    }
}
