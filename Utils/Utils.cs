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


        //rounds values (found off of stack overflow I don't know how it works)
        public static decimal RoundNum(decimal d, int digits)
        {
            int neg = 1;
            if (d < 0)
            {
                d = d * (-1);
                neg = -1;
            }

            int n = 0;
            if (d > 1)
            {
                while (d > 1)
                {
                    d = d / 10;
                    n++;
                }
                d = Math.Round(d * (decimal)Math.Pow(10, digits));
                d = d * (decimal)Math.Pow(10, n - digits);
            }
            else
            {
                while ((double)d < 0.1)
                {
                    d = d * 10;
                    n++;
                }
                d = Math.Round(d * (decimal)Math.Pow(10, digits));
                d = d / (decimal)Math.Pow(10, n + digits);
            }

            return d * neg;
        }

    }
}