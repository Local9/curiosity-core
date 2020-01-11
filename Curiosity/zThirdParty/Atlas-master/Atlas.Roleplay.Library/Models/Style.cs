using Newtonsoft.Json;

namespace Atlas.Roleplay.Library.Models
{
    public class Style
    {
        public StyleComponent Sex { get; set; } = new StyleComponent("Kön");
        public StyleComponent Skin { get; set; } = new StyleComponent("Hudfärg");
        public StyleComponent Face { get; set; } = new StyleComponent("Ansikte");
        public StyleComponent Wrinkles { get; set; } = new StyleComponent("Rynkor");
        public StyleComponent WrinklesThickness { get; set; } = new StyleComponent("Tjocklek");
        public StyleComponent Freckles { get; set; } = new StyleComponent("Fränkar");
        public StyleComponent FrecklesThickness { get; set; } = new StyleComponent("Tjocklek");
        public StyleComponent EyeColor { get; set; } = new StyleComponent("Ögonfärg");
        public StyleComponent Sunburn { get; set; } = new StyleComponent("Solbränna");
        public StyleComponent SunburnThickness { get; set; } = new StyleComponent("Tjocklek");
        public StyleComponent Complexion { get; set; } = new StyleComponent("Skador");
        public StyleComponent ComplexionThickness { get; set; } = new StyleComponent("Skador storlek");
        public StyleComponent Hair { get; set; } = new StyleComponent("Hår");
        public StyleComponent HairThickness { get; set; } = new StyleComponent("Tjocklek");
        public StyleComponent HairColorPrimary { get; set; } = new StyleComponent("Primär färg");
        public StyleComponent HairColorSecondary { get; set; } = new StyleComponent("Sekundär färg");
        public StyleComponent Eyebrows { get; set; } = new StyleComponent("Ögonbryn");
        public StyleComponent EyebrowsThickness { get; set; } = new StyleComponent("Tjocklek");
        public StyleComponent EyebrowsColorPrimary { get; set; } = new StyleComponent("Primär färg");
        public StyleComponent EyebrowsColorSecondary { get; set; } = new StyleComponent("Sekundär färg");
        public StyleComponent Beard { get; set; } = new StyleComponent("Skägg");
        public StyleComponent BeardSize { get; set; } = new StyleComponent("Storlek");
        public StyleComponent BeardColorPrimary { get; set; } = new StyleComponent("Primär färg");
        public StyleComponent BeardColorSecondary { get; set; } = new StyleComponent("Sekundär färg");
        public StyleComponent ChestHair { get; set; } = new StyleComponent("Brösthår");
        public StyleComponent ChestHairType { get; set; } = new StyleComponent("Typ");
        public StyleComponent ChestHairPrimaryColor { get; set; } = new StyleComponent("Primär färg");
        public StyleComponent ChestHairSecondaryColor { get; set; } = new StyleComponent("Sekundär färg");
        public StyleComponent Blush { get; set; } = new StyleComponent("Målning");
        public StyleComponent BlushThickness { get; set; } = new StyleComponent("Tjocklek");
        public StyleComponent BlushPrimaryColor { get; set; } = new StyleComponent("Primär färg");
        public StyleComponent BlushSecondaryColor { get; set; } = new StyleComponent("Sekundär färg");
        public StyleComponent Makeup { get; set; } = new StyleComponent("Smink");
        public StyleComponent MakeupThickness { get; set; } = new StyleComponent("Tjocklek");
        public StyleComponent MakeupColorPrimary { get; set; } = new StyleComponent("Primär färg");
        public StyleComponent MakeupColorSecondary { get; set; } = new StyleComponent("Sekundär färg");
        public StyleComponent Lipstick { get; set; } = new StyleComponent("Läppstift");
        public StyleComponent LipstickThickness { get; set; } = new StyleComponent("Tjocklek");
        public StyleComponent LipstickColorPrimary { get; set; } = new StyleComponent("Primär färg");
        public StyleComponent LipstickColorSecondary { get; set; } = new StyleComponent("Sekundär färg");
        public StyleComponent Shirt { get; set; } = new StyleComponent("Tröjor / T-Shirts");
        public StyleComponent ShirtType { get; set; } = new StyleComponent("Färg / Typ");
        public StyleComponent Torso { get; set; } = new StyleComponent("Tröjor / Jackor");
        public StyleComponent TorsoType { get; set; } = new StyleComponent("Färg / Typ");
        public StyleComponent Decals { get; set; } = new StyleComponent("Dekaler");
        public StyleComponent DecalsType { get; set; } = new StyleComponent("Dekal Typ");
        public StyleComponent Body { get; set; } = new StyleComponent("Kropp / Armar");
        public StyleComponent BodyType { get; set; } = new StyleComponent("Färg / Typ");
        public StyleComponent Pants { get; set; } = new StyleComponent("Byxor");
        public StyleComponent PantsType { get; set; } = new StyleComponent("Färg / Typ");
        public StyleComponent Shoes { get; set; } = new StyleComponent("Skor");
        public StyleComponent ShoesType { get; set; } = new StyleComponent("Färg / Typ");
        public StyleComponent BodyArmor { get; set; } = new StyleComponent("Skyddsvästar");
        public StyleComponent BodyArmorColor { get; set; } = new StyleComponent("Färg / Typ");
        public StyleComponent Neck { get; set; } = new StyleComponent("Scarfs / Halsband / Smycke");
        public StyleComponent NeckType { get; set; } = new StyleComponent("Färg / Typ");
        public StyleComponent Head { get; set; } = new StyleComponent("Huvudbonad / Hjälmar ", -1);
        public StyleComponent HeadType { get; set; } = new StyleComponent("Färg / Typ");
        public StyleComponent Glasses { get; set; } = new StyleComponent("Glasögon / Skyddsglasögon");
        public StyleComponent GlassesType { get; set; } = new StyleComponent("Färg / Typ");
        public StyleComponent Watch { get; set; } = new StyleComponent("Klockor", -1);
        public StyleComponent WatchType { get; set; } = new StyleComponent("Färg / Typ");
        public StyleComponent Wristband { get; set; } = new StyleComponent("Armband", -1);
        public StyleComponent WristbandType { get; set; } = new StyleComponent("Färg / Typ");
        public StyleComponent EarAccessories { get; set; } = new StyleComponent("Öron accessoarer", -1);
        public StyleComponent EarAccessoriesType { get; set; } = new StyleComponent("Typ / Färg");
        public StyleComponent Bag { get; set; } = new StyleComponent("Väskor");
        public StyleComponent BagColor { get; set; } = new StyleComponent("Färg / Typ");
        public StyleComponent Mask { get; set; } = new StyleComponent("Masker");
        public StyleComponent MaskType { get; set; } = new StyleComponent("Färg / Typ");

        public StyleComponent GetByName(string name)
        {
            switch (name.ToUpper())
            {
                case "SEX":
                    return Sex;
                case "FACE":
                    return Face;
                case "COMPLEXION":
                    return Complexion;
                case "COMPLEXIONTHICKNESS":
                    return ComplexionThickness;
                case "SKIN":
                    return Skin;
                case "HAIR":
                    return Hair;
                case "HAIRTHICKNESS":
                    return HairThickness;
                case "HAIRCOLORPRIMARY":
                    return HairColorPrimary;
                case "HAIRCOLORSECONDARY":
                    return HairColorSecondary;
                case "SHIRT":
                    return Shirt;
                case "SHIRTTYPE":
                    return ShirtType;
                case "TORSO":
                    return Torso;
                case "TORSOTYPE":
                    return TorsoType;
                case "DECALS":
                    return Decals;
                case "DECALSTYPE":
                    return DecalsType;
                case "BODY":
                    return Body;
                case "BODYTYPE":
                    return BodyType;
                case "PANTS":
                    return Pants;
                case "PANTSTYPE":
                    return PantsType;
                case "SHOES":
                    return Shoes;
                case "SHOESTYPE":
                    return ShoesType;
                case "MASK":
                    return Mask;
                case "MASKTYPE":
                    return MaskType;
                case "BODYARMOR":
                    return BodyArmor;
                case "BODYARMORCOLOR":
                    return BodyArmorColor;
                case "NECK":
                    return Neck;
                case "NECKTYPE":
                    return NeckType;
                case "HEAD":
                    return Head;
                case "HEADTYPE":
                    return HeadType;
                case "GLASSES":
                    return Glasses;
                case "GLASSESTYPE":
                    return GlassesType;
                case "WATCH":
                    return Watch;
                case "WATCHTYPE":
                    return WatchType;
                case "WRISTBAND":
                    return Wristband;
                case "WRISTBANDTYPE":
                    return WristbandType;
                case "BAG":
                    return Bag;
                case "BAGCOLOR":
                    return BagColor;
                case "EYECOLOR":
                    return EyeColor;
                case "EYEBROWS":
                    return Eyebrows;
                case "EYEBROWSTHICKNESS":
                    return EyebrowsThickness;
                case "EYEBROWSCOLORPRIMARY":
                    return EyebrowsColorPrimary;
                case "EYEBROWSCOLORSECONDARY":
                    return EyebrowsColorSecondary;
                case "MAKEUP":
                    return Makeup;
                case "MAKEUPTHICKNESS":
                    return MakeupThickness;
                case "MAKEUPCOLORPRIMARY":
                    return MakeupColorPrimary;
                case "MAKEUPCOLORSECONDARY":
                    return MakeupColorSecondary;
                case "LIPSTICK":
                    return Lipstick;
                case "LIPSTICKTHICKNESS":
                    return LipstickThickness;
                case "LIPSTICKCOLORPRIMARY":
                    return LipstickColorPrimary;
                case "LIPSTICKCOLORSECONDARY":
                    return LipstickColorSecondary;
                case "EARACCESSORIES":
                    return EarAccessories;
                case "EARACCESSORIESTYPE":
                    return EarAccessoriesType;
                case "CHESTHAIR":
                    return ChestHair;
                case "CHESTHAIRTYPE":
                    return ChestHairType;
                case "CHESTHAIRPRIMARYCOLOR":
                    return ChestHairPrimaryColor;
                case "CHESTHAIRSECONDARYCOLOR":
                    return ChestHairSecondaryColor;
                case "WRINKLES":
                    return Wrinkles;
                case "WRINKLESTHICKNESS":
                    return WrinklesThickness;
                case "BLUSH":
                    return Blush;
                case "BLUSHTHICKNESS":
                    return BlushThickness;
                case "BLUSHPRIMARYCOLOR":
                    return BlushPrimaryColor;
                case "BLUSHSECONDARYCOLOR":
                    return BlushSecondaryColor;
                case "SUNBURN":
                    return Sunburn;
                case "SUNBURNTHICKNESS":
                    return SunburnThickness;
                case "FRECKLES":
                    return Freckles;
                case "FRECKLESTHICKNESS":
                    return FrecklesThickness;
                case "BEARD":
                    return Beard;
                case "BEARDSIZE":
                    return BeardSize;
                case "BEARDCOLORPRIMARY":
                    return BeardColorPrimary;
                case "BEARDCOLORSECONDARY":
                    return BeardColorSecondary;
                default:
                    return null;
            }
        }
    }

    public class StyleComponent
    {
        [JsonIgnore] public string Label { get; set; }
        [JsonIgnore] public int Maximum { get; set; }
        [JsonIgnore] public int Minimum { get; set; }
        public int Current { get; set; }

        public StyleComponent(string label)
        {
            Label = label;
            Current = 0;
        }

        public StyleComponent(string label, int minimum)
        {
            Label = label;
            Minimum = minimum;
            Current = minimum;
        }
    }
}