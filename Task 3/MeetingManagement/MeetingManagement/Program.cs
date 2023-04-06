using Data;
using System;
using System.Threading.Tasks;

namespace MeetingManagement
{
    class Program
    {
        static void Main(string[] args)
        {
            MeetingData data = new MeetingData();

            Task.Run(() => data.CheckRemind()); 
            
            var finishApp = false;
            
            Console.WriteLine("This is Meeting Management!\n\nWhat action do you want to take?");

            while (!finishApp)
            {
                try
                {
                    Console.WriteLine("\ta - Add meeting\n\tr - Remove meeting\n\tc - Change meeting\n\tv- View meeting list\n\ts- Save meeting list to file \n\te - Exit the application\n");

                    var operation = Console.ReadLine().ToLower();

                    switch (operation)
                    {
                        case "a":
                            data.AddMeeting();
                            break;
                        case "r":
                            data.RemoveMeeting();
                            break;
                        case "c":
                            data.ChangeMeeting();
                            break;
                        case "v":
                            data.GetMeetings();
                            break;
                        case "s":
                            data.Export();
                            break;
                        case "e":
                            finishApp = true;
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error! Detailed information: " + e.Message);
                }
            }
        }
    }
}
