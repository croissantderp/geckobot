using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace GeckoBot.Commands
{
    public class DailyDM : ModuleBase<SocketCommandContext>
    {
        [Command("last checked")]
        [Summary("Gets the time since the daily dm was last checked.")]
        public async Task last()
        {
            await ReplyAsync(Globals.lastCheck.ToString());
        }

        //starts timer for various checks
        [Command("start")]
        [Summary("Starts the timer.")]
        public async Task test()
        {
            await Context.Client.SetGameAsync("`what do you do?");

            //if started or not
            if (Globals.started)
            {
                await ReplyAsync("hourly check already started");
            }
            else
            {
                //now
                DateTime time = DateTime.Now;

                //getting minutes
                int minutes = time.Minute;

                if (Globals.isCounting)
                {
                    await ReplyAsync("hourly check already scheduled, will start in t - " + (61 - minutes) + " minutes");
                }
                else
                {
                    //sets timer to amount of time until next hour plus a little bit
                    System.Timers.Timer timer = new System.Timers.Timer((61 - minutes) * 60 * 1000);
                    timer.Elapsed += async (sender, e) => await trueStart(timer);
                    timer.Start();

                    //checks
                    await check("");

                    Globals.isCounting = true;

                    await ReplyAsync("hourly check will start in t - " + (61 - minutes) + " minutes");
                }
            }
        }

        //actually starts timer
        public async Task trueStart(System.Timers.Timer timer)
        {
            Start();

            if (Globals.lastrun != DateTime.Now.DayOfYear)
            {
                //checks
                await dailydm();
            }

            if (!Globals.counterStarted)
            {
                await ReplyAsync("hourly check started");
                Globals.counterStarted = true;
            }

            timer.Stop();
        }
        
        //checks
        [Command("check")]
        [Summary("Checks whether the daily dm needs to be sent.")]
        public async Task check(string passcode)
        {
            //if password matches secret password
            if (passcode == Top.Secret)
            {
                //forces update by subtracting one from day of the year
                Globals.lastrun = DateTime.Now.DayOfYear - 1;

                await dailydm();

                await ReplyAsync("checked and force updated");
            }
            else if (Globals.lastrun != DateTime.Now.DayOfYear)
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

        //sets up daily dms
        [Command("dm")]
        [Summary("Signs you up for daily dm.")]
        public async Task dmgec(bool yes)
        {
            FileUtils.checkForExistance();

            if (yes)
            {
                //clears
                Globals.dmUsers.Clear();

                //gets info
                string[] temp = FileUtils.Load(@"..\..\Cache\gecko3.gek").Split(",");

                //adds info to list
                foreach (string a in temp)
                {
                    Globals.dmUsers.Add(ulong.Parse(a));
                }

                //gets current user
                IUser user = Context.User;

                //if they are already signed up
                if (Globals.dmUsers.Contains(user.Id))
                {
                    await ReplyAsync("you are already signed up!");
                }
                else
                {
                    //adds id
                    Globals.dmUsers.Add(user.Id);

                    //saves info
                    FileUtils.Save(string.Join(",", Globals.dmUsers.ToArray()), @"..\..\Cache\gecko3.gek");

                    //DMs the user
                    await user.SendMessageAsync("hi, daily gecko updates have been set up, cancel by '\\`dm false'");
                }

            }
            else
            {
                //loads things the same way as above
                Globals.dmUsers.Clear();
                string[] temp = FileUtils.Load(@"..\..\Cache\gecko3.gek").Split(",");
                foreach (string a in temp)
                {
                    Globals.dmUsers.Add(ulong.Parse(a));
                }
                
                //gets current user
                IUser user = Context.User;

                //if the are already not signed up
                if (Globals.dmUsers.Contains(user.Id))
                {
                    //removes user form list
                    Globals.dmUsers.Remove(user.Id);

                    //saves info
                    FileUtils.Save(string.Join(",", Globals.dmUsers.ToArray()), @"..\..\Cache\gecko3.gek");

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
            System.Timers.Timer timer = new System.Timers.Timer(1000*60*60);
            timer.Elapsed += async (sender, e) => await daily(timer);
            timer.Start();

            //makes sure there is only one timer
            Globals.started = true;
        }

        //checks when timer runs out
        async Task daily(System.Timers.Timer timer)
        {
            //now
            DateTime time = DateTime.Now;

            //getting minutes
            int minutes = time.Minute;

            Globals.lastCheck = DateTime.Now;

            //if timer is misaligned with hour, realign it
            if (minutes > 1)
            {
                //stops current timer
                timer.Stop();

                //sets timer to amount of time until next hour plus a little bit
                System.Timers.Timer timer2 = new System.Timers.Timer((61 - minutes) * 60 * 1000);
                timer2.Elapsed += async (sender, e) => await trueStart(timer2);
                timer2.Start();

                //sets some variables so stats show up
                Globals.started = false;
                Globals.isCounting = true;
            }

            if (Globals.lastrun != DateTime.Now.DayOfYear)
            {
                //checks
                await dailydm();
            }
        }

        //sends daily dm
        async Task dailydm()
        {
            //loads file in same way as described above
            if (FileUtils.Load(@"..\..\Cache\gecko3.gek") != null)
            {
                Globals.dmUsers.Clear();
                string[] temp = FileUtils.Load(@"..\..\Cache\gecko3.gek").Split(",");

                foreach (string a in temp)
                {
                    Globals.dmUsers.Add(ulong.Parse(a));
                }
            }

            //generates statement to send
            DateTime date = DateTime.Today;
            string final = (date.DayOfYear - 1).ToString();

            //gets client
            DiscordSocketClient client = Context.Client;

            //DMs everybody on the list
            await dmGroup(
                DriveUtils.ImagePath(date.DayOfYear - 1),
                $"Today is {date.ToString("d")}. Day {date.DayOfYear} of the year {date.Year} (gecko #{final})");

            //changes geckobot's profile to new gecko
            Utils.changeProfile(
                Context.Client, 
                DriveUtils.ImagePath(date.DayOfYear - 1));

            //updates last run counter
            Globals.lastrun = DateTime.Now.DayOfYear;
        }

        public async Task dmGroup(string path, string content)
        {
            DiscordSocketClient client = Context.Client;

            DateTime date = DateTime.Today;

            //if it is geckobot's birthday
            bool isBirthday = date.DayOfYear == 288;

            //DMs everybody on the list
            foreach (ulong a in Globals.dmUsers)
            {
                //gets user from id
                IUser b = client.GetUser(a);

                //sends file with exception for leap years
                await b.SendFileAsync(
                    path,
                    content);

                if (isBirthday)
                {
                    await b.SendMessageAsync("happy birthday geckobot :cake:");
                }
            }
        }
    }
}