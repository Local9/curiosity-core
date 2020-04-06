using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Curiosity.Systems.Client.Interface.Menus.Data
{
    class CharacterClothing
    {
        public const int MAX_TOP = 118;
        public const int MAX_PANTS = 57;
        public const int MAX_SHOE = 50;
        public const int MAX_HAT = 32;
        public const int MAX_WATCH = 12;
        public const int MAX_EARPIECE = 15;
        public const int MAX_GLASSES = 12;

        public static void SetPedTop(Ped ped, int topType)
        {
            // Keep these 4 variations together.
            // It avoids empty arms or noisy clothes superposition
            switch (topType)
            {
                case 0:
                    API.SetPedComponentVariation(ped.Handle, 3, 15, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 15, 0, 2); // Torso 2
                    break;
                case 1:
                    API.SetPedComponentVariation(ped.Handle, 3, 0, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 0, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 0, 1, 2); // Torso 2
                    break;
                case 2:
                    API.SetPedComponentVariation(ped.Handle, 3, 0, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 0, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 0, 7, 2); // Torso 2
                    break;
                case 3:
                    API.SetPedComponentVariation(ped.Handle, 3, 2, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 2, 9, 2); // Torso 2
                    break;
                case 4:
                    API.SetPedComponentVariation(ped.Handle, 3, 6, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 5, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 3, 11, 2); // Torso 2
                    break;
                case 5:
                    API.SetPedComponentVariation(ped.Handle, 3, 6, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 5, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 3, 15, 2); // Torso 2
                    break;
                case 6:
                    API.SetPedComponentVariation(ped.Handle, 3, 6, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 23, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 4, 0, 2); // Torso 2
                    break;
                case 7:
                    API.SetPedComponentVariation(ped.Handle, 3, 6, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 4, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 4, 0, 2); // Torso 2
                    break;
                case 8:
                    API.SetPedComponentVariation(ped.Handle, 3, 6, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 26, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 4, 0, 2); // Torso 2
                    break;
                case 9:
                    API.SetPedComponentVariation(ped.Handle, 3, 5, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 5, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 5, 0, 2); // Torso 2
                    break;
                case 10:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 2, 4, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 6, 11, 2); // Torso 2
                    break;
                case 11:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 2, 4, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 6, 0, 2); // Torso 2
                    break;
                case 12:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 2, 4, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 6, 3, 2); // Torso 2
                    break;
                case 13:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 23, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 7, 4, 2); // Torso 2
                    break;
                case 14:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 23, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 7, 10, 2); // Torso 2
                    break;
                case 15:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 23, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 7, 12, 2); // Torso 2
                    break;
                case 16:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 23, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 7, 13, 2); // Torso 2
                    break;
                case 17:
                    API.SetPedComponentVariation(ped.Handle, 3, 0, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 9, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 9, 0, 2); // Torso 2
                    break;
                case 18:
                    API.SetPedComponentVariation(ped.Handle, 3, 4, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 10, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 10, 0, 2); // Torso 2
                    break;
                case 19:
                    API.SetPedComponentVariation(ped.Handle, 3, 4, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 12, 2, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 10, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 10, 0, 2); // Torso 2
                    break;
                case 20:
                    API.SetPedComponentVariation(ped.Handle, 3, 4, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 18, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 10, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 10, 0, 2); // Torso 2
                    break;
                case 21:
                    API.SetPedComponentVariation(ped.Handle, 3, 4, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 11, 2, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 10, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 10, 0, 2); // Torso 2
                    break;
                case 22:
                    API.SetPedComponentVariation(ped.Handle, 3, 12, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 12, 10, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 12, 10, 2); // Torso 2
                    break;
                case 23:
                    API.SetPedComponentVariation(ped.Handle, 3, 11, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 13, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 13, 0, 2); // Torso 2
                    break;
                case 24:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 14, 0, 2); // Torso 2
                    break;
                case 25:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 14, 1, 2); // Torso 2
                    break;
                case 26:
                    API.SetPedComponentVariation(ped.Handle, 3, 0, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 16, 0, 2); // Torso 2
                    break;
                case 27:
                    API.SetPedComponentVariation(ped.Handle, 3, 0, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 16, 1, 2); // Torso 2
                    break;
                case 28:
                    API.SetPedComponentVariation(ped.Handle, 3, 0, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 16, 2, 2); // Torso 2
                    break;
                case 29:
                    API.SetPedComponentVariation(ped.Handle, 3, 5, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 17, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 17, 0, 2); // Torso 2
                    break;
                case 30:
                    API.SetPedComponentVariation(ped.Handle, 3, 5, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 17, 1, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 17, 1, 2); // Torso 2
                    break;
                case 31:
                    API.SetPedComponentVariation(ped.Handle, 3, 5, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 17, 4, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 17, 4, 2); // Torso 2
                    break;
                case 32:
                    API.SetPedComponentVariation(ped.Handle, 3, 11, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 27, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 26, 0, 2); // Torso 2
                    break;
                case 33:
                    API.SetPedComponentVariation(ped.Handle, 3, 11, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 27, 5, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 26, 5, 2); // Torso 2
                    break;
                case 34:
                    API.SetPedComponentVariation(ped.Handle, 3, 11, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 27, 6, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 26, 6, 2); // Torso 2
                    break;
                case 35:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 63, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 31, 0, 2); // Torso 2
                    break;
                case 36:
                    API.SetPedComponentVariation(ped.Handle, 3, 5, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 57, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 36, 4, 2); // Torso 2
                    break;
                case 37:
                    API.SetPedComponentVariation(ped.Handle, 3, 5, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 57, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 36, 5, 2); // Torso 2
                    break;
                case 38:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 24, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 37, 0, 2); // Torso 2
                    break;
                case 39:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 24, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 37, 1, 2); // Torso 2
                    break;
                case 40:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 24, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 37, 2, 2); // Torso 2
                    break;
                case 41:
                    API.SetPedComponentVariation(ped.Handle, 3, 8, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 38, 0, 2); // Torso 2
                    break;
                case 42:
                    API.SetPedComponentVariation(ped.Handle, 3, 8, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 38, 3, 2); // Torso 2
                    break;
                case 43:
                    API.SetPedComponentVariation(ped.Handle, 3, 0, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 39, 0, 2); // Torso 2
                    break;
                case 44:
                    API.SetPedComponentVariation(ped.Handle, 3, 0, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 39, 1, 2); // Torso 2
                    break;
                case 45:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 41, 0, 2); // Torso 2
                    break;
                case 46:
                    API.SetPedComponentVariation(ped.Handle, 3, 11, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 42, 0, 2); // Torso 2
                    break;
                case 47:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 50, 0, 2); // Torso 2
                    break;
                case 48:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 50, 3, 2); // Torso 2
                    break;
                case 49:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 50, 4, 2); // Torso 2
                    break;
                case 50:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 57, 0, 2); // Torso 2
                    break;
                case 51:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 50, 1, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 23, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 70, 0, 2); // Torso 2
                    break;
                case 52:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 50, 1, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 23, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 70, 1, 2); // Torso 2
                    break;
                case 53:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 50, 1, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 23, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 70, 7, 2); // Torso 2
                    break;
                case 54:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 3, 1, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 72, 1, 2); // Torso 2
                    break;
                case 55:
                    API.SetPedComponentVariation(ped.Handle, 3, 6, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 87, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 5, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 74, 0, 2); // Torso 2
                    break;
                case 56:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 12, 2, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 28, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 77, 0, 2); // Torso 2
                    break;
                case 57:
                    API.SetPedComponentVariation(ped.Handle, 3, 15, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 79, 0, 2); // Torso 2
                    break;
                case 58:
                    API.SetPedComponentVariation(ped.Handle, 3, 0, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 80, 0, 2); // Torso 2
                    break;
                case 59:
                    API.SetPedComponentVariation(ped.Handle, 3, 0, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 80, 1, 2); // Torso 2
                    break;
                case 60:
                    API.SetPedComponentVariation(ped.Handle, 3, 0, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 82, 5, 2); // Torso 2
                    break;
                case 61:
                    API.SetPedComponentVariation(ped.Handle, 3, 0, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 82, 8, 2); // Torso 2
                    break;
                case 62:
                    API.SetPedComponentVariation(ped.Handle, 3, 0, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 82, 9, 2); // Torso 2
                    break;
                case 63:
                    API.SetPedComponentVariation(ped.Handle, 3, 0, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 86, 0, 2); // Torso 2
                    break;
                case 64:
                    API.SetPedComponentVariation(ped.Handle, 3, 0, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 86, 2, 2); // Torso 2
                    break;
                case 65:
                    API.SetPedComponentVariation(ped.Handle, 3, 0, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 86, 4, 2); // Torso 2
                    break;
                case 66:
                    API.SetPedComponentVariation(ped.Handle, 3, 0, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 87, 11, 2); // Torso 2
                    break;
                case 67:
                    API.SetPedComponentVariation(ped.Handle, 3, 0, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 87, 0, 2); // Torso 2
                    break;
                case 68:
                    API.SetPedComponentVariation(ped.Handle, 3, 0, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 87, 1, 2); // Torso 2
                    break;
                case 69:
                    API.SetPedComponentVariation(ped.Handle, 3, 0, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 87, 2, 2); // Torso 2
                    break;
                case 70:
                    API.SetPedComponentVariation(ped.Handle, 3, 0, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 87, 4, 2); // Torso 2
                    break;
                case 71:
                    API.SetPedComponentVariation(ped.Handle, 3, 0, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 87, 8, 2); // Torso 2
                    break;
                case 72:
                    API.SetPedComponentVariation(ped.Handle, 3, 0, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 89, 0, 2); // Torso 2
                    break;
                case 73:
                    API.SetPedComponentVariation(ped.Handle, 3, 11, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 95, 0, 2); // Torso 2
                    break;
                case 74:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 31, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 99, 1, 2); // Torso 2
                    break;
                case 75:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 31, 13, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 99, 3, 2); // Torso 2
                    break;
                case 76:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 31, 13, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 101, 0, 2); // Torso 2
                    break;
                case 77:
                    API.SetPedComponentVariation(ped.Handle, 3, 0, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 105, 0, 2); // Torso 2
                    break;
                case 78:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 10, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 106, 0, 2); // Torso 2
                    break;
                case 79:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 73, 2, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 109, 0, 2); // Torso 2
                    break;
                case 80:
                    API.SetPedComponentVariation(ped.Handle, 3, 4, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 111, 0, 2); // Torso 2
                    break;
                case 81:
                    API.SetPedComponentVariation(ped.Handle, 3, 4, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 111, 3, 2); // Torso 2
                    break;
                case 82:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 113, 0, 2); // Torso 2
                    break;
                case 83:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 126, 5, 2); // Torso 2
                    break;
                case 84:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 126, 9, 2); // Torso 2
                    break;
                case 85:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 126, 10, 2); // Torso 2
                    break;
                case 86:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 126, 14, 2); // Torso 2
                    break;
                case 87:
                    API.SetPedComponentVariation(ped.Handle, 3, 0, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 131, 0, 2); // Torso 2
                    break;
                case 88:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 134, 0, 2); // Torso 2
                    break;
                case 89:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 134, 1, 2); // Torso 2
                    break;
                case 90:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 143, 0, 2); // Torso 2
                    break;
                case 91:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 143, 2, 2); // Torso 2
                    break;
                case 92:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 143, 4, 2); // Torso 2
                    break;
                case 93:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 143, 5, 2); // Torso 2
                    break;
                case 94:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 143, 6, 2); // Torso 2
                    break;
                case 95:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 143, 8, 2); // Torso 2
                    break;
                case 96:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 143, 9, 2); // Torso 2
                    break;
                case 97:
                    API.SetPedComponentVariation(ped.Handle, 3, 0, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 146, 0, 2); // Torso 2
                    break;
                case 98:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 16, 2, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 166, 0, 2); // Torso 2
                    break;
                case 99:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 38, 1, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 167, 0, 2); // Torso 2
                    break;
                case 100:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 38, 1, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 167, 4, 2); // Torso 2
                    break;
                case 101:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 38, 1, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 167, 6, 2); // Torso 2
                    break;
                case 102:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 38, 1, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 167, 12, 2); // Torso 2
                    break;
                case 103:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 38, 1, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 169, 0, 2); // Torso 2
                    break;
                case 104:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 38, 1, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 172, 0, 2); // Torso 2
                    break;
                case 105:
                    API.SetPedComponentVariation(ped.Handle, 3, 2, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 38, 1, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 173, 0, 2); // Torso 2
                    break;
                case 106:
                    API.SetPedComponentVariation(ped.Handle, 3, 2, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 41, 2, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 185, 0, 2); // Torso 2
                    break;
                case 107:
                    API.SetPedComponentVariation(ped.Handle, 3, 2, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 202, 0, 2); // Torso 2
                    break;
                case 108:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 203, 10, 2); // Torso 2
                    break;
                case 109:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 203, 16, 2); // Torso 2
                    break;
                case 110:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 203, 25, 2); // Torso 2
                    break;
                case 111:
                    API.SetPedComponentVariation(ped.Handle, 3, 2, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 205, 0, 2); // Torso 2
                    break;
                case 112:
                    API.SetPedComponentVariation(ped.Handle, 3, 0, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 226, 0, 2); // Torso 2
                    break;
                case 113:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 257, 0, 2); // Torso 2
                    break;
                case 114:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 257, 9, 2); // Torso 2
                    break;
                case 115:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 257, 17, 2); // Torso 2
                    break;
                case 116:
                    API.SetPedComponentVariation(ped.Handle, 3, 1, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 259, 9, 2); // Torso 2
                    break;
                case 117:
                    API.SetPedComponentVariation(ped.Handle, 3, 5, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 5, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 269, 2, 2); // Torso 2
                    break;
                case 118:
                    API.SetPedComponentVariation(ped.Handle, 3, 0, 0, 2); // Torso
                    API.SetPedComponentVariation(ped.Handle, 7, 0, 0, 2); // Neck
                    API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 2); // Undershirt
                    API.SetPedComponentVariation(ped.Handle, 11, 282, 6, 2); // Torso 2
                    break;
            }
        }

        public static void SetPedPants(Ped ped, int pantsType)
        {
            switch (pantsType)
            {
                case 0:
                    API.SetPedComponentVariation(ped.Handle, 4, 61, 4, 2);
                    break;
                case 1:
                    API.SetPedComponentVariation(ped.Handle, 4, 0, 0, 2);
                    break;
                case 2:
                    API.SetPedComponentVariation(ped.Handle, 4, 0, 2, 2);
                    break;
                case 3:
                    API.SetPedComponentVariation(ped.Handle, 4, 1, 12, 2);
                    break;
                case 4:
                    API.SetPedComponentVariation(ped.Handle, 4, 2, 11, 2);
                    break;
                case 5:
                    API.SetPedComponentVariation(ped.Handle, 4, 3, 0, 2);
                    break;
                case 6:
                    API.SetPedComponentVariation(ped.Handle, 4, 4, 0, 2);
                    break;
                case 7:
                    API.SetPedComponentVariation(ped.Handle, 4, 4, 1, 2);
                    break;
                case 8:
                    API.SetPedComponentVariation(ped.Handle, 4, 4, 4, 2);
                    break;
                case 9:
                    API.SetPedComponentVariation(ped.Handle, 4, 5, 0, 2);
                    break;
                case 10:
                    API.SetPedComponentVariation(ped.Handle, 4, 5, 2, 2);
                    break;
                case 11:
                    API.SetPedComponentVariation(ped.Handle, 4, 7, 0, 2);
                    break;
                case 12:
                    API.SetPedComponentVariation(ped.Handle, 4, 7, 1, 2);
                    break;
                case 13:
                    API.SetPedComponentVariation(ped.Handle, 4, 9, 0, 2);
                    break;
                case 14:
                    API.SetPedComponentVariation(ped.Handle, 4, 9, 1, 2);
                    break;
                case 15:
                    API.SetPedComponentVariation(ped.Handle, 4, 10, 0, 2);
                    break;
                case 16:
                    API.SetPedComponentVariation(ped.Handle, 4, 10, 2, 2);
                    break;
                case 17:
                    API.SetPedComponentVariation(ped.Handle, 4, 12, 0, 2);
                    break;
                case 18:
                    API.SetPedComponentVariation(ped.Handle, 4, 12, 5, 2);
                    break;
                case 19:
                    API.SetPedComponentVariation(ped.Handle, 4, 12, 7, 2);
                    break;
                case 20:
                    API.SetPedComponentVariation(ped.Handle, 4, 14, 0, 2);
                    break;
                case 21:
                    API.SetPedComponentVariation(ped.Handle, 4, 14, 1, 2);
                    break;
                case 22:
                    API.SetPedComponentVariation(ped.Handle, 4, 14, 3, 2);
                    break;
                case 23:
                    API.SetPedComponentVariation(ped.Handle, 4, 15, 0, 2);
                    break;
                case 24:
                    API.SetPedComponentVariation(ped.Handle, 4, 20, 0, 2);
                    break;
                case 25:
                    API.SetPedComponentVariation(ped.Handle, 4, 24, 0, 2);
                    break;
                case 26:
                    API.SetPedComponentVariation(ped.Handle, 4, 24, 1, 2);
                    break;
                case 27:
                    API.SetPedComponentVariation(ped.Handle, 4, 24, 5, 2);
                    break;
                case 28:
                    API.SetPedComponentVariation(ped.Handle, 4, 26, 0, 2);
                    break;
                case 29:
                    API.SetPedComponentVariation(ped.Handle, 4, 26, 4, 2);
                    break;
                case 30:
                    API.SetPedComponentVariation(ped.Handle, 4, 26, 5, 2);
                    break;
                case 31:
                    API.SetPedComponentVariation(ped.Handle, 4, 26, 6, 2);
                    break;
                case 32:
                    API.SetPedComponentVariation(ped.Handle, 4, 28, 0, 2);
                    break;
                case 33:
                    API.SetPedComponentVariation(ped.Handle, 4, 28, 3, 2);
                    break;
                case 34:
                    API.SetPedComponentVariation(ped.Handle, 4, 28, 8, 2);
                    break;
                case 35:
                    API.SetPedComponentVariation(ped.Handle, 4, 28, 14, 2);
                    break;
                case 36:
                    API.SetPedComponentVariation(ped.Handle, 4, 42, 0, 2);
                    break;
                case 37:
                    API.SetPedComponentVariation(ped.Handle, 4, 42, 1, 2);
                    break;
                case 38:
                    API.SetPedComponentVariation(ped.Handle, 4, 48, 0, 2);
                    break;
                case 39:
                    API.SetPedComponentVariation(ped.Handle, 4, 48, 1, 2);
                    break;
                case 40:
                    API.SetPedComponentVariation(ped.Handle, 4, 49, 0, 2);
                    break;
                case 41:
                    API.SetPedComponentVariation(ped.Handle, 4, 49, 1, 2);
                    break;
                case 42:
                    API.SetPedComponentVariation(ped.Handle, 4, 54, 1, 2);
                    break;
                case 43:
                    API.SetPedComponentVariation(ped.Handle, 4, 55, 0, 2);
                    break;
                case 44:
                    API.SetPedComponentVariation(ped.Handle, 4, 60, 2, 2);
                    break;
                case 45:
                    API.SetPedComponentVariation(ped.Handle, 4, 60, 9, 2);
                    break;
                case 46:
                    API.SetPedComponentVariation(ped.Handle, 4, 71, 0, 2);
                    break;
                case 47:
                    API.SetPedComponentVariation(ped.Handle, 4, 75, 2, 2);
                    break;
                case 48:
                    API.SetPedComponentVariation(ped.Handle, 4, 76, 2, 2);
                    break;
                case 49:
                    API.SetPedComponentVariation(ped.Handle, 4, 78, 0, 2);
                    break;
                case 50:
                    API.SetPedComponentVariation(ped.Handle, 4, 78, 2, 2);
                    break;
                case 51:
                    API.SetPedComponentVariation(ped.Handle, 4, 78, 4, 2);
                    break;
                case 52:
                    API.SetPedComponentVariation(ped.Handle, 4, 82, 0, 2);
                    break;
                case 53:
                    API.SetPedComponentVariation(ped.Handle, 4, 82, 2, 2);
                    break;
                case 54:
                    API.SetPedComponentVariation(ped.Handle, 4, 82, 3, 2);
                    break;
                case 55:
                    API.SetPedComponentVariation(ped.Handle, 4, 86, 9, 2);
                    break;
                case 56:
                    API.SetPedComponentVariation(ped.Handle, 4, 88, 9, 2);
                    break;
                case 57:
                    API.SetPedComponentVariation(ped.Handle, 4, 100, 9, 2);
                    break;
            }
        }

        public static void SetPedShoes(Ped ped, int shoeType)
        {
            switch (shoeType)
            {
                case 0:
                    API.SetPedComponentVariation(ped.Handle, 6, 34, 0, 2);
                    break;
                case 1:
                    API.SetPedComponentVariation(ped.Handle, 6, 0, 10, 2);
                    break;
                case 2:
                    API.SetPedComponentVariation(ped.Handle, 6, 1, 0, 2);
                    break;
                case 3:
                    API.SetPedComponentVariation(ped.Handle, 6, 1, 1, 2);
                    break;
                case 4:
                    API.SetPedComponentVariation(ped.Handle, 6, 1, 3, 2);
                    break;
                case 5:
                    API.SetPedComponentVariation(ped.Handle, 6, 3, 0, 2);
                    break;
                case 6:
                    API.SetPedComponentVariation(ped.Handle, 6, 3, 6, 2);
                    break;
                case 7:
                    API.SetPedComponentVariation(ped.Handle, 6, 3, 14, 2);
                    break;
                case 8:
                    API.SetPedComponentVariation(ped.Handle, 6, 48, 0, 2);
                    break;
                case 9:
                    API.SetPedComponentVariation(ped.Handle, 6, 48, 1, 2);
                    break;
                case 10:
                    API.SetPedComponentVariation(ped.Handle, 6, 49, 0, 2);
                    break;
                case 11:
                    API.SetPedComponentVariation(ped.Handle, 6, 49, 1, 2);
                    break;
                case 12:
                    API.SetPedComponentVariation(ped.Handle, 6, 5, 0, 2);
                    break;
                case 13:
                    API.SetPedComponentVariation(ped.Handle, 6, 6, 0, 2);
                    break;
                case 14:
                    API.SetPedComponentVariation(ped.Handle, 6, 7, 0, 2);
                    break;
                case 15:
                    API.SetPedComponentVariation(ped.Handle, 6, 7, 9, 2);
                    break;
                case 16:
                    API.SetPedComponentVariation(ped.Handle, 6, 7, 13, 2);
                    break;
                case 17:
                    API.SetPedComponentVariation(ped.Handle, 6, 9, 3, 2);
                    break;
                case 18:
                    API.SetPedComponentVariation(ped.Handle, 6, 9, 6, 2);
                    break;
                case 19:
                    API.SetPedComponentVariation(ped.Handle, 6, 9, 7, 2);
                    break;
                case 20:
                    API.SetPedComponentVariation(ped.Handle, 6, 10, 0, 2);
                    break;
                case 21:
                    API.SetPedComponentVariation(ped.Handle, 6, 12, 0, 2);
                    break;
                case 22:
                    API.SetPedComponentVariation(ped.Handle, 6, 12, 2, 2);
                    break;
                case 23:
                    API.SetPedComponentVariation(ped.Handle, 6, 12, 13, 2);
                    break;
                case 24:
                    API.SetPedComponentVariation(ped.Handle, 6, 15, 0, 2);
                    break;
                case 25:
                    API.SetPedComponentVariation(ped.Handle, 6, 15, 1, 2);
                    break;
                case 26:
                    API.SetPedComponentVariation(ped.Handle, 6, 16, 0, 2);
                    break;
                case 27:
                    API.SetPedComponentVariation(ped.Handle, 6, 20, 0, 2);
                    break;
                case 28:
                    API.SetPedComponentVariation(ped.Handle, 6, 24, 0, 2);
                    break;
                case 29:
                    API.SetPedComponentVariation(ped.Handle, 6, 27, 0, 2);
                    break;
                case 30:
                    API.SetPedComponentVariation(ped.Handle, 6, 28, 0, 2);
                    break;
                case 31:
                    API.SetPedComponentVariation(ped.Handle, 6, 28, 1, 2);
                    break;
                case 32:
                    API.SetPedComponentVariation(ped.Handle, 6, 28, 3, 2);
                    break;
                case 33:
                    API.SetPedComponentVariation(ped.Handle, 6, 28, 2, 2);
                    break;
                case 34:
                    API.SetPedComponentVariation(ped.Handle, 6, 31, 2, 2);
                    break;
                case 35:
                    API.SetPedComponentVariation(ped.Handle, 6, 31, 4, 2);
                    break;
                case 36:
                    API.SetPedComponentVariation(ped.Handle, 6, 36, 0, 2);
                    break;
                case 37:
                    API.SetPedComponentVariation(ped.Handle, 6, 36, 3, 2);
                    break;
                case 38:
                    API.SetPedComponentVariation(ped.Handle, 6, 42, 0, 2);
                    break;
                case 39:
                    API.SetPedComponentVariation(ped.Handle, 6, 42, 1, 2);
                    break;
                case 40:
                    API.SetPedComponentVariation(ped.Handle, 6, 42, 7, 2);
                    break;
                case 41:
                    API.SetPedComponentVariation(ped.Handle, 6, 57, 0, 2);
                    break;
                case 42:
                    API.SetPedComponentVariation(ped.Handle, 6, 57, 3, 2);
                    break;
                case 43:
                    API.SetPedComponentVariation(ped.Handle, 6, 57, 8, 2);
                    break;
                case 44:
                    API.SetPedComponentVariation(ped.Handle, 6, 57, 9, 2);
                    break;
                case 45:
                    API.SetPedComponentVariation(ped.Handle, 6, 57, 10, 2);
                    break;
                case 46:
                    API.SetPedComponentVariation(ped.Handle, 6, 57, 11, 2);
                    break;
                case 47:
                    API.SetPedComponentVariation(ped.Handle, 6, 75, 4, 2);
                    break;
                case 48:
                    API.SetPedComponentVariation(ped.Handle, 6, 75, 7, 2);
                    break;
                case 49:
                    API.SetPedComponentVariation(ped.Handle, 6, 75, 8, 2);
                    break;
                case 50:
                    API.SetPedComponentVariation(ped.Handle, 6, 77, 0, 2);
                    break;
            }
        }

        public static void SetPedHat(Ped ped, int hatType)
        {
            switch (hatType)
            {
                case 0:
                    API.ClearPedProp(ped.Handle, 0);
                    break;
                case 1:
                    API.SetPedPropIndex(ped.Handle, 0, 2, 0, true);
                    break;
                case 2:
                    API.SetPedPropIndex(ped.Handle, 0, 2, 6, true);
                    break;
                case 3:
                    API.SetPedPropIndex(ped.Handle, 0, 3, 2, true);
                    break;
                case 4:
                    API.SetPedPropIndex(ped.Handle, 0, 4, 0, true);
                    break;
                case 5:
                    API.SetPedPropIndex(ped.Handle, 0, 4, 1, true);
                    break;
                case 6:
                    API.SetPedPropIndex(ped.Handle, 0, 5, 0, true);
                    break;
                case 7:
                    API.SetPedPropIndex(ped.Handle, 0, 7, 0, true);
                    break;
                case 8:
                    API.SetPedPropIndex(ped.Handle, 0, 7, 1, true);
                    break;
                case 9:
                    API.SetPedPropIndex(ped.Handle, 0, 7, 2, true);
                    break;
                case 10:
                    API.SetPedPropIndex(ped.Handle, 0, 7, 5, true);
                    break;
                case 11:
                    API.SetPedPropIndex(ped.Handle, 0, 10, 5, true);
                    break;
                case 12:
                    API.SetPedPropIndex(ped.Handle, 0, 9, 5, true);
                    break;
                case 13:
                    API.SetPedPropIndex(ped.Handle, 0, 10, 7, true);
                    break;
                case 14:
                    API.SetPedPropIndex(ped.Handle, 0, 9, 7, true);
                    break;
                case 15:
                    API.SetPedPropIndex(ped.Handle, 0, 12, 0, true);
                    break;
                case 16:
                    API.SetPedPropIndex(ped.Handle, 0, 12, 1, true);
                    break;
                case 17:
                    API.SetPedPropIndex(ped.Handle, 0, 13, 2, true);
                    break;
                case 18:
                    API.SetPedPropIndex(ped.Handle, 0, 14, 0, true);
                    break;
                case 19:
                    API.SetPedPropIndex(ped.Handle, 0, 14, 1, true);
                    break;
                case 20:
                    API.SetPedPropIndex(ped.Handle, 0, 15, 1, true);
                    break;
                case 21:
                    API.SetPedPropIndex(ped.Handle, 0, 15, 2, true);
                    break;
                case 22:
                    API.SetPedPropIndex(ped.Handle, 0, 20, 5, true);
                    break;
                case 23:
                    API.SetPedPropIndex(ped.Handle, 0, 21, 0, true);
                    break;
                case 24:
                    API.SetPedPropIndex(ped.Handle, 0, 25, 1, true);
                    break;
                case 25:
                    API.SetPedPropIndex(ped.Handle, 0, 26, 0, true);
                    break;
                case 26:
                    API.SetPedPropIndex(ped.Handle, 0, 27, 0, true);
                    break;
                case 27:
                    API.SetPedPropIndex(ped.Handle, 0, 34, 0, true);
                    break;
                case 28:
                    API.SetPedPropIndex(ped.Handle, 0, 55, 0, true);
                    break;
                case 29:
                    API.SetPedPropIndex(ped.Handle, 0, 55, 1, true);
                    break;
                case 30:
                    API.SetPedPropIndex(ped.Handle, 0, 55, 2, true);
                    break;
                case 31:
                    API.SetPedPropIndex(ped.Handle, 0, 76, 19, true);
                    break;
                case 32:
                    API.SetPedPropIndex(ped.Handle, 0, 96, 2, true);
                    break;
            }
        }

        public static void SetPedWatch(Ped ped, int watchType)
        {
            switch (watchType)
            {
                case 0:
                    API.ClearPedProp(ped.Handle, 1);
                    break;
                case 1:
                    API.SetPedPropIndex(ped.Handle, 1, 3, 0, true);
                    break;
                case 2:
                    API.SetPedPropIndex(ped.Handle, 1, 3, 9, true);
                    break;
                case 3:
                    API.SetPedPropIndex(ped.Handle, 1, 4, 4, true);
                    break;
                case 4:
                    API.SetPedPropIndex(ped.Handle, 1, 4, 9, true);
                    break;
                case 5:
                    API.SetPedPropIndex(ped.Handle, 1, 5, 0, true);
                    break;
                case 6:
                    API.SetPedPropIndex(ped.Handle, 1, 5, 8, true);
                    break;
                case 7:
                    API.SetPedPropIndex(ped.Handle, 1, 7, 0, true);
                    break;
                case 8:
                    API.SetPedPropIndex(ped.Handle, 1, 8, 1, true);
                    break;
                case 9:
                    API.SetPedPropIndex(ped.Handle, 1, 9, 0, true);
                    break;
                case 10:
                    API.SetPedPropIndex(ped.Handle, 1, 15, 6, true);
                    break;
                case 11:
                    API.SetPedPropIndex(ped.Handle, 1, 17, 9, true);
                    break;
                case 12:
                    API.SetPedPropIndex(ped.Handle, 1, 25, 0, true);
                    break;
            }
        }

        public static void SetPedEarPiece(Ped ped, int earPieceType)
        {
            switch (earPieceType)
            {
                case 0:
                    API.ClearPedProp(ped.Handle, 2);
                    break;
                case 1:
                    API.SetPedPropIndex(ped.Handle, 2, 3, 0, true);
                    break;
                case 2:
                    API.SetPedPropIndex(ped.Handle, 2, 4, 0, true);
                    break;
                case 3:
                    API.SetPedPropIndex(ped.Handle, 2, 5, 0, true);
                    break;
                case 4:
                    API.SetPedPropIndex(ped.Handle, 2, 9, 1, true);
                    break;
                case 5:
                    API.SetPedPropIndex(ped.Handle, 2, 10, 1, true);
                    break;
                case 6:
                    API.SetPedPropIndex(ped.Handle, 2, 11, 1, true);
                    break;
                case 7:
                    API.SetPedPropIndex(ped.Handle, 2, 18, 3, true);
                    break;
                case 8:
                    API.SetPedPropIndex(ped.Handle, 2, 19, 3, true);
                    break;
                case 9:
                    API.SetPedPropIndex(ped.Handle, 2, 20, 3, true);
                    break;
                case 10:
                    API.SetPedPropIndex(ped.Handle, 2, 27, 0, true);
                    break;
                case 11:
                    API.SetPedPropIndex(ped.Handle, 2, 28, 0, true);
                    break;
                case 12:
                    API.SetPedPropIndex(ped.Handle, 2, 29, 0, true);
                    break;
                case 13:
                    API.SetPedPropIndex(ped.Handle, 2, 30, 0, true);
                    break;
                case 14:
                    API.SetPedPropIndex(ped.Handle, 2, 31, 0, true);
                    break;
                case 15:
                    API.SetPedPropIndex(ped.Handle, 2, 32, 0, true);
                    break;
            }
        }

        public static void SetPedGlasses(Ped ped, int glassesType)
        {
            switch (glassesType)
            {
                case 0:
                    API.ClearPedProp(ped.Handle, 1);
                    break;
                case 1:
                    API.SetPedPropIndex(ped.Handle, 1, 3, 0, true);
                    break;
                case 2:
                    API.SetPedPropIndex(ped.Handle, 1, 3, 9, true);
                    break;
                case 3:
                    API.SetPedPropIndex(ped.Handle, 1, 4, 4, true);
                    break;
                case 4:
                    API.SetPedPropIndex(ped.Handle, 1, 4, 9, true);
                    break;
                case 5:
                    API.SetPedPropIndex(ped.Handle, 1, 5, 0, true);
                    break;
                case 6:
                    API.SetPedPropIndex(ped.Handle, 1, 5, 8, true);
                    break;
                case 7:
                    API.SetPedPropIndex(ped.Handle, 1, 7, 0, true);
                    break;
                case 8:
                    API.SetPedPropIndex(ped.Handle, 1, 8, 1, true);
                    break;
                case 9:
                    API.SetPedPropIndex(ped.Handle, 1, 9, 0, true);
                    break;
                case 10:
                    API.SetPedPropIndex(ped.Handle, 1, 15, 6, true);
                    break;
                case 11:
                    API.SetPedPropIndex(ped.Handle, 1, 17, 9, true);
                    break;
                case 12:
                    API.SetPedPropIndex(ped.Handle, 1, 25, 0, true);
                    break;
            }
        }
    }
}
