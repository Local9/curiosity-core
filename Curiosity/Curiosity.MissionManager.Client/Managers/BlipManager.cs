using CitizenFX.Core;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.MissionManager.Client.Environment.Entities.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.MissionManager.Client.Managers
{
    public class BlipManager : Manager<BlipManager>
    {
        List<int> blipHandles = new List<int>();
        int columnDisplayed = 1;
        Dictionary<int, BlipMissionInfo> BlipMissionData = new Dictionary<int, BlipMissionInfo>();

        int labels = 0;
        int entries = 0;

        public override void Begin()
        {
            
        }

        public void AddBlip(string blipName, BlipSprite blipSprite, BlipColor blipColor, Vector3 position)
        {
            Blip blip = World.CreateBlip(position);
            blip.Name = blipName;
            blip.Sprite = blipSprite;
            blip.Color = blipColor;

            blipHandles.Add(blip.Handle);
        }

        public Blip AddBlip(string blipName, BlipSprite blipSprite, BlipColor blipColor, Vector3 position, BlipMissionInfo missionInfo)
        {
            Blip blip = World.CreateBlip(position);
            blip.Name = blipName;
            blip.Sprite = blipSprite;
            blip.Color = blipColor;

            string key = $"blipLabel{blipName.Trim().Replace(" ", "")}";
            AddTextEntry(key, blipName);

            AddTextComponentSubstringBlipName(blip.Handle);
            BeginTextCommandSetBlipName(key);
            EndTextCommandSetBlipName(blip.Handle);

            blipHandles.Add(blip.Handle);
            BlipMissionData.Add(blip.Handle, missionInfo);

            SetBlipAsMissionCreatorBlip(blip.Handle, true);

            return blip;
        }

        public void RemoveBlip(Blip blip)
        {
            if (BlipMissionData.ContainsKey(blip.Handle))
            {
                BlipMissionData.Remove(blip.Handle);
            }

            if (!blip.Exists()) return;

            blipHandles.Remove(blip.Handle);

            blip.Delete();
        }

        [TickHandler]
        private async Task OnMissionBlipHandler()
        {
            Blip currentBlip = null;

            if (!IsFrontendReadyForControl()) return;

            int blipHandle = DisableBlipNameForVar();
            Blip blip = new Blip(blipHandle);

            if (!IsHoveringOverMissionCreatorBlip())
            {
                currentBlip = null;
                ShowMissionBlipInformation(false);
                return;
            }

            if (!blip.Exists()) return;

            if (currentBlip != blip)
            {
                currentBlip = blip;
                if (!BlipMissionData.ContainsKey(currentBlip.Handle))
                {
                    currentBlip = null;
                    ShowMissionBlipInformation(false);
                    return;
                }

                TakeControlOfFrontend();
                ClearCurrentDisplay();
                BlipMissionInfo blipMissionInfo = BlipMissionData[currentBlip.Handle];
                SetMissionData(blipMissionInfo);
                ShowMissionBlipInformation(true);
                UpdateMissionBlipInformation();
                ReleaseControlOfFrontend();
            }
        }

        void SetMissionData(BlipMissionInfo blipMissionInfo)
        {
            if (PushScaleformMovieFunctionN("SET_COLUMN_TITLE")) {
                PushScaleformMovieFunctionParameterInt(columnDisplayed);
                SetString("");
                SetString(CreateLabel(blipMissionInfo.Title));
                PushScaleformMovieFunctionParameterInt(blipMissionInfo.RockstarVerified);
                PushScaleformMovieFunctionParameterString(blipMissionInfo.TextureDictionary);
                PushScaleformMovieFunctionParameterString(blipMissionInfo.TextureName);
                PushScaleformMovieFunctionParameterInt(0);
                PushScaleformMovieFunctionParameterInt(0);
                if (blipMissionInfo.Rp == "") {
                    PushScaleformMovieFunctionParameterBool(false);
                } else {
                    SetString(CreateLabel(blipMissionInfo.Rp));
                }
                if (blipMissionInfo.Money == "") {
                    PushScaleformMovieFunctionParameterBool(false);
                } else {
                    SetString(CreateLabel(blipMissionInfo.Money));
                }
            }
            PopScaleformMovieFunctionVoid();
        }

        void SetString(string txt)
        {
            BeginTextCommandScaleformString(txt);
            EndTextCommandScaleformString();
        }

        string CreateLabel(string text)
        {
            string lbl = $"LBL{labels}";
            AddTextEntry(lbl, text);
            labels++;
            return lbl;
        }

        void ClearCurrentDisplay()
        {
            if (PushScaleformMovieFunctionN("SET_DATA_SLOT_EMPTY"))
            {
                PushScaleformMovieFunctionParameterInt(columnDisplayed);
                PopScaleformMovieFunctionVoid();
            }
            labels = 0;
            entries = 0;
        }

        void UpdateMissionBlipInformation()
        {
            if (PushScaleformMovieFunctionN("DISPLAY_DATA_SLOT"))
            {
                PushScaleformMovieFunctionParameterInt(columnDisplayed);
                PopScaleformMovieFunctionVoid();
            }
        }

        void SetColumnState(int column, bool state)
        {
            if (PushScaleformMovieFunctionN("SHOW_COLUMN"))
            {
                PushScaleformMovieFunctionParameterInt(column);
                PushScaleformMovieFunctionParameterBool(state);
                PopScaleformMovieFunctionVoid();
            }
        }

        void ShowMissionBlipInformation(bool show)
        {
            SetColumnState(columnDisplayed, show);
        }
    }
}
