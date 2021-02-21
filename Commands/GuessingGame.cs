using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace GeckoBot.Commands
{
    //number guessing game
    [Group("g")]
    [Summary("A number guessing game.")]
    public class GuessingGame : ModuleBase<SocketCommandContext>
    {
        //guessing game variables
        private static int _gNumber;
        private static int _attempts;
        private static bool _easyMode;
        
        //generates new number
        [Command("new")]
        [Summary("Starts a new game from the min and max provided.")]
        public async Task newNumber(int min, int max)
        {
            //generates number based on min and max value
            Random random = new();
            int number = random.Next(min, max + 1);

            //achievement :D
            await ReplyAsync(min == max 
                ? "achievement get! play on the easiest difficulty!" 
                : $"new number generated between {min} and {max}");
            
            //assigns variables
            _gNumber = number;
            _easyMode = min == max;
            _attempts = 0;
        }

        [Command("")]
        [Summary("Guesses the picked number.")]
        public async Task attempt(int value)
        {
            _attempts += 1;

            //checks values and adds to attempts
            if (value < _gNumber)
            {
                await ReplyAsync("too low");
            }
            else if (value > _gNumber)
            {
                await ReplyAsync("too high");
            }
            else
            {
                //achievement :D
                await ReplyAsync(_easyMode && _attempts > 1 
                    ? "achievement get! lose the game on the easiest difficulty!" 
                    : $"{Context.User} got it! The number was {_gNumber}. It took {_attempts} attempts!");
            }
        }
    }
}