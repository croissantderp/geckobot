using System.Linq;
using System.Text.RegularExpressions;
using GeckoBot.Commands;

namespace GeckoBot.Utils
{
    public class EmoteUtils
    {
        //loads emote dictionary as string and converts it back into dictionary
        public static void RefreshEmoteDict()
        {
            Emotes.EmoteDict = Regex.Split(FileUtils.Load(@"..\..\Cache\gecko2.gek"), @"\s(?<!\\)ҩ\s")
                .Select(part => Regex.Split(part, @"\s(?<!\\)\⁊\s"))
                .Where(part => part.Length == 2)
                .ToDictionary(sp => sp[0], sp => sp[1]);
        }

        //replaces strings with emotes
        public static string emoteReplace(string stuff)
        {
            stuff = escapeforbidden(stuff);

            //loads emote dictionary as string and converts it back into dictionary
            RefreshEmoteDict();
            
            //splits string by $
            string[] yesnt = Regex.Split(stuff, @"(?<!\\)\$");

            //removes empty strings which would break this command
            yesnt = yesnt.Where(x => !string.IsNullOrEmpty(x)).ToArray();

            //final string array with converted segments
            string[] final = new string[yesnt.Length];

            for (int i = 0; i < yesnt.Length; i++)
            {
                //checks if a key exists for the segment and if it is prefaced by \
                if (Emotes.EmoteDict.ContainsKey(yesnt[i]))
                {
                    final[i] = Emotes.EmoteDict[yesnt[i]];
                    if (final[i].Contains("@फΉ̚ᐼㇶ⤊"))
                    {
                        final[i] = final[i].Replace("@फΉ̚ᐼㇶ⤊", "");
                    }
                }
                else
                {
                    //if segment is prefaced by \
                    if (yesnt[i][0] == '\\')
                    {
                        //removes \
                        final[i] = yesnt[i].Remove(0, 1);
                    }
                    else
                    {
                        final[i] = yesnt[i];
                    }
                }
            }

            //returns joined segments
            return removeforbidden(string.Join("", final.Select(p => p.ToString())));
        }

        public static string escapeforbidden(string input)
        {
            string final = input.Replace(@"@फΉ̚ᐼㇶ⤊", @"\@फΉ̚ᐼㇶ⤊");

            final = final.Replace(@"⁊", @"\⁊");

            final = final.Replace(@"ҩ", @"\ҩ");

            return final;
        }

        public static string removeforbidden(string input)
        {
            string final = input.Replace(@"@फΉ̚ᐼㇶ⤊", @"\@फΉ̚ᐼㇶ⤊");

            final = final.Replace(@"\⁊", @"⁊");

            final = final.Replace(@"\ҩ", @"ҩ");

            return final;
        }
    }
}