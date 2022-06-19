using Curiosity.Core.Client.Managers;

namespace Curiosity.Core.Client.ClientExports
{
    public class PlayerExport : Manager<PlayerExport>
    {
        public override void Begin()
        {
            Instance.ExportDictionary.Add("Passive", new Func<bool>(
                () =>
                {
                    return PlayerOptionsManager.GetModule().IsPassive;
                }
            ));
            Instance.ExportDictionary.Add("IsWanted", new Func<bool>(
                () =>
                {
                    return PlayerOptionsManager.GetModule().IsWanted;
                }
            ));
        }
    }
}
