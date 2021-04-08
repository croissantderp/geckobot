using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using GeckoBot.Utils;

namespace GeckoBot.Commands
{
    [Summary("Alert commands. There is a 5 minute cooldown between alerts. A user may only have one alert at a time.")]
    public class Alert : ModuleBase<SocketCommandContext>
    {
        //prefix dictionary
        public static Dictionary<string, string> alerts = new();

        //loads poll dictionary as string and converts it back into dictionary
        private static void RefreshAlertsDict()
        {
            FileUtils.checkForExistance();

            alerts = Regex.Split(FileUtils.Load(@"..\..\Cache\gecko9.gek"), @"\s(?<!\\)ҩ\s")
                .Select(part => Regex.Split(part, @"\s(?<!\\)\⁊\s"))
                .Where(part => part.Length == 2)
                .ToDictionary(sp => sp[0], sp => sp[1]);
        }

        [Command("as")]
        [Summary("Sets an alert phrase for the user")]
        public async Task SetAlert(string input)
        {
            RefreshAlertsDict();

            if (alerts.ContainsKey(Context.User.Id.ToString()))
            {
                alerts.Remove(Context.User.Id.ToString());
            }

            alerts.Add(Context.User.Id.ToString(), input);

            FileUtils.Save(Globals.DictToString(alerts, "{0} ⁊ {1} ҩ "), @"..\..\Cache\gecko9.gek");

            //adds check emote after done
            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

        [Command("ar")]
        [Summary("Removes an alert phrase for the user")]
        public async Task RemoveAlert()
        {
            RefreshAlertsDict();

            if (alerts.ContainsKey(Context.User.Id.ToString()))
            {
                alerts.Remove(Context.User.Id.ToString());

                FileUtils.Save(Globals.DictToString(alerts, "{0} ⁊ {1} ҩ "), @"..\..\Cache\gecko9.gek");

                //adds check emote after done
                await Context.Message.AddReactionAsync(new Emoji("✅"));
            }
            else
            {
                //if emote is not found
                await ReplyAsync("you do not have any alerts");
            }

        }

        [Command("al")]
        [Summary("Sends the raw alert storage text file.")]
        public async Task al()
        {
            await Context.Channel.SendFileAsync(@"..\..\Cache\gecko9.gek");
        }
        
        public static void AlertCooldown(string key, string value, System.Timers.Timer timer)
        {
            alerts.Add(key, value);

            //stops timer
            timer.Close();
        }

    }
}
