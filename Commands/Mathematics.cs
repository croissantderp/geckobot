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
        public async Task add([Summary("First number.")] decimal num1, [Summary("Number to add to first number.")] decimal num2)
        {
            await ReplyAsync((num1 + num2).ToString());
        }

        [Command("subtract")]
        [Summary("Subtracts two numbers.")]
        public async Task subtract([Summary("First number.")] decimal num1, [Summary("Number to add to first number.")] decimal num2)
        {
            await ReplyAsync((num1 - num2).ToString());
        }

        [Command("multiply")]
        [Summary("Multiplies two numbers.")]
        public async Task multiply([Summary("First number.")] decimal num1, [Summary("Number to multiply first number by.")] decimal num2)
        {
            await ReplyAsync((num1 * num2).ToString());
        }

        [Command("divide")]
        [Summary("Divides two numbers.")]
        public async Task divide([Summary("First number.")] decimal num1, [Summary("Number to divide first number by.")] decimal num2)
        {
            await ReplyAsync((num1 / num2).ToString());
        }
    }
}