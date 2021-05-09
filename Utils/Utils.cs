using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Discord;
using GeckoBot.Commands;

namespace GeckoBot.Utils
{
    // Miscellaneous utils go here
    public class Utils
    {
        public static async void changeProfile(IDiscordClient client, string path)
        {
            ISelfUser self = client.CurrentUser;

            Image image = new (path);

            try
            {
                await self.ModifyAsync(x =>
                {
                    x.Avatar = image;
                });
            }
            catch
            {

            }
        }

        public static async void changeName(IDiscordClient client, string name)
        {
            ISelfUser self = client.CurrentUser;

            await self.ModifyAsync(x =>
            {
                x.Username = name;
            });
        }
        
        
    }
}