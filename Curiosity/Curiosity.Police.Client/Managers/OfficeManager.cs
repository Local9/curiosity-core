using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core.UI;

namespace Curiosity.Police.Client.Managers
{
    public class OfficeManager : Manager<OfficeManager>
    {
        Dictionary<int, string> _interiors = new Dictionary<int, string>()
        {
            { 236289, "ex_dt1_02_office_01a" }, // Office 0: Warm
            { 236545, "ex_dt1_02_office_01b" }, // Office 0: Classical
            { 236801, "ex_dt1_02_office_01c" }, // Office 0: Vintage
            { 237057, "ex_dt1_02_office_02a" }, // Office 0: Contrast
            { 237313, "ex_dt1_02_office_02b" }, // Office 0: Rich
            { 237569, "ex_dt1_02_office_02c" }, // Office 0: Cool
            { 237825, "ex_dt1_02_office_03a" }, // Office 0: Ice
            { 238081, "ex_dt1_02_office_03b" }, // Office 0: Conservative
            { 238337, "ex_dt1_02_office_03c" }, // Office 0: Polished
        };

        int _currentInterior = -1;
        int _organisationNameStage = 0;
        int _renderId = -1;
        int _scaleformHandle = -1;

        string _officeProp = "ex_prop_ex_office_text";
        string _officeTarget = "prop_ex_office_text";

        Dictionary<int, Vector3> _officeLocations = new Dictionary<int, Vector3>()
        {
            { 0, new Vector3(-141.1987f, -620.913f, 168.8205f) } // Arcadius Business Centre
        };

        public override void Begin()
        {
            
        }

        // [TickHandler]
        private async Task OnOfficeTeleport()
        {
            if (Game.IsControlJustPressed(0, Control.Context))
            {
                ToggleIplState("ex_dt1_02_office_01a", true); // think about IPL to load vrs location when teleporting

                Vector3 position = _officeLocations[0];
                Game.PlayerPed.Position = position;

                await BaseScript.Delay(500);

                int interior = GetInteriorAtCoords(position.X, position.Y, position.Z);

                if (interior > 0)
                {
                    string iplToLoad = _interiors[interior];

                    ToggleIplState(iplToLoad, true);

                    RefreshInterior(interior);

                    Screen.ShowNotification($"LOAD: {iplToLoad}");
                    
                    _organisationNameStage = 0;
                    _renderId = -1;
                    _scaleformHandle = -1;

                    Instance.AttachTickHandler(OnDisplayCompanyName);
                    return;
                }
                Screen.ShowNotification($"interior invalid: {interior}");
            }
        }

        private async Task OnDisplayCompanyName()
        {
            switch(_organisationNameStage)
            {
                case 0:
                    if (_renderId == -1)
                        _renderId = CreateNamedRenderTargetForModel(_officeTarget, (uint)GetHashKey(_officeProp));
                    if (_scaleformHandle == -1)
                        _scaleformHandle = RequestScaleformMovie("ORGANISATION_NAME");
                    _organisationNameStage = 1;
                    break;
                case 1:
                    if (HasScaleformMovieLoaded(_scaleformHandle))
                    {
                        List<Tuple<string, dynamic>> settings = new List<Tuple<string, dynamic>>()
                        {
                            new Tuple<string, dynamic>( "string", "::1 knows what" ),
                            new Tuple<string, dynamic>( "int", 10 ), // style
                            new Tuple<string, dynamic>( "int", 0 ), // color
                            new Tuple<string, dynamic>( "int", 3 ), // font
                        };
                        SetupScaleform(_scaleformHandle, "SET_ORGANISATION_NAME", settings);
                        _organisationNameStage = 2;
                    }
                    break;
                case 2:
                    SetTextRenderId(_renderId);
                    SetUiLayer(4);
                    N_0xc6372ecd45d73bcd(true);
                    ScreenDrawPositionBegin(73, 73);
                    DrawScaleformMovie(_scaleformHandle, 0.196f, 0.245f, 0.46f, 0.66f, 255, 255, 255, 255, 0);
                    SetTextRenderId(GetDefaultScriptRendertargetRenderId());
                    ScreenDrawPositionEnd();
                    break;
            }
        }

        int CreateNamedRenderTargetForModel(string name, uint model)
        {
            int handle = 0;
            if (!IsNamedRendertargetRegistered(name)) RegisterNamedRendertarget(name, false);
            if (!IsNamedRendertargetLinked(model)) LinkNamedRendertarget(model);
            if (IsNamedRendertargetRegistered(name)) handle = GetNamedRendertargetRenderId(name);
            return handle;
        }

        void SetupScaleform(int scaleformHandle, string function, List<Tuple<string, dynamic>> parameters)
        {
            BeginScaleformMovieMethod(scaleformHandle, function);
            
            if (function == "SET_ORGANISATION_NAME")
                ScaleformMovieMethodAddParamTextureNameString_2(parameters[0].Item2);

            foreach(Tuple<string, dynamic> parameter in parameters)
            {
                switch(parameter.Item1)
                {
                    case "bool":
                        PushScaleformMovieMethodParameterBool(parameter.Item2);
                        break;
                    case "int":
                        PushScaleformMovieMethodParameterInt(parameter.Item2);
                        break;
                    case "float":
                        PushScaleformMovieMethodParameterFloat(parameter.Item2);
                        break;
                    case "string":
                        PushScaleformMovieMethodParameterString(parameter.Item2);
                        break;
                    case "buttonName":
                        PushScaleformMovieMethodParameterButtonName(parameter.Item2);
                        break;
                }
            }
            EndScaleformMovieMethod();
            N_0x32f34ff7f617643b(scaleformHandle, 2);
        }

        void ToggleIplState(string ipl, bool activate = false)
        {
            if (!IsIplActive(ipl) && activate)
            {
                RequestIpl(ipl);
            }

            if (IsIplActive(ipl) && !activate)
            {
                RemoveIpl(ipl);
            }
        }
    }
}
