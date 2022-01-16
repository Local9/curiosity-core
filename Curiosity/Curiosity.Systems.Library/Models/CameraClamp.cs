namespace Curiosity.Systems.Library.Models
{
    public class CameraClamp
    {
        private float _leftHorizontalValue;
        private float _maxVerticalValue;
        private float _minVerticalValue;
        private float _rightHorizontalValue;

        public float LeftHorizontalValue
        {
            get => this._leftHorizontalValue;
            set => this._leftHorizontalValue = value;
        }

        public float MaxVerticalValue
        {
            get => this._maxVerticalValue;
            set => this._maxVerticalValue = value;
        }

        public float MinVerticalValue
        {
            get => this._minVerticalValue;
            set => this._minVerticalValue = value;
        }

        public float RightHorizontalValue
        {
            get => this._rightHorizontalValue;
            set => this._rightHorizontalValue = value;
        }
    }
}
