using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Discord;
using System;
using System.Linq;
using Discord.Commands;
using GeckoBot.Utils;

namespace GeckoBot.Commands
{
    [Summary("Miscellaneous commands.")]
    public class Misc : ModuleBase<SocketCommandContext>
    {
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
        public async Task now()
        {
            var n = DateTimeOffset.Now.UtcDateTime;
            var n2 = DateTimeOffset.Now.DateTime;

            long secondsSince = DateTimeOffset.Now.ToUnixTimeSeconds();

            await ReplyAsync(
                $"{n.ToLongDateString()} ({n.ToShortDateString()}), {n.Hour}:{n.Minute}:{n.Second}.{n.Millisecond} UTC \n" + //simple now date and time
                $"{secondsSince} ({new TimeSpan(secondsSince * 10000000)}) seconds since midnight 1 / 1 / 1970 \n" + //unix epoch
                $"The {DateTime.Now.DayOfYear} day of the year{(DateTime.IsLeapYear(DateTime.Now.Year) ? " (which is a leap year)" : "")}, {DateTime.Now.DayOfWeek} \n" + //days
                $"Geckobot is currently running in the {TimeZoneInfo.Local.Id} time zone ({TimeZoneInfo.Local.DisplayName}) which currently is {(DateTime.Now.IsDaylightSavingTime() ? "" : "not ")}in daylight savings time \n" + //local time
                $"local time is {n2.ToLongDateString()} ({n2.ToShortDateString()}), {n2.Hour}:{n2.Minute}:{n2.Second}.{n2.Millisecond} \n" + //local time
                $""
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
        public async Task test()
        {
            await ReplyAsync("test");
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
