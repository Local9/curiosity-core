using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.System.Client.Environment.Entities;
using Curiosity.System.Client.Environment.Entities.Models;
using Curiosity.System.Client.Managers;
using System.Linq;
using System.Threading.Tasks;
using Style = Curiosity.System.Library.Models.Style;

namespace Curiosity.System.Client.Extensions
{
    public static class StyleExtensions
    {
        public static Style Merge(this Style style, Style target, params string[] components)
        {
            var merged = new Style();

            foreach (var component in style.GetType().GetProperties().Select(self => self.Name))
            {
                merged.GetByName(component).Current = style.GetByName(component).Current;
            }

            foreach (var component in StyleManager.GetModule().Cached
                .Where(self => components.Select(name => name.ToLower()).Contains(self.Seed.ToLower()))
                .SelectMany(self => self.Group).Select(self => self.Seed))
            {
                merged.GetByName(component).Current = target.GetByName(component).Current;
            }

            return merged;
        }

        public static int Differ(this Style source, Style target)
        {
            var type = typeof(Style);
            var amount = 0;

            foreach (var property in type.GetProperties().Select(self => self.Name).ToList())
            {
                if (source.GetByName(property).Current != target.GetByName(property).Current) amount++;
            }

            return amount;
        }

        public static void Update(this Style style, CuriosityPlayer player)
        {
            var handle = player.Entity.Id;

            style.Sex.Maximum = 1;
            style.Face.Maximum = 45;
            style.Skin.Maximum = 45;
            style.Wrinkles.Maximum = API.GetNumHeadOverlayValues(3) - 1;
            style.WrinklesThickness.Maximum = 10;
            style.Beard.Maximum = API.GetNumHeadOverlayValues(1) - 1;
            style.BeardSize.Maximum = 10;
            style.BeardColorPrimary.Maximum = API.GetNumHairColors() - 1;
            style.BeardColorSecondary.Maximum = API.GetNumHairColors() - 1;
            style.Hair.Maximum = API.GetNumberOfPedDrawableVariations(handle, 2) - 1;
            style.HairThickness.Maximum = API.GetNumberOfPedTextureVariations(handle, 2, style.Hair.Current) - 1;
            style.HairColorPrimary.Maximum = API.GetNumHairColors() - 1;
            style.HairColorSecondary.Maximum = API.GetNumHairColors() - 1;
            style.EyeColor.Maximum = 31;
            style.Eyebrows.Maximum = API.GetNumHeadOverlayValues(2) - 1;
            style.EyebrowsThickness.Maximum = 10;
            style.EyebrowsColorPrimary.Maximum = API.GetNumHairColors() - 1;
            style.EyebrowsColorSecondary.Maximum = API.GetNumHairColors() - 1;
            style.Makeup.Maximum = API.GetNumHairColors() - 1;
            style.MakeupThickness.Maximum = 10;
            style.MakeupColorPrimary.Maximum = API.GetNumHairColors() - 1;
            style.MakeupColorSecondary.Maximum = API.GetNumHairColors() - 1;
            style.Lipstick.Maximum = API.GetNumHeadOverlayValues(8) - 1;
            style.LipstickThickness.Maximum = 10;
            style.LipstickColorPrimary.Maximum = API.GetNumHairColors() - 1;
            style.LipstickColorSecondary.Maximum = API.GetNumHairColors() - 1;
            style.Blush.Maximum = API.GetNumHeadOverlayValues(5) - 1;
            style.BlushThickness.Maximum = 10;
            style.BlushPrimaryColor.Maximum = API.GetNumHairColors() - 1;
            style.BlushSecondaryColor.Maximum = API.GetNumHairColors() - 1;
            style.Complexion.Maximum = API.GetNumHeadOverlayValues(6) - 1;
            style.ComplexionThickness.Maximum = 10;
            style.Sunburn.Maximum = API.GetNumHeadOverlayValues(7) - 1;
            style.SunburnThickness.Maximum = 10;
            style.Freckles.Maximum = API.GetNumHeadOverlayValues(9) - 1;
            style.FrecklesThickness.Maximum = 10;
            style.ChestHair.Maximum = API.GetNumHeadOverlayValues(10) - 1;
            style.ChestHairType.Maximum = 10;
            style.ChestHairPrimaryColor.Maximum = API.GetNumHairColors() - 1;
            style.ChestHairSecondaryColor.Maximum = API.GetNumHairColors() - 1;
            style.EarAccessories.Maximum = API.GetNumberOfPedPropDrawableVariations(handle, 1) - 1;
            style.EarAccessoriesType.Maximum =
                API.GetNumberOfPedPropTextureVariations(handle, 1, style.EarAccessories.Current) - 1;
            style.Shirt.Maximum = API.GetNumberOfPedDrawableVariations(handle, 8) - 1;
            style.ShirtType.Maximum = API.GetNumberOfPedTextureVariations(handle, 8, style.Shirt.Current) - 1;
            style.Torso.Maximum = API.GetNumberOfPedDrawableVariations(handle, 11) - 1;
            style.TorsoType.Maximum = API.GetNumberOfPedTextureVariations(handle, 11, style.Torso.Current) - 1;
            style.Decals.Maximum = API.GetNumberOfPedDrawableVariations(handle, 10) - 1;
            style.DecalsType.Maximum = API.GetNumberOfPedTextureVariations(handle, 10, style.Decals.Current) - 1;
            style.Body.Maximum = API.GetNumberOfPedDrawableVariations(handle, 3) - 1;
            style.BodyType.Maximum = 10;
            style.Pants.Maximum = API.GetNumberOfPedDrawableVariations(handle, 4) - 1;
            style.PantsType.Maximum = API.GetNumberOfPedTextureVariations(handle, 4, style.Pants.Current) - 1;
            style.Shoes.Maximum = API.GetNumberOfPedDrawableVariations(handle, 6) - 1;
            style.ShoesType.Maximum = API.GetNumberOfPedTextureVariations(handle, 6, style.Shoes.Current) - 1;
            style.Mask.Maximum = API.GetNumberOfPedDrawableVariations(handle, 1) - 1;
            style.MaskType.Maximum = API.GetNumberOfPedTextureVariations(handle, 1, style.Mask.Current) - 1;
            style.BodyArmor.Maximum = API.GetNumberOfPedDrawableVariations(handle, 9) - 1;
            style.BodyArmorColor.Maximum =
                API.GetNumberOfPedTextureVariations(handle, 9, style.BodyArmor.Current) - 1;
            style.Neck.Maximum = API.GetNumberOfPedDrawableVariations(handle, 7) - 1;
            style.NeckType.Maximum = API.GetNumberOfPedTextureVariations(handle, 7, style.Neck.Current) - 1;
            style.Bag.Maximum = API.GetNumberOfPedDrawableVariations(handle, 5) - 1;
            style.BagColor.Maximum = API.GetNumberOfPedTextureVariations(handle, 5, style.Bag.Current) - 1;
            style.Head.Maximum = API.GetNumberOfPedPropDrawableVariations(handle, 0) - 1;
            style.HeadType.Maximum = API.GetNumberOfPedPropTextureVariations(handle, 0, style.Head.Current) - 1;
            style.Glasses.Maximum = API.GetNumberOfPedPropDrawableVariations(handle, 1) - 1;
            style.GlassesType.Maximum = API.GetNumberOfPedPropTextureVariations(handle, 1, style.Glasses.Current) - 1;
            style.Watch.Maximum = API.GetNumberOfPedPropDrawableVariations(handle, 6) - 1;
            style.WatchType.Maximum = API.GetNumberOfPedPropTextureVariations(handle, 6, style.Watch.Current) - 1;
            style.Wristband.Maximum = API.GetNumberOfPedPropDrawableVariations(handle, 7) - 1;
            style.WristbandType.Maximum =
                API.GetNumberOfPedPropTextureVariations(handle, 7, style.Wristband.Current) - 1;
        }

        public static async Task Commit(this Style style, CuriosityPlayer player, bool overrideStyle = true)
        {
            var hash = style.Sex.Current == style.Sex.Minimum
                ? API.GetHashKey("mp_m_freemode_01")
                : API.GetHashKey("mp_f_freemode_01");

            if (Game.PlayerPed.Model.Hash != hash)
            {
                var model = new Model(hash);

                if (model.IsInCdImage && model.IsValid)
                {
                    await player.CommitModel(model);

                    var ped = Game.PlayerPed.Handle;

                    player.Entity.SetDefaultStyle();
                    player.Entity.Id = ped;
                    player.Entity.AnimationQueue = new AnimationQueue(ped);

                    Session.Join(Session.LastSession);

                    await BaseScript.Delay(10);
                }
            }

            var handle = player.Entity.Id;

            API.SetPedHeadBlendData(handle, style.Face.Current, style.Face.Current, style.Face.Current,
                style.Skin.Current,
                style.Skin.Current, style.Skin.Current, style.Sex.Current == 0 ? 1f : 0f, 1f, 1f, true);
            API.SetPedHairColor(handle, style.HairColorPrimary.Current, style.HairColorSecondary.Current);
            API.SetPedHeadOverlay(handle, 3, style.Wrinkles.Current, style.WrinklesThickness.Current / 10f);
            API.SetPedHeadOverlay(handle, 1, style.Beard.Current,
                style.BeardSize.Current / 10f);
            API.SetPedEyeColor(handle, style.EyeColor.Current);
            API.SetPedHeadOverlay(handle, 2, style.Eyebrows.Current, style.EyebrowsThickness.Current / 10f);
            API.SetPedHeadOverlay(handle, 4, style.Makeup.Current, style.MakeupThickness.Current / 10f);
            API.SetPedHeadOverlay(handle, 8, style.Lipstick.Current, style.LipstickThickness.Current / 10f);
            API.SetPedComponentVariation(handle, 2, style.Hair.Current, style.HairThickness.Current, 2);
            API.SetPedHeadOverlayColor(handle, 1, 1, style.BeardColorPrimary.Current,
                style.BeardColorSecondary.Current);
            API.SetPedHeadOverlayColor(handle, 2, 1, style.EyebrowsColorPrimary.Current, style.EyebrowsColorSecondary.Current);
            API.SetPedHeadOverlayColor(handle, 4, 1, style.Makeup.Current, style.MakeupThickness.Current);
            API.SetPedHeadOverlayColor(handle, 8, 1, style.LipstickColorPrimary.Current,
                style.LipstickColorSecondary.Current);
            API.SetPedHeadOverlay(handle, 5, style.Blush.Current, style.BlushThickness.Current / 10f);
            API.SetPedHeadOverlayColor(handle, 5, 2, style.BlushPrimaryColor.Current,
                style.BlushSecondaryColor.Current);
            API.SetPedHeadOverlay(handle, 6, style.Complexion.Current,
                style.ComplexionThickness.Current / 10f);
            API.SetPedHeadOverlay(handle, 7, style.Sunburn.Current, style.SunburnThickness.Current / 10f);
            API.SetPedHeadOverlay(handle, 9, style.Freckles.Current,
                style.FrecklesThickness.Current / 10f);
            API.SetPedHeadOverlay(handle, 10, style.ChestHair.Current, style.ChestHairType.Current / 10f);
            API.SetPedHeadOverlayColor(handle, 10, 1, style.ChestHairPrimaryColor.Current,
                style.ChestHairSecondaryColor.Current);

            if (style.EarAccessories.Current == -1)
            {
                API.ClearPedProp(handle, 2);
            }
            else
            {
                API.SetPedPropIndex(handle, 2, style.EarAccessories.Current, style.EarAccessoriesType.Current, true);
            }

            API.SetPedComponentVariation(handle, 8, style.Shirt.Current, style.ShirtType.Current, 2);
            API.SetPedComponentVariation(handle, 11, style.Torso.Current, style.TorsoType.Current, 2);
            API.SetPedComponentVariation(handle, 3, style.Body.Current, style.BodyType.Current, 2);
            API.SetPedComponentVariation(handle, 10, style.Decals.Current, style.DecalsType.Current, 2);
            API.SetPedComponentVariation(handle, 4, style.Pants.Current, style.PantsType.Current, 2);
            API.SetPedComponentVariation(handle, 6, style.Shoes.Current, style.ShoesType.Current, 2);
            API.SetPedComponentVariation(handle, 1, style.Mask.Current, style.MaskType.Current, 2);
            API.SetPedComponentVariation(handle, 9, style.BodyArmor.Current, style.BodyArmorColor.Current, 2);
            API.SetPedComponentVariation(handle, 7, style.Neck.Current, style.NeckType.Current, 2);
            API.SetPedComponentVariation(handle, 5, style.Bag.Current, style.BagColor.Current, 2);

            if (style.Head.Current == -1)
            {
                API.ClearPedProp(handle, 0);
            }
            else
            {
                API.SetPedPropIndex(handle, 0, style.Head.Current, style.HeadType.Current, true);
            }

            if (style.Glasses.Current == -1)
            {
                API.ClearPedProp(handle, 1);
            }
            else
            {
                API.SetPedPropIndex(handle, 1, style.Glasses.Current, style.GlassesType.Current, true);
            }

            if (style.Watch.Current == -1)
            {
                API.ClearPedProp(handle, 6);
            }
            else
            {
                API.SetPedPropIndex(handle, 6, style.Watch.Current, style.WatchType.Current, true);
            }

            if (style.Wristband.Current == -1)
            {
                API.ClearPedProp(handle, 7);
            }
            else
            {
                API.SetPedPropIndex(handle, 7, style.Wristband.Current, style.WristbandType.Current, true);
            }

            if (overrideStyle) player.Character.Style = style;

            style.Update(player);
        }
    }
}