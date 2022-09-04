using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using TheFall.ViewModel;

namespace TheFall
{
    public partial class MainWindow : Window
    {
        Game? _game;

        bool inputTextDirty = false; // input text changed = save input on next Run
        readonly MainViewModel MainVm = new MainViewModel();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = MainVm;

            // Try load saved map when window open (app start)
            if (File.Exists("InputText.txt"))
            {
                InputText.Text = File.ReadAllText("InputText.txt");
                OnRun(this, new RoutedEventArgs());
            }
        }

        private void OnRun(object sender, RoutedEventArgs e)
        {
            string textMap = InputText.Text;
            // Save map for convenient app restart
            if (inputTextDirty)
                Task.Run(() => File.WriteAllText("InputText.txt", textMap));

            var inputStream = new StringReader(textMap);
            _game = new Game(inputStream);

            MainVm.WorldVm = new WorldViewModel(_game.World);
        }

        private void OnInputTextChanged(object sender, TextChangedEventArgs e)
        {
            inputTextDirty = true;
        }
    }
}
