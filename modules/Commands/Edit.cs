using System.Threading.Tasks;
using Discord.Commands;

namespace GeckoBot.Commands
{
    [Group("edit")]
    public class Edit : ModuleBase<SocketCommandContext>
    {
        //flips the edited tag on messages
        [Command("flip")]
        public async Task flip(string text, string text2)
        {
            //joins text with dark magic
            string final = "؜" + Utils.emoteReplace(text) + "\n" + Utils.emoteReplace(text2) + "؜؜؜";

            //sends a placeholder message
            var Message1 = await ReplyAsync("ahaaha");

            //edits it with content
            await Message1.ModifyAsync(m => { m.Content = final; });
        }

        [Command("")]
        public async Task edit(string text, string text2)
        {
            //joins text with dark magic
            string final = "؜" + Utils.emoteReplace(text2) + "؜؜؜؜؜؜؜؜؜؜؜؜" + Utils.emoteReplace(text);

            //sends a placeholder message
            var Message1 = await ReplyAsync("ahaaha");

            //edits it with content
            await Message1.ModifyAsync(m => { m.Content = final; });
        }
    }
}