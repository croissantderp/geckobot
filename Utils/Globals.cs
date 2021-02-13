using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GeckoBot
{
    //global variables
    public class Globals
    {
        public static Discord.AllowedMentions allowed = new (Discord.AllowedMentionTypes.Users);

        //guessing game variables
        public static int gNumber;
        public static int attempts;
        public static bool easyMode;

        //emote dictionary
        public static Dictionary<string, string> emoteDict = new();
        //loads emote dictionary as string and converts it back into dictionary
        public static void RefreshEmoteDict()
        {
            emoteDict = Regex.Split(FileUtils.Load(@"..\..\Cache\gecko2.gek"), @"\s(?<!\\)ҩ\s")
                .Select(part => Regex.Split(part, @"\s(?<!\\)\⁊\s"))
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
        public static List<ulong> dmUsers = new();

        //if a timer exists
        public static bool timerExists;

        public static System.Timers.Timer timer = new();

        public static ulong timerChannel = new();
        public static ulong timerMessage = new();

        public static bool isSleep = false;

        //if timer should be terminated
        public static bool terminate;

        public static DateTime datetime = new();

        public static string[] strings = new string[3];

        public static List<ulong> undeletable = new();

        //last time bot was run and daily geckoimage was sent
        public static int lastrun = DateTime.Now.DayOfYear;

        public static DateTime lastCheck = DateTime.Now;

        //days since bot was reset
        public static int daysSinceReset = 0;

        //is timer is running
        public static bool started = false;

        //if the counter has started at least once
        public static bool counterStarted = false;

        //if the counter is counting
        public static bool isCounting = false;

        //if timer has ever started
        public static bool everStarted = false;

        //the primary timer for dms
        public static System.Timers.Timer dmTimer = new();

        public static string[] names =
        {
            Top.SecretName,
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