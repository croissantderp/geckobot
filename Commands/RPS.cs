using System;
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

        [Command("actual rps")]
        [Summary("Utilizes cutting edge technology.")]
        public async Task rps(string input)
        {
            int inputNum = 0;
            switch (input)
            {
                case "scissors":
                    inputNum = 1;
                    break;
                case "rock":
                    inputNum = 2;
                    break;
                case "paper":
                    inputNum = 3;
                    break;
                default:
                    await ReplyAsync("not a valid entry");
                    return;
            }
            Random random = new Random();
            int geckoInput = random.Next(1,4);
            if (geckoInput == inputNum)
            {
                await ReplyAsync("draw.");
            }
            else
            {
                if (inputNum == 1 && geckoInput == 2)
                {
                    await ReplyAsync("a rock crushes your scissors's hopes and dreams");
                }
                else if (inputNum == 2 && geckoInput == 3)
                {
                    await ReplyAsync("paper envelops your rock, digesting it and consuming it's nutrients");
                }
                else if (inputNum == 3 && geckoInput == 1)
                {
                    await ReplyAsync("scissors ruthlessly stab through your paper, leaving deep wounds and killing your paper");
                }

                else if (inputNum == 2 && geckoInput == 1)
                {
                    await ReplyAsync("your rock crushes the hopes of dreams of geckobot's scissors");
                }
                else if (inputNum == 3 && geckoInput == 2)
                {
                    await ReplyAsync("paper envelops your rock, digesting it and consuming it's nutrients");
                }
                else if (inputNum == 1 && geckoInput == 3)
                {
                    await ReplyAsync("scissors ruthlessly stab through your paper, leaving deep wounds and killing your paper");
                }
            }
        }

        [Command("gun")]
        [Summary("No such command exists, please never speak of it again or use it.")]
        public async Task gun()
        {
            await ReplyAsync("fine, you win. Now if you would lower the gun. please?");
        }
    }
}