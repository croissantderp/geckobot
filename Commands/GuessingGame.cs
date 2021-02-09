using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace GeckoBot.Commands
{
    //number guessing game
    [Group("g")]
    public class GuessingGame : ModuleBase<SocketCommandContext>
    {
        //generates new number
        [Command("new")]
        [Summary("Starts a new game from the min and max provided.")]
        public async Task newNumber(int min, int max)
        {
            //generates number based on min and max value
            Random random = new Random();
            int number = random.Next(min, max + 1);

            //achievement :D
            await ReplyAsync(min == max 
                ? "achievement get! play on the easiest difficulty!" 
                : $"new number generated between {min} and {max}");
            
            //assigns variables
            Globals.gNumber = number;
            Globals.easyMode = min == max;
            Globals.attempts = 0;
        }

        [Command("")]
        [Summary("Guesses the picked number.")]
        public async Task attempt(int value)
        {
            //gets value from global variables
            int gNumber = Globals.gNumber;
            
            Globals.attempts += 1;

            //checks values and adds to attempts
            if (value < gNumber)
            {
                await ReplyAsync("too low");
            }
            else if (value > gNumber)
            {
                await ReplyAsync("too high");
            }
            else
            {
                //achievement :D
                await ReplyAsync(Globals.easyMode && Globals.attempts > 1 
                    ? "achievement get! lose the game on the easiest difficulty!" 
                    : $"{Context.User} got it! The number was {gNumber}. It took {Globals.attempts} attempts!");
            }
        }
    }
}