using CitizenFX.Core;
using CitizenFX.Core.Native;
using System.Drawing;

namespace Curiosity.Missions.Client.DataClasses
{
    class ParticleEffect : IDeletable
    {
        public Color Color
        {
            set
            {
                Function.Call((Hash)9191676997121112123L, new InputArgument[] { this.Handle, value.R, value.G, value.B, true });
            }
        }

        public int Handle
        {
            get;
        }

        internal ParticleEffect(int handle)
        {
            this.Handle = handle;
        }

        public void Delete()
        {
            Function.Call(unchecked((Hash)(-4323085940105063473L)), new InputArgument[] { this.Handle, 1 });
        }

        public bool Exists()
        {
            return Function.Call<bool>((Hash)8408201869211353243L, new InputArgument[] { this.Handle });
        }
    }
}
