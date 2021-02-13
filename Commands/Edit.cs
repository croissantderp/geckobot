using System.Threading.Tasks;
using Discord.Commands;
using GeckoBot.Utils;

namespace GeckoBot.Commands
{
    [Group("edit")]
    [Summary("Commands for doing tricks with the discord edited tag.")]
    public class Edit : ModuleBase<SocketCommandContext>
    {
        //flips the edited tag on messages
        [Command("flip")]
        [Summary("Adds an inverted edited tag to a message.")]
        public async Task flip(string text, string text2)
        {
            //joins text with dark magic
            string final = "؜" + EmoteUtils.emoteReplace(text) + "\n" + EmoteUtils.emoteReplace(text2) + "؜؜؜";

            //sends a placeholder message
            var Message1 = await ReplyAsync("ahaaha", allowedMentions: Globals.allowed);

            //edits it with content
            await Message1.ModifyAsync(m => { m.Content = final; m.AllowedMentions = Globals.allowed; });
        }

        [Command("")]
        [Summary("Adds an edited tag between the two string arguments.")]
        public async Task edit(string text, string text2)
        {
            //joins text with dark magic
            string final = "؜" + EmoteUtils.emoteReplace(text2) + "؜؜؜؜؜؜؜؜؜؜؜؜" + EmoteUtils.emoteReplace(text);

            //sends a placeholder message
            var Message1 = await ReplyAsync("ahaaha");

            //edits it with content
            await Message1.ModifyAsync(m => { m.Content = final; m.AllowedMentions = Globals.allowed; });
        }
    }
}