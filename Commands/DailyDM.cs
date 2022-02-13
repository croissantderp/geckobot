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

        //user/channel id, (is channel, year, last gecko, time)
        public static Dictionary<ulong, (bool, int, int, string)> DmUsers = new(); //people to dm for daily gecko images
        public static Dictionary<ulong, System.Timers.Timer> DmTimers = new Dictionary<ulong, System.Timers.Timer>();
        public static Dictionary<ulong, DateTime> DmTimersLastCheck = new Dictionary<ulong, DateTime>();

        public static bool allowedToPfp = true;

        public static void LoadLocalInfo()
        {
            try
            {
                _year = int.Parse(FileUtils.Load(@"..\..\Cache\gecko4.gek").Split("$")[0]);
                _lastRun = int.Parse(FileUtils.Load(@"..\..\Cache\gecko4.gek").Split("$")[1]);
            }
            catch
            {

            }
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
            try
            {
                Dictionary<string, string> parsedDict =
                    DmUsers.Select(a => new KeyValuePair<string, string>(a.Key.ToString(), a.Value.Item1.ToString() + "," + a.Value.Item2.ToString() + "," + a.Value.Item3.ToString() + "," + a.Value.Item4))
                    .ToDictionary(a => a.Key, a => a.Value);

                //saves info
                FileUtils.Save(Globals.DictToString(parsedDict, "{0} ⁊ {1} ҩ "), @"..\..\Cache\gecko3.gek");
            }
            catch
            {

            }
        }

        //parses time in hh:mm:ss format
        public static double returnTimeToNextCheck(string unparsed, bool forceNextDay = false)
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

            //extra one for edge case of time falling on 12:00 am
            newTime = newTime.AddDays((newTime <= DateTime.Now || forceNextDay) ? 1 : 0);
            newTime = newTime.AddDays((newTime <= DateTime.Now) ? 1 : 0);

            Console.WriteLine(newTime <= DateTime.Now);
            Console.WriteLine(newTime);

            return (newTime - DateTime.Now).TotalMilliseconds;
        }

        //checks
        [Command("check")]
        [Summary("Checks whether the daily dm needs to be sent.")]
        public async Task check()
        {
            RefreshUserDict();

            var ran = await runChecks(Context.User.Id);
            
            if (ran == 1)
                await ReplyAsync("checked and updated");
            else if (ran == 0)
                await ReplyAsync("checked");
            else
            {
                await ReplyAsync("you are not signed up");
            }
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
                int yes = await runChecks(key, true, false);
                if (yes == 1) i++;
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

        //checks
        [Command("ddm last checked")]
        [Summary("Sends your last check time.")]
        public async Task lastCheckInfo([Summary("The user to send info about.")] string user = null)
        {
            if (user == null)
            {
                if (!DmUsers.ContainsKey(Context.User.Id))
                {
                    await ReplyAsync("you have not signed up for the daily dm!");
                    return;
                }

                var a = DmTimersLastCheck[Context.User.Id];
                await ReplyAsync(a + " UTC");
            }
            else
            {
                ulong temp = ulong.Parse(user);

                if (!DmUsers.ContainsKey(temp))
                {
                    await ReplyAsync("that user has not signed up for the daily dm!");
                    return;
                }

                var a = DmTimersLastCheck[temp];
                await ReplyAsync(a + " UTC");
            }
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
            if (year > ((Gec._highestGecko + 1) / 366))
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
        [Command("user join")]
        [Alias("sign up")]
        [Summary("Signs you up for daily dm.")]
        public async Task dmgec([Summary("Bool whether you want to sign up or not.")] bool yes, [Summary("The time to send the notice everyday. (Note: The time is in a 1-24 hour format in UTC)")] string time = "00:00:00", [Summary("The year to sign up for.")] int year = 1)
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
                    DmUsers.Add(user.Id, (false, year, DateTime.UtcNow.DayOfYear - 1, time));

                    //saves info
                    SaveUserDict();

                    //DMs the user
                    await user.SendMessageAsync("daily gecko updates have been set up.");

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
                    await user.SendMessageAsync("gecko updates have been canceled.");

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
        [Command("channel join")]
        [Summary("adds a channel to the daily dm system.")]
        public async Task addchannel([Summary("Bool whether you want to sign up or not.")] bool join, [Summary("The id of the channel you want to sign up.")] string strchannelid, [Summary("The time to send the notice everyday. (Note: The time is in a 1-24 hour format in UTC)")] string time = "00:00:00", [Summary("The year to sign up for.")] int year = 1)
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
                    DmUsers.Add(channelid, (true, year, DateTime.UtcNow.DayOfYear - 1, time));

                    //saves info
                    SaveUserDict();

                    //adds reaction
                    await (Context.Client.GetChannel(channelid) as IMessageChannel).SendMessageAsync("This channel has been set up to recieve gecko updates.");
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
                    await (Context.Client.GetChannel(channelid) as IMessageChannel).SendMessageAsync("This channel has been removed from gecko updates.");
                }
                else
                {
                    await ReplyAsync("this channel is already not signed up!");
                }
            }
        }

        public async Task<int> runChecks(ulong id, bool natural = false, bool force = false)
        {
            Console.WriteLine($"Initiated runChecks with id {id} at {DateTime.Now}");
            RefreshUserDict();

            if (!DmUsers.ContainsKey(id))
            {
                return -1;
            }

            bool wasRefreshed = false;
            
            var timeArray = DmUsers[id].Item4.Split(":");
            int seconds = (int.Parse(timeArray[0]) * 60 + int.Parse(timeArray[1])) * 60 + int.Parse(timeArray[2]);
            int useconds = (DateTime.UtcNow.Hour * 60 + DateTime.UtcNow.Minute) * 60 + DateTime.UtcNow.Second;

            // Run daily dm if it has been a day since the last dm
            if (force || (DmUsers[id].Item3 != DateTime.UtcNow.DayOfYear && seconds <= useconds))
            {
                await dailydm(id);
                wasRefreshed = true;
            }

            if (natural)
            {
                if (DmTimersLastCheck.ContainsKey(id)) DmTimersLastCheck.Remove(id);
                DmTimersLastCheck.Add(id , DateTime.UtcNow);

                initiateUserTimer(id, wasRefreshed);
            }

            return wasRefreshed == false ? 0 : 1;
        }

        public void initiateUserTimer(ulong id, bool force = false)
        {
            if (!DmUsers.ContainsKey(id))
            {
                return;
            }

            if (DmTimers.ContainsKey(id))
            {
                DmTimers[id].Dispose();
                DmTimers.Remove(id);
            }

            //starts a timer with desired amount of time

            double time = returnTimeToNextCheck(DmUsers[id].Item4, force);

            System.Timers.Timer t = new(time);
            t.Elapsed += async (sender, e) => await runChecks(id, true);
            t.Start();

            DmTimers.Add(id, t);

            Console.WriteLine($"Initiated timer with id {id} to {DateTime.Now.AddMilliseconds(time)}");
        }

        //sends daily dm
        async Task dailydm(ulong id)
        {
            Console.WriteLine($"Initiated dailydm with id {id} at {DateTime.Now}");
            var timeArray = DmUsers[id].Item4.Split(":");
            int seconds = (int.Parse(timeArray[0]) * 60 + int.Parse(timeArray[1])) * 60 + int.Parse(timeArray[2]);

            Gec.RefreshGec();

            int year = DmUsers[id].Item2;

            //generates statement to send
            DateTime date = DateTime.UtcNow;

            string final = $"Today is {date.ToString("d")}. Day {date.DayOfYear} of the year {date.Year} (gecko: {EmoteUtils.removeforbidden(Gec.geckos[DriveUtils.addZeros(((year - 1) * 367) + (date.DayOfYear - 1))])}) \n" +
                $"Other geckos of today include: ";

            for (int i = 0; (date.DayOfYear - 1) + (i * 367) < Gec._highestGecko; i++)
            {
                final += $" {EmoteUtils.removeforbidden(Gec.geckos[DriveUtils.addZeros((date.DayOfYear - 1) + (i * 367))])}";
            }

            if (DmUsers[id].Item1 == false)
            {
                if (_client.GetUser(id) == null)
                {
                    //removes user form list
                    DmUsers.Remove(id);

                    //saves info
                    SaveUserDict();

                    return;
                }

                await (_client.GetUser(id) as IUser).SendFileAsync(DriveUtils.ImagePath(((year - 1) * 367) + (date.DayOfYear - 1), false), final);
            }
            else
            {
                if (_client.GetChannel(id) == null)
                {
                    //removes user form list
                    DmUsers.Remove(id);

                    //saves info
                    SaveUserDict();

                    return;
                }

                IChannel channel = _client.GetChannel(id);
                IUserMessage message = await (channel as IMessageChannel).SendFileAsync(DriveUtils.ImagePath(((year - 1) * 367) + (date.DayOfYear - 1), false), final);

                if (channel is INewsChannel)
                {
                    await message.CrosspostAsync();
                }
            }

            var temp = DmUsers[id];
            temp.Item3 = date.DayOfYear;
            DmUsers.Remove(id);
            DmUsers.Add(id, temp);

            SaveUserDict();

            checkProfile();
        }

        // Force updates
        [Command("check profile")]
        [Summary("Checks the profile.")]
        public async Task checkProfileCmd()
        {
            allowedToPfp = true;
            checkProfile();
            await ReplyAsync("profile checked");
        }

        public void checkProfile()
        {
            if (!allowedToPfp)
                return;

            LoadLocalInfo();

            string path = "";
            if (DriveUtils.ImagePath(((_year - 1) * 367) + (DateTime.Now.DayOfYear - 1), false).Split(".").Last() == "gif")
            {
                try
                {
                    if (DriveUtils.ImagePath(((_year - 1) * 367) + (DateTime.Now.DayOfYear - 1), true).Split(".").Last() == "gif")
                    {
                        path = @"..\..\..\IMG_20190521_203810.jpg";
                    }
                    else
                    {
                        path = DriveUtils.ImagePath(((_year - 1) * 367) + (DateTime.Now.DayOfYear - 1), true);
                    }
                }
                catch
                {
                    path = @"..\..\..\IMG_20190521_203810.jpg";
                }
            }
            else
            {
                path = DriveUtils.ImagePath(((_year - 1) * 367) + (DateTime.Now.DayOfYear - 1), false);
            }

            //changes geckobot's profile to new gecko
            Utils.Utils.changeProfile(
                _client, path
                ).GetAwaiter().GetResult();

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

            //updates last run counter
            _lastRun = DateTime.Now.DayOfYear;

            try
            {
                SaveLocalInfo();
            }
            catch
            {

            }
        }
    }
}