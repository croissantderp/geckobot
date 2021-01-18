using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;

namespace GeckoBot.Commands
{
    public class RNG : ModuleBase<SocketCommandContext>
    {
        //how to stonks
        [Command("lottery")]
        public async Task lottery()
        {
            //variables
            string results = " ";
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
                    number2[i] = "**" + number.ToString() + "**";
                    key2[i] = "**" + key.ToString() + "**";
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
                          key2.Select(p => p.ToString())) + "\nmatches: " + matches.ToString();
            await ReplyAsync(results);
        }

        //tem
        [Command("tem")]
        public async Task tem()
        {
            Random random = new Random();

            int number = random.Next(0, 1000);

            if (number == 0)
            {
                await ReplyAsync("░░░░░░░░░░▄▄░░░░░░░░░░░░░░░░░░░░░░░░░░░" + System.Environment.NewLine +
                "░░░░░░░░░██▀█▄░░▄██▀░░░░▄██▄░░░░░░░░░░░" + System.Environment.NewLine +
                "░░░░░░░░██▄▄▄████████▄▄█▀░▀█░░░░░░░░░░░" + System.Environment.NewLine +
                "░░░░░░░▄██████████████████▄█░░░░░░░░░░░" + System.Environment.NewLine +
                "░░░░░░▄██████████████████████░░░░░░░░░░" + System.Environment.NewLine +
                "░░░░░▄████████▀░▀█████████████░░░░░░░░░" + System.Environment.NewLine +
                "░░░░░███████▀░░░░▀████████████▄░░░░░░░░" + System.Environment.NewLine +
                "░░░░██████▀░░░░░░░░▀███████████▄▄▄▀▀▄░░" + System.Environment.NewLine +
                "░▄▀▀████▀░░░▄▄░░░░░░░░░████████░░░░░█░░" + System.Environment.NewLine +
                "█░░░▀█▀█░░░░▀▀░░░░░░██░░███░██▀░░░░▄▀░░" + System.Environment.NewLine +
                "█░░░░▀██▄░░░▄░░░▀░░░▄░░░███░██▄▄▄▀▀░░░░" + System.Environment.NewLine +
                "░▀▄▄▄▄███▄░░▀▄▄▄▀▄▄▄▀░░░███░█░░░░░░░░░░" + System.Environment.NewLine +
                "░░░░░░████▄░░░░░░░░░░░░▄██░█░░░░░░░░▄▀▀" + System.Environment.NewLine +
                "░░░░░░███▀▀█▀▄▄▄▄▄▄▄▄▄▀█████░░░░░░▄▀░░▄" + System.Environment.NewLine +
                "░░░░░░█▀░░█░▀▄▄▄▄▄▄▄▄▄▀░███░█▀▀▀▄▀░░▄▀░" + System.Environment.NewLine +
                "░░░░░░░░░▄▀░░░░░░░░░░░░░▀█▀░░█░░░░█▀░░░" + System.Environment.NewLine +
                "░░░░░░░░░█░░░█░░░░░░░░░░░░░░░█░░░░░█░░░" + System.Environment.NewLine +
                "░░░░░░░░░█▄▄█░▀▄░░█▄░░░░░░░░█░░▄█░░█░░░" + System.Environment.NewLine +
                "░░░░░░░░░▀▄▄▀░░█▀▀█░▀▀▀▀█▀▀█▀▀▀▀█░█░░░░" + System.Environment.NewLine);
            }
        }
        
        //contest function
        [Command("contest")]
        public async Task contest(string user)
        {
            //generates a random number
            Random random = new Random();
            int number = random.Next(1, 3);

            //decides who wins
            if (number == 1)
            {
                await ReplyAsync(Context.User + " wins!");
            }
            else if (number == 2)
            {
                await ReplyAsync(user + " wins!");
            }
        }
    }
}