using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        private static int _lastRun = DateTime.Now.DayOfYear; //last time bot was run and daily geckoimage was sent
        private static DateTime _lastCheck = DateTime.Now;
        
        public static readonly List<ulong> DmUsers = new(); //people to dm for daily gecko images

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
            _lastRun = int.Parse(FileUtils.Load(@"..\..\Cache\gecko4.gek"));
            bool wasRefreshed = false;
            
            DateTime time = DateTime.Now;
            int minutes = time.Minute;

            _lastCheck = DateTime.Now;

            // If the hour loop is misaligned, reset timer and rerun FirstStart at the strike of the next hour
            if (minutes > 0)
            {
                //stops current timer
                dmTimer.Close();

                //sets timer to amount of time until next hour plus a little bit
                System.Timers.Timer timer2 = new((60 - minutes) * 60 * 1000 + 1000);
                timer2.Elapsed += async (sender, e) => await FirstStart(timer2, false);
                timer2.Start();

                dmTimer2 = timer2;

                //sets some variables so stats show up
                Started = false;
                IsCounting = true;
            }

            // Run daily dm if it has been a day since the last dm
            if (force || _lastRun != DateTime.Now.DayOfYear)
            {
                await dailydm();
                wasRefreshed = true;
            }

            // Refresh highest gecko
            await Program.gec.RefreshHighestGec();

            return wasRefreshed;
        }

        // Starts the loop and runs first check
        // Called when the first hour (hh:00) is reached so that the subsequent 1 hour loop runs checks at :00 as well
        private async Task FirstStart(System.Timers.Timer timer, bool reply)
        {
            timer.Close();
            
            await runChecks();

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
            //generates statement to send
            DateTime date = DateTime.Today;
            
            //DMs everybody on the list
            await DmGroup(
                DriveUtils.ImagePath(date.DayOfYear - 1, false),
                $"Today is {date.ToString("d")}. Day {date.DayOfYear} of the year {date.Year} (gecko: {Gec.geckos[DriveUtils.addZeros(date.DayOfYear - 1)]})");

            //changes geckobot's profile to new gecko
            Utils.Utils.changeProfile(
                _client, 
                DriveUtils.ImagePath(date.DayOfYear - 1, false));

            //updates last run counter
            _lastRun = DateTime.Now.DayOfYear;

            FileUtils.Save(_lastRun.ToString(), @"..\..\Cache\gecko4.gek");
        }

        // Dms a group of users
        public async Task DmGroup(string path, string content)
        {
            RefreshDmGroup();
            DiscordSocketClient client = _client;

            DateTime date = DateTime.Today;

            //if it is geckobot's birthday
            bool isBirthday = date.DayOfYear == 288;
            
            // Map ids to users
            var users = DmUsers.Select(client.GetUser);
            
            //Console.WriteLine(DmUsers);
            // Note: line 349 throws concurrent modification on my computer
            // Should get around to fixing that some time

            //DMs everybody on the list
            foreach (var a in users)
            {
                //sends file with exception for leap years
                await a.SendFileAsync(
                    path,
                    content);

                if (isBirthday)
                {
                    await a.SendMessageAsync("happy birthday geckobot :cake:");
                }
            }
        }
        
        // Updates the DmUsers list from the file
        private static void RefreshDmGroup()
        {
            FileUtils.checkForExistance();
            
            //clears
            DmUsers.Clear();

            //gets info
            string content = FileUtils.Load(@"..\..\Cache\gecko3.gek");
            if (content != "") 
                DmUsers.AddRange(content.Split(",").Select(ulong.Parse));
        }
    }
}