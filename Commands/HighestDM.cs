﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GeckoBot.Preconditions;

using GeckoBot.Utils;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace GeckoBot.Commands
{
    [Summary("Commands relating to the highest gecko message system.")]
    public class HighestDM : ModuleBase<SocketCommandContext>
    {
        // Receive the client via dependency injection
        public DiscordSocketClient _client { get; set; }
        private static System.Timers.Timer htimer = new ();
        private static DateTime lastchecked = new DateTime();

        // Initialize timers and run initial checks
        public async Task initiatethings()
        {
            //starts a timer with desired amount of time
            System.Timers.Timer t = new(1000 * 60 * 10);
            t.Elapsed += async (sender, e) => await checkHighest();
            t.Start();

            htimer = t;

            await checkHighest();
        }

        // Initialize timers and run initial checks
        private async Task checkHighest()
        {
            await Program.gec.RefreshHighestGec();
            Program.ddm.checkProfile();

            DailyDM.RefreshUserDict();

            foreach (ulong key in DailyDM.DmUsers.Keys)
            {
                await Program.ddm.runChecks(key, true);
            }

            lastchecked = DateTime.Now.ToUniversalTime();
        }

        //checks
        [Command("last checked")]
        [Summary("Gets the time when the highest gecko was last sent.")]
        public async Task lastcheck()
        {
            await ReplyAsync(lastchecked + " UTC");
        }

        //checks
        [Command("hcheck")]
        [Alias("highest check")]
        [Summary("Checks whether the highest gecko needs to be sent.")]
        public async Task check()
        {
            await Program.gec.RefreshHighestGec();
            Program.ddm.checkProfile();

            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

        [RequireGeckobotAdmin]
        [Command("send to group")]
        [Summary("sends a message to the daily dm group.")]
        public async Task test([Summary("the message to send to the group.")][Remainder] string message)
        {
            await DmGroup("", message, false);

            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

        // Dms a group of users
        public async Task DmGroup(string path, string content, bool isFile = true)
        {
            DiscordSocketClient client = _client;

            DateTime date = DateTime.Today;

            //if it is geckobot's birthday
            bool isBirthday = date.DayOfYear == 288;

            // Map ids to users

            var users = DailyDM.DmUsers.Keys.Where(a => !DailyDM.DmUsers[a].Item1).Select(client.GetUser);
            var channels = DailyDM.DmUsers.Keys.Where(a => DailyDM.DmUsers[a].Item1).Select(client.GetChannel);

            //DMs everybody on the list
            foreach (var a in users.Distinct().ToList())
            {
                if (isFile)
                {
                    await a.SendFileAsync(path, content);
                }
                else
                {
                    await a.SendMessageAsync(content);
                }

                if (isBirthday)
                {
                    await a.SendMessageAsync("happy birthday geckobot :cake:");
                }
            }

            //sends messages in channels
            foreach (var a in channels.Distinct().ToList())
            {
                if (!(a is IMessageChannel temp)) return;
                IUserMessage message = (isFile) 
                    ? await temp.SendFileAsync(path, content) 
                    : await temp.SendMessageAsync(content);
                if (temp is INewsChannel)
                    await message.CrosspostAsync();
            }
        }

    }
}
