using Curiosity.Framework.Shared;

namespace Curiosity.Framework.Client.Extensions
{
    internal static class PedExtensions
    {
        public static void SetDefaultComponentVariation(this Ped ped) => SetPedDefaultComponentVariation(ped.Handle);
        public static void SetComponentVariation(this Ped ped, int componentId, int drawableId, int textureId, int paletteId) => SetPedComponentVariation(ped.Handle, componentId, drawableId, textureId, paletteId);

        public static void SetDefaultVariation(this Ped ped)
        {
            ped.SetDefaultComponentVariation();
            if (ped.Gender == Gender.Male)
            {
                ped.SetComponentVariation(3, 15, 0, 2);
                ped.SetComponentVariation(4, 21, 0, 2);
                ped.SetComponentVariation(6, 34, 0, 2);
                ped.SetComponentVariation(8, 15, 0, 2);
                ped.SetComponentVariation(11, 15, 0, 2);
                return;
            }

            ped.SetComponentVariation(3, 15, 0, 2);
            ped.SetComponentVariation(4, 10, 0, 2);
            ped.SetComponentVariation(6, 35, 0, 2);
            ped.SetComponentVariation(8, 15, 0, 2);
            ped.SetComponentVariation(11, 15, 0, 2);
        }

        public static void SetRandomFacialMood(this Ped ped)
        {
            string facialIdleAnim = GetRandomMood(Common.RANDOM.Next(9));
            if (facialIdleAnim == "mood_smug_1")
                facialIdleAnim = "mood_Happy_1";
            if (facialIdleAnim == "mood_sulk_1")
                facialIdleAnim = "mood_Angry_1";
            if (!ped.IsInjured)
                SetFacialIdleAnimOverride(ped.Handle, facialIdleAnim, "0");
        }

        public static string GetRandomMood(int a_0)
        {
            switch (a_0)
            {
                case 0:
                    return "mood_Aiming_1";
                case 1:
                    return "mood_Angry_1";
                case 2:
                    return "mood_Happy_1";
                case 3:
                    return "mood_Injured_1";
                case 4:
                    return "mood_Normal_1";
                case 5:
                    return "mood_stressed_1";
                case 6:
                    return "mood_smug_1";
                case 7:
                    return "mood_sulk_1";
            }

            return "mood_Normal_1";
        }
    }
}
