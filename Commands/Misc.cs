using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Discord;
using System;
using System.IO;
using System.Linq;
using Discord.Commands;
using GeckoBot.Utils;
using GeckoBot.Preconditions;

namespace GeckoBot.Commands
{
    [Summary("Miscellaneous commands.")]
    public class Misc : ModuleBase<SocketCommandContext>
    {
        static int frame = 0;
        static System.Timers.Timer timer;
        static System.Timers.Timer Frametimer;

        //creates new files if there are none
        // Shouldn't this be better as a preliminary check instead of a command?
        [Command("instantiate")]
        [Summary("Ensures the bot's cache exists.")]
        public async Task instantiate()
        {
            //checks if files exist
            FileUtils.checkForExistance();

            //adds reaction
            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

        [Command("pins")]
        [Summary("Gets number of pins in current channel.")]
        public async Task pins()
        {
            await ReplyAsync((await Context.Channel.GetPinnedMessagesAsync()).Count().ToString() + " pins");
        }

        [Command("now")]
        [Summary("NOW. You're looking at now, sir. Everything that happens now is happening now. Go back to then! What? THEN! I can't! Why not? We passed it! When? Just now! When will then be now? soon.")]
        public async Task now(string timeZone = null)
        {
            var n = DateTimeOffset.Now.UtcDateTime;
            var n2 = DateTimeOffset.Now.DateTime;

            TimeZoneInfo tzinfo = TimeZoneInfo.Local;

            if (timeZone != null)
            {
                tzinfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            }
            var ns = TimeZoneInfo.ConvertTimeFromUtc(n2.ToUniversalTime(), tzinfo);

            long secondsSince = DateTimeOffset.Now.ToUnixTimeSeconds();

            await ReplyAsync(
                $"{n.ToLongDateString()} ({n.ToShortDateString()}), {n.Hour}:{n.Minute}:{n.Second}.{n.Millisecond} UTC \n" + //simple now date and time
                $"{secondsSince} ({new TimeSpan(secondsSince * 10000000)}) seconds since midnight 1 / 1 / 1970 \n" + //unix epoch
                $"The {DateTime.Now.DayOfYear} day of the year{(DateTime.IsLeapYear(DateTime.Now.Year) ? " (which is a leap year)" : "")}, {DateTime.Now.DayOfWeek} \n" + //days
                $"Geckobot is currently running in the {TimeZoneInfo.Local.Id} time zone ({TimeZoneInfo.Local.DisplayName}) which currently is {(DateTime.Now.IsDaylightSavingTime() ? "" : "not ")}in daylight savings time \n" + //local time
                $"local time is {n2.ToLongDateString()} ({n2.ToShortDateString()}), {n2.Hour}:{n2.Minute}:{n2.Second}.{n2.Millisecond} \n" + //local time
                $"specified timezone time is {ns.ToLongDateString()} ({ns.ToShortDateString()}), {ns.Hour}:{ns.Minute}:{ns.Second}.{ns.Millisecond} \n"
                );
        }

        [Command("guilds")]
        [Summary("Gets number of guilds the bot is in.")]
        public async Task guilds()
        {
            await ReplyAsync(Context.Client.Guilds.Count.ToString() + " guilds");
        }

        [Command("shard")]
        [Summary("Gets recommended shard count.")]
        public async Task shard()
        {
            await ReplyAsync((await Context.Client.GetRecommendedShardCountAsync()).ToString());
        }

        [Command("open dm")]
        [Summary("Opens a DM with the user.")]
        public async Task openDM()
        {
            await Context.User.SendMessageAsync("DM opened");
        }

        [Command("ping")]
        [Summary("Returns the latency.")]
        public async Task ping()
        {
            await ReplyAsync("latency is " + Context.Client.Latency.ToString() + "ms");
        }

        [Command("test")]
        [Summary("test")]
        public async Task test([Summary("test")] string test = "test")
        {
            await ReplyAsync(test);
        }

        [RequireGeckobotAdmin]
        [Command("apple")]
        [Summary("plays bad apple in text. (taken from the transcript of https://youtu.be/G8DjxY8FNKA )")]
        public async Task badApple([Summary("The frame rate in milliseconds, lower framerates might cause geckobot to skip (range 0 to 3000)")] int frameRate = 100)
        {
            if (frame != 0)
            {
                await ReplyAsync("Ongoing animation elsewhere");
                return;
            }

            if (frameRate > 3000)
            {
                await ReplyAsync("Framerate cannot be greater than the message update speed");
                return;
            }

            if (frameRate <= 0)
            {
                await ReplyAsync("Framerate cannot be less than or equal to 0");
                return;
            }

            string unsplit = File.ReadAllText(@"..\..\..\badApple.txt");

            string[] split = unsplit.Split("\n\n");

            var message = await ReplyAsync(split[0]);

            frame++;

            //time between checks
            System.Timers.Timer t2 = new(frameRate);
            t2.Elapsed += (sender, e) => frameAdd(split);
            t2.Start();
            Frametimer = t2;

            //time between checks
            System.Timers.Timer t = new(3000);
            t.Elapsed += async (sender, e) => await timerUp(message, split);
            t.Start();
            timer = t;
        }

        [RequireGeckobotAdmin]
        [Command("bad apple")]
        [Summary("stops bad apple")]
        public async Task badAppleStop()
        {
            if (frame == 0)
            {
                await ReplyAsync("no ongoing animation");
                return;
            }

            Frametimer.Dispose();
            timer.Dispose();

            frame = 0;

            //adds reaction
            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

        private void frameAdd(string[] split)
        {
            frame++;

            if (frame == split.Length - 1)
            {
                Frametimer.Dispose();
            }
        }

        //the task that is activated when time is up
        private async Task timerUp(IUserMessage toEdit, string[] split)
        { 
            await toEdit.ModifyAsync(a => a.Content = split[frame]);

            if (frame >= split.Length - 1)
            {
                //adds reaction
                await toEdit.AddReactionAsync(new Emoji("✅"));
                timer.Dispose();
                frame = 0;
                return;
            }
        }



        [Command("match")]
        [Summary("String matcher using al go r i th m s.")]
        public async Task match([Summary("the string to search.")] string aha1, [Summary("The pattern to search with.")] string aha2)
        {
            int outValue;
            bool result = Globals.FuzzyMatch(aha1, aha2, out outValue);

            await ReplyAsync($"match: {result}, score {outValue}");
        }

        // This is probably a bad idea
        // Also, there's probably a better way of deleting large bot messages than using a delete command
        [Command("delete")]
        [Summary("Delete a geckobot message.")]
        public async Task delete([Summary("First input, either link or channel id")] string input = null, [Summary("Second input, message id or leave blank for link.")] string input2 = null)
        {
            string[] ids = (await Globals.getIds(input, input2, Context)).ToString().Split("$");
            string channel = ids[0];
            string message = ids[1];
            
            var channel2 = Context.Client.GetChannel(ulong.Parse(channel)) as IMessageChannel;

            //parses message id provided and gets message from channel
            var message2 = await channel2.GetMessageAsync(ulong.Parse(message));

            if (message2.Author.Id == Context.Client.CurrentUser.Id)
            {
                if (!Globals.undeletable.Contains(message2.Id))
                {
                    await message2.DeleteAsync();

                    //adds reaction
                    await Context.Message.AddReactionAsync(new Emoji("✅"));
                }
                else
                {
                    await ReplyAsync("message is undeletable");
                }
            }
            else
            {
                await ReplyAsync("can only delete messages sent by geckobot");
            }
        }
    }
}
