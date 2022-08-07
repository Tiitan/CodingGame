using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Numerics;
using System.IO;


namespace TheFall
{
    public partial class MainWindow : Window
    {
        bool inputTextDirty = false; // input text changed = save input on next Run

        public MainWindow()
        {
            InitializeComponent();

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
            Game game = new Game(inputStream);
        }

        private void OnInputTextChanged(object sender, TextChangedEventArgs e)
        {
            inputTextDirty = true;
        }
    }
}
