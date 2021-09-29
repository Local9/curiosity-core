using CitizenFX.Core;
using CitizenFX.Core.Native;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.Racing.Client.Environment.Entities.Models
{
    public class MugshotBoardAttachment
    {
        public bool IsAttached { get; set; }

        public async Task Attach(CuriosityPlayer player)
        {
            while (IsAttached)
            {
                await BaseScript.Delay(100);
            }

            IsAttached = true;

            World.GetAllProps()
                .Where(self =>
                    self.Model.Hash == API.GetHashKey("prop_police_id_text") ||
                    self.Model.Hash == API.GetHashKey("prop_police_id_board")).ToList().ForEach(self => self.Delete());

            var textHash = API.GetHashKey("prop_police_id_text");
            var board = API.CreateObject(API.GetHashKey("prop_police_id_board"), 1, 1, 1, false, false, false);
            var text = API.CreateObject(textHash, 1, 1, 1, false, false, false);

            API.AttachEntityToEntity(board, player.Entity.Id, API.GetPedBoneIndex(API.GetPlayerPed(-1), 58868),
                0.12f, 0.24f,
                0.0f, 5.0f, 0.0f, 70.0f, true, true, false, false, 2, true);
            API.AttachEntityToEntity(text, player.Entity.Id, API.GetPedBoneIndex(API.GetPlayerPed(-1), 58868),
                0.12f, 0.24f,
                0.0f, 5.0f, 0.0f, 70.0f, true, true, false, false, 2, true);

            var handle = CreateScaleformHandle("ID_Text", textHash);
            var movie = API.RequestScaleformMovie("MUGSHOT_BOARD_01");

            while (!API.HasScaleformMovieLoaded(movie))
            {
                await BaseScript.Delay(10);
            }

            while (API.HasScaleformMovieLoaded(movie) && IsAttached)
            {
                API.PushScaleformMovieFunction(movie, "SET_BOARD");
                API.PushScaleformMovieFunctionParameterString($"Life V #{player.User.UserId}"); // Top Line
                API.PushScaleformMovieFunctionParameterString(player.User.LatestName); // Main Line
                API.PushScaleformMovieFunctionParameterString($"LOS SANTOS POLICE DEPT"); // Bottom
                API.PushScaleformMovieFunctionParameterString($"${player.Character.Cash}"); // Sub top line
                API.PopScaleformMovieFunctionVoid();
                API.SetTextRenderId(handle);

                Function.Call((Hash)0x40332D115A898AF5, movie, true);

                API.SetUiLayer(4);

                Function.Call((Hash)0xc6372ecd45d73bcd, movie, true);

                API.DrawScaleformMovie(movie, 0.4f, 0.35f, 0.8f, 0.75f, 255, 255, 255, 255, 255);
                API.SetTextRenderId(API.GetDefaultScriptRendertargetRenderId());

                Function.Call((Hash)0x40332D115A898AF5, movie, false);

                await BaseScript.Delay(0);
            }

            API.DeleteEntity(ref board);
            API.DeleteEntity(ref text);

            IsAttached = false;
        }

        private int CreateScaleformHandle(string name, int model)
        {
            var handle = 0;

            if (!API.IsNamedRendertargetRegistered(name))
            {
                API.RegisterNamedRendertarget(name, false);
            }

            if (!API.IsNamedRendertargetLinked((uint)model))
            {
                API.LinkNamedRendertarget((uint)model);
            }

            if (API.IsNamedRendertargetRegistered(name))
            {
                handle = API.GetNamedRendertargetRenderId(name);
            }

            return handle;
        }
    }
}