using DailyToDo.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace DailyToDo.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<TaskItem> Tasks { get; set; }

        public string CurrentDate => DateTime.Now.ToString("M月d日 dddd", CultureInfo.CreateSpecificCulture("zh-CN"));

        private string? _newTaskTitle;
        public string? NewTaskTitle
        {
            get => _newTaskTitle;
            set
            {
                _newTaskTitle = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddTaskCommand { get; }

        public MainViewModel()
        {
            Tasks = new ObservableCollection<TaskItem> { };

            //foreach (var task in Tasks)
            //{
            //    task.PropertyChanged += Task_PropertyChanged;
            //}

            Tasks.CollectionChanged += Tasks_CollectionChanged;

            AddTaskCommand = new RelayCommand(AddTask);
        }

        private void AddTask(object? parameter)
        {
            if (string.IsNullOrWhiteSpace(NewTaskTitle)) return;

            var newTask = new TaskItem { Title = NewTaskTitle, IsCompleted = false };
            newTask.PropertyChanged += Task_PropertyChanged;
            Tasks.Add(newTask);
            NewTaskTitle = string.Empty;
        }

        private void Tasks_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (TaskItem item in e.NewItems)
                {
                    item.PropertyChanged += Task_PropertyChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (TaskItem item in e.OldItems)
                {
                    item.PropertyChanged -= Task_PropertyChanged;
                }
            }
        }

        private async void Task_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TaskItem.IsCompleted))
            {
                if (sender is TaskItem task)
                {
                    // Wait for confetti animation to complete (1.2 seconds)
                    // Animation duration is 0.6-1.0 seconds, so 1.2s ensures it's finished
                    if (task.IsCompleted)
                    {
                        await System.Threading.Tasks.Task.Delay(1200);
                    }
                    MoveTask(task);
                }
            }
        }

        private void MoveTask(TaskItem task)
        {
            var oldIndex = Tasks.IndexOf(task);
            if (oldIndex < 0) return;

            if (task.IsCompleted)
            {
                // Move to bottom
                if (oldIndex < Tasks.Count - 1)
                {
                    Tasks.Move(oldIndex, Tasks.Count - 1);
                }
            }
            else
            {
                // Move to top
                if (oldIndex > 0)
                {
                    Tasks.Move(oldIndex, 0);
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? key = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(key));
        }
    }
}
