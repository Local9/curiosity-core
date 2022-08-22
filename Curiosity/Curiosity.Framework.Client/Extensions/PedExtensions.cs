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
    }
}
