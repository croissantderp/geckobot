using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Discord;
using GeckoBot.Commands;

namespace GeckoBot.Utils
{
    // Miscellaneous utils go here
    public class Utils
    {
        public static async Task<bool> changeProfile(IDiscordClient client, string path)
        {
            ISelfUser self = client.CurrentUser;

            try
            {
                Image image = new(path);

                await self.ModifyAsync(x =>
                {
                    x.Avatar = image;
                });

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> changeName(IDiscordClient client, string name)
        {
            ISelfUser self = client.CurrentUser;

            try
            {
                await self.ModifyAsync(x =>
                {
                    x.Username = name;
                });

                return true;
            }
            catch
            {
                return false;
            }
        }
        
        
    }
}