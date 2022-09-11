namespace Curiosity.Framework.Shared.SerializedModels
{
    [Serializable]
    public partial class CharacterOutfit
    {
        public string? Name;
        public string? Description;
        public ComponentDrawables ComponentDrawables = new();
        public ComponentDrawables ComponentTextures = new();
        public PropDrawables PropDrawables = new();
        public PropDrawables PropTextures = new();

        public CharacterOutfit() { }

        public CharacterOutfit(string name, string description, ComponentDrawables components, ComponentDrawables componentTextures, PropDrawables props, PropDrawables propTextures)
        {
            Name = name;
            Description = description;
            ComponentDrawables = components;
            ComponentTextures = componentTextures;
            PropDrawables = props;
            PropTextures = propTextures;
        }
    }
}
