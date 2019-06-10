using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using Curiosity.Client.net.Helpers.Dictionary;

namespace Curiosity.Client.net.Helpers
{
    public class DuiContainer
    {
        public int TargetHandle { get; set; }
        public long TxdHandle { get; set; }
        public List<Prop> Props = new List<Prop>();
        public String RenderTargetName { get; set; }
        public long duiObj { get; set; }
        public string UniqId { get; set; }

        public void Draw()
        {
            API.SetTextRenderId(TargetHandle);
            API.Set_2dLayer(4);
            Function.Call((Hash)0xC6372ECD45D73BCD, 1);
            API.DrawRect(0.5f, 0.5f, 1.0f, 1.0f, 0, 0, 0, 255);
            API.DrawSprite($"{this.RenderTargetName}-{UniqId}", $"{this.RenderTargetName}-{UniqId}-test", 0.5f, 0.5f, 1.0f, 1.0f, 0.0f, 255, 255, 255, 255);
            API.SetTextRenderId(API.GetDefaultScriptRendertargetRenderId());
            Function.Call((Hash)0xC6372ECD45D73BCD, 0);
        }
    }

    class DuiHandler
    {
        public static Dictionary<string, List<string>> Targets { get; set; }
        public static Dictionary<string, List<DuiContainer>> DuiContainers { get; set; } = new Dictionary<string, List<DuiContainer>>();
        public static List<String> UsedRenderTargets = new List<string>();
        public static Dictionary<string, List<DuiContainer>> DeletedContainers = new Dictionary<string, List<DuiContainer>>();

        public static void Init()
        {
            //Exports.Add("createDui", new Func<string, string, Task<DuiContainer>>(AddDui));
            //Exports.Add("CreateRandomUniqueDuiContainer", new Func<string, Task<DuiContainer>>(CreateRandomUniqueDuiContainer));
            //Exports.Add("destroyAllDui", new Func<Task>(DestroyAllDui));

            Targets = RenderTargets.targets;

            Client.GetInstance().RegisterTickHandler(DuiHandler_Tick);
        }

        public static async Task<DuiContainer> CreateRandomUniqueDuiContainer(string url)
        {
            var keys = Targets.Keys;
            var random = new Random();
            string renderName = null;

            while (UsedRenderTargets.Contains(renderName) || renderName == null)
            {
                renderName = keys.ElementAt(random.Next(0, keys.Count));
            }

            return await AddDui(renderName, url);
        }

        public static async Task DestroyAllDui()
        {
            List<DuiContainer> containers = new List<DuiContainer>();
            foreach (var renderTargetName in DuiContainers)
            {
                foreach (var duiContainer in renderTargetName.Value)
                {
                    containers.Add(duiContainer);
                }
            }

            foreach (var duiContainer in containers)
            {
                RemoveDuiContainer(duiContainer);
            }

            await Task.FromResult(0);
        }

        private static void RemoveDuiContainer(DuiContainer duiContainer)
        {
            API.SetDuiUrl(duiContainer.duiObj, "about:blank");

            foreach (var prop in duiContainer.Props)
            {
                prop.Delete();
            }

            if (!DeletedContainers.ContainsKey(duiContainer.RenderTargetName))
                DeletedContainers[duiContainer.RenderTargetName] = new List<DuiContainer>();

            DeletedContainers[duiContainer.RenderTargetName].Add(duiContainer);
            DuiContainers[duiContainer.RenderTargetName].Remove(duiContainer);
            if (!DuiContainers[duiContainer.RenderTargetName].Any())
                DuiContainers.Remove(duiContainer.RenderTargetName);
        }

        private static async Task DuiHandler_Tick()
        {
            foreach (var renderTargetName in DuiContainers)
            {
                foreach (var duiContainer in renderTargetName.Value)
                {
                    duiContainer.Draw();
                }
            }
            await Task.FromResult(0);
        }

        public static async Task<DuiContainer> AddDui(String renderTarget, string url)
        {
            DuiContainer duiContainer = null;
            if (DeletedContainers.ContainsKey(renderTarget) && DeletedContainers[renderTarget].Any())
            {
                duiContainer = ReuseDuiContainer(renderTarget, url);
            }

            var propAndModelName = await CreateModelForRender(renderTarget);
            var modelName = propAndModelName.Item2;
            if (duiContainer == null)
                duiContainer = AddDuiInternal(renderTarget, modelName, url);
            duiContainer.Props = new List<Prop>() { propAndModelName.Item1 };
            if (!DuiContainers.ContainsKey(renderTarget))
                DuiContainers.Add(renderTarget, new List<DuiContainer>());

            DuiContainers[renderTarget].Add(duiContainer);
            if (!UsedRenderTargets.Contains(renderTarget))
            {
                UsedRenderTargets.Add(renderTarget);
            }
            return duiContainer;
        }

        private static DuiContainer ReuseDuiContainer(string renderTarget, string url)
        {
            DuiContainer duiContainer = DeletedContainers[renderTarget][0];
            DeletedContainers[renderTarget].Remove(duiContainer);
            if (!DeletedContainers[renderTarget].Any())
            {
                DeletedContainers.Remove(renderTarget);
            }
            API.SetDuiUrl(duiContainer.duiObj, url);
            return duiContainer;
        }

        private static async Task<Tuple<Prop, string>> CreateModelForRender(String renderTarget, String modelName = null)
        {
            if (modelName == null)
            {
                var random = new Random();
                var props = Targets[renderTarget];
                modelName = props[random.Next(0, props.Count)];
            }
            Model model = new Model(modelName);
            model.Request();

            var prop = await World.CreateProp(model, Game.PlayerPed.Position + Game.PlayerPed.ForwardVector * 3, false, false);
            prop.IsCollisionEnabled = false;
            prop.Heading = Game.PlayerPed.Heading;
            return new Tuple<Prop, String>(prop, modelName);
        }

        public static async Task<Tuple<Prop, string>> CreateModelForRenderInPosition(String renderTarget, String modelName, Vector3 position, float heading)
        {
            Model model = new Model(modelName);
            model.Request();

            var prop = await World.CreateProp(model, position, false, false);
            prop.IsCollisionEnabled = false;
            prop.Heading = heading;
            return new Tuple<Prop, String>(prop, modelName);
        }

        private static DuiContainer AddDuiInternal(string renderTarget, string modelName, string url)
        {
            var duiContainer = new DuiContainer();
            var res = SetupScreen(url, renderTarget, modelName);
            duiContainer.TxdHandle = res.Item1;
            duiContainer.TargetHandle = res.Item2;
            duiContainer.duiObj = res.Item3;
            duiContainer.UniqId = res.Item4;
            duiContainer.RenderTargetName = renderTarget;
            return duiContainer;
        }

        private static Tuple<long, int, long, string> SetupScreen(String url, String renderTargetName, String modelName)
        {
            var model = API.GetHashKey(modelName);
            var uniqID = Guid.NewGuid();

            //var scale = 1.5;
            var screenWidth = Screen.Width; // / scale);
            var screenHeight = Screen.Height; //  / scale);
            var handle = CreateNamedRenderTargetForModel(renderTargetName, (uint)model);
            var txd = API.CreateRuntimeTxd($"{renderTargetName}-{uniqID}");
            var duiObj = API.CreateDui(url, (int)screenWidth, (int)screenHeight);

            var dui = API.GetDuiHandle(duiObj);
            var tx = Function.Call<long>((Hash)((uint)Hash.CREATE_RUNTIME_TEXTURE_FROM_DUI_HANDLE & 0xFFFFFFFF), txd, $"{renderTargetName}-{uniqID}-test", dui);

            return new Tuple<long, int, long, string>(tx, handle, duiObj, uniqID.ToString());
        }

        private static int CreateNamedRenderTargetForModel(string name, uint model)
        {
            var handle = 0;
            if (!IsNamedRendertargetRegistered(name))
            {
                RegisterNamedRendertarget(name, false);
            }

            if (!IsNamedRendertargetLinked(model))
            {
                LinkNamedRendertarget(model);
            }

            if (IsNamedRendertargetRegistered(name))
                handle = GetNamedRendertargetRenderId(name);
            return handle;
        }
    }
}
