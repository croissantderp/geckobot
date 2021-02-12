using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.ServiceProcess;
using System.Management;
using GeckoBot.Preconditions;
using Microsoft.Win32;

namespace GeckoBot.Commands
{
    public class Timer : ModuleBase<SocketCommandContext>
    {
        //timer
        [Command("timer")]
        [Summary("Sets an alarm which will be sent after the specified length of time (in hh:mm:ss format).")]
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
        [Summary("Sets an alarm which will be sent on the specified date and time (in hh:mm:ss format).")]
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
            await user.SendMessageAsync(Utils.emoteReplace(message));

            //stops timer
            timer2.Stop();
        }

        //visible timer command
        [RequireGeckobotAdmin]
        [Command("countdown")]
        [Summary("Sets a countdown which will be updated every 3 seconds.")]
        public async Task vt(string target, bool isTimer, string message, string date, string time)
        {
            //requires passcode and only one timer may exist at a time because of resource problems
            if (!Globals.timerExists)
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

                int hour = times2[0];
                int minute = times2[1];
                int second = times2[2];

                int days = 0;

                //final time when timer runs out
                DateTime finalTime;

                if (!isTimer)
                {
                    string[] date1 = date.Split("/");
                    int[] date2 = new int[3];
                    for (int i = 0; i < 3; i++)
                    {
                        date2[i] = int.Parse(date1[i]);
                    }

                    finalTime = new DateTime(date2[2], date2[0], date2[1], hour, minute, second);
                }
                else
                {
                    days = int.Parse(date);

                    //turns input time into a time span
                    TimeSpan timeLeft = new TimeSpan(days, hour, minute, second);

                    //final time when timer runs out
                    finalTime = DateTime.Now + timeLeft;
                }

                string[] endMessage = finalMessage[1].Split("[end]");

                //gets target channel
                IMessageChannel channel = Context.Client.GetChannel(ulong.Parse(target)) as IMessageChannel;

                //gets duration for first message
                TimeSpan duration = finalTime - DateTime.Now;

                TimeSpan duration2 = TimeSpan.FromSeconds(Math.Round(duration.TotalSeconds));

                var message2 = await channel.SendMessageAsync(Utils.emoteReplace(finalMessage[0]) + duration2 + Utils.emoteReplace(endMessage[0]));

                //initializes things
                await ReplyAsync("countdown initialized");

                //sets timer as exists
                Globals.timerExists = true;

                Globals.undeletable.Add(message2.Id);
                
                Globals.datetime = finalTime;

                Globals.strings = new [] { Utils.emoteReplace(finalMessage[0]), Utils.emoteReplace(endMessage[0]), Utils.emoteReplace(endMessage[1]) };

                //time between checks
                System.Timers.Timer timer = new System.Timers.Timer(3000);
                timer.Elapsed += async (sender, e) => await vtimerUp(message2);
                timer.Start();

                Globals.timer = timer;

            }
        }
        
        [RequireGeckobotAdmin]
        [Command("pause")]
        [Summary("Pauses the countdown.")]
        public async Task pause()
        {
            Globals.timer.Stop();
            await ReplyAsync("paused");
        }
        
        [RequireGeckobotAdmin]
        [Command("unpause")]
        [Summary("Unpauses the countdown.")]
        public async Task unpause()
        {
            Globals.timer.Start();
            await ReplyAsync("unpaused");
        }


        //the task that is activated when time is up
        public async Task vtimerUp(IUserMessage toEdit)
        {
            try
            {
                //gets the duration between final time and now
                TimeSpan duration = Globals.datetime - DateTime.Now;

                //rounds to seconds
                TimeSpan duration2 = TimeSpan.FromSeconds(Math.Round(duration.TotalSeconds));

                //edits the message
                await toEdit.ModifyAsync(a => a.Content = Globals.strings[0] + duration2 + Globals.strings[1]);

                //stops the timer when time runs out
                if (duration.TotalSeconds <= 0)
                {
                    await toEdit.ModifyAsync(a => a.Content = Globals.strings[2]);

                    endCountdown(toEdit.Id);
                }

                //if timer is prematurely terminated
                if (Globals.terminate)
                {
                    await toEdit.ModifyAsync(a => a.Content = "countdown aborted");

                    endCountdown(toEdit.Id);

                    Globals.terminate = false;
                }
            }
            catch(Exception ex)
            {
                Globals.bugs.Add(ex.ToString());

                //saves info
                FileUtils.Save(string.Join(",", Globals.bugs.ToArray()), @"..\..\Cache\gecko1.gek");
            }
        }

        public void endCountdown(ulong id)
        {
            Globals.undeletable.Remove(id);

            //stops timer
            Globals.timer.Stop();

            Globals.timerExists = false;

            Globals.datetime = new DateTime();

            Globals.strings = new string[0];
        }

        //ends timer
        [RequireGeckobotAdmin]
        [Command("end countdown")]
        [Summary("End the countdown.")]
        public async Task endTimer()
        {
            //terminates timer
            Globals.terminate = true;
            await ReplyAsync("countdown terminated");
        }
    }
}