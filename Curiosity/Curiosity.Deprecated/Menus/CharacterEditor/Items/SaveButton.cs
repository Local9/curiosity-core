namespace Curiosity.Client.net.Classes.Menus.CharacterEditor.MainMenu
{
    class SaveButton : MenuItemStandard
    {
        private CharacterEditorMenu Root;

        public SaveButton(CharacterEditorMenu root)
        {
            Root = root;
            Title = "Save character & Play!";
            OnActivate = SaveAndExit;
        }

        private void SaveAndExit(MenuItemStandard m)
        {
            // TODO: Proper support for this
            //Roleplay.Client.CharacterEditor.Save();
            //Roleplay.Client.CharacterEditor.Exit();
        }
    }
}
