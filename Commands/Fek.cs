using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace GeckoBot.Commands
{
    public class Fek : ModuleBase<SocketCommandContext>
    {
        //generates a random order of characters
        [Command("fek")]
        public async Task fek()
        {
            //generates random
            Random random = new Random();

            //generate random length of the word
            int charNum = random.Next(2, 10);
            
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
    }
}