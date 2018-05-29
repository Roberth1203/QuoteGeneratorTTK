using System;
using System.Configuration;
using System.IO;

namespace Utilities
{
    public class Logs
    {
        String filePath = String.Empty;
        static String rootFolder = ConfigurationManager.AppSettings["LogFolder"];
        String fileLog = String.Empty;

        private void LogFolderExist()
        {
            if (!System.IO.Directory.Exists(rootFolder))
                System.IO.Directory.CreateDirectory(rootFolder);
        }

        public void createLog()
        {
            LogFolderExist();

            fileLog = System.IO.Path.Combine(rootFolder, DateTime.Now.ToString("yyyy-MM-dd") + ".txt");
            if (!System.IO.File.Exists(fileLog))
            {
                System.IO.StreamWriter file = new System.IO.StreamWriter(fileLog);
                file.Close();
            }
        }

        public void writeOnLog(String text)
        {
            Console.WriteLine(fileLog);
            using (StreamWriter sw = File.AppendText(fileLog))
                sw.WriteLine(text);
        }
    }
}
