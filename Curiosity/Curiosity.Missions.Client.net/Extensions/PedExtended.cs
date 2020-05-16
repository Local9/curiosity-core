using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Missions.Client.net.Wrappers;
using System;

namespace Curiosity.Missions.Client.net.Extensions
{
    static class PedExtended
    {
        internal readonly static string[] SpeechModifierNames;

        static PedExtended()
        {
            PedExtended.SpeechModifierNames = new string[] { "SPEECH_PARAMS_STANDARD", "SPEECH_PARAMS_ALLOW_REPEAT", "SPEECH_PARAMS_BEAT", "SPEECH_PARAMS_FORCE", "SPEECH_PARAMS_FORCE_FRONTEND", "SPEECH_PARAMS_FORCE_NO_REPEAT_FRONTEND", "SPEECH_PARAMS_FORCE_NORMAL", "SPEECH_PARAMS_FORCE_NORMAL_CLEAR", "SPEECH_PARAMS_FORCE_NORMAL_CRITICAL", "SPEECH_PARAMS_FORCE_SHOUTED", "SPEECH_PARAMS_FORCE_SHOUTED_CLEAR", "SPEECH_PARAMS_FORCE_SHOUTED_CRITICAL", "SPEECH_PARAMS_FORCE_PRELOAD_ONLY", "SPEECH_PARAMS_MEGAPHONE", "SPEECH_PARAMS_HELI", "SPEECH_PARAMS_FORCE_MEGAPHONE", "SPEECH_PARAMS_FORCE_HELI", "SPEECH_PARAMS_INTERRUPT", "SPEECH_PARAMS_INTERRUPT_SHOUTED", "SPEECH_PARAMS_INTERRUPT_SHOUTED_CLEAR", "SPEECH_PARAMS_INTERRUPT_SHOUTED_CRITICAL", "SPEECH_PARAMS_INTERRUPT_NO_FORCE", "SPEECH_PARAMS_INTERRUPT_FRONTEND", "SPEECH_PARAMS_INTERRUPT_NO_FORCE_FRONTEND", "SPEECH_PARAMS_ADD_BLIP", "SPEECH_PARAMS_ADD_BLIP_ALLOW_REPEAT", "SPEECH_PARAMS_ADD_BLIP_FORCE", "SPEECH_PARAMS_ADD_BLIP_SHOUTED", "SPEECH_PARAMS_ADD_BLIP_SHOUTED_FORCE", "SPEECH_PARAMS_ADD_BLIP_INTERRUPT", "SPEECH_PARAMS_ADD_BLIP_INTERRUPT_FORCE", "SPEECH_PARAMS_FORCE_PRELOAD_ONLY_SHOUTED", "SPEECH_PARAMS_FORCE_PRELOAD_ONLY_SHOUTED_CLEAR", "SPEECH_PARAMS_FORCE_PRELOAD_ONLY_SHOUTED_CRITICAL", "SPEECH_PARAMS_SHOUTED", "SPEECH_PARAMS_SHOUTED_CLEAR", "SPEECH_PARAMS_SHOUTED_CRITICAL" };
        }

        public static void ApplyDamagePack(this Ped ped, float damage, float multiplier, DamagePack damagePack)
        {
            Function.Call((Hash)5106960513763051839L, new InputArgument[] { ped.Handle, damagePack.ToString(), damage, multiplier });
        }

        public static bool CanHearPlayer(this Ped ped, Player player)
        {
            return API.CanPedHearPlayer(player.Handle, ped.Handle);
        }

        public static bool IsHandsUp(this Ped ped)
        {
            return API.GetIsTaskActive(ped.Handle, 0);
        }

        public static void ClearFleeAttributes(this Ped ped)
        {
            Function.Call((Hash)8116279360099375049L, new InputArgument[] { ped.Handle, 0, 0 });
        }

        public static void DisablePainAudio(this Ped ped, bool toggle)
        {
            API.DisablePedPainAudio(ped.Handle, toggle);
        }

        public static int GetDrawableVariation(this Ped ped, ComponentId id)
        {
            return Function.Call<int>((Hash)7490462606036423932L, new InputArgument[] { ped.Handle, id });
        }

        public static int GetNumberOfDrawableVariations(this Ped ped, ComponentId id)
        {
            return Function.Call<int>((Hash)2834476523764480066L, new InputArgument[] { ped.Handle, id });
        }

        public static bool GetStealthMovement(this Ped ped)
        {
            return Function.Call<bool>((Hash)8947185480862490559L, new InputArgument[] { ped.Handle });
        }

        public static bool HasBeenDamagedBy(this Ped ped, WeaponHash weapon)
        {
            return Function.Call<bool>((Hash)1377327512274689684L, new InputArgument[] { ped.Handle, weapon, 0 });
        }

        public static bool HasBeenDamagedByMelee(this Ped ped)
        {
            return Function.Call<bool>((Hash)1377327512274689684L, new InputArgument[] { ped.Handle, 0, 1 });
        }

        public static bool IsAmbientSpeechPlaying(this Ped ped)
        {
            return API.IsAmbientSpeechPlaying(ped.Handle);
        }

        public static bool IsCurrentWeaponSileced(this Ped ped)
        {
            return Function.Call<bool>((Hash)7345588343449861831L, new InputArgument[] { ped.Handle });
        }

        public static bool IsDriving(this Ped ped)
        {
            return (ped.IsSubttaskActive(Subtask.DrivingWandering) ? true : ped.IsSubttaskActive(Subtask.DrivingGoingToDestinationOrEscorting));
        }

        public static bool IsSubttaskActive(this Ped ped, Subtask task)
        {
            return API.GetIsTaskActive(ped.Handle, (int)task);
            //return Function.Call<bool>((Hash)-5731389963444272811L, new InputArgument[] { ped, task });
        }

        public static bool IsUsingAnyScenario(this Ped ped)
        {
            return Function.Call<bool>((Hash)6317224474499895619L, new InputArgument[] { ped.Handle });
        }

        public static void Jump(this Ped ped)
        {
            Function.Call((Hash)784761447855974321L, new InputArgument[] { ped.Handle, true, 0, 0 });
        }

        public static Bone LastDamagedBone(this Ped ped)
        {
            int num = 0;
            Bone bone;
            API.GetPedLastDamageBone(ped.Handle, ref num);
            bone = (Bone)num;
            return bone;
        }

        public static void PlayAmbientSpeech(this Ped ped, string speechName, SpeechModifier modifier = 0)
        {
            if ((modifier < SpeechModifier.Standard ? true : (int)modifier >= (int)PedExtended.SpeechModifierNames.Length))
            {
                throw new ArgumentOutOfRangeException("modifier");
            }

            Function.Call(unchecked((Hash)(-8213159594590722974L)), new InputArgument[] { ped.Handle, speechName, PedExtended.SpeechModifierNames[(int)modifier] });
        }

        public static void PlayFacialAnim(this Ped ped, string animSet, string animName)
        {
            API.PlayFacialAnim(ped.Handle, animSet, animName);
        }

        public static void PlayPain(this Ped ped, int type)
        {
            API.PlayPain(ped.Handle, type, 0);

            //Function.Call((Hash)-4856321419903345428L, new InputArgument[] { ped.Handle, type, 0, 0 });
        }

        public static void Cuff(this Ped ped, bool cuff = false)
        {
            API.SetEnableHandcuffs(ped.Handle, cuff);
        }

