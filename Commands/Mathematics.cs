using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using GeckoBot.Utils;

namespace GeckoBot.Commands
{
    [Summary("Basic math commands.")]
    public class Mathematics : ModuleBase<SocketCommandContext>
    {
        //maf
        [Command("add")]
        [Summary("Adds two numbers.")]
        public async Task add([Remainder][Summary("Numbers to add, seperated by spaces.")] string numstring)
        {
            decimal[] nums = numstring.Split(" ").Where(a => a != "").Select(a => decimal.Parse(a)).ToArray();

            await ReplyAsync(nums.Sum().ToString());
        }

        [Command("subtract")]
        [Summary("Subtracts two numbers.")]
        public async Task subtract([Remainder][Summary("Numbers to subtract, seperated by spaces.")] string numstring)
        {
            decimal[] nums = numstring.Split(" ").Where(a => a != "").Select(a => decimal.Parse(a)).ToArray();

            decimal final = nums[0];

            foreach (decimal num in nums.Skip(1))
            {
                final -= num;
            }

            await ReplyAsync(final.ToString());
        }

        [Command("multiply")]
        [Summary("Multiplies two numbers.")]
        public async Task multiply([Remainder][Summary("Numbers to multiply, seperated by spaces.")] string numstring)
        {
            decimal[] nums = numstring.Split(" ").Where(a => a != "").Select(a => decimal.Parse(a)).ToArray();

            decimal final = nums[0];

            foreach (decimal num in nums.Skip(1))
            {
                final *= num;
            }

            await ReplyAsync(final.ToString());
        }

        [Command("divide")]
        [Summary("Divides two numbers.")]
        public async Task divide([Remainder][Summary("Numbers to divide, seperated by spaces.")] string numstring)
        {
            decimal[] nums = numstring.Split(" ").Where(a => a != "").Select(a => decimal.Parse(a)).ToArray();

            decimal final = nums[0];

            foreach (decimal num in nums.Skip(1))
            {
                final /= num;
            }

            await ReplyAsync(final.ToString());
        }

        [Command("power")]
        [Summary("Powers a number.")]
        public async Task power([Summary("Value to raise second number by..")] double root, [Summary("The number to empower.")] double value)
        {
            double thing = Math.Pow(value, root);

            await ReplyAsync(thing.ToString());
        }

        [Command("scinot")]
        [Summary("puts a number in scientific notation.")]
        public async Task scinot([Summary("Number to convert to/from scientific notation.")] double value)
        {
            await ReplyAsync("Standard: " + decimal.Parse(value.ToString(), System.Globalization.NumberStyles.Float) + "\nScientific: " + string.Format("{0:#.##E+0}", value));
        }

        [Command("round")]
        [Summary("Rounds a number.")]
        public async Task round([Summary("Numbers of digits to round to.")] int digits, [Summary("Number to round.")] decimal value)
        {
            await ReplyAsync(Utils.Utils.RoundNum(value, digits).ToString());
        }

        //maf but time
        [Command("time add")]
        [Summary("Adds two numbers.")]
        public async Task Tadd([Remainder][Summary("Times seperated by spaces")] string num1)
        {
            TimeSpan finalTime = new TimeSpan(num1.Split(" ").Where(a => a != "").Select(a => Timer.parseTime(a).Ticks).Sum());

            await ReplyAsync(finalTime.ToString());
        }

        //maf but time
        [Command("time convert")]
        [Summary("Converts a time between two time zones.")]
        public async Task Tconvert([Summary("Initial time.")] string num1, [Summary("Initial time zone.")] string timeZone, [Summary("Destination time zone.")] string timeZone2)
        {
            try
            {
                TimeZoneInfo tzinfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
                TimeZoneInfo tzinfo2 = TimeZoneInfo.FindSystemTimeZoneById(timeZone2);

                DateTime final = TimeZoneInfo.ConvertTimeFromUtc(TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(num1), tzinfo), tzinfo2);
                await ReplyAsync(final.ToString());
            }
            catch
            {
                await ReplyAsync("invalid time zone");
            }

        }

        [Command("time subtract")]
        [Summary("Subtracts two numbers.")]
        public async Task Tsubtract([Remainder][Summary("Times seperated by spaces")] string num1)
        {
            TimeSpan[] finalTimes = num1.Split(" ").Where(a => a != "").Select(a => Timer.parseTime(a)).ToArray();

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