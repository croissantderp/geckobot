using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace GeckoBot.Commands
{
    [Summary("A bunch of commands that utilize the random feature.")]
    public class RNG : ModuleBase<SocketCommandContext>
    {
        //random number generator
        [Command("rng")]
        [Summary("Generates a number between min and max.")]
        public async Task rng(int min, int max)
        {
            Random random = new Random();
            int number = random.Next(min, max + 1);
            await ReplyAsync(number.ToString());
        }

        //generates a random order of characters
        [Command("fek")]
        [Summary("Generates a random string.")]
        public async Task fek(string length = null)
        {
            //generates random
            Random random = new Random();

            //generate random length of the word
            int charNum = length != null ? int.Parse(length) : random.Next(2, 10);

            string[] charFinal = new string[charNum];

            //generates random characters
            for (int i = 0; i < charNum; i++)
            {
                int num = random.Next(0, 26);
                char let = (char)('a' + num);
                charFinal[i] = let.ToString();
            }

            //joins characters and sends
            await ReplyAsync(string.Join("", charFinal));
        }

        //how to stonks
        [Command("lottery")]
        [Summary("Gambles your life savings away.")]
        public async Task lottery()
        {
            //variables
            string results;
            string[] number2 = new string[6];
            string[] key2 = new string[6];
            int matches = 0;
            Random random = new Random();

            //generates 6 values and matches them
            for (int i = 0; i < 6; i++)
            {
                int number = random.Next(1, 100);
                int key = random.Next(1, 100);
                if (number == key)
                {
                    number2[i] = $"**{number}**";
                    key2[i] = $"**{key}**";
                    matches += 1;
                }
                else
                {
                    number2[i] = number.ToString();
                    key2[i] = key.ToString();
                }
            }

            //generates results and replies
            results = "numbs: " + string.Join(" ", 
                number2.Select(p => p.ToString())) + "\nresults: " + 
                      string.Join(" ", 
                          key2.Select(p => p.ToString())) + "\nmatches: " + matches;
            await ReplyAsync(results);
        }

        //tem
        [Command("tem")]
        [Summary("???")]
        public async Task tem()
        {
            Random random = new Random();

            int number = random.Next(0, 1000);

            if (number == 0)
            {
                await ReplyAsync("░░░░░░░░░░▄▄░░░░░░░░░░░░░░░░░░░░░░░░░░░" + Environment.NewLine +
                "░░░░░░░░░██▀█▄░░▄██▀░░░░▄██▄░░░░░░░░░░░" + Environment.NewLine +
                "░░░░░░░░██▄▄▄████████▄▄█▀░▀█░░░░░░░░░░░" + Environment.NewLine +
                "░░░░░░░▄██████████████████▄█░░░░░░░░░░░" + Environment.NewLine +
                "░░░░░░▄██████████████████████░░░░░░░░░░" + Environment.NewLine +
                "░░░░░▄████████▀░▀█████████████░░░░░░░░░" + Environment.NewLine +
                "░░░░░███████▀░░░░▀████████████▄░░░░░░░░" + Environment.NewLine +
                "░░░░██████▀░░░░░░░░▀███████████▄▄▄▀▀▄░░" + Environment.NewLine +
                "░▄▀▀████▀░░░▄▄░░░░░░░░░████████░░░░░█░░" + Environment.NewLine +
                "█░░░▀█▀█░░░░▀▀░░░░░░██░░███░██▀░░░░▄▀░░" + Environment.NewLine +
                "█░░░░▀██▄░░░▄░░░▀░░░▄░░░███░██▄▄▄▀▀░░░░" + Environment.NewLine +
                "░▀▄▄▄▄███▄░░▀▄▄▄▀▄▄▄▀░░░███░█░░░░░░░░░░" + Environment.NewLine +
                "░░░░░░████▄░░░░░░░░░░░░▄██░█░░░░░░░░▄▀▀" + Environment.NewLine +
                "░░░░░░███▀▀█▀▄▄▄▄▄▄▄▄▄▀█████░░░░░░▄▀░░▄" + Environment.NewLine +
                "░░░░░░█▀░░█░▀▄▄▄▄▄▄▄▄▄▀░███░█▀▀▀▄▀░░▄▀░" + Environment.NewLine +
                "░░░░░░░░░▄▀░░░░░░░░░░░░░▀█▀░░█░░░░█▀░░░" + Environment.NewLine +
                "░░░░░░░░░█░░░█░░░░░░░░░░░░░░░█░░░░░█░░░" + Environment.NewLine +
                "░░░░░░░░░█▄▄█░▀▄░░█▄░░░░░░░░█░░▄█░░█░░░" + Environment.NewLine +
                "░░░░░░░░░▀▄▄▀░░█▀▀█░▀▀▀▀█▀▀█▀▀▀▀█░█░░░░" + Environment.NewLine);
            }
        }
        
        //contest function
        [Command("contest")]
        [Summary("Launches a contest between you and someone else to see who wins!")]
        public async Task contest(IUser user)
        {
            //generates a random number
            Random random = new Random();
            int number = random.Next(1, 3);

            //decides who wins
            if (number == 1)
            {
                await ReplyAsync($"<@!{Context.User.Id}> wins!");
            }
            else
            {
                await ReplyAsync($"<@!{user.Id}> wins!");
            }
        }
    }
}