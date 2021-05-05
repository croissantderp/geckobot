using System.Collections.Generic;
using System.IO;

namespace GeckoBot.Utils
{
    public class FileUtils
    {
        //file save function
        public static void Save(string data, string path)
        {
            checkForExistance();

            using StreamWriter file = new (path);
            file.Write(data);
        }

        //file load function
        public static string Load(string path)
        {
            checkForExistance();

            using StreamReader file = new (path);
            return file.ReadToEnd();
        }

        //check if cache exists
        public static void checkForCacheExistance()
        {
            //checks if cache folder exists
            if (!Directory.Exists(@"..\..\Cache\"))
            {
                Directory.CreateDirectory(@"..\..\Cache\");
            }
        }

        //checks if cache and save files exist
        public static void checkForExistance()
        {
            checkForCacheExistance();
            
            // Cache numbers
            // gecko1 stores exceptions and reported bugs
            // gecko2 stores the emote dictionary
            // gecko3 stores the list of users who have signed up for the daily dm
            // gecko4 stores the most recent gecko
            // gecko5 stores rate dictionary
            // gecko6 stores custom commands
            // gecko7 stores gecko names and other info
            // gecko8 stores prefixes
            // gecko9 stores alerts
            List<int> caches = new()
            {
                1, 2, 3, 4, 5, 6, 7, 8, 9
            };
            
            //checks if files exist
            foreach (int cache in caches)
            {
                string path = $@"..\..\Cache\gecko{cache}.gek";
                
                if (!File.Exists(path))
                {
                    File.Create(path);

                    // Initialize cache 4 with current datetime
                    if (cache == 4)
                    {
                        using (StreamWriter file = new(path))
                        {
                            file.Write("1$" + System.DateTime.Now.DayOfYear.ToString());
                        }
                    }
                }
            }
        }
    }
}