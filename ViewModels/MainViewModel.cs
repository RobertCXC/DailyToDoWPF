using DailyToDo.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Input;

namespace DailyToDo.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
        private readonly string _storageFilePath;

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
            _storageFilePath = BuildStorageFilePath();
            Tasks = new ObservableCollection<TaskItem>();
            Tasks.CollectionChanged += Tasks_CollectionChanged;

            AddTaskCommand = new RelayCommand(AddTask);
            LoadPendingTasks();
        }

        private void AddTask(object? parameter)
        {
            if (string.IsNullOrWhiteSpace(NewTaskTitle))
            {
                return;
            }

            Tasks.Add(new TaskItem
            {
                Title = NewTaskTitle.Trim(),
                IsCompleted = false
            });

            NewTaskTitle = string.Empty;
            SavePendingTasks();
        }

        private void Tasks_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
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

            SavePendingTasks();
        }

        private async void Task_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not TaskItem task)
            {
                return;
            }

            if (e.PropertyName == nameof(TaskItem.IsCompleted))
            {
                if (task.IsCompleted)
                {
                    await System.Threading.Tasks.Task.Delay(1200);
                }

                MoveTask(task);
            }

            if (e.PropertyName == nameof(TaskItem.IsCompleted)
                || e.PropertyName == nameof(TaskItem.Title)
                || e.PropertyName == nameof(TaskItem.IsImportant))
            {
                SavePendingTasks();
            }
        }

        private void MoveTask(TaskItem task)
        {
            var oldIndex = Tasks.IndexOf(task);
            if (oldIndex < 0)
            {
                return;
            }

            if (task.IsCompleted)
            {
                if (oldIndex < Tasks.Count - 1)
                {
                    Tasks.Move(oldIndex, Tasks.Count - 1);
                }
            }
            else
            {
                if (oldIndex > 0)
                {
                    Tasks.Move(oldIndex, 0);
                }
            }
        }

        public void SavePendingTasks()
        {
            try
            {
                var directory = Path.GetDirectoryName(_storageFilePath);
                if (string.IsNullOrWhiteSpace(directory))
                {
                    return;
                }

                Directory.CreateDirectory(directory);

                var pendingTasks = Tasks
                    .Where(task => !task.IsCompleted)
                    .Select(task => new TaskStorageItem
                    {
                        Title = task.Title,
                        IsImportant = task.IsImportant
                    })
                    .ToList();

                var json = JsonSerializer.Serialize(pendingTasks, JsonOptions);
                File.WriteAllText(_storageFilePath, json);
                File.SetAttributes(_storageFilePath, FileAttributes.Hidden);
            }
            catch
            {
                // Ignore local persistence errors so the UI keeps working.
            }
        }

        private void LoadPendingTasks()
        {
            try
            {
                if (!File.Exists(_storageFilePath))
                {
                    return;
                }

                var json = File.ReadAllText(_storageFilePath);
                var pendingTasks = JsonSerializer.Deserialize<List<TaskStorageItem>>(json) ?? new List<TaskStorageItem>();

                foreach (var task in pendingTasks.Where(task => !string.IsNullOrWhiteSpace(task.Title)))
                {
                    Tasks.Add(new TaskItem
                    {
                        Title = task.Title,
                        IsCompleted = false,
                        IsImportant = task.IsImportant
                    });
                }
            }
            catch
            {
                // Ignore corrupted cache files and start with an empty list.
            }
        }

        private static string BuildStorageFilePath()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(appDataPath, "DailyToDo", "pending-tasks.json");
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? key = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(key));
        }

        private sealed class TaskStorageItem
        {
            public string Title { get; set; } = string.Empty;
            public bool IsImportant { get; set; }
        }
    }
}
