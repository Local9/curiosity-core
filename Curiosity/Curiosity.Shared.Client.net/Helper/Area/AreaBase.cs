using CitizenFX.Core;
using System;

namespace Curiosity.Shared.Client.net.Helper.Area
{
    public abstract class AreaBase
    {
        public string Identifier { get; set; }
        public AreaType Type { get; set; }
        public dynamic OnEnter { get; set; }
        public dynamic OnExit { get; set; }
        public dynamic Params { get; set; }
        public dynamic Debug { get; set; }

        protected bool PlayerInside = false;

        protected void TriggerEnter()
        {
            Vector3 PlayerPos = Game.PlayerPed.Position;
            BaseScript.TriggerEvent("curiosity:Client:Player:Environment:OnEnterArea", Identifier, new { X = PlayerPos.X, Y = PlayerPos.Y, Z = PlayerPos.Z });
            BaseScript.TriggerServerEvent("curiosity:Server:Player:Environment:OnEnterArea", Identifier, new { X = PlayerPos.X, Y = PlayerPos.Y, Z = PlayerPos.Z });

            if (this.OnEnter == null)
            {
                return;
            }

            if (this.OnEnter.GetType() == typeof(System.String))
            {
                BaseScript.TriggerEvent((String)this.OnEnter, this.Params);
            }
            else if (this.OnEnter.GetType() == typeof(CitizenFX.Core.CallbackDelegate))
            {   
                if (this.Params != null)
                {
                    ((CallbackDelegate)this.OnEnter).Invoke(this.Params);
                }
                else
                {
                    ((CallbackDelegate)this.OnEnter).Invoke();
                }
            }
        }

        protected void TriggerExit()
        {
            Vector3 PlayerPos = Game.PlayerPed.Position;
            BaseScript.TriggerEvent("curiosity:Client:Player:Environment:OnExitArea", Identifier, new { X = PlayerPos.X, Y = PlayerPos.Y, Z = PlayerPos.Z });
            BaseScript.TriggerServerEvent("curiosity:Server:Player:Environment:OnExitArea", Identifier, new { X = PlayerPos.X, Y = PlayerPos.Y, Z = PlayerPos.Z });

            if (this.OnExit == null)
            {
                return;
            }

            if (this.OnExit.GetType() == typeof(System.String))
            {
                BaseScript.TriggerEvent((String)this.OnExit, this.Params);
            }
            else if (this.OnExit.GetType() == typeof(CitizenFX.Core.CallbackDelegate))
            {
                if (this.Params != null)
                {
                    ((CallbackDelegate)this.OnExit).Invoke(this.Params);
                }
                else
                {
                    ((CallbackDelegate)this.OnExit).Invoke();
                }
            }
        }

        abstract public bool CoordsInside(Vector3 coords);
        abstract public void Check();
        abstract public void Draw();
    }
}
