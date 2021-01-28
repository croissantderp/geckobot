using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace GeckoBot.Commands
{
    public class Timer : ModuleBase<SocketCommandContext>
    {
        //timer
        [Command("timer")]
        public async Task timer(string message, string time)
        {
            //parses time in hh:mm:ss format
            string[] times1 = time.Split(":");
            int[] times2 = new int[3];
            for (int i = 0; i < 3; i++)
            {
                times2[i] = int.Parse(times1[i]);
            }

            //gets user
            IUser user = Context.User;

            //starts a timer with desired amount of time
            System.Timers.Timer timer = new System.Timers.Timer(((times2[0] * 60 * 60) + (times2[1] * 60) + times2[2])* 1000);
            timer.Elapsed += async (sender, e) => await timerUp(user, message, timer);
            timer.Start();

            //adds reaction
            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

        //alarm
        [Command("alarm")]
        public async Task alarm(string message, string date, string time)
        {
            string[] date1 = date.Split("/");
            int[] date2 = new int[3];
            for (int i = 0; i < 3; i++)
            {
                date2[i] = int.Parse(date1[i]);
            }

            //parses time in hh:mm:ss format
            string[] times1 = time.Split(":");
            int[] times2 = new int[3];
            for (int i = 0; i < 3; i++)
            {
                times2[i] = int.Parse(times1[i]);
            }

            DateTime target = new DateTime(date2[2], date2[0], date2[1], times2[0], times2[1], times2[2]);

            TimeSpan final = target - DateTime.Now;

            //gets user
            IUser user = Context.User;

            //sets timer to exact amount of time
            System.Timers.Timer timer = new System.Timers.Timer(final.TotalMilliseconds);
            timer.Elapsed += async (sender, e) => await timerUp(user, message, timer);
            timer.Start();

            //adds reaction
            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

        //the task that is activated when time is up
        public async Task timerUp(IUser user, string message, System.Timers.Timer timer2)
        {
            //dms user
            await Discord.UserExtensions.SendMessageAsync(user, Utils.emoteReplace(message));

            //stops timer
            timer2.Stop();
        }

        //visible timer command
        [Command("countdown")]
        public async Task vt(string passcode, string target, bool isTimer, string message, string date, string time, string endMessage)
        {
            //requires passcode and only one timer may exist at a time because of resource problems
            if (passcode == Top.Secret && !Globals.timerExists)
            {
                //splits the final message
                string[] finalMessage = message.Split("[time]");

                //parses time in hh:mm:ss format
                string[] times1 = time.Split(":");
                int[] times2 = new int[3];
                for (int i = 0; i < 3; i++)
                {
                    times2[i] = int.Parse(times1[i]);
                }

                string[] date1 = date.Split("/");
                int[] date2 = new int[3];
                for (int i = 0; i < 3; i++)
                {
                    date2[i] = int.Parse(date1[i]);
                }

                int hour = times2[0];
                int minute = times2[1];
                int second = times2[2];

                if (!isTimer)
                {
                    //gets current time
                    hour -= DateTime.Now.Hour;
                    minute -= DateTime.Now.Minute;
                    second -= DateTime.Now.Second;

                }

                //gets target channel
                IMessageChannel channel = Context.Client.GetChannel(ulong.Parse(target)) as IMessageChannel;

                DateTime days = new DateTime(date2[2], date2[0], date2[1]);

                int days2 = (int)(days - DateTime.Now.Date).TotalDays;

                //turns inpout time into a time span
                TimeSpan timeLeft = new TimeSpan(days2, hour, minute, second);

                //final time when timer runs out
                DateTime finalTime = DateTime.Now + timeLeft;

                //gets duration for first message
                TimeSpan duration = finalTime - DateTime.Now;

                TimeSpan duration2 = TimeSpan.FromSeconds(Math.Round(duration.TotalSeconds));

                var message2 = await channel.SendMessageAsync(finalMessage[0] + duration2 + finalMessage[1]);

                //initializes things
                await ReplyAsync("countdown initialized");

                //sets timer as exists
                Globals.timerExists = true;

                //time between checks/updates
                System.Timers.Timer timer = new System.Timers.Timer(4000);
                timer.Elapsed += async (sender, e) => await vtimerUp(message2, finalMessage, endMessage, finalTime, timer);
                timer.Start();
            }
        }

        //the task that is activated when time is up
        public async Task vtimerUp(IUserMessage toEdit, string[] message, string endMessage, DateTime finalTime, System.Timers.Timer timer2)
        {
            //gets the duration between final time and now
            TimeSpan duration = finalTime - DateTime.Now;
            
            //rounds to seconds
            TimeSpan duration2 = TimeSpan.FromSeconds(Math.Round(duration.TotalSeconds));

            //edits the message
            await toEdit.ModifyAsync(a => a.Content = message[0] + duration2 + message[1]);

            //stops the timer when time runs out
            if (duration.TotalSeconds <= 0)
            {
                await toEdit.ModifyAsync(a => a.Content = endMessage);

                //stops timer
                timer2.Stop();
            }

            //if timer is prematurely terminated
            if (Globals.terminate)
            {
                await toEdit.ModifyAsync(a => a.Content = "countdown aborted");

                //stops timer
                timer2.Stop();

                Globals.terminate = false;
                Globals.timerExists = false;
            }
        }

        //ends timer
        [Command("end countdown")]
        public async Task endTimer(string passcode)
        {
            //terminates timer
            if (passcode == Top.Secret)
            {
                Globals.terminate = true;
                await ReplyAsync("countdown terminated");
            }
        }
    }
   
}