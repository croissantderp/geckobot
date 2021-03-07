using System.Threading.Tasks;
using Discord;
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

        //sends a message with a link to the gecko collection
        [Command("ice cream")]
        [Summary("Links the ice cream cards Google Drive folder.")]
        public async Task iceCream()
        {
            //buils an embed
            var embed = new EmbedBuilder
            {
                Title = "gecko collection",
                Description = (
                "[see the ice cream cards here](https://drive.google.com/drive/folders/1O7SVg5D0n8t3gOjkArjhhH7_YllcwAXA?usp=sharing)"
                )
            };

            embed.WithColor(180, 212, 85);

            var embed2 = embed.Build();

            await ReplyAsync(embed: embed2);
        }

        // This is probably a bad idea
        // Also, there's probably a better way of deleting large bot messages than using a delete command
        [Command("delete")]
        [Summary("Delete a geckobot message.")]
        public async Task delete([Summary("First input, either link or channel id")] string input, [Summary("Second input, message id or leave blank for link.")] string input2 = null)
        {
            string channel = "";
            string message = "";

            if (input2 == null)
            {
                input = input.Remove(0, 8);
                string[] final = input.Split("/");

                channel = final[3];
                message = final[4];
            }
            else
            {
                channel = input;
                message = input2;
            }

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
