using Curiosity.MobilePhone.Client.net.ClientData;
using System.Threading.Tasks;

namespace Curiosity.MobilePhone.Client.net.Models
{
    public abstract class App
    {
        public string Name { get; set; }
        public int Icon { get; set; }
        public bool OverrideBack { get; set; }
        public Phone Phone { get; set; }

        public App(string name, int icon, Phone phone, bool overrideBack = true)
        {
            Name = name;
            Icon = icon;
            Phone = phone;
            OverrideBack = overrideBack;
        }

        public abstract Task Tick();

        public abstract void Initialize(Phone phone);

        public abstract void Kill();
    }
}
