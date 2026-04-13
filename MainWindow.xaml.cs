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
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.SavePendingTasks();
            }
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

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // Find the ConfettiControl in the visual tree
            if (sender is System.Windows.Controls.CheckBox checkBox)
            {
                var parent = System.Windows.Media.VisualTreeHelper.GetParent(checkBox);
                while (parent != null && !(parent is System.Windows.Controls.Grid))
                {
                    parent = System.Windows.Media.VisualTreeHelper.GetParent(parent);
                }

                if (parent is System.Windows.Controls.Grid grid)
                {
                    foreach (var child in grid.Children)
                    {
                        if (child is Controls.ConfettiControl confetti)
                        {
                            confetti.Burst();
                            break;
                        }
                    }
                }
            }
        }
    }
}