        public static void Recruit(this Ped ped, Ped leader, bool canBeTargeted, bool invincible, int accuracy)
        {
            if (leader != null)
            {
                ped.LeaveGroup();
                ped.SetRagdollOnCollision(false);
                ped.Task.ClearAll();
                PedGroup currentPedGroup = leader.PedGroup;
                currentPedGroup.SeparationRange = 2.14748365E+09f;
                if (!currentPedGroup.Contains(leader))
                {
                    currentPedGroup.Add(leader, true);
                }
                if (!currentPedGroup.Contains(ped))
                {
                    currentPedGroup.Add(ped, false);
                }
                ped.CanBeTargetted = canBeTargeted;
                ped.Accuracy = accuracy;
                ped.IsInvincible = invincible;
                ped.IsPersistent = true;
                ped.RelationshipGroup = leader.RelationshipGroup;
                ped.NeverLeavesGroup = true;
                Blip blip = ped.AttachedBlip;
                if (blip != null)
                {
                    blip.Delete();
                }
                Blip blip1 = ped.AttachBlip();
                blip1.Color = BlipColor.Blue;
                blip1.Scale = 0.7f;
                blip1.Name = "Friend";
                EntityEventWrapper entityEventWrapper = new EntityEventWrapper(ped);
                entityEventWrapper.Died += new EntityEventWrapper.OnDeathEvent((EntityEventWrapper sender, Entity entity) =>
                {
                    Blip currentBlip = entity.AttachedBlip;
                    if (currentBlip != null)
                    {
                        currentBlip.Delete();
                    }
                    entityEventWrapper.Dispose();
                });
                ped.PlayAmbientSpeech("GENERIC_HI", SpeechModifier.Standard);
            }
        }

        public static void Recruit(this Ped ped, Ped leader, bool canBeTargetted)
        {
            ped.Recruit(leader, canBeTargetted, false, 100);
        }

        public static void Recruit(this Ped ped, Ped leader)
        {
            ped.Recruit(leader, true);
        }

        public static void RemoveElegantly(this Ped ped)
        {
            int pedHandle = ped.Handle;
            API.RemovePedElegantly(ref pedHandle);
            //Function.Call((Hash)-6022081966519748258L, new InputArgument[] { ped.Handle });
        }

        public static void SetAlertness(this Ped ped, Alertness alertness)
        {
            API.SetPedAlertness(ped.Handle, (int)alertness);
            //Function.Call((Hash)-2619105872414424666L, new InputArgument[] { ped.Handle, alertness });
        }

        public static void SetCanAttackFriendlies(this Ped ped, FirendlyFireType type)
        {
            FirendlyFireType firendlyFireType = type;
            if (firendlyFireType == FirendlyFireType.CantAttack)
            {
                API.SetCanAttackFriendly(ped.Handle, false, false);
                // Function.Call((Hash)-5498390243159980195L, new InputArgument[] { ped.Handle, false, false });
            }
            else if (firendlyFireType == FirendlyFireType.CanAttack)
            {
                API.SetCanAttackFriendly(ped.Handle, true, false);
                //Function.Call((Hash)-5498390243159980195L, new InputArgument[] { ped.Handle, true, false });
            }
        }

        public static void SetCanEvasiveDive(this Ped ped, bool toggle)
        {
            InputArgument[] handle = new InputArgument[] { ped.Handle, default(InputArgument) };
            handle[1] = (toggle ? 1 : 0);
            Function.Call((Hash)7744612924842995801L, handle);
        }

        public static void SetCanPlayAmbientAnims(this Ped ped, bool toggle)
        {
            InputArgument[] handle = new InputArgument[] { ped.Handle, default(InputArgument) };
            handle[1] = (toggle ? 1 : 0);
            Function.Call((Hash)7166301455914477326L, handle);
        }

        public static void SetCombatAblility(this Ped ped, CombatAbility ability)
        {
            API.SetPedCombatAbility(ped.Handle, (int)ability);
            //Function.Call((Hash)-4079649877180351064L, new InputArgument[] { ped.Handle, ability });
        }

        public static void SetCombatAttributes(this Ped ped, CombatAttributes attribute, bool enabled)
        {
            API.SetPedCombatAttributes(ped.Handle, (int)attribute, enabled);
            //Function.Call((Hash)-6955927877681029095L, new InputArgument[] { ped.Handle, attribute, enabled });
        }

        public static void SetCombatMovement(this Ped ped, CombatMovement movement)
        {
            Function.Call((Hash)5592521861259579479L, new InputArgument[] { ped.Handle, movement });
        }

        public static void SetCombatRange(this Ped ped, CombatRange range)
        {
            Function.Call((Hash)4350590797670664571L, new InputArgument[] { ped.Handle, range });
        }

        public static void SetComponentVariation(this Ped ped, ComponentId id, int drawableId, int textureId, int paletteId)
        {
            Function.Call((Hash)2750315038012726912L, new InputArgument[] { ped.Handle, id, drawableId, textureId, paletteId });
        }

        public static void SetHearingRange(this Ped ped, float hearingRange)
        {
            Function.Call((Hash)3722497735840494396L, new InputArgument[] { ped.Handle, hearingRange });
        }

        public static async void SetMovementAnimSet(this Ped ped, string animation)
        {
            if (ped != null)
            {
                while (true)
                {
                    if (Function.Call<bool>(unchecked((Hash)(-4257582536886376016L)), new InputArgument[] { animation }))
                    {
                        break;
                    }
                    Function.Call((Hash)7972635428772450029L, new InputArgument[] { animation });
                    await Client.Delay(0);
                }
                Function.Call(unchecked((Hash)(-5797657820774978577L)), new InputArgument[] { ped.Handle, animation, 1048576000 });
            }
        }

        public static void SetPathAvoidFires(this Ped ped, bool toggle)
        {
            InputArgument[] handle = new InputArgument[] { ped.Handle, default(InputArgument) };
            handle[1] = (toggle ? 1 : 0);
            Function.Call((Hash)4923931356997885536L, handle);
        }

        public static void SetPathAvoidWater(this Ped ped, bool toggle)
        {
            InputArgument[] handle = new InputArgument[] { ped.Handle, default(InputArgument) };
            handle[1] = (toggle ? 1 : 0);
            Function.Call((Hash)4106753751182965052L, handle);
        }

        public static void SetPathCanClimb(this Ped ped, bool toggle)
        {
            API.SetPedPathCanUseClimbovers(ped.Handle, toggle);

            //InputArgument[] handle = new InputArgument[] { ped.Handle, default(InputArgument) };
            //handle[1] = (toggle ? 1 : 0);
            //Function.Call((Hash)-8212693258618671116L, handle);
        }

        public static void SetPathCanUseLadders(this Ped ped, bool toggle)
        {
            InputArgument[] handle = new InputArgument[] { ped.Handle, default(InputArgument) };
            handle[1] = (toggle ? 1 : 0);
            Function.Call((Hash)8621491691477485422L, handle);
        }

        public static void SetRagdollOnCollision(this Ped ped, bool toggle)
        {
            API.SetPedRagdollOnCollision(ped.Handle, toggle);
            //Function.Call((Hash)-1106493818855066473L, new InputArgument[] { ped.Handle, toggle });
        }

        public static void SetStealthMovement(this Ped ped, bool toggle)
        {
            API.SetPedStealthMovement(ped.Handle, toggle, "DEFAULT_ACTION");
            // Function.Call((Hash)-8589571964800369710L, new InputArgument[] { (toggle ? 1 : 0), "DEFAULT_ACTION" });
        }

        public static void SetToRagdoll(this Ped ped, int time)
        {
            API.SetPedToRagdoll(ped.Handle, time, 0, 0, false, false, false);
            //Function.Call((Hash)-5865380420870110134L, new InputArgument[] { ped.Handle, time, 0, 0, 0, 0, 0 });
        }

        public static void StopAmbientSpeechThisFrame(this Ped ped)
        {
            if (ped.IsAmbientSpeechPlaying())
            {
                API.StopCurrentPlayingAmbientSpeech(ped.Handle);
                //Function.Call((Hash)-5134454549476615409L, new InputArgument[] { ped.Handle });
            }
        }

        public static void StopSpeaking(this Ped ped, bool shaking)
        {
            API.StopPedSpeaking(ped.Handle, shaking);
            //InputArgument[] handle = new InputArgument[] { ped.Handle, default(InputArgument) };
            //handle[1] = (shaking ? 1 : 0);
            //Function.Call((Hash)-7105317640777702445L, handle);
        }
    }
}
