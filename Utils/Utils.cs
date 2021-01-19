using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Discord;

namespace GeckoBot
{
    // Miscellaneous utils go here
    public class Utils
    {
        public static async void changeProfile(IDiscordClient client, string path)
        {
            ISelfUser self = client.CurrentUser;

            Discord.Image image = new Discord.Image(path);

            await self.ModifyAsync(x =>
            {
                x.Avatar = image;
            });
            //static extern Image image(string path);

        }
        
        //replaces strings with emotes
        public static string emoteReplace(string stuff)
        {
            //loads emote dictionary as string and converts it back into dictionary
            Globals.RefreshEmoteDict();
            
            //splits string by $
            string[] yesnt = Regex.Split(stuff, @"(?<!\\)\$");

            //removes empty strings which would break this command
            yesnt = yesnt.Where(x => !string.IsNullOrEmpty(x)).ToArray();

            //final string array with converted segments
            string[] final = new string[yesnt.Length];

            for (int i = 0; i < yesnt.Length; i++)
            {
                //checks if a key exists for the segment and if it is prefaced by \
                if (Globals.emoteDict.ContainsKey(yesnt[i]) && yesnt[i][0] != '\\')
                {
                    final[i] = Globals.emoteDict[yesnt[i]];
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
            return string.Join("", final.Select(p => p.ToString()));
        }
    }
}