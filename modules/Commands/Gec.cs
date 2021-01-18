using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace GeckoBot.Commands
{
    public class Gec : ModuleBase<SocketCommandContext>
    {
        //gets daily gecko image
        [Command("gec")]
        public async Task gec()
        {
            //gets day of the year
            DateTime date = DateTime.Today;
            string final = (date.DayOfYear - 1).ToString();

            //sends file with exception for leap years
            await Context.Channel.SendFileAsync(filePath: Utils.pathfinder(date.DayOfYear - 1, true), text: "Today is " + date.ToString("d") + ". Day " + date.DayOfYear + " of the year " + date.Year + " (gecko #" + final + ")");
            if (date.DayOfYear == 366)
            {
                await Context.Channel.SendFileAsync(filePath: Utils.pathfinder(date.DayOfYear, false), text: "Today is " + date.ToString("d") + ". Day " + date.DayOfYear + " of the year " + date.Year + "(gecko #366)");
            }
        }
        
        //sends a message with a link to the gecko collection
        [Command("GecColle")]
        public async Task gecColle()
        {
            //buils an embed
            var embed = new EmbedBuilder
            {
                Title = "gecko collection",
                Description = ("[see the gecko collection here](https://drive.google.com/drive/folders/1Omwv0NNV0k_xlECZq3d4r0MbSbuHC_Og?usp=sharing)")
            };

            embed.WithColor(180, 212, 85);

            var embed2 = embed.Build();

            await ReplyAsync(embed: embed2);
        }

        //sends a random geckoimage
        [Command("rgec")]
        public async Task rgec()
        {
            //gets random value
            Random random = new Random();
            int numb = random.Next(0,367);
            string final = (numb).ToString();

            //adds 0s as needed
            while (final.Length < 3)
            {
                final = "0" + final;
            }

            //sends file
            await Context.Channel.SendFileAsync(filePath: Utils.pathfinder(numb, (numb == 366 ? false : true)), text: "gecko #" + final);
        }

        //finds a gecko
        [Command("fgec")]
        public async Task fgec(int value)
        {
            //converts int to string
            string final = value.ToString();

            //adds 0s as needed
            while (final.Length < 3)
            {
                final = "0" + final;
            }

            //sends files
            await Context.Channel.SendFileAsync(filePath: Utils.pathfinder(value, (value == 366 ? false : true)), text: "gecko #" + final);
        }
    }
}