using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace GeckoBot.Commands
{
    [Summary("Basic math commands.")]
    public class Mathematics : ModuleBase<SocketCommandContext>
    {
        //maf
        [Command("add")]
        [Summary("Adds two numbers.")]
        public async Task add(decimal num1, decimal num2)
        {
            await ReplyAsync((num1 + num2).ToString());
        }

        [Command("subtract")]
        [Summary("Subtracts two numbers.")]
        public async Task subtract(decimal num1, decimal num2)
        {
            await ReplyAsync((num1 - num2).ToString());
        }

        [Command("multiply")]
        [Summary("Multiplies two numbers.")]
        public async Task multiply(decimal num1, decimal num2)
        {
            await ReplyAsync((num1 * num2).ToString());
        }

        [Command("divide")]
        [Summary("Divides two numbers.")]
        public async Task divide(decimal num1, decimal num2)
        {
            await ReplyAsync((num1 / num2).ToString());
        }
    }
}