using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace GeckoBot.Commands
{
    public class DailyDM : ModuleBase<SocketCommandContext>
    {
        //starts timer for various checks
        [Command("start")]
        public async Task test()
        {
            //if started or not
            if (Globals.started)
            {
                await ReplyAsync("already started");
            }
            else
            {
                Start();
                await ReplyAsync("started");
            }
        }
        
        //checks
        [Command("check")]
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
        public async Task dmgec(bool yes)
        {
            if (yes)
            {
                //if file exists, load it
                if (FileUtils.Load("C:\\gecko3.gek") != null)
                {
                    //clears
                    Globals.dmUsers.Clear();

                    //gets info
                    string[] temp = FileUtils.Load("C:\\gecko3.gek").Split(",");

                    //adds info to list
                    foreach (string a in temp)
                    {
                        Globals.dmUsers.Add(ulong.Parse(a));
                    }
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
                    FileUtils.Save(string.Join(",", Globals.dmUsers.ToArray()), "C:\\gecko3.gek");

                    //DMs the user
                    await user.SendMessageAsync("hi, daily gecko updates have been set up, cancel by '\\`dm false'");
                }

            }
            else
            {
                //loads things the same way as above
                if (FileUtils.Load("C:\\gecko3.gek") != null)
                {
                    Globals.dmUsers.Clear();
                    string[] temp = FileUtils.Load("C:\\gecko3.gek").Split(",");
                    foreach (string a in temp)
                    {
                        Globals.dmUsers.Add(ulong.Parse(a));
                    }
                }
                
                //gets current user
                IUser user = Context.User;

                //if the are already not signed up
                if (Globals.dmUsers.Contains(user.Id))
                {
                    //removes user form list
                    Globals.dmUsers.Remove(user.Id);

                    //saves info
                    FileUtils.Save(string.Join(",", Globals.dmUsers.ToArray()), "C:\\gecko3.gek");

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
            timer.Elapsed += new System.Timers.ElapsedEventHandler(daily);
            timer.Start();

            //makes sure there is only one timer
            Globals.started = true;
        }

        //checks when timer runs out
        async void daily(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Globals.lastrun != DateTime.Now.DayOfYear)
            {
                Globals.daysSinceReset += 1;
                await dailydm();
            }
        }

        //sends daily dm
        async Task dailydm()
        {
            //loads file in same way as described above
            if (FileUtils.Load("C:\\gecko3.gek") != null)
            {
                Globals.dmUsers.Clear();
                string[] temp = FileUtils.Load("C:\\gecko3.gek").Split(",");

                foreach (string a in temp)
                {
                    Globals.dmUsers.Add(ulong.Parse(a));
                }
            }

            //generates statement to send
            DateTime date = DateTime.Today;
            string final = (date.DayOfYear - 1).ToString();
            while (final.Length < 3)
            {
                final = "0" + final;
            }

            //gets client
            DiscordSocketClient client = Context.Client;

            //DMs everybody on the list
            foreach (ulong a in Globals.dmUsers)
            {
                //gets user from id
                IUser b = client.GetUser(a);

                //sends file with exception for leap years
                await b.SendFileAsync(
                    DriveUtils.ImagePath(date.DayOfYear - 1), 
                    $"Today is {date.ToString("d")}. Day {date.DayOfYear} of the year {date.Year} (gecko #{final})");
            }

            //DriveUtils.pathfinder(date.DayOfYear - 1, true)

            Utils.changeProfile(
                Context.Client, 
                DriveUtils.ImagePath(date.DayOfYear - 1));

            //updates last run counter
            Globals.lastrun = DateTime.Now.DayOfYear;
        }

    }
}