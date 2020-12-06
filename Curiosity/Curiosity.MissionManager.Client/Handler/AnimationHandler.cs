using CitizenFX.Core;
using Curiosity.MissionManager.Client.Environment.Entities.Models;
using System.Threading.Tasks;

namespace Curiosity.MissionManager.Client.Handler
{
    public static class AnimationHandler
    {
        static bool animationPlaying = false;

        public static async Task AnimationSearch()
        {
            if (animationPlaying) return;
            animationPlaying = true;

            AnimationBuilder animationBuilder1 = new AnimationBuilder().Select("PROP_HUMAN_BUM_BIN");
            Cache.Entity.AnimationQueue.PlayDirectInQueue(animationBuilder1);
            await BaseScript.Delay(3000);
            Cache.Entity.Task.ClearAll();

            animationPlaying = false;
        }

        public static async Task AnimationClipboard()
        {
            if (animationPlaying) return;
            animationPlaying = true;

            AnimationBuilder animationBuilder1 = new AnimationBuilder().Select("WORLD_HUMAN_CLIPBOARD");
            Cache.Entity.AnimationQueue.PlayDirectInQueue(animationBuilder1);
            await BaseScript.Delay(3000);
            Cache.Entity.Task.ClearAll();
            animationPlaying = false;
        }

        public static void AnimationRadio()
        {
            if (animationPlaying) return;
            animationPlaying = true;

            AnimationBuilder animationBuilder1 = new AnimationBuilder().Select("random@arrests", "generic_radio_enter");
            AnimationBuilder animationBuilder2 = new AnimationBuilder().Select("random@arrests", "generic_radio_chatter");
            AnimationBuilder animationBuilder3 = new AnimationBuilder().Select("random@arrests", "generic_radio_exit");

            Cache.Entity.AnimationQueue.AddToQueue(animationBuilder1);
            Cache.Entity.AnimationQueue.AddToQueue(animationBuilder2);
            Cache.Entity.AnimationQueue.AddToQueue(animationBuilder3);

            Cache.Entity.AnimationQueue.PlayQueue();

            animationPlaying = false;
        }

        public async static Task AnimationTicket(this Ped officer, Ped pedBeingTicketed = null)
        {
            if (animationPlaying) return;
            animationPlaying = true;

            officer.Task.PlayAnimation("veh@busted_std", "issue_ticket_cop", 8f, 9000, AnimationFlags.UpperBodyOnly);
            
            if (pedBeingTicketed != null)
                pedBeingTicketed.Task.PlayAnimation("veh@busted_std", "issue_ticket_crim", 8f, 9000, AnimationFlags.UpperBodyOnly);

            await BaseScript.Delay(9000);

            Cache.Entity.Task.ClearAll();

            animationPlaying = false;
        }
    }
}
