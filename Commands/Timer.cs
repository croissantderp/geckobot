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
    [Summary("Commands having to do with time.")]
    public class Timer : ModuleBase<SocketCommandContext>
    {
        public static System.Timers.Timer timer = new();
        
        // Seemingly unused
        //public static ulong TimerChannel = new();
        //public static ulong TimerMessage = new();
        
        private static bool _timerExists; //if a timer exists
        private static bool _terminate; //if timer should be terminated
        
        //basic timer that dms you when the time runs out
        [Command("timer")]
        [Summary("Sets an alarm which will be sent after the specified length of time.")]
        public async Task startTimer([Summary("The message to send when time is up.")] string message, [Summary("The time in hh:mm:ss.")] string time)
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

        //an alarm that counts down towards a specified time then dms you
        [Command("alarm")]
        [Summary("Sets an alarm which will be sent on the specified date and time (in hh:mm:ss format).")]
        public async Task alarm([Summary("The message to send when time is up.")] string message, [Summary("The time in hh:mm:ss.")] string time, [Summary("Optional date in mm/dd/yyyy.")] string date = null)
        {
            //parses the input time and calculates the amount of time until the input time
            TimeSpan final = (date != null ? parseDate(date) : DateTime.Today) + parseTime(time) - DateTime.Now;

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
            //dms user prespecified message
            await user.SendMessageAsync(EmoteUtils.emoteReplace(message));

            //stops timer
            timer2.Close();
        }

        //visible timer command
        [RequireGeckobotAdmin]
        [Command("countdown")]
        [Summary("Sets a countdown which will be updated every 3 seconds.")]
        public async Task vt([Summary("The target channel id.")] string target, [Summary("If the countdown will be a timer.")] bool isTimer, [Summary("The message, put '[time]' to insert time, and put '[end]' to show what to show when the countdown ends.")] string message, [Summary("Date for alarms or days for timer.")] string date, [Summary("The time in hh:mm:ss.")] string time)
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

                //if it is alarm or timer styled
                if (!isTimer)
                {
                    //adds specified date with the parsed time
                    finalTime = parseDate(date) + parsedTime;
                }
                else
                {
                    //gets number of days
                    int days = int.Parse(date);

                    //turns input time into a time span
                    TimeSpan timeLeft = parsedTime.Add(new TimeSpan(days, 0, 0, 0));

                    //final time when timer runs out
                    finalTime = DateTime.Now + timeLeft;
                }

                //splits the input message with ending message
                string[] endMessage = finalMessage[1].Split("[end]");
                
                //gets duration for first message
                TimeSpan duration = finalTime - DateTime.Now;

                //rounds the first duration for initial message
                TimeSpan duration2 = TimeSpan.FromSeconds(Math.Round(duration.TotalSeconds));

                //sends the initial message
                var message2 = await (Context.Client.GetChannel(ulong.Parse(target)) as IMessageChannel).SendMessageAsync(
                    EmoteUtils.emoteReplace(finalMessage[0]) + duration2 + EmoteUtils.emoteReplace(endMessage[0]));

                //initializes things
                await ReplyAsync("countdown initialized");

                //sets timer as exists
                _timerExists = true;

                //makes countdown message undeletable
                Globals.undeletable.Add(message2.Id);
                
                //sets global datetime to specified time
                Globals.datetime = finalTime;

                //stores strings in global
                Globals.strings = new [] { EmoteUtils.emoteReplace(finalMessage[0]), EmoteUtils.emoteReplace(endMessage[0]), EmoteUtils.emoteReplace(endMessage[1]) };

                //time between checks
                System.Timers.Timer t = new(3000);
                t.Elapsed += async (sender, e) => await vtimerUp(message2);
                t.Start();

                timer = t;

            }
        }
        
        //pauses the timer and countdown
        [RequireGeckobotAdmin]
        [Command("pause countdown")]
        [Summary("Pauses the countdown.")]
        public async Task pause()
        {
            timer.Stop();
            await ReplyAsync("paused");
        }
        
        //unpauses the timer and countdown
        [RequireGeckobotAdmin]
        [Command("unpause countdown")]
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
            catch(Exception ex)
            {
                FileUtils.checkForExistance();

                //clears
                Bugs.BugList.Clear();

                //adds info to list
                Bugs.BugList.AddRange(FileUtils.Load(@"..\..\Cache\gecko1.gek").Split(","));

                Bugs.BugList.Add(ex.ToString());

                //saves info
                FileUtils.Save(string.Join(",", Bugs.BugList.ToArray()), @"..\..\Cache\gecko1.gek");
            }
        }

        //ends and disposes of timer.
        private void endCountdown(ulong id)
        {
            Globals.undeletable.Remove(id);

            //stops timer
            timer.Close();

            _timerExists = false;

            Globals.datetime = new DateTime();

            Globals.strings = Array.Empty<string>();
        }

        //ends timer by force
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
        public static TimeSpan parseTime(string unparsed)
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
        public static DateTime parseDate(string unparsed)
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