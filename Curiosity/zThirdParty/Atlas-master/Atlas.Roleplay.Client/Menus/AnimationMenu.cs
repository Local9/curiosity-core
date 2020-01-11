using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Atlas.Roleplay.Client.Environment.Entities.Models;
using Atlas.Roleplay.Client.Interface;
using Atlas.Roleplay.Client.Managers;
using CitizenFX.Core;

namespace Atlas.Roleplay.Client.Menus
{
    public class AnimationMenu : Manager<AnimationMenu>
    {
        public Dictionary<string, Dictionary<string, AnimationBuilder[]>> Categories { get; set; } =
            new Dictionary<string, Dictionary<string, AnimationBuilder[]>>
            {
                ["Polis"] = new Dictionary<string, AnimationBuilder[]>
                {
                    ["Undersök"] = new[]
                    {
                        new AnimationBuilder().Select("amb@code_human_police_investigate@idle_b", "idle_d")
                    },
                    ["Prata i rakel"] = new[]
                    {
                        new AnimationBuilder().Select("random@arrests", "generic_radio_chatter"),
                        new AnimationBuilder().Select("random@arrests", "generic_radio_emter"),
                        new AnimationBuilder().Select("random@arrests", "generic_radio_exit")
                    },
                    ["Håll på bältet"] = new[]
                    {
                        new AnimationBuilder().Select("amb@world_human_cop_idles@male@idle_enter", "idle_intro"),
                        new AnimationBuilder().Select("amb@world_human_cop_idles@male@base", "base").WithFlags(AnimationFlags.Loop),
                        new AnimationBuilder().Select("amb@world_human_cop_idles@male@exit", "exit")
                    },
                    ["Ge upp"] = new[]
                    {
                        new AnimationBuilder().Select("random@arrests", "idle_2_hands_up"),
                        new AnimationBuilder().Select("random@arrests", "kneeling_arrest_idle").WithFlags(AnimationFlags.Loop),
                        new AnimationBuilder().Select("random@arrests", "kneeling_arrest_get_up")
                    }
                },
                ["Vård"] = new Dictionary<string, AnimationBuilder[]>
                {
                    ["Undersök kropp"] = new[]
                    {
                        new AnimationBuilder().Select("CODE_HUMAN_MEDIC_KNEEL"),
                    },
                    ["Läsa journal"] = new[]
                    {
                        new AnimationBuilder().Select("WORLD_HUMAN_CLIPBOARD")
                    }
                },
                ["Mekaniker"] = new Dictionary<string, AnimationBuilder[]>
                {
                    ["Meka bil"] = new[]
                    {
                        new AnimationBuilder().Select("missmechanic", "work2_in"),
                        new AnimationBuilder().Select("missmechanic", "work2_base").WithFlags(AnimationFlags.Loop),
                        new AnimationBuilder().Select("missmechanic", "work2_exit"),
                    },
                    ["Meka på golvet"] = new[]
                    {
                        new AnimationBuilder().Select("anim@heists@narcotics@funding@gang_idle",
                            "gang_chatting_idle01").WithFlags(AnimationFlags.Loop),
                    },
                    ["Arbeta med en hammare"] = new[]
                    {
                        new AnimationBuilder().Select("WORLD_HUMAN_HAMMERING")
                    }
                },
                ["Funktionalitet"] = new Dictionary<string, AnimationBuilder[]>
                {
                    ["Kolla där"] = new[]
                    {
                        new AnimationBuilder().Select("anim@mp_radio@garage@high", "idle_a")
                    },
                    ["Visa telefonen"] = new[]
                    {
                        new AnimationBuilder().Select("cellphone@", "f_cellphone_text_read_base"),
                    },
                    ["Flexa"] = new[]
                    {
                        new AnimationBuilder().Select("amb@world_human_muscle_flex@arms_at_side@idle_a", "idle_a")
                    },
                    ["BBQ"] = new[]
                    {
                        new AnimationBuilder().Select("PROP_HUMAN_BBQ")
                    },
                    ["Kryp skadad"] = new[]
                    {
                        new AnimationBuilder().Select("move_injured_ground", "front_loop").WithFlags(AnimationFlags.Loop)
                    }
                },
                ["Gester"] = new Dictionary<string, AnimationBuilder[]>
                {
                    ["Tummen upp"] = new[]
                    {
                        new AnimationBuilder().Select("anim@mp_player_intselfiethumbs_up", "enter"),
                        new AnimationBuilder().Select("anim@mp_player_intselfiethumbs_up", "idle_a"),
                        new AnimationBuilder().Select("anim@mp_player_intselfiethumbs_up", "exit")
                    },
                    ["Jack off"] = new[]
                    {
                        new AnimationBuilder().Select("anim@mp_player_intselfiewank", "enter"),
                        new AnimationBuilder().Select("anim@mp_player_intselfiewank", "idle_a"),
                        new AnimationBuilder().Select("anim@mp_player_intselfiewank", "exit")
                    },
                    ["Jack off 2"] = new[]
                    {
                        new AnimationBuilder().Select("anim@mp_player_intupperwank", "idle_a_fp")
                    },
                    ["2 Birdies"] = new[]
                    {
                        new AnimationBuilder().Select("anim@mp_player_intupperfinger", "enter_fp"),
                        new AnimationBuilder().Select("anim@mp_player_intupperfinger", "exit_fp")
                    },
                    ["Shush"] = new[]
                    {
                        new AnimationBuilder().Select("anim@mp_player_intuppershush", "idle_a_fp")
                    },
                    ["Boop"] = new[]
                    {
                        new AnimationBuilder().Select("anim@mp_radio@garage@high", "action_a"),
                    },
                    ["Facepalm"] = new[]
                    {
                        new AnimationBuilder().Select("anim@mp_player_intcelebrationmale@face_palm", "face_palm")
                    },
                    ["Ska vi ligga?"] = new[]
                    {
                        new AnimationBuilder().Select("anim@mp_player_intupperdock", "idle_a")
                    },
                    ["Gangsign"] = new[]
                    {
                        new AnimationBuilder().Select("mp_player_int_uppergang_sign_a",
                            "mp_player_int_uppergang_sign_a")
                    },
                    ["Gangsign 2"] = new[]
                    {
                        new AnimationBuilder().Select("mp_player_int_uppergang_sign_b",
                            "mp_player_int_uppergang_sign_b")
                    },
                    ["Peta i näsan"] = new[]
                    {
                        new AnimationBuilder().Select("anim@mp_player_intincarnose_pickbodhi@ds@", "enter"),
                        new AnimationBuilder().Select("anim@mp_player_intincarnose_pickbodhi@ds@", "idle_a"),
                        new AnimationBuilder().Select("anim@mp_player_intincarnose_pickbodhi@ds@", "exit")
                    },
                    ["Bring it on"] = new[]
                    {
                        new AnimationBuilder().Select("misscommon@response", "bring_it_on")
                    },
                    ["Förbannat"] = new[]
                    {
                        new AnimationBuilder().Select("misscommon@response", "curse")
                    },
                    ["Screw you"] = new[]
                    {
                        new AnimationBuilder().Select("misscommon@response", "screw_you")
                    },
                    ["Peace"] = new[]
                    {
                        new AnimationBuilder().Select("anim@mp_player_intincarpeacebodhi@ds@", "enter"),
                        new AnimationBuilder().Select("anim@mp_player_intincarpeacebodhi@ds@", "idle_a"),
                        new AnimationBuilder().Select("anim@mp_player_intincarpeacebodhi@ds@", "exit")
                    },
                    ["Kvinnlig Fuck You"] = new[]
                    {
                        new AnimationBuilder().Select("anim@mp_player_intcelebrationfemale@finger", "finger")
                    },
                    ["Klappa händerna sakta"] = new[]
                    {
                        new AnimationBuilder().Select("amb@world_human_cheering@male_e", "base")
                    },
                    ["Gestikulerar fuck you (kille)"] = new[]
                    {
                        new AnimationBuilder().Select("anim@mp_player_intcelebrationmale@finger", "finger")
                    },
                    ["Vinka högt"] = new[]
                    {
                        new AnimationBuilder().Select("missmic4premiere", "wave_a")
                    },
                    ["Vinka med två händer"] = new[]
                    {
                        new AnimationBuilder().Select("missmic4premiere", "wave_d")
                    }
                },
                ["Danser"] = new Dictionary<string, AnimationBuilder[]>
                {
                    ["Berusad fuldans"] = new[]
                    {
                        new AnimationBuilder().Select("move_clown@p_m_two_idles@", "fidget_short_dance")
                    },
                    ["Mobbad dans"] = new[]
                    {
                        new AnimationBuilder().Select("missfbi3_sniping", "dance_m_default")
                    },
                    ["Sjukt stel dans"] = new[]
                    {
                        new AnimationBuilder().Select("amb@world_human_strip_watch_stand@male_a@idle_a", "idle_c")
                    },
                    ["Dansa med en öl i handen"] = new[]
                    {
                        new AnimationBuilder().Select("WORLD_HUMAN_PARTYING")
                    },
                    ["Dansa med armarna"] = new[]
                    {
                        new AnimationBuilder().Select("misschinese2_crystalmazemcs1_cs", "dance_loop_tao")
                    },
                    ["Jogging dans"] = new[]
                    {
                        new AnimationBuilder().Select("amb@world_human_jog_standing@male@fitidle_a", "idle_a")
                    },
                    ["Flippad DJ"] = new[]
                    {
                        new AnimationBuilder().Select("anim@mp_player_intcelebrationfemale@dj", "dj")
                    },
                    ["Noname"] = new[]
                    {
                        new AnimationBuilder().Select("missfam2leadinoutmcs3", "onboat_leadin_tracy")
                    },
                    ["DJ"] = new[]
                    {
                        new AnimationBuilder().Select("mini@strip_club@idles@dj@idle_02", "idle_02")
                    }
                },
                ["Stances"] = new Dictionary<string, AnimationBuilder[]>
                {
                    ["Täck händerna för ansiktet"] = new[]
                    {
                        new AnimationBuilder().Select("anim@mp_player_intuppersurrender", "idle_a_fp").WithFlags(AnimationFlags.Loop)
                    },
                    ["Stå still och ryck med händerna"] = new[]
                    {
                        new AnimationBuilder().Select("amb@world_human_bum_standing@twitchy@base", "base")
                    },
                    ["Luta mot väggen"] = new[]
                    {
                        new AnimationBuilder().Select("amb@world_human_leaning@male@wall@back@foot_up@idle_a", "idle_a").WithFlags(AnimationFlags.Loop)
                    },
                    ["Luta mot väggen 2"] = new[]
                    {
                        new AnimationBuilder().Select("amb@world_human_leaning@male@wall@back@legs_crossed@base",
                            "base").WithFlags(AnimationFlags.Loop)
                    },
                    ["Vakt"] = new[]
                    {
                        new AnimationBuilder().Select("amb@world_human_stand_guard@male@idle_b", "idle_a").WithFlags(AnimationFlags.Loop)
                    },
                    ["Ligg ner"] = new[]
                    {
                        new AnimationBuilder().Select("anim@gangops@morgue@table@", "ko_back")
                    },
                    ["Bouncer"] = new[]
                    {
                        new AnimationBuilder().Select("mini@strip_club@idles@bouncer@idle_intro", "idle_intro"),
                        new AnimationBuilder().Select("mini@strip_club@idles@bouncer@idle_c", "idle_c").WithFlags(AnimationFlags.Loop)
                    },
                    ["Bouncer 2"] = new[]
                    {
                        new AnimationBuilder().Select("mini@strip_club@idles@bouncer@idle_a", "idle_a")
                    },
                    ["Uppvärmning"] = new[]
                    {
                        new AnimationBuilder().Select("mini@triathlon", "ig_2_gen_warmup_04")
                    },
                    ["Uppvärmning 2"] = new[]
                    {
                        new AnimationBuilder().Select("mini@triathlon", "ig_2_gen_warmup_10")
                    },
                    ["Sitt ner på marken"] = new[]
                    {
                        new AnimationBuilder().Select("amb@world_human_picnic@male@enter", "enter"),
                        new AnimationBuilder().Select("amb@world_human_picnic@male@base", "base").WithFlags(AnimationFlags.Loop),
                        new AnimationBuilder().Select("amb@world_human_picnic@male@exit", "exit"),
                    },
                    ["Armarna i kors"] = new[]
                    {
                        new AnimationBuilder().Select("anim@heists@heist_corona@single_team", "single_team_loop_boss").WithFlags(AnimationFlags.Loop)
                    }
                }
            };

        private bool IsPlayingAnimation { get; set; }

        [TickHandler(SessionWait = true)]
        private async Task OnTick()
        {
            if (Game.IsControlJustPressed(0, Control.SelectCharacterMichael))
            {
                OpenAnimationMenu();

                await BaseScript.Delay(3000);
            }

            await Task.FromResult(0);
        }

        private void OpenAnimationMenu()
        {
            new Menu("Animationsmeny")
            {
                Items = Categories.Select(self => self.Key)
                    .Select(self => new MenuItem($"category_{self.ToLower()}", self)).ToList(),
                Callback = (menu, item, operation) =>
                {
                    if (operation.Type != MenuOperationType.Select) return;

                    OpenAnimationCategory(item.Label);
                }
            }.Commit();
        }

        private void OpenAnimationCategory(string category)
        {
            new Menu($"{category}")
            {
                Items = Categories[category]
                    .Select(self => new MenuItem($"__{self.Key.ToLower()}", self.Key, self.Value.ToList())).ToList(),
                Callback = async (menu, item, operation) =>
                {
                    if (operation.Type == MenuOperationType.PostClose)
                    {
                        OpenAnimationMenu();
                    }
                    else if (operation.Type == MenuOperationType.Select)
                    {
                        var animations = (List<AnimationBuilder>) item.Metadata[0];

                        foreach (var animation in animations)
                        {
                            await Cache.Entity.AnimationQueue.PlayDirectInQueue(animation);
                        }
                    }
                }
            }.Commit();
        }
    }
}