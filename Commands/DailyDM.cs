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
        
        
        [Command("last checked")]
        [Summary("Gets the time since the daily dm was last checked.")]
        public async Task last()
        {
            await ReplyAsync(_lastCheck.ToString());
        }

        //starts timer for various checks
        [Command("start")]
        [Summary("Starts the timer.")]
        public async Task test()
        {
            await Context.Client.SetGameAsync("`what do you do?");

            //if started or not
            if (Timer.Started)
            {
                await ReplyAsync("hourly check already started");
            }
            else
            {
                //getting minutes
                int minutes = DateTime.Now.Minute;

                if (Timer.IsCounting)
                {
                    await ReplyAsync("hourly check already scheduled, will start in t - " + (60 - minutes) + " minutes");
                }
                else
                {
                    //sets timer to amount of time until next hour plus a little bit
                    System.Timers.Timer timer = new((60 - minutes) * 60 * 1000 + 1000);
                    timer.Start();

                    dmTimer2 = timer;

                    //checks
                    await check();

                    Timer.IsCounting = true;

                    await ReplyAsync("hourly check will start in t - " + (60 - minutes) + " minutes");
                }

                if (!Timer.EverStarted)
                {
                    SystemEvents.PowerModeChanged += PowerEvents;
                    Timer.EverStarted = true;
                }
            }
        }

        public void initiatethings()
        {
            //sets timer to amount of time until next hour plus a little bit
            System.Timers.Timer timer = new((60 - DateTime.Now.Minute)* 60 * 1000 + 1000);
            timer.Elapsed += async (sender, e) => await trueStart(timer, false);
            timer.Start();

            dmTimer2 = timer;

            Timer.IsCounting = true;

            if (_lastRun != DateTime.Now.DayOfYear)
            {
                //checks
                daily().RunSynchronously();
            }

            //sets timer to amount of time until next hour plus a little bit
            System.Timers.Timer timer2 = new(10000);
            timer2.Elapsed += async (sender, e) => await wait(timer2);
            timer2.Start();

            if (!Timer.EverStarted)
            {
                SystemEvents.PowerModeChanged += PowerEvents;
                Timer.EverStarted = true;
            }
        }

        public async Task wait(System.Timers.Timer timer)
        {
            await _client.SetGameAsync("`what do you do?");
            timer.Stop();
        }

        public async void PowerEvents(object sender, PowerModeChangedEventArgs e)
        {
            if (!Globals.isSleep)
            {
                Timer.timer.Stop();
                dmTimer.Stop();
                dmTimer2.Stop();

                Globals.isSleep = true;
            }
            else
            {
                Timer.timer.Start();
                dmTimer.Start();
                dmTimer2.Start();

                _lastRun = int.Parse(FileUtils.Load(@"..\..\Cache\gecko4.gek"));

                if (_lastRun != DateTime.Now.DayOfYear)
                {
                    //checks
                    await daily();
                }

                Globals.isSleep = false;
            }
        }

        //actually starts timer
        public async Task trueStart(System.Timers.Timer timer, bool reply)
        {
            Start();

            _lastRun = int.Parse(FileUtils.Load(@"..\..\Cache\gecko4.gek"));

            if (_lastRun != DateTime.Now.DayOfYear)
            {
                //checks
                await dailydm();
            }

            if (!Timer.CounterStarted)
            {
                Timer.CounterStarted = true;
                if (reply)
                {
                    await ReplyAsync("hourly check started");
                }
            }

            timer.Stop();
        }
        
        //checks
        [Command("check")]
        [Summary("Checks whether the daily dm needs to be sent.")]
        public async Task check()
        {
            _lastRun = int.Parse(FileUtils.Load(@"..\..\Cache\gecko4.gek"));
            
            if (_lastRun != DateTime.Now.DayOfYear)
            {
                //checks
                await dailydm();

                await ReplyAsync("checked and updated");
            }
            else
            {
                await ReplyAsync("checked");
            }
        }
        
        // Force updates
        [RequireGeckobotAdmin]
        [Command("fcheck")]
        [Summary("Force updates the daily dm.")]
        public async Task fcheck()
        {
            //forces update by subtracting one from day of the year
            _lastRun = DateTime.Now.DayOfYear - 1;

            await dailydm();

            await ReplyAsync("force updated");
        }

        //sets up daily dms
        [Command("dm")]
        [Summary("Signs you up for daily dm.")]
        public async Task dmgec([Summary("Bool whether you want to sign up or not.")] bool yes)
        {
            if (yes)
            {
                RefreshDmGroup();

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
                RefreshDmGroup();
                
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

        //actually starts the timer
        public void Start()
        {
            //one hour looping timer for checking
            System.Timers.Timer timer = new(1000*60*60);
            timer.Elapsed += async (sender, e) => await daily();
            timer.Start();

            dmTimer = timer;

            //makes sure there is only one timer
            Timer.Started = true;
        }

        //checks when timer runs out
        async Task daily()
        {
            //now
            DateTime time = DateTime.Now;

            //getting minutes
            int minutes = time.Minute;

            _lastCheck = DateTime.Now;

            //if timer is misaligned with hour, realign it
            if (minutes > 0)
            {
                //stops current timer
                dmTimer.Stop();

                //sets timer to amount of time until next hour plus a little bit
                System.Timers.Timer timer2 = new((60 - minutes) * 60 * 1000 + 1000);
                timer2.Elapsed += async (sender, e) => await trueStart(timer2, false);
                timer2.Start();

                dmTimer2 = timer2;

                //sets some variables so stats show up
                Timer.Started = false;
                Timer.IsCounting = true;
            }

            if (_lastRun != DateTime.Now.DayOfYear)
            {
                //checks
                await dailydm();
            }
        }

        //sends daily dm
        async Task dailydm()
        {
            RefreshDmGroup();

            //generates statement to send
            DateTime date = DateTime.Today;
            string final = (date.DayOfYear - 1).ToString();
            
            //DMs everybody on the list
            await dmGroup(
                DriveUtils.ImagePath(date.DayOfYear - 1, false),
                $"Today is {date.ToString("d")}. Day {date.DayOfYear} of the year {date.Year} (gecko #{final})");

            //changes geckobot's profile to new gecko
            Utils.Utils.changeProfile(
                _client, 
                DriveUtils.ImagePath(date.DayOfYear - 1, false));

            //updates last run counter
            _lastRun = DateTime.Now.DayOfYear;

            FileUtils.Save(_lastRun.ToString(), @"..\..\Cache\gecko4.gek");
        }

        private async Task dmGroup(string path, string content)
        {
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