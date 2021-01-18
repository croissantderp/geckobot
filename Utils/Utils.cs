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
        //generates paths for gecko image functions
        public static string pathfinder(int num, bool isPNG)
        {
            string final = num.ToString();

            //adds 0s as needed
            while (final.Length < 3)
            {
                final = "0" + final;
            }

            //generates filepath based on number and if image is png
            // Looks in \bin\Cache for png
            string finalPath = @"..\..\Cache\" + final + "_icon" + (isPNG ? ".png" : ".gif");

            return finalPath;
        }

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
            Globals.emoteDict = File.Load("C:\\gecko2.gek").ToString().Split('ҩ').Select(part => part.Split('⁊')).Where(part => part.Length == 2).ToDictionary(sp => sp[0], sp => sp[1]);

            //splits string by $
            string[] yesnt = Regex.Split(stuff, @"(?<!\\)\$");

            //removes empty strings which would break this command
            yesnt = yesnt.Where(x => !string.IsNullOrEmpty(x)).ToArray();

            //final string array with converted segments
            string[] final = new string[yesnt.Length];

            for (int i = 0; i < yesnt.Length; i++)
            {
                //checks if a key exists for the segment and if it is prefaced by \
                if (Globals.emoteDict.ContainsKey(yesnt[i]) && !(yesnt[i][0] == '\\'))
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