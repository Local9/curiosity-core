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

        #region PedTasks

        public static async Task TaskWalkInToCharacterCreationRoom(this Ped ped, string animationDict)
        {
            int sequence = 0;
            Vector3 pos = new Vector3(404.834f, -997.838f, -97.841f);
            OpenSequenceTask(ref sequence);
            TaskPlayAnimAdvanced(0, animationDict, "Intro", pos.X, pos.Y, pos.Z - 1f, 0.0f, 0.0f, -40.0f, 8.0f, -8.0f, -1, 4608, 0f, 2, 0);
            TaskPlayAnim(0, animationDict, "Loop", 8f, -4f, -1, 513, 0, false, false, false);
            CloseSequenceTask(sequence);
            TaskPerformSequence(ped.Handle, sequence);
            ClearSequenceTask(ref sequence);
            await BaseScript.Delay(2000);
            ped.IsPositionFrozen = true;
        }

        public static async Task TaskPlayOutroOfCharacterCreationRoom(this Ped ped, string an)
        {
            ped.IsPositionFrozen = false;
            int sequence = 0;
            OpenSequenceTask(ref sequence);
            TaskPlayAnim(0, an, "outro", 8.0f, -8.0f, -1, 512, 0, false, false, false);
            CloseSequenceTask(sequence);
            TaskPerformSequence(ped.Handle, sequence);
            ClearSequenceTask(ref sequence);
            await BaseScript.Delay(2500);
        }

        public static void TaskLookLeft(this Ped ped, string animationDict)
        {
            int sequence = 0;
            OpenSequenceTask(ref sequence);
            TaskPlayAnim(0, animationDict, "Profile_L_Intro", 4.0f, -4.0f, -1, 512, 0, false, false, false);
            TaskPlayAnim(0, animationDict, "Profile_L_Loop", 4.0f, -4.0f, -1, 513, 0, false, false, false);
            CloseSequenceTask(sequence);
            TaskPerformSequence(ped.Handle, sequence);
            ClearSequenceTask(ref sequence);
        }

        public static void TaskLookRight(this Ped ped, string animationDict)
        {
            int sequence = 0;
            OpenSequenceTask(ref sequence);
            TaskPlayAnim(0, animationDict, "Profile_R_Intro", 4.0f, -4.0f, -1, 512, 0, false, false, false);
            TaskPlayAnim(0, animationDict, "Profile_R_Loop", 4.0f, -4.0f, -1, 513, 0, false, false, false);
            CloseSequenceTask(sequence);
            TaskPerformSequence(ped.Handle, sequence);
            ClearSequenceTask(ref sequence);
        }

        public static void TaskStopLookingLeft(this Ped ped, string animationDict)
        {
            int sequence = 0;
            OpenSequenceTask(ref sequence);
            TaskPlayAnim(0, animationDict, "Profile_L_Outro", 4.0f, -4.0f, -1, 512, 0, false, false, false);
            TaskPlayAnim(0, animationDict, "Loop", 4.0f, -4.0f, -1, 513, 0, false, false, false);
            CloseSequenceTask(sequence);
            TaskPerformSequence(ped.Handle, sequence);
            ClearSequenceTask(ref sequence);
        }

        public static void TaskStopLookingRight(this Ped ped, string animationDict)
        {
            int sequence = 0;
            OpenSequenceTask(ref sequence);
            TaskPlayAnim(0, animationDict, "Profile_R_Outro", 4.0f, -4.0f, -1, 512, 0, false, false, false);
            TaskPlayAnim(0, animationDict, "Loop", 4.0f, -4.0f, -1, 513, 0, false, false, false);
            CloseSequenceTask(sequence);
            TaskPerformSequence(ped.Handle, sequence);
            ClearSequenceTask(ref sequence);
        }

        public static void TaskHoldMugshotBoard(this Ped ped, string animationDict)
        {
            int sequence = 0;
            OpenSequenceTask(ref sequence);
            TaskPlayAnim(0, animationDict, "react_light", 8.0f, -8.0f, -1, 512, 0, false, false, false);
            TaskPlayAnim(0, animationDict, "Loop", 8.0f, -8.0f, -1, 513, 0, false, false, false);
            CloseSequenceTask(sequence);
            TaskPerformSequence(ped.Handle, sequence);
            ClearSequenceTask(ref sequence);
        }

        public static void TaskRaiseMugshotBoard(this Ped ped, string animationDict)
        {
            int sequence = 0;
            OpenSequenceTask(ref sequence);
            TaskPlayAnim(0, animationDict, "low_to_high", 4.0f, -4.0f, -1, 512, 0, false, false, false);
            TaskPlayAnim(0, animationDict, "Loop_raised", 8.0f, -8.0f, -1, 513, 0, false, false, false);
            CloseSequenceTask(sequence);
            TaskPerformSequence(ped.Handle, sequence);
            ClearSequenceTask(ref sequence);
        }

        public static void TaskLowerMugshotBoard(this Ped ped, string animationDict)
        {
            int sequence = 0;
            OpenSequenceTask(ref sequence);
            TaskPlayAnim(0, animationDict, "high_to_low", 4.0f, -4.0f, -1, 512, 0, false, false, false);
            TaskPlayAnim(0, animationDict, "Loop", 8.0f, -8.0f, -1, 513, 0, false, false, false);
            CloseSequenceTask(sequence);
            TaskPerformSequence(ped.Handle, sequence);
            ClearSequenceTask(ref sequence);
        }

        public static void TaskCreatorClothes(this Ped ped, string animationDict)
        {
            int sequence = 0;
            OpenSequenceTask(ref sequence);
            TaskPlayAnim(0, animationDict, "DROP_INTRO", 8.0f, -8.0f, -1, 512, 0, false, false, false);
            TaskPlayAnim(0, animationDict, "DROP_LOOP", 4.0f, -4.0f, -1, 513, 0, false, false, false);
            CloseSequenceTask(sequence);
            TaskPerformSequence(ped.Handle, sequence);
            ClearSequenceTask(ref sequence);
        }

        public static void TaskEvidenceClothes(this Ped ped, string animationDict)
        {
            string anim = "";
            int a = GetRandomIntInRange(0, 2);

            switch (a)
            {
                case 0:
                    anim = "DROP_CLOTHES_A";

                    break;
                case 1:
                    anim = "DROP_CLOTHES_B";

                    break;
                case 2:
                    anim = "DROP_CLOTHES_C";

                    break;
            }

            int sequence = 0;
            OpenSequenceTask(ref sequence);
            TaskPlayAnim(0, animationDict, anim, 8.0f, -8.0f, -1, 512, 0, false, false, false);
            TaskPlayAnim(0, animationDict, "DROP_LOOP", 8.0f, -8.0f, -1, 513, 0, false, false, false);
            CloseSequenceTask(sequence);
            TaskPerformSequence(ped.Handle, sequence);
            ClearSequenceTask(ref sequence);
        }

        public static void TaskClothesALoop(this Ped ped, string animationDict)
        {
            int sequence = 0;
            OpenSequenceTask(ref sequence);
            TaskPlayAnim(0, animationDict, "DROP_OUTRO", 8.0f, -8.0f, -1, 512, 0, false, false, false);
            TaskPlayAnim(0, animationDict, "Loop", 8.0f, -8.0f, -1, 513, 0, false, false, false);
            CloseSequenceTask(sequence);
            TaskPerformSequence(ped.Handle, sequence);
            ClearSequenceTask(ref sequence);
        }

        #endregion
    }
}
