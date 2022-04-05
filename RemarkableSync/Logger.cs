using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace RemarkableSync
{
    public class Logger
    {
        public static void LogMessage(
            string message,
            [CallerFilePath] string fullFileName = "",
            [CallerMemberName] string funcName = "",
            [CallerLineNumber] int lineNum = 0
            )
        {
            string[] fileParts = fullFileName.Split('\\');
            string classname = "";
            if (fileParts.Length > 0)
            {
                classname = fileParts[fileParts.Length - 1];
            }
            if (classname.EndsWith(".cs"))
            {
                classname = classname.Substring(0, classname.Length - 3);
            }
            string logMessage = $"{DateTime.Now.ToString()}\t{classname}.{funcName}({lineNum}) - {message}";
            Console.WriteLine(logMessage);
        }
    }
}
