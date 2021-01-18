using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace GeckoBot.Commands
{
    [Group("m")]
    public class Mathematics : ModuleBase<SocketCommandContext>
    {
        //maf
        [Command("add")]
        public async Task add(decimal num1, decimal num2)
        {
            await ReplyAsync((num1 + num2).ToString());
        }

        [Command("subtract")]
        public async Task subtract(decimal num1, decimal num2)
        {
            await ReplyAsync((num1 - num2).ToString());
        }

        [Command("multiply")]
        public async Task multiply(decimal num1, decimal num2)
        {
            await ReplyAsync((num1 * num2).ToString());
        }

        [Command("divide")]
        public async Task divide(decimal num1, decimal num2)
        {
            await ReplyAsync((num1 / num2).ToString());
        }

        //random number generator
        [Command("rng")]
        public async Task rng(int min, int max)
        {
            Random random = new Random();
            int number = random.Next(min, max + 1);
            await ReplyAsync(number.ToString());
        }
    }
}