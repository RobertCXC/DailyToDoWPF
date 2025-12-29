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
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? key = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(key));
        }
    }
}
