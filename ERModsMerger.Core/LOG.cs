using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace ERModsMerger.Core
{
    public class LOG
    {
        DateTime Date;
        public string Message { get; set; }
        public LOGTYPE Type { get; set; }
        public LOG(string logMessage, LOGTYPE logType = LOGTYPE.INFO)
        {
            Date = DateTime.Now;    
            Message = logMessage.Replace("\r", "");
            Type = logType;

            switch (Type)
            {
                case LOGTYPE.INFO: Message = "🛈  " + logMessage; break;
                case LOGTYPE.ERROR: Message = "🔴  " + logMessage; break;
                case LOGTYPE.WARNING: Message = "⚠  " + logMessage; break;
                case LOGTYPE.QUERY_USER__YES_NO: Message = "❔  " + logMessage; break;
            }

            OnNewLog(this);
        }

        public delegate void NewLogEventHandler(
        NewLogEventArgs args);

        public static event NewLogEventHandler NewLog;

        protected static void OnNewLog(LOG log)
        {
            if (NewLog != null)
            {
                NewLog(new NewLogEventArgs(log));
            }
        }

        private static bool _consoleOutput = false;
        public static bool ConsoleOutput {  get { return _consoleOutput; } set { _consoleOutput = value; } }

        private static bool _wpfOutput = false;
        public static bool WpfOutput { get { return _wpfOutput; } set { _wpfOutput = value; } }

        public static List<LOG> lOGs = new List<LOG>();
        
        public static void Log(string logMessage, LOGTYPE logType = LOGTYPE.INFO)
        {
            lOGs.Add(new LOG(logMessage, logType));

            if(ConsoleOutput && logType != LOGTYPE.QUERY_USER__YES_NO)
            {
                Console.WriteLine(lOGs.Last().Message);
            }
        }

        public static bool QueryUserYesNoQuestion(string queryMessage)
        {
            bool result = false;
            if(ConsoleOutput)
            {
                Console.WriteLine("❔  " + queryMessage);

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\n\n<<YES (Press 'Y')>>\t\t<<NO (Press any other key)>>\n\n");

                Console.ResetColor();

                char keyPressed = Console.ReadKey(true).KeyChar;
                if (char.ToUpper(keyPressed) == 'Y')
                    result = true;
                else
                    result = false;

                Log(queryMessage + "  -" + result, LOGTYPE.QUERY_USER__YES_NO);
            }
            else if(WpfOutput)
            {
                var Result = MessageBox.Show(queryMessage, "", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (Result == MessageBoxResult.Yes)
                    result = true;


                Log(queryMessage + " - " + result, LOGTYPE.QUERY_USER__YES_NO);
            }

            return result;
        }

    }

    public enum LOGTYPE
    {
        ERROR = 0,
        INFO = 1,
        WARNING = 2,
        QUERY_USER__YES_NO = 3,
    }

    public class NewLogEventArgs : EventArgs
    {
        private readonly LOG log;

        public NewLogEventArgs(LOG log)
        {
            this.log = log;
        }

        public LOG Log
        {
            get { return this.log; }
        }
    }

}
