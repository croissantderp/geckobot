using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeckoBot
{
    //global variables
    public class Globals
    {
        //guessing game variables
        public static int gNumber;
        public static int attempts;
        public static bool easyMode;

        //emote dictionary
        public static Dictionary<string, string> emoteDict = new();
        //loads emote dictionary as string and converts it back into dictionary
        public static void RefreshEmoteDict()
        {
            emoteDict = FileUtils.Load("C:\\gecko2.gek")
                .Split('ҩ')
                .Select(part => part.Split('⁊'))
                .Where(part => part.Length == 2)
                .ToDictionary(sp => sp[0], sp => sp[1]);
        }
        //dictionary to string
        public static string DictToString<T, V>(IEnumerable<KeyValuePair<T, V>> items, string format)
        {
            format = String.IsNullOrEmpty(format) ? "{0}='{1}' " : format;

            StringBuilder itemString = new StringBuilder();
            foreach (var (key, value) in items)
                itemString.AppendFormat(format, key, value);

            return itemString.ToString();
        }

        //people to dm for daily gecko images
        public static List<ulong> dmUsers = new ();

        //last time bot was run and daily geckoimage was sent
        public static int lastrun = DateTime.Now.DayOfYear;

        //days since bot was reset
        public static int daysSinceReset = 0;

        //is timer is running
        public static bool started = false;

        //various dictionaries for music generation
        public static Dictionary<int, string> notes = new()
        {
            {0, "c"},
            {1, "c#"},
            {2, "d"},
            {3, "d#"},
            {4, "e"},
            {5, "f"},
            {6, "f#"},
            {7, "g"},
            {8, "g#"},
            {9, "a"},
            {10, "a#"},
            {11, "b"},
            {12, "c2"},
            {13, "c#2"},
            {14, "d2"},
            {15, "d#2"},
            {16, "e2"},
            {17, "f2"},
            {18, "f#2"},
            {19, "g2"},
            {20, "g#2"},
            {21, "a2"},
            {22, "a#2"},
            {23, "b2"},
            {24, "c3"},
            {25, "c#3"},
            {26, "d3"},
            {27, "d#3"},
            {28, "e3"},
            {29, "f3"},
            {30, "f#3"},
            {31, "g3"},
            {32, "g#3"},
            {33, "a3"},
            {34, "a#3"},
            {35, "b3"},
            {36, "c4"},
            {37, "rest"},
        };
        public static Dictionary<int, int> major = new()
        {
            {1, 2},
            {2, 4},
            {3, 5},
            {4, 7},
            {5, 9},
            {6, 11},
            {7, 12},
        };
        public static Dictionary<int, int> minor = new Dictionary<int, int>()
        {
            {1, 2},
            {2, 3},
            {3, 5},
            {4, 7},
            {5, 8},
            {6, 10},
            {7, 12},
        };
        public static Dictionary<int, int> timeDict = new()
        {
            {1, 2},
            {2, 3},
            {3, 4},
            {4, 5},
            {5, 8},
            {6, 16},
        };
        public static Dictionary<int, int> timeDict2 = new()
        {
            {1, 2},
            {2, 4},
            {3, 8},
        };

        public static string[] names =
        {
            " 1: A Game Of Tokens",
            " 2: Electric boogaloo",
            " 3: return of the rbot",
            " Act 4: flight of the paradox bots",
            " V: Artoo Strikes Back",
            " 6: The Undiscovered Server",
            " and the deathly nitros",
            " part 8: Geckolion",
            " IX: Rise of Moofy"
        };

        public static int currentValue = 0;

        public static int HighestGecko;
    }
}