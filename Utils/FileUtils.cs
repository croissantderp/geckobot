using System.IO;

namespace GeckoBot
{
    public class FileUtils
    {
        //file save function
        public static void Save(string data, string path)
        {
            checkForExistance();

            using StreamWriter file = new StreamWriter(path);
            file.Write(data);
        }

        //file load function
        public static string Load(string path)
        {
            checkForExistance();

            string line = "";
            using StreamReader file = new StreamReader(path);
            line = file.ReadToEnd();
            
            return line;
        }

        //check if cache exists
        public static void checkForCacheExistance()
        {
            //checks in cache folder exists
            if (!Directory.Exists(@"..\..\Cache\"))
            {
                Directory.CreateDirectory(@"..\..\Cache\");
            }
        }

        //checks if cache and save files exist
        public static void checkForExistance()
        {
            checkForCacheExistance();

            //checks if files exist
            if (!File.Exists(@"..\..\Cache\gecko1.gek"))
            {
                File.Create(@"..\..\Cache\gecko1.gek");
            }
            
            if (!File.Exists(@"..\..\Cache\gecko2.gek"))
            {
                File.Create(@"..\..\Cache\gecko2.gek");
            }

            if (!File.Exists(@"..\..\Cache\gecko3.gek"))
            {
                File.Create(@"..\..\Cache\gecko3.gek");
            }
        }
    }
}