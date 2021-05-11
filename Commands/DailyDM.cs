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
using Microsoft.Win32;

namespace GeckoBot.Commands
{
    [Summary("Commands relating to the daily message system.")]
    public class DailyDM : ModuleBase<SocketCommandContext>
    {
        // Receive the client via dependency injection
        public DiscordSocketClient _client { get; set; }

        private static System.Timers.Timer dmTimer = new(); //the primary timer for dms
        private static System.Timers.Timer dmTimer2 = new(); //the secondary timer for dms

        public static int year = 1;
        private static int _lastRun = DateTime.Now.DayOfYear; //last time bot was run and daily geckoimage was sent
        private static DateTime _lastCheck = DateTime.Now;
        
        public static readonly List<ulong> DmUsers = new(); //people to dm for daily gecko images
        public static readonly List<ulong> Channelthings = new(); //people to dm for daily gecko images

        public static bool Started = false; //is timer is running
        public static bool CounterStarted = false; //if the counter has started at least once
        public static bool IsCounting = false; //if the counter is counting
        public static bool EverStarted = false; //if timer has ever started

        [Command("last checked")]
        [Summary("Gets the time since the daily dm was last checked.")]
        public async Task last()
        {
            await ReplyAsync(_lastCheck.ToString());
        }

        //starts timer for various checks
        [Command("start")]
        [Summary("Starts the timer.")]
        public async Task start()
        {
            await Context.Client.SetGameAsync("`what do you do?");

            //if started or not
            if (Started)
            {
                await ReplyAsync("hourly check already started");
            }
            else
            {
                //getting minutes
                int minutes = DateTime.Now.Minute;

                if (IsCounting)
                {
                    await ReplyAsync("hourly check already scheduled, will start in t - " + (60 - minutes) + " minutes");
                }
                else
                {
                    //checks
                    await check();

                    IsCounting = true;

                    await ReplyAsync("hourly check will start in t - " + (60 - minutes) + " minutes");
                }
            }
        }

        //checks
        [Command("check")]
        [Summary("Checks whether the daily dm needs to be sent.")]
        public async Task check()
        {
            var ran = await runChecks();
            
            if (ran)
                await ReplyAsync("checked and updated");
            else
                await ReplyAsync("checked");
        }
        
        // Force updates
        [RequireGeckobotAdmin]
        [Command("fcheck")]
        [Alias("force check")]
        [Summary("Force updates the daily dm.")]
        public async Task fcheck()
        {
            await runChecks(true);
            await ReplyAsync("force updated");
        }

        [RequireGeckobotAdmin]
        [Command("year")]
        [Summary("Sets a new year for the daily dm.")]
        public async Task setyear(int year2)
        {
            // Refresh highest gecko
            await Program.gec.RefreshHighestGec();

            if (year2 > (Gec._highestGecko % 367))
            {
                await ReplyAsync("this year does not exist yet");
                return;
            }

            year = year2;

            FileUtils.Save(year + "$" + _lastRun.ToString(), @"..\..\Cache\gecko4.gek");

            await ReplyAsync("year updated to " + year);
        }

        //sets up daily dms
        [Command("dm")]
        [Alias("sign up")]
        [Summary("Signs you up for daily dm.")]
        public async Task dmgec([Summary("Bool whether you want to sign up or not.")] bool yes)
        {
            RefreshDmGroup();
            if (yes)
            {
                //gets current user
                IUser user = Context.User;

                //if they are already signed up
                if (DmUsers.Contains(user.Id))
                {
                    await ReplyAsync("you are already signed up!");
                }
                else
                {
                    //adds id
                    DmUsers.Add(user.Id);

                    //saves info
                    FileUtils.Save(string.Join(",", DmUsers), @"..\..\Cache\gecko3.gek");

                    //DMs the user
                    await user.SendMessageAsync("hi, daily gecko updates have been set up, cancel by '\\`dm false'");
                }

            }
            else
            {
                //gets current user
                IUser user = Context.User;

                //if the are already not signed up
                if (DmUsers.Contains(user.Id))
                {
                    //removes user form list
                    DmUsers.Remove(user.Id);

                    //saves info
                    FileUtils.Save(string.Join(",", DmUsers.ToArray()), @"..\..\Cache\gecko3.gek");

                    //DMs the user
                    await user.SendMessageAsync("hi, daily gecko updates have been canceled");

                }
                else
                {
                    await ReplyAsync("you are already not signed up!");
                }
            }
        }

        // Initialize timers and run initial checks
        public async Task initiatethings()
        {
            //checks
            await runChecks();
        }

        // Runs checks every hour
        // Returns a boolean indicating whether the daily dm was sent due to this function call
        // Passing true to force forces the dailydm to be sent
        // while passing true to skipHGecCheck will skip the refresh of the highest gecko
        // to prevent null reference exceptions during initialization if RefreshHighestGec finds a new highest gecko and calls dmGroup
        public async Task<bool> runChecks(bool force = false)
        {
            // Refresh highest gecko
            await Program.gec.RefreshHighestGec();

            year = int.Parse(FileUtils.Load(@"..\..\Cache\gecko4.gek").Split("$")[0]);
            _lastRun = int.Parse(FileUtils.Load(@"..\..\Cache\gecko4.gek").Split("$")[1]);
            bool wasRefreshed = false;
            
            DateTime time = DateTime.Now;
            int seconds = time.Minute * 60 + time.Second;

            _lastCheck = DateTime.Now;

            // If the hour loop is misaligned, reset timer and rerun FirstStart at the strike of the next hour
            if (time.Minute > 0)
            {
                //stops current timer
                dmTimer.Close();

                //sets timer to amount of time until next hour plus a little bit
                System.Timers.Timer timer2 = new((3601 - seconds) * 1000);
                timer2.Elapsed += async (sender, e) => await FirstStart(timer2, false);
                timer2.Start();

                dmTimer2 = timer2;

                //sets some variables so stats show up
                Started = false;
                IsCounting = true;
            }

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

            // Run daily dm if it has been a day since the last dm
            if (force || _lastRun != DateTime.Now.DayOfYear)
            {
                await dailydm();
                wasRefreshed = true;
            }

            return wasRefreshed;
        }

        // Starts the loop and runs first check
        // Called when the first hour (hh:00) is reached so that the subsequent 1 hour loop runs checks at :00 as well
        private async Task FirstStart(System.Timers.Timer timer, bool reply)
        {
            timer.Close();
            
            Start();

            if (!CounterStarted)
            {
                CounterStarted = true;
                if (reply)
                {
                    await ReplyAsync("hourly check started");
                }
            }

        }

        // Starts the one hour loop for running checks
        private void Start()
        {
            dmTimer.Close();
            
            System.Timers.Timer timer = new(1000*60*60);
            timer.Elapsed += async (sender, e) => await runChecks();
            timer.Start();

            dmTimer = timer;

            //makes sure there is only one timer
            Started = true;
        }

        //sends daily dm
        async Task dailydm()
        {
            Gec.RefreshGec();

            //generates statement to send
            DateTime date = DateTime.Today;
            
            string final = $"Today is {date.ToString("d")}. Day {date.DayOfYear} of the year {date.Year} (gecko: {Gec.geckos[DriveUtils.addZeros(((year - 1) * 367) + (date.DayOfYear - 1))]}) \n" +
                $"Other geckos of today include: ";

            int i = 0;
            while ((date.DayOfYear - 1) + (i * 367) < Gec._highestGecko)
            {
                final += $" {Gec.geckos[DriveUtils.addZeros((date.DayOfYear - 1) + (i * 367))]}";
                i++;
            }

            //DMs everybody on the list
            await DmGroup(
                DriveUtils.ImagePath(((year - 1) * 367) + (date.DayOfYear - 1), false),
                final);

            //changes geckobot's profile to new gecko
            Utils.Utils.changeProfile(
                _client, 
                DriveUtils.ImagePath(((year - 1) * 367) + (date.DayOfYear - 1), false));

            //updates last run counter
            _lastRun = DateTime.Now.DayOfYear;

            FileUtils.Save(year + "$" + _lastRun.ToString(), @"..\..\Cache\gecko4.gek");
        }

        // Dms a group of users
        public async Task DmGroup(string path, string content, bool isFile = true)
        {
            RefreshDmGroup();
            DiscordSocketClient client = _client;

            DateTime date = DateTime.Today;

            //if it is geckobot's birthday
            bool isBirthday = date.DayOfYear == 288;
            
            // Map ids to users

            var users = DmUsers.Select(client.GetUser);
            var channels = Channelthings.Select(client.GetChannel);

            //DMs everybody on the list
            foreach (var a in users.Distinct().ToList())
            {
                if (isFile)
                {
                    await a.SendFileAsync(
                        path,
                        content);
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
                var temp = a as IMessageChannel;
                if (isFile)
                {
                    await temp.SendFileAsync(
                        path,
                        content);
                }
                else
                {
                    await temp.SendMessageAsync(content);
                }
            }
        }
        
        // Updates the DmUsers list from the file
        public static void RefreshDmGroup()
        {
            FileUtils.checkForExistance();
            
            //clears
            DmUsers.Clear();

            //gets info
            string content = FileUtils.Load(@"..\..\Cache\gecko3.gek");
            if (content != "")
            {
                DmUsers.AddRange(content.Split(",").Where(a => !a.Contains("c")).Select(ulong.Parse));
                Channelthings.AddRange(content.Split(",").Where(a => a.Contains("c")).Select(a => ulong.Parse(a.Remove(0,1))));
            }
        }
    }
}