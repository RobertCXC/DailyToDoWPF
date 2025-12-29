using DailyToDo.ViewModels;
using System.Windows;

namespace DailyToDo
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        private void PinBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Primitives.ToggleButton toggleBtn)
            {
                Topmost = toggleBtn.IsChecked == true;
            }
        }

        private void MinimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}