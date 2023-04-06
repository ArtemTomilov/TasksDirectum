using System;

namespace Models
{
    public class Meeting
    {
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsRemind { get; set; }

        public DateTime? ReminderTime { get; set; }

        public Meeting(DateTime startDate, DateTime endDate, bool isRemind, DateTime? reminderDate)
        {
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.IsRemind = isRemind;
            this.ReminderTime = reminderDate;
        }
    }
}
