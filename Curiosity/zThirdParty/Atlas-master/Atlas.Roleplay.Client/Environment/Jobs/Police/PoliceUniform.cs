using Atlas.Roleplay.Library.Models;

namespace Atlas.Roleplay.Client.Environment.Jobs.Police
{
    public class MalePoliceUniform : Style
    {
        public MalePoliceUniform()
        {
            Shirt.Current = 38;
            Torso.Current = 149;
            TorsoType.Current = 0; // Badge
            Decals.Current = 1;
            Body.Current = 19;
            Pants.Current = 45;
            Shoes.Current = 25;
            Bag.Current = 3;
            Mask.Current = 121;
        }
    }

    public class FemalePoliceUniform : Style
    {
        public FemalePoliceUniform()
        {
            Shirt.Current = 35;
            Torso.Current = 2;
            TorsoType.Current = 0; // Badge
            Decals.Current = 1;
            Body.Current = 31;
            Pants.Current = 51;
            Shoes.Current = 25;       
            Bag.Current = 3;     
            Mask.Current = 121;
        }
    }
}