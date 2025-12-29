namespace DailyToDo.Models
{
    public class TaskItem
    {
        public string Title { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public bool IsImportant { get; set; }
    }
}
