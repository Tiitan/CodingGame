using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TheFall.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        WorldViewModel? _worldVm;

        public WorldViewModel? WorldVm
        {
            get => _worldVm;
            set { _worldVm = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
