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
        public async Task alarm(string message, string time)
        {
            //parses time in hh:mm:ss format
            string[] times1 = time.Split(":");
            int[] times2 = new int[3];
            for (int i = 0; i < 3; i++)
            {
                times2[i] = int.Parse(times1[i]);
            }

            //gets current time
            int hour = DateTime.Now.Hour;
            int minute = DateTime.Now.Minute;
            int second = DateTime.Now.Second;

            //does maths and subtracts the current time from the alarm time
            int final = (((times2[0] - hour) * 60 * 60) + ((times2[1] - minute) * 60) + (times2[2] - second)) * 1000;

            //gets user
            IUser user = Context.User;

            //sets timer to exact amount of time
            System.Timers.Timer timer = new System.Timers.Timer(final);
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
    }
   
}