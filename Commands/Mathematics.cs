using System;
using System.Linq;
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

        //maf but time
        [Command("time add")]
        [Summary("Adds two numbers.")]
        public async Task Tadd([Remainder][Summary("Times seperated by spaces")] string num1)
        {
            TimeSpan finalTime = new TimeSpan(num1.Split(" ").Select(a => Timer.parseTime(a).Ticks).Sum());

            await ReplyAsync(finalTime.ToString());
        }

        [Command("time subtract")]
        [Summary("Subtracts two numbers.")]
        public async Task Tsubtract([Remainder][Summary("Times seperated by spaces")] string num1)
        {
            TimeSpan[] finalTimes = num1.Split(" ").Select(a => Timer.parseTime(a)).ToArray();

            TimeSpan finalTime = finalTimes[0];

            bool first = false;
            foreach(TimeSpan a in finalTimes)
            {
                if (!first)
                {
                    first = true;
                    continue;
                }
                finalTime -= a;
            }

            await ReplyAsync(finalTime.ToString());
        }

        [Command("day of the year")]
        [Summary("Gets the day of the year for a specified date.")]
        public async Task dayofYear([Remainder][Summary("The specified date.")] string num1)
        {
            DateTime finalTime = Timer.parseDate(num1);

            await ReplyAsync(finalTime.DayOfYear.ToString());
        }

        [Command("count")]
        [Summary("Counts characters.")]
        public async Task count([Remainder][Summary("The string to count.")] string text)
        {
            await ReplyAsync(text.Length.ToString());
        }
    }
}