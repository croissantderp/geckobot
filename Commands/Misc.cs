using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Discord;
using System;
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

        [Command("open dm")]
        [Summary("Opens a DM with the user")]
        public async Task openDM()
        {
            await Context.User.SendMessageAsync("DM opened");
        }

        //sends a message with a link to the ice cream cards collection
        [Command("ice cream")]
        [Summary("Links the ice cream cards Google Drive folder.")]
        public async Task iceCream()
        {
            //buils an embed
            var embed = new EmbedBuilder
            {
                Title = "ice cream cards",
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
