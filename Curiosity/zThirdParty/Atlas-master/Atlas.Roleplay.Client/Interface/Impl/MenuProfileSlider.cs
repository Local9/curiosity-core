namespace Atlas.Roleplay.Client.Interface.Impl
{
    public class MenuProfileSlider : IMenuItemProfile
    {
        public int Minimum { get; set; }
        public int Maximum { get; set; }
        public int Current { get; set; }
        public string[] Translations { get; set; }

        public void Begin(Menu menu, MenuItem item)
        {
            if (string.IsNullOrEmpty(item.SecondaryLabel)) item.SecondaryLabel = $"- {GetSecondaryLabel()} -";
        }

        public void On(Menu menu, MenuItem item, MenuOperation operation)
        {
            if (operation.Type == MenuOperationType.SliderUpdate)
            {
                item.SecondaryLabel = $"- {GetSecondaryLabel()} -";
            }
        }

        private string GetSecondaryLabel()
        {
            return Translations != null && Translations.Length >= Maximum ? Translations[Current] : Current.ToString();
        }
    }
}