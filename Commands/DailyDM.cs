using System;
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
    [Summary("Commands relating to the daily message system.")]
    public class DailyDM : ModuleBase<SocketCommandContext>
    {
        // Receive the client via dependency injection
        public DiscordSocketClient _client { get; set; }

        private static int _lastRun = DateTime.Now.DayOfYear; //last time bot was run and daily geckoimage was sent
        private static int _year = 1;

        //user/channel id, (is channel, year, last gecko, string)
        public static Dictionary<ulong, (bool, int, int, string)> DmUsers = new(); //people to dm for daily gecko images
        public static Dictionary<ulong, System.Timers.Timer> DmTimers = new Dictionary<ulong, System.Timers.Timer>();

        public static void LoadLocalInfo()
        {
            _year = int.Parse(FileUtils.Load(@"..\..\Cache\gecko4.gek").Split("$")[0]);
            _lastRun = int.Parse(FileUtils.Load(@"..\..\Cache\gecko4.gek").Split("$")[1]);
        }

        public static void SaveLocalInfo()
        {
            FileUtils.Save(_year + "$" + _lastRun, @"..\..\Cache\gecko4.gek");
        }

        //loads user dictionary as string and converts it back into dictionary
        public static void RefreshUserDict()
        {
            DmUsers = Regex.Split(FileUtils.Load(@"..\..\Cache\gecko3.gek"), @"\s(?<!\\)ҩ\s")
                .Select(part => Regex.Split(part, @"\s(?<!\\)\⁊\s"))
                .Where(part => part.Length == 2)
                .ToDictionary(
                    sp => ulong.Parse(sp[0]), 
                    sp => (bool.Parse(sp[1].Split(",")[0]), int.Parse(sp[1].Split(",")[1]), int.Parse(sp[1].Split(",")[2]), sp[1].Split(",")[3]));
        }

        //saves user dictionary into a file
        public static void SaveUserDict()
        {
            Dictionary<string, string> parsedDict = 
                DmUsers.Select(a => new KeyValuePair<string, string>(a.Key.ToString(), a.Value.Item1.ToString() + "," + a.Value.Item2.ToString() + "," + a.Value.Item3.ToString() + "," + a.Value.Item4))
                .ToDictionary(a => a.Key, a => a.Value);

            //saves info
            FileUtils.Save(Globals.DictToString(parsedDict, "{0} ⁊ {1} ҩ "), @"..\..\Cache\gecko3.gek");
        }

        //parses time in hh:mm:ss format
        public static double returnTimeToNextCheck(string unparsed)
        {
            var parsed = unparsed
                .Split(":")
                .Select(int.Parse)
                .ToArray();

            var newTime = TimeZoneInfo.ConvertTimeFromUtc(new DateTime(
                DateTime.Now.Year,
                DateTime.Now.Month,
                DateTime.Now.Day,
                parsed[0],
                parsed[1],
                parsed[2]), TimeZoneInfo.Local);

            if (newTime < DateTime.Now)
            {
                return (TimeZoneInfo.ConvertTimeFromUtc(new DateTime(
                    DateTime.Now.Year,
                    DateTime.Now.Month,
                    DateTime.Now.Day + 1,
                    parsed[0],
                    parsed[1],
                    parsed[2]), TimeZoneInfo.Local) - DateTime.Now).TotalMilliseconds;
            }

            return (newTime - DateTime.Now).TotalMilliseconds;
        }

        //checks
        [Command("check")]
        [Summary("Checks whether the daily dm needs to be sent.")]
        public async Task check()
        {
            RefreshUserDict();

            var ran = await runChecks(Context.User.Id);
            
            if (ran)
                await ReplyAsync("checked and updated");
            else
                await ReplyAsync("checked");
        }

        
        //checks
        [Command("bcheck")]
        [Summary("Checks whether the daily dm needs to be sent for all users.")]
        public async Task bcheck()
        {
            RefreshUserDict();

            int i = 0;
            foreach (ulong key in DmUsers.Keys)
            {
                bool yes = await runChecks(key, true, false);
                if (yes) i++;
            }

            await ReplyAsync("checked, " + i + " users updated.");
        }

        //checks
        [Command("ddm info")]
        [Summary("Sends your info.")]
        public async Task info()
        {
            RefreshUserDict();

            if (!DmUsers.ContainsKey(Context.User.Id))
            {
                await ReplyAsync("you have not signed up for the daily dm!");
                return;
            }

            var a = DmUsers[Context.User.Id];
            await ReplyAsync("year: " + a.Item2 + ", most recent gecko: " + a.Item3 + ", check time: " + a.Item4 + " UTC");
        }

        // Force updates
        [Command("fcheck")]
        [Alias("force check")]
        [Summary("Force updates the daily dm.")]
        public async Task fcheck()
        {
            await runChecks(Context.User.Id, false, true);
            await ReplyAsync("force updated");
        }

        [Command("year")]
        [Summary("Sets a new year for the daily dm.")]
        public async Task setyear([Summary("The year to set")] int year)
        {
            RefreshUserDict();

            // Refresh highest gecko
            await Program.gec.RefreshHighestGec();

            if (!validyear(year))
            {
                await ReplyAsync("this year is not completed yet");
                return;
            }

            var temp = DmUsers[Context.User.Id];
            temp.Item2 = year;
            DmUsers.Remove(Context.User.Id);
            DmUsers.Add(Context.User.Id, temp);

            SaveUserDict();

            //adds reaction
            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

        [RequireGeckobotAdmin]
        [Command("local year")]
        [Summary("Sets a new year for geckobot's profile.")]
        public async Task setlyear([Summary("The year to set.")] int year)
        {
            // Refresh highest gecko
            await Program.gec.RefreshHighestGec();

            if (!validyear(year))
            {
                await ReplyAsync("this year is not completed yet");
                return;
            }

            LoadLocalInfo();

            _year = year;

            SaveLocalInfo();
            //adds reaction
            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

        bool validyear(int year)
        {
            if (year > (Gec._highestGecko / 367))
            {
                return false;
            }

            return true;
        }

        [Command("time")]
        [Summary("Sets a new time for the daily dm.")]
        public async Task settime([Summary("The new time to set. (Note: The time is in a 1-24 hour format in UTC)")] string time)
        {
            RefreshUserDict();

            if (!validtime(time))
            {
                await ReplyAsync("please enter a valid time");
                return;
            }

            var temp = DmUsers[Context.User.Id];
            temp.Item4 = time;
            DmUsers.Remove(Context.User.Id);
            DmUsers.Add(Context.User.Id, temp);

            SaveUserDict();

            if (DmTimers.ContainsKey(Context.User.Id))
            {
                DmTimers[Context.User.Id].Dispose();
                DmTimers.Remove(Context.User.Id);
            }
            initiateUserTimer(Context.User.Id);

            //adds reaction
            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

        bool validtime(string time)
        {
            var split = time.Split(":");
            if (split.Length != 3)
            {
                return false;
            }

            int aaaaa = 0;

            if (!int.TryParse(split[0], out aaaaa) || !int.TryParse(split[1], out aaaaa) || !int.TryParse(split[2], out aaaaa))
            {
                return false;
            }

            if (int.Parse(split[0]) > 23 || int.Parse(split[1]) > 59 || int.Parse(split[2]) > 59)
            {
                return false;
            }
            
            return true;
        }

        //sets up daily dms
        [Command("dm")]
        [Alias("sign up")]
        [Summary("Signs you up for daily dm.")]
        public async Task dmgec([Summary("Bool whether you want to sign up or not.")] bool yes, [Summary("The year to sign up for.")] int year = 1, [Summary("The time to send the notice everyday. (Note: The time is in a 1-24 hour format in UTC)")] string time = "")
        {
            if (yes)
            {
                if (!validtime(time))
                {
                    await ReplyAsync("please enter a valid time");
                    return;
                }

                if (!validyear(year))
                {
                    await ReplyAsync("this year is not completed yet");
                    return;
                }

                //gets current user
                IUser user = Context.User;

                //if they are already signed up
                if (DmUsers.Keys.Contains(user.Id))
                {
                    await ReplyAsync("you are already signed up!");
                }
                else
                {
                    //adds id
                    DmUsers.Add(user.Id, (false, year, DateTime.Now.ToUniversalTime().DayOfYear - 1, time));

                    //saves info
                    SaveUserDict();

                    //DMs the user
                    await user.SendMessageAsync("hi, daily gecko updates have been set up, cancel by '\\`dm false'");

                    //adds reaction
                    await Context.Message.AddReactionAsync(new Emoji("✅"));
                }

            }
            else
            {
                //gets current user
                IUser user = Context.User;

                //if the are already not signed up
                if (DmUsers.Keys.Contains(user.Id))
                {
                    //removes user form list
                    DmUsers.Remove(user.Id);

                    //saves info
                    SaveUserDict();

                    //DMs the user
                    await user.SendMessageAsync("hi, daily gecko updates have been canceled");

                    //adds reaction
                    await Context.Message.AddReactionAsync(new Emoji("✅"));
                }
                else
                {
                    await ReplyAsync("you are already not signed up!");
                }
            }
        }

        [RequireGeckobotAdmin]
        [Command("add channel")]
        [Summary("adds a channel to the daily dm system.")]
        public async Task addchannel([Summary("Bool whether you want to sign up or not.")] bool join, [Summary("The id of the channel you want to sign up.")] string strchannelid, [Summary("The year to sign up for.")] int year = 1, [Summary("The time to send the notice everyday. (Note: The time is in a 1-24 hour format in UTC)")] string time = "")
        {
            ulong channelid = ulong.Parse(strchannelid);

            if (join)
            {
                if (!validtime(time))
                {
                    await ReplyAsync("please enter a valid time");
                    return;
                }

                //if they are already signed up
                if (DmUsers.Keys.Contains(channelid))
                {
                    await ReplyAsync("this channel is already signed up!");
                }
                else
                {
                    //adds id
                    DmUsers.Add(channelid, (true, year, DateTime.Now.ToUniversalTime().DayOfYear - 1, time));

                    //saves info
                    SaveUserDict();

                    //adds reaction
                    await Context.Message.AddReactionAsync(new Emoji("✅"));
                }

            }
            else
            {
                //if the are already not signed up
                if (DmUsers.Keys.Contains(channelid))
                {
                    //removes user form list
                    DmUsers.Remove(channelid);

                    //saves info
                    SaveUserDict();

                    //adds reaction
                    await Context.Message.AddReactionAsync(new Emoji("✅"));
                }
                else
                {
                    await ReplyAsync("this channel is already not signed up!");
                }
            }
        }

        // Initialize timers and run initial checks
        public async Task initiatethings()
        {
            RefreshUserDict();

            foreach (ulong key in DmUsers.Keys)
            {
                await runChecks(key, true);
            }
        }

        public async Task<bool> runChecks(ulong id, bool natural = false, bool force = false)
        {
            RefreshUserDict();

            bool wasRefreshed = false;
            
            DirectoryInfo dir = new DirectoryInfo(@"../../../dectalk/audio/");

            //clears files in dectalk audio cache if some still exist for some reason
            foreach (FileInfo file in dir.GetFiles())
            {
                try
                {
                    file.Delete();
                }
                catch
                {
                    continue;
                }
            }

            var timeArray = DmUsers[id].Item4.Split(":");
            int seconds = (int.Parse(timeArray[0]) * 60 + int.Parse(timeArray[1])) * 60 + int.Parse(timeArray[2]);
            int useconds = (DateTime.Now.ToUniversalTime().Hour * 60 + DateTime.Now.ToUniversalTime().Minute) * 60 + DateTime.Now.ToUniversalTime().Second;

            // Run daily dm if it has been a day since the last dm
            if (force || (DmUsers[id].Item3 != DateTime.Today.ToUniversalTime().AddSeconds(seconds).DayOfYear && seconds < useconds))
            {
                await dailydm(id);
                wasRefreshed = true;
            }

            if (natural)
            {
                initiateUserTimer(id);
            }

            return wasRefreshed;
        }

        public void initiateUserTimer(ulong id)
        {
            RefreshUserDict();

            if (DmTimers.ContainsKey(id))
            {
                DmTimers[id].Dispose();
                DmTimers.Remove(id);
            }

            //starts a timer with desired amount of time
            System.Timers.Timer t = new(returnTimeToNextCheck(DmUsers[id].Item4));
            t.Elapsed += async (sender, e) => await runChecks(id, true);
            t.Start();

            DmTimers.Add(id, t);
        }

        //sends daily dm
        async Task dailydm(ulong id)
        {
            var timeArray = DmUsers[id].Item4.Split(":");
            int seconds = (int.Parse(timeArray[0]) * 60 + int.Parse(timeArray[1])) * 60 + int.Parse(timeArray[2]);

            RefreshUserDict();

            await Program.gec.RefreshHighestGec();

            Gec.RefreshGec();

            int year = DmUsers[id].Item2;

            //generates statement to send
            DateTime date = DateTime.Today.ToUniversalTime().AddSeconds(seconds);

            string final = $"Today is {date.ToString("d")}. Day {date.DayOfYear} of the year {date.Year} (gecko: {Gec.geckos[DriveUtils.addZeros(((year - 1) * 367) + (date.DayOfYear - 1))]}) \n" +
                $"Other geckos of today include: ";

            for (int i = 0; (date.DayOfYear - 1) + (i * 367) < Gec._highestGecko; i++)
            {
                final += $" {Gec.geckos[DriveUtils.addZeros((date.DayOfYear - 1) + (i * 367))]}";
            }

            if (DmUsers[id].Item1 == false)
            {
                await (_client.GetUser(id) as IUser).SendFileAsync(DriveUtils.ImagePath(((year - 1) * 367) + (date.DayOfYear - 1), false), final);
            }
            else
            {
                await (_client.GetChannel(id) as IMessageChannel).SendFileAsync(DriveUtils.ImagePath(((year - 1) * 367) + (date.DayOfYear - 1), false), final);
            }

            var temp = DmUsers[id];
            temp.Item3 = date.DayOfYear;
            DmUsers.Remove(id);
            DmUsers.Add(id, temp);

            SaveUserDict();

            checkProfile();
        }

        public void checkProfile()
        {
            LoadLocalInfo();

            if (DateTime.Now.DayOfYear != _lastRun)
            {
                //changes geckobot's profile to new gecko
                Utils.Utils.changeProfile(
                    _client,
                    DriveUtils.ImagePath(((_year - 1) * 367) + (DateTime.Now.DayOfYear - 1), false));

                //updates last run counter
                _lastRun = DateTime.Now.DayOfYear;
            }

            SaveLocalInfo();
        }
    }
}