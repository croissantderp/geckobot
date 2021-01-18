namespace GeckoBot
{
    public class File
    {
        //file save function
        public static void Save(string data, string path)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(path))
            {
                file.Write(data);
            }
        }

        //file load function
        public static string Load(string path)
        {
            string line = "";
            using (System.IO.StreamReader file = new System.IO.StreamReader(path))
            {
                line = file.ReadToEnd();
            }
            return line;
        }
    }
}