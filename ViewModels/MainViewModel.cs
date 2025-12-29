using DailyToDo.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System;
using System.Globalization;

namespace DailyToDo.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<TaskItem> Tasks { get; set; }

        public string CurrentDate => DateTime.Now.ToString("M月d日 dddd", CultureInfo.CreateSpecificCulture("zh-CN"));

        public MainViewModel()
        {
            Tasks = new ObservableCollection<TaskItem>
            {
                new TaskItem { Title = "开个 doll 聚餐两餐产品海带叫", IsCompleted = false },
                new TaskItem { Title = "你像花落冬令时想", IsCompleted = false },
                new TaskItem { Title = "来说有快死顶多 sk", IsCompleted = false },
                new TaskItem { Title = "多想学怪冷新增肯", IsCompleted = false },
                new TaskItem { Title = "程序陈奕迅抽或许", IsCompleted = false }
            };

            foreach (var task in Tasks)
            {
                task.PropertyChanged += Task_PropertyChanged;
            }

            Tasks.CollectionChanged += Tasks_CollectionChanged;
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

        private void Task_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TaskItem.IsCompleted))
            {
                if (sender is TaskItem task)
                {
                    // Delay execution to let the binding update complete, though not strictly necessary for simple property changes
                    // Using Dispatcher or just running directly. 
                    // Direct manipulation of ObservableCollection is usually fine on UI thread.
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
