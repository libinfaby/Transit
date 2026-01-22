using System.Windows;
using Transit.Client.ViewModels;

namespace Transit.Client
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}