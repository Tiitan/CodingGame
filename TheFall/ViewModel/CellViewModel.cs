using System;
using System.ComponentModel;

namespace TheFall.ViewModel
{
    public class CellViewModel : INotifyPropertyChanged
    {
        static readonly float[] rotationMap = { 0, 0, 0, 90, 90, 0, 180, 270, 0, 90, 90, 180, 270, 0 };

        readonly int[,] _map;
        readonly Vector2i _position;

        public int Type => Math.Abs(_map[_position.X, _position.Y]);
        public bool IsLocked => _map[_position.X, _position.Y] < 0;
        public float Rotation => rotationMap[Type];

        public event PropertyChangedEventHandler? PropertyChanged;

        public CellViewModel(int[,] map, Vector2i position)
        {
            _map = map;
            _position = position;
        }

        public void OnRotate()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Rotation)));
        }
    }
}
