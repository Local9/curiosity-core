using Curiosity.Core.Client.Extensions;

namespace Curiosity.Core.Client.Managers.GameWorld.Properties.Models
{
    internal class SaleSign
    {
        private Prop _propForSaleSign;

        public string Model { get; set; }
        public Quaternion Position { get; set; }

        public SaleSign(string model, Quaternion position)
        {
            Model = model;
            Position = position;
        }

        public async void CreateForSaleSign()
        {
            if (_propForSaleSign is not null) return;
            Model propModel = Model;
            _propForSaleSign = await World.CreateProp(propModel, Position.AsVector(), true, false);
            _propForSaleSign.IsPersistent = true;
            _propForSaleSign.IsPositionFrozen = true;
            _propForSaleSign.Heading = Position.W;
            propModel.MarkAsNoLongerNeeded();
        }

        public void Delete()
        {
            if (_propForSaleSign?.Exists() ?? false)
                _propForSaleSign.Dispose();
        }
    }
}
