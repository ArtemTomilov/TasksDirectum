using Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Data
{
    public class MeetingData
    {
        #region Fields

        private const string TIME_PATTERN = "^(2[0-3]|[01]?[0-9]):([0-5]?[0-9])$";
        private const string PAST_TIME_START_MEETING_MESSAGE = "Past time, please try again\n";
        private const string PAST_TIME_END_MEETING_MESSAGE = "The end time of the meeting is earlier than the start time of the meeting, please try again\n";

        private Dictionary<int, Meeting> _meetings;

        #endregion

        #region Constructor

        public MeetingData()
        {
            _meetings = new Dictionary<int, Meeting>();
        }

        #endregion

        #region Methods
        public void AddMeeting()
        {
            DateTime startMeeting;

            DateTime endMeeting;

            DateTime reminderDate;
            
            Console.WriteLine("Enter the date of the meeting in the format DD/MM/YYYY:\n");

            bool isValidDate;

            do
            {
                DateFormatCheck(out startMeeting);

                if (startMeeting.Date < DateTime.Now.Date)
                {
                    Console.WriteLine("Past date, please try again\n");
                    isValidDate = false;
                }
                else
                {
                    isValidDate = true;
                }
            }
            while (!isValidDate);

            Console.WriteLine("Enter the start time of the meeting in HH:MM format:\n");

            startMeeting = SetTime(startMeeting, DateTime.Now, true);

            Console.WriteLine("Enter the end time of the meeting in HH:MM format.\n");

            endMeeting = SetTime(startMeeting.Date, startMeeting, false);

            var meeting = new Meeting(startMeeting, endMeeting, false, null);

            bool isChoosed;

            do
            {
                Console.WriteLine("Want to set a meeting reminder?\n");

                Console.WriteLine("\ty - Yes\n\tn - No\n");

                //var keyRemind = ;

                switch (Console.ReadLine().ToLower())
                {
                    case "y":
                        reminderDate = SetRemind(startMeeting);
                        meeting.IsRemind = isChoosed  = true;
                        meeting.ReminderTime = reminderDate;
                        break;
                    case "n":
                        isChoosed = true;
                        break;
                    default:
                        isChoosed = false;
                        break;
                }
            }
            while (!isChoosed);

            if (!MeetingIntersectionCheck(meeting, 0))
            {
                
                return;
            }

            _meetings.Add(_meetings.Count + 1, meeting);

            Console.WriteLine("Meeting added successfully!\n");
        }

        public void RemoveMeeting()
        {
            int keyOfMeeting;

            do
            {
                Console.WriteLine("Enter the meeting number you want to delete:\n");

                int.TryParse(Console.ReadLine(), out keyOfMeeting);
            }
            while (keyOfMeeting < 1);

            if(keyOfMeeting > _meetings.Count)
            {
                Console.WriteLine("This meeting number does not exist.\n");

                return;
            }

            _meetings.Remove(keyOfMeeting);

            Console.WriteLine("Meeting successfully deleted!\n");
        }

        public void ChangeMeeting()
        {
            int keyOfMeeting;

            do
            {
                Console.WriteLine("Enter the meeting number you want to change:\n");

                int.TryParse(Console.ReadLine(), out keyOfMeeting);
            }
            while (keyOfMeeting < 1);

            if (keyOfMeeting > _meetings.Count)
            {
                Console.WriteLine("This meeting number does not exist.\n");

                return;
            }

            bool isFinishChange = false;

            do
            {
                Console.WriteLine("What would you like to change in the meeting?\n" +
                        "\t1 - Meeting start\n\t2 - End of the meeting\n\t3 - Meeting reminder\n\t4 - Cancel\n");

                switch (Console.ReadLine())
                {
                    case "1":
                        isFinishChange = ChangeMeetingStart(keyOfMeeting);
                        break;
                    case "2":
                        isFinishChange = ChangeEndMeeting(keyOfMeeting);
                        break;
                    case "3":
                        isFinishChange = ChangeMeetingReminder(keyOfMeeting);
                        break;
                    case "4":
                        isFinishChange = true;
                        break;
                }
            }
            while (!isFinishChange);
        }

        public void GetMeetings()
        {
            DateTime date;
            
            Console.WriteLine("For what date to show the list of meetings?\n");

            DateFormatCheck(out date);

            var meetingsPerDay = _meetings.Where(m => m.Value.StartDate.Date == date.Date);

            if (!meetingsPerDay.Any())
            {
                Console.WriteLine("There are no meetings for this date! Operation cancelled!\n");
                return;
            }

            Console.WriteLine("List of meetings:\n\n");

            //Отображается именно номер встречи из всего списка встреч, а не номер встречи за день, чтобы пользователь мог по номеру встречи сделать изменения
            foreach(var meeting in meetingsPerDay)
            {
                Console.WriteLine("Number #{0}. Meeting date - {1}", meeting.Key, meeting.Value.StartDate.ToShortDateString());
                Console.WriteLine("\tMeeting start time: {0}\n\tMeeting end time: {1}", meeting.Value.StartDate.TimeOfDay, meeting.Value.EndDate.TimeOfDay);
                if (meeting.Value.IsRemind)
                {
                    Console.WriteLine("\tReminder enabled\n\tReminder time: {0}\n", meeting.Value.ReminderTime.Value.TimeOfDay);
                }
                else
                {
                    Console.WriteLine("\tReminder disabled\n");
                }
            }
        }

        public void Export()
        {
            DateTime date;

            Console.WriteLine("What date to save the meeting schedule to a file?\n");

            DateFormatCheck(out date);

            var meetingsPerDay = _meetings.Where(m => m.Value.StartDate.Date == date.Date);

            if (!meetingsPerDay.Any())
            {
                Console.WriteLine("There are no meetings for this date! Operation cancelled!\n");
                return;
            }

            using (StreamWriter sw = new StreamWriter("Meetings on the date " + date.ToShortDateString().ToString() + ".txt", false))
            {
                foreach (var meeting in meetingsPerDay)
                {
                    sw.WriteLine("Meeting start time: {0}\nMeeting end time: {1}", meeting.Value.StartDate.TimeOfDay, meeting.Value.EndDate.TimeOfDay);
                    if (meeting.Value.IsRemind)
                    {
                        sw.WriteLine("Reminder enabled\nReminder time: {0}\n", meeting.Value.ReminderTime.Value.TimeOfDay);
                    }
                    else
                    {
                        sw.WriteLine("Reminder disabled\n");
                    }
                }

                Console.WriteLine("File saved successfully!");
            }
        }

        public void CheckRemind()
        {
            while (true)
            {
                var meetingWithRemind = _meetings.Where(m => m.Value.IsRemind == true && m.Value.ReminderTime.HasValue);

                foreach (var meeting in meetingWithRemind)
                {
                    if (meeting.Value.ReminderTime.Value.Date == DateTime.Now.Date
                        && meeting.Value.ReminderTime.Value.TimeOfDay.Hours == DateTime.Now.TimeOfDay.Hours
                        && meeting.Value.ReminderTime.Value.TimeOfDay.Minutes == DateTime.Now.TimeOfDay.Minutes)
                    {
                        Console.WriteLine("Your meeting will start at {0}!", meeting.Value.StartDate.TimeOfDay);
                    }
                }

                Thread.Sleep(new TimeSpan(0, 1, 0));
            }
        }

        #endregion

        #region Helpers

        private bool MeetingIntersectionCheck(Meeting meeting, int key)
        {
            var meetingsDay = _meetings.Where(m => m.Value.StartDate.Date == meeting.StartDate.Date);

            if (!meetingsDay.Any())
            {
                return true;
            }

            var previousMeetings = meetingsDay.Where(m => m.Value.StartDate.TimeOfDay < meeting.StartDate.TimeOfDay).OrderBy(m => m.Value.StartDate).LastOrDefault();
            var nextMeetings = meetingsDay.Where(m => m.Value.StartDate.TimeOfDay > meeting.StartDate.TimeOfDay).OrderBy(m => m.Value.StartDate).FirstOrDefault();

            if(!previousMeetings.Equals(default(KeyValuePair<int, Meeting>)) 
                && previousMeetings.Key != key
                && previousMeetings.Value.EndDate > meeting.StartDate)
            {
                Console.WriteLine("This meeting overlaps with scheduled meetings! Operation cancelled!\n");
                return false;
            }

            if (!nextMeetings.Equals(default(KeyValuePair<int, Meeting>)) 
                && nextMeetings.Key != key
                && nextMeetings.Value.StartDate < meeting.EndDate)
            {
                Console.WriteLine("This meeting overlaps with scheduled meetings! Operation cancelled!\n");
                return false;
            }

            return true;
        }

        private bool ChangeMeetingStart(int keyOfMeeting)
        {
            var meeting = _meetings[keyOfMeeting];
            
            Console.WriteLine("Enter a new meeting start time:\n");

            var changedMeetingStart = SetTime(meeting.StartDate.Date, DateTime.Now, true);

            if(changedMeetingStart > meeting.EndDate)
            {
                Console.WriteLine("You have selected the start of the meeting which is later than the end of the meeting! Operation cancelled!\n");
                return false;
            }

            var changedMeeting = new Meeting(changedMeetingStart, meeting.EndDate, meeting.IsRemind, meeting.ReminderTime);

            if (!MeetingIntersectionCheck(changedMeeting, keyOfMeeting))
            {
                return false;
            }

            _meetings[keyOfMeeting] = changedMeeting;

            Console.WriteLine("The start of the meeting has been successfully changed!\n");

            return true;
        }

        private bool ChangeEndMeeting(int keyOfMeeting)
        {
            var meeting = _meetings[keyOfMeeting];

            Console.WriteLine("Enter a new meeting end time:\n");

            var changerEndMeeting = SetTime(meeting.EndDate.Date, meeting.StartDate, false);

            var changedMeeting = new Meeting(meeting.StartDate, changerEndMeeting, meeting.IsRemind, meeting.ReminderTime);

            if (!MeetingIntersectionCheck(changedMeeting, keyOfMeeting))
            {
                return false;
            }

            _meetings[keyOfMeeting] = changedMeeting;

            Console.WriteLine("The end of the meeting has been successfully changed!\n");

            return true;
        }
        
        //Устанавливается напоминание если его не было во встрече или корректируется если было ранее установлено
        private bool ChangeMeetingReminder(int keyOfMeeting)
        {
            var meeting = _meetings[keyOfMeeting];

            var changedRemind = SetRemind(meeting.StartDate);

            _meetings[keyOfMeeting] = new Meeting(meeting.StartDate, meeting.EndDate, true, changedRemind);

            Console.WriteLine("Meeting reminder changed successfully!\n");

            return true;
        }

        private DateTime SetRemind(DateTime startMeeting)
        {
            int minutes;

            do
            {
                Console.WriteLine("How many minutes before the meeting do I remind you? Please enter a number greater than 0:\n");

                int.TryParse(Console.ReadLine(), out minutes);
            }
            while (minutes < 1);

            return startMeeting.Subtract(TimeSpan.FromMinutes(minutes));
        }

        private DateTime SetTime(DateTime date, DateTime dateToCompare, bool isStartMeeting)
        {
            var pastTimeMessage = isStartMeeting ? PAST_TIME_START_MEETING_MESSAGE : PAST_TIME_END_MEETING_MESSAGE;

            bool isValidTime;

            var time = Console.ReadLine();

            do
            {
                while (!Regex.IsMatch(time, TIME_PATTERN))
                {
                    Console.WriteLine("Invalid time format, please try again\n");
                    time = Console.ReadLine();
                }

                date = date.AddHours(Convert.ToDouble(time.Substring(0, 2)));
                date = date.AddMinutes(Convert.ToDouble(time.Substring(3, 2)));

                if (date <= dateToCompare)
                {
                    Console.WriteLine(pastTimeMessage);
                    time = Console.ReadLine();
                    isValidTime = false;
                    date = date.Date;
                }
                else
                {
                    isValidTime = true;
                }
            }
            while (!isValidTime);

            return date;
        }

        private void DateFormatCheck(out DateTime date)
        {
            while (!DateTime.TryParseExact(Console.ReadLine(), "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out date))
            {
                Console.WriteLine("Invalid date format, please try again\n");
            }
        }

        #endregion
    }
}
