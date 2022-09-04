using System.Collections.Generic;
using System.ComponentModel;

namespace TheFall.ViewModel
{
    public class WorldViewModel : INotifyPropertyChanged
    {
        public List<CellViewModel> CellViewModels { get; }

        readonly World _world;

        public int Width => _world.Width;
        public int Height => _world.Height;

        public WorldViewModel(World world)
        {
            _world = world;

            CellViewModels = new List<CellViewModel>();
            for (int i = 0; i < world.Height; i++)
                for (int j = 0; j < world.Width; j++)
                    CellViewModels.Add(new CellViewModel(world.Map, new Vector2i(j, i)));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
