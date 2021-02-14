using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.ServiceProcess;
using System.Management;
using GeckoBot.Preconditions;
using GeckoBot.Utils;
using Microsoft.Win32;

namespace GeckoBot.Commands
{
    public class Timer : ModuleBase<SocketCommandContext>
    {
        public static System.Timers.Timer timer = new();
        
        // Seemingly unused
        //public static ulong TimerChannel = new();
        //public static ulong TimerMessage = new();
        
        private static bool _timerExists; //if a timer exists
        private static bool _terminate; //if timer should be terminated
        
        public static bool Started = false; //is timer is running
        public static bool CounterStarted = false; //if the counter has started at least once
        public static bool IsCounting = false; //if the counter is counting
        public static bool EverStarted = false; //if timer has ever started


        //timer
        [Command("timer")]
        [Summary("Sets an alarm which will be sent after the specified length of time (in hh:mm:ss format).")]
        public async Task startTimer(string message, string time)
        {
            //gets user
            IUser user = Context.User;

            //starts a timer with desired amount of time
            System.Timers.Timer t = new(parseTime(time).TotalMilliseconds);
            t.Elapsed += async (sender, e) => await timerUp(user, message, t);
            t.Start();

            //adds reaction
            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

        //alarm
        [Command("alarm")]
        [Summary("Sets an alarm which will be sent on the specified date and time (in hh:mm:ss format).")]
        public async Task alarm(string message, string date, string time)
        {
            TimeSpan final = parseDate(date) + parseTime(time) - DateTime.Now;

            //gets user
            IUser user = Context.User;

            //sets timer to exact amount of time
            System.Timers.Timer t = new(final.TotalMilliseconds);
            t.Elapsed += async (sender, e) => await timerUp(user, message, t);
            t.Start();

            //adds reaction
            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

        //the task that is activated when time is up
        private async Task timerUp(IUser user, string message, System.Timers.Timer timer2)
        {
            //dms user
            await user.SendMessageAsync(EmoteUtils.emoteReplace(message));

            //stops timer
            timer2.Stop();
        }

        //visible timer command
        [RequireGeckobotAdmin]
        [Command("countdown")]
        [Summary("Sets a countdown which will be updated every 3 seconds.")]
        public async Task vt(string target, bool isTimer, string message, string date, string time)
        {
            //only one timer may exist at a time because of resource problems
            if (!_timerExists)
            {
                //splits the final message
                string[] finalMessage = message.Split("[time]");

                //parses time in hh:mm:ss format
                TimeSpan parsedTime = parseTime(time);

                //final time when timer runs out
                DateTime finalTime;

                if (!isTimer)
                {
                    finalTime = parseDate(date) + parsedTime;
                }
                else
                {
                    int days = int.Parse(date);

                    //turns input time into a time span
                    TimeSpan timeLeft = parsedTime.Add(new TimeSpan(days, 0, 0, 0));

                    //final time when timer runs out
                    finalTime = DateTime.Now + timeLeft;
                }

                string[] endMessage = finalMessage[1].Split("[end]");
                
                //gets duration for first message
                TimeSpan duration = finalTime - DateTime.Now;

                TimeSpan duration2 = TimeSpan.FromSeconds(Math.Round(duration.TotalSeconds));

                var message2 = await (Context.Client.GetChannel(ulong.Parse(target)) as IMessageChannel).SendMessageAsync(
                    EmoteUtils.emoteReplace(finalMessage[0]) + duration2 + EmoteUtils.emoteReplace(endMessage[0]));

                //initializes things
                await ReplyAsync("countdown initialized");

                //sets timer as exists
                _timerExists = true;

                Globals.undeletable.Add(message2.Id);
                
                Globals.datetime = finalTime;

                Globals.strings = new [] { EmoteUtils.emoteReplace(finalMessage[0]), EmoteUtils.emoteReplace(endMessage[0]), EmoteUtils.emoteReplace(endMessage[1]) };

                //time between checks
                System.Timers.Timer t = new(3000);
                t.Elapsed += async (sender, e) => await vtimerUp(message2);
                t.Start();

                timer = t;

            }
        }
        
        [RequireGeckobotAdmin]
        [Command("pause")]
        [Summary("Pauses the countdown.")]
        public async Task pause()
        {
            timer.Stop();
            await ReplyAsync("paused");
        }
        
        [RequireGeckobotAdmin]
        [Command("unpause")]
        [Summary("Unpauses the countdown.")]
        public async Task unpause()
        {
            timer.Start();
            await ReplyAsync("unpaused");
        }


        //the task that is activated when time is up
        private async Task vtimerUp(IUserMessage toEdit)
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
                if (_terminate)
                {
                    await toEdit.ModifyAsync(a => a.Content = "countdown aborted");

                    endCountdown(toEdit.Id);

                    _terminate = false;
                }
            }
            // Shouldn't other catch statements also log errors in Bugs or is Bugs specifically targeted towards timer errors?
            catch(Exception ex)
            {
                Bugs.BugList.Add(ex.ToString());

                //saves info
                FileUtils.Save(string.Join(",", Bugs.BugList.ToArray()), @"..\..\Cache\gecko1.gek");
            }
        }

        private void endCountdown(ulong id)
        {
            Globals.undeletable.Remove(id);

            //stops timer
            timer.Stop();

            _timerExists = false;

            Globals.datetime = new DateTime();

            Globals.strings = Array.Empty<string>();
        }

        //ends timer
        [RequireGeckobotAdmin]
        [Command("end countdown")]
        [Summary("End the countdown.")]
        public async Task endTimer()
        {
            //terminates timer
            _terminate = true;
            await ReplyAsync("countdown terminated");
        }
        
        //parses time in hh:mm:ss format
        private TimeSpan parseTime(string unparsed)
        {
            var parsed = unparsed
                .Split(":")
                .Select(int.Parse)
                .ToArray();

            return new TimeSpan(
                parsed[0], 
                parsed[1], 
                parsed[2]);
        }
        
        //parses date in mm/dd/yyyy format
        private DateTime parseDate(string unparsed)
        {
            var parsed = unparsed
                .Split("/")
                .Select(int.Parse)
                .ToArray();

            return new DateTime(
                parsed[2], 
                parsed[0], 
                parsed[1]);
        }
    }
}