using CitizenFX.Core;
using System;

namespace Curiosity.Missions.Client.MissionPeds
{
    abstract class PedTemplate : Entity, IEquatable<Ped>
    {
        // RAGE ENGINE
        public readonly Ped Ped;

        static PedTemplate()
        {
        }

        protected PedTemplate(int handle) : base(handle)
        {

        }

        public bool Equals(PedTemplate other)
        {
            return (!base.Equals(other) ? false : object.Equals(this.Ped, other.Ped));
        }

        public override bool Equals(object obj)
        {
            bool flag;
            if (obj == null)
            {
                flag = false;
            }
            else
            {
                flag = (this) != obj ? obj.GetType() != GetType() ? false : Equals((PedTemplate)obj) : true;
            }
            return flag;
        }

        public bool Equals(Ped other)
        {
            return object.Equals(this.Ped, other);
        }

        public static implicit operator Ped(PedTemplate v)
        {
            return v.Ped;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() * 397 ^ (this.Ped != null ? this.Ped.GetHashCode() : 0);
        }
    }
}
