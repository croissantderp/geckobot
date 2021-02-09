using System.Threading.Tasks;
using Discord.Commands;

namespace GeckoBot.Commands
{
    //rock paper scissors
    [Group("rps")]
    [Summary("A fair game of rock paper scissors.")]
    public class RPS : ModuleBase<SocketCommandContext>
    {
        [Command("scissors")]
        [Summary("Deploys a rock to your location.")]
        public async Task scissors()
        {
            await ReplyAsync("a rock crushes your scissors's hopes and dreams");
        }

        [Command("rock")]
        [Summary("Converts trees to pulp and presses it into a thin sheet.")]
        public async Task rock()
        {
            await ReplyAsync("paper envelops your rock, digesting it and consuming it's nutrients");
        }

        [Command("paper")]
        [Summary("Utilizes cutting edge technology.")]
        public async Task paper()
        {
            await ReplyAsync("scissors ruthlessly stab through your paper, leaving deep wounds and killing your paper");
        }

        [Command("gun")]
        [Summary("No such command exists, please never speak of it again or use it.")]
        public async Task gun()
        {
            await ReplyAsync("fine, you win. Now if you would lower the gun. please?");
        }
    }
}