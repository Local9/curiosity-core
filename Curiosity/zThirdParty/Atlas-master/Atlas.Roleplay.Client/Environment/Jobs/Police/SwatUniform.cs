using Atlas.Roleplay.Library.Models;

namespace Atlas.Roleplay.Client.Environment.Jobs.Police
{
    public class MaleSwatUniform : Style
    {
        public MaleSwatUniform()
        {
            Shirt.Current = 38;
            Torso.Current = 220;
            TorsoType.Current = 20;
            Decals.Current = 1;
            Body.Current = 17;
            Pants.Current = 31;
            Shoes.Current = 24;
            BodyArmor.Current = 12;
            Head.Current = 123;
            Bag.Current = 3;
            Mask.Current = 52;
        }
    }

    public class FemaleSwatUniform : Style
    {
        public FemaleSwatUniform()
        {
            
        }
    }
}