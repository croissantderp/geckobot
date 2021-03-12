using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GeckoBot.Utils
{
    //global variables
    public class Globals
    {
        public static readonly Discord.AllowedMentions allowed = new (Discord.AllowedMentionTypes.Users);
        public static readonly Discord.AllowedMentions notAllowed = new(Discord.AllowedMentionTypes.None);

        //dictionary to string
        public static string DictToString<T, V>(IEnumerable<KeyValuePair<T, V>> items, string format)
        {
            format = String.IsNullOrEmpty(format) ? "{0}='{1}' " : format;

            StringBuilder itemString = new StringBuilder();
            foreach (var (key, value) in items)
                itemString.AppendFormat(format, key, value);

            return itemString.ToString();
        }

        public static DateTime datetime = new();
        public static bool isSleep = false;
        
        // Very descriptive name gecko, tells me exactly what this is for
        public static string[] strings = new string[3];

        public static List<ulong> undeletable = new();

        //days since bot was reset
        public static int daysSinceReset = 0;

        public static readonly string[] names =
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

        public static int CurrentName = 0;
    }
}