using System.Threading.Tasks;
using Discord.Commands;

namespace GeckoBot.Commands
{
    //rock paper scissors
    [Group("rps")]
    public class RPS : ModuleBase<SocketCommandContext>
    {
        [Command("scissors")]
        public async Task scissors()
        {
            await ReplyAsync("a rock crushes your scissors's hopes and dreams");
        }

        [Command("rock")]
        public async Task rock()
        {
            await ReplyAsync("paper envelops your rock, digesting it and consuming it's nutrients");
        }

        [Command("paper")]
        public async Task paper()
        {
            await ReplyAsync("scissors ruthlessly stab through your paper, leaving deep wounds and killing your paper");
        }

        [Command("gun")]
        public async Task gun()
        {
            await ReplyAsync("fine, you win. Now if you would lower the gun. please?");
        }
    }
}