using CitizenFX.Core.UI;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Curiosity.Police.Client.Diagnostics;
using Curiosity.Systems.Library.Models.Casino;
using Newtonsoft.Json;
using Curiosity.Police.Client.Extensions;

namespace Curiosity.Police.Client.Managers
{
    public class CasinoPedManager : Manager<CasinoPedManager>
    {
        const int CASINO_INTERIOR_ID = 275201;
        bool isInsideCasino = false;
        CasinoData _casinoConfig;

        List<PedStruct> peds = new List<PedStruct>()
        {
            new PedStruct() { CasinoPed = eCasinoPed.CasinoMaleWorkerOne },
            new PedStruct() { CasinoPed = eCasinoPed.CasinoMaleWorkerTwo },
            new PedStruct() { CasinoPed = eCasinoPed.CasinoMaleWorkerThree },
            new PedStruct() { CasinoPed = eCasinoPed.CasinoMaleWorkerFour },
            new PedStruct() { CasinoPed = eCasinoPed.CasinoMaleWorkerFive },
            new PedStruct() { CasinoPed = eCasinoPed.CasinoMaleWorkerSix },
            new PedStruct() { CasinoPed = eCasinoPed.CasinoMaleWorkerSeven },
            new PedStruct() { CasinoPed = eCasinoPed.CasinoMaleWorkerEight },
            new PedStruct() { CasinoPed = eCasinoPed.CasinoMaleWorkerNine },
            new PedStruct() { CasinoPed = eCasinoPed.CasinoMaleWorkerTen },
            new PedStruct() { CasinoPed = eCasinoPed.CasinoMaleWorkerEleven },
            new PedStruct() { CasinoPed = eCasinoPed.CasinoMaleWorkerTwelve },
            new PedStruct() { CasinoPed = eCasinoPed.CasinoMaleWorkerThirteen },
            new PedStruct() { CasinoPed = eCasinoPed.CasinoMaleWorkerFourteen },
        };

        List<RouletteTable> rouletteTables = new List<RouletteTable>();
        
        public override void Begin()
        {
            
        }

        private CasinoData GetPoliceConfig()
        {
            if (_casinoConfig is not null)
                return _casinoConfig;

            CasinoData config = new();

            try
            {
                _casinoConfig = JsonConvert.DeserializeObject<CasinoData>(Properties.Resources.casinoInterior);
                return _casinoConfig;
            }
            catch (Exception ex)
            {
                Logger.Error($"Casino Config JSON File Exception\nDetails: {ex.Message}\nStackTrace:\n{ex.StackTrace}");
            }

            return config;
        }

        [TickHandler]
        private async Task OnIsPlayerInsideCasinoTick()
        {
            try
            {
                int interiorId = GetInteriorFromEntity(Game.PlayerPed.Handle);

                if (interiorId == CASINO_INTERIOR_ID && !isInsideCasino)
                    Init();

                if (interiorId != CASINO_INTERIOR_ID && isInsideCasino)
                    Dispose();

                if (Game.IsControlJustPressed(0, Control.Context))
                {
                    RemoveAllPeds();

                    //for (int i = 0; i < peds.Count; i++)
                    //{
                    //    PedStruct pedStruct = peds[i];
                    //    pedStruct = SetupPedModel(pedStruct);

                    //    if (pedStruct.Model == -1)
                    //    {
                    //        Logger.Error($"Failed to create '{peds[i].CasinoPed}'.");
                    //        continue;
                    //    }

                    //    pedStruct.Ped = await World.CreatePed(pedStruct.Model, Game.PlayerPed.Position);
                    //    SetupPedVariations(pedStruct);
                    //    peds[i] = pedStruct;
                    //}
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"OnIsPlayerInsideCasinoTick");
            }
        }

        private void SetupPedVariations(PedStruct pedStruct)
        {
            if (pedStruct.PedComponentVariations.Length == 0) return;

            for(int i = 0; i < pedStruct.PedComponentVariations.Length; i++)
            {
                PedComponentVariation variation = pedStruct.PedComponentVariations[i];
                SetPedComponentVariation(pedStruct.Handle, i, variation.DrawableId, variation.TextureId, variation.paletteId);
            }
        }

        private async void Init()
        {
            try
            {
                isInsideCasino = true;
                CasinoData cd = GetPoliceConfig();

                foreach (RouletteTable rouletteTable in cd.RouletteTables)
                {
                    CreateRouletteTable(rouletteTable);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Casino Ped Manager Init");
            }
        }

        private void Dispose()
        {
            isInsideCasino = false;

            RemoveAllPeds();
            RemoveAllProps();
        }

        private void RemoveAllPeds()
        {
            for (int i = 0; i < peds.Count; i++)
            {
                PedStruct pedStruct = peds[i];
                int handle = pedStruct.Handle;
                if (DoesEntityExist(handle))
                    DeleteEntity(ref handle);
            }
        }

        private void RemoveAllProps()
        {
            List<RouletteTable> rouletteTablesCopy = new List<RouletteTable>(rouletteTables);

            foreach(RouletteTable rouletteTable in rouletteTablesCopy)
            {
                int handle = rouletteTable.Handle;
                SetModelAsNoLongerNeeded((uint)GetHashKey(rouletteTable.Table));

                if (DoesEntityExist(handle))
                    DeleteEntity(ref handle);
            }

            rouletteTables.Clear();
        }

        private async Task CreateRouletteTable(RouletteTable rouletteTable)
        {
            Model tableModel = new Model(rouletteTable.Table);
            await tableModel.Request(3000);

            Vector3 pos = rouletteTable.Position.AsVector();
            float heading = rouletteTable.Heading;

            rouletteTable.Handle = CreateObject(tableModel.Hash, pos.X, pos.Y, pos.Z, false, false, false);
            SetEntityHeading(rouletteTable.Handle, heading);

            rouletteTables.Add(rouletteTable);
        }

        private PedStruct SetupPedModel(PedStruct pedStruct)
        {
            switch(pedStruct.CasinoPed)
            {
                case eCasinoPed.CasinoMaleWorkerOne:
                    return CasinoMaleWorker(pedStruct);
                case eCasinoPed.CasinoMaleWorkerTwo:
                    return CasinoMaleWorkerTwo(pedStruct);
                case eCasinoPed.CasinoMaleWorkerThree:
                    return CasinoMaleWorkerThree(pedStruct);
                case eCasinoPed.CasinoMaleWorkerFour:
                    return CasinoMaleWorkerFour(pedStruct);
            }
            return pedStruct;
        }

        private PedStruct CasinoPatronSmartFemale(PedStruct pedStruct)
        {
            pedStruct.Model = GetHashKey("a_f_y_smartcaspat_01");
            return pedStruct;
        }

        private PedStruct CasinoPatronSmartMale(PedStruct pedStruct)
        {
            pedStruct.Model = GetHashKey("a_m_y_smartcaspat_01");
            return pedStruct;
        }

        private PedStruct CasinoPatronGeneralFemale(PedStruct pedStruct)
        {
            pedStruct.Model = GetHashKey("a_f_y_gencaspat_01");
            return pedStruct;
        }

        private PedStruct CasinoPatronGeneralMale(PedStruct pedStruct)
        {
            pedStruct.Model = GetHashKey("a_m_y_gencaspat_01");
            return pedStruct;
        }

        private PedStruct CasinoCash(PedStruct pedStruct)
        {
            pedStruct.Model = GetHashKey("u_f_m_casinocash_01");
            return pedStruct;
        }

        private PedStruct CasinoMaleWorkerFour(PedStruct pedStruct) // func_341
        {
            pedStruct.Model = GetHashKey("s_m_y_casino_01");

            pedStruct.PedComponentVariations = new PedComponentVariation[12];

            pedStruct.PedComponentVariations[0].DrawableId = 2;
            pedStruct.PedComponentVariations[1].DrawableId = 1;
            pedStruct.PedComponentVariations[2].DrawableId = 3;
            pedStruct.PedComponentVariations[3].DrawableId = 1;
            pedStruct.PedComponentVariations[3].TextureId = 3;
            pedStruct.PedComponentVariations[6].DrawableId = 1;
            pedStruct.PedComponentVariations[7].DrawableId = 2;
            pedStruct.PedComponentVariations[8].DrawableId = 3;
            pedStruct.PedComponentVariations[10].DrawableId = 1;
            pedStruct.PedComponentVariations[11].DrawableId = 1;

            return pedStruct;
        }

        private PedStruct CasinoMaleWorkerThree(PedStruct pedStruct) // func_342
        {
            pedStruct.Model = GetHashKey("s_m_y_casino_01");

            pedStruct.PedComponentVariations = new PedComponentVariation[12];

            pedStruct.PedComponentVariations[0].DrawableId = 2;
            pedStruct.PedComponentVariations[0].TextureId = 1;
            pedStruct.PedComponentVariations[1].DrawableId = 1;
            pedStruct.PedComponentVariations[2].DrawableId = 2;
            pedStruct.PedComponentVariations[3].TextureId = 3;
            pedStruct.PedComponentVariations[6].DrawableId = 1;
            pedStruct.PedComponentVariations[7].DrawableId = 2;
            pedStruct.PedComponentVariations[8].DrawableId = 1;
            pedStruct.PedComponentVariations[10].DrawableId = 1;
            pedStruct.PedComponentVariations[11].DrawableId = 1;

            return pedStruct;
        }

        private PedStruct CasinoMaleWorkerTwo(PedStruct pedStruct) // func_343
        {
            pedStruct.Model = GetHashKey("s_m_y_casino_01");

            pedStruct.PedComponentVariations = new PedComponentVariation[12];

            pedStruct.PedComponentVariations[0].DrawableId = 2;
            pedStruct.PedComponentVariations[0].TextureId = 2;
            pedStruct.PedComponentVariations[1].DrawableId = 1;
            pedStruct.PedComponentVariations[2].DrawableId = 4;
            pedStruct.PedComponentVariations[3].TextureId = 4;
            pedStruct.PedComponentVariations[6].DrawableId = 1;
            pedStruct.PedComponentVariations[7].DrawableId = 2;
            pedStruct.PedComponentVariations[8].DrawableId = 1;
            pedStruct.PedComponentVariations[10].DrawableId = 1;
            pedStruct.PedComponentVariations[11].DrawableId = 1;

            return pedStruct;
        }

        private PedStruct CasinoMaleWorker(PedStruct pedStruct) // func_344
        {
            pedStruct.Model = GetHashKey("s_m_y_casino_01");

            pedStruct.PedComponentVariations = new PedComponentVariation[12];

            pedStruct.PedComponentVariations[0].DrawableId = 3;
            pedStruct.PedComponentVariations[1].DrawableId = 1;
            pedStruct.PedComponentVariations[2].DrawableId = 3;
            pedStruct.PedComponentVariations[3].DrawableId = 1;
            pedStruct.PedComponentVariations[6].DrawableId = 1;
            pedStruct.PedComponentVariations[7].DrawableId = 2;
            pedStruct.PedComponentVariations[8].DrawableId = 3;
            pedStruct.PedComponentVariations[10].DrawableId = 1;
            pedStruct.PedComponentVariations[11].DrawableId = 1;

            return pedStruct;
        }
    }

    internal struct PedStruct
    {
        // Configuration Values
        public eCasinoPed CasinoPed;
        public int Location;
        public PedComponentVariation[] PedComponentVariations;

        // Set values
        public Model Model = -1;
        public Ped Ped;
        public int Handle => Ped?.Handle ?? -1;
        public Vector3 Position;
        public float Heading;
    }

    internal struct PedComponentVariation
    {
        public int DrawableId;
        public int TextureId;
        public int paletteId;

        public PedComponentVariation()
        {
            this.DrawableId = 0;
            this.TextureId = 0;
            this.paletteId = 0;
        }
    }

    internal enum eCasinoPed
    {
        GeneralMale,
        SmartMale,
        GeneralFemale,
        SmartFemale,
        TomCasino,
        CasinoCash,
        Agatha,
        CasinoMaleWorkerOne,
        CasinoMaleWorkerTwo,
        CasinoMaleWorkerThree,
        CasinoMaleWorkerFour,
        CasinoMaleWorkerFive,
        CasinoMaleWorkerSix,
        CasinoMaleWorkerSeven,
        CasinoMaleWorkerEight,
        CasinoMaleWorkerNine,
        CasinoMaleWorkerTen,
        CasinoMaleWorkerEleven,
        CasinoMaleWorkerTwelve,
        CasinoMaleWorkerThirteen,
        CasinoMaleWorkerFourteen,
        CasinoFemaleWorkerOne,
        CasinoFemaleWorkerTwo,
        CasinoFemaleWorkerThree,
        CasinoFemaleWorkerFour,
        CasinoFemaleWorkerFive,
        CasinoFemaleWorkerSix,
        CasinoFemaleWorkerSeven,
        Celeb,
        Ushi,
        Gabriel,
        Vince,
        Dean,
        Carol,
        Beth,
        Lauren,
        Taylor,
        Blane,
        Eileen,
        Curtis
    }
}
