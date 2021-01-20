using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Google.Apis.Drive.v3;

namespace GeckoBot.Commands
{
    public class Gec : ModuleBase<SocketCommandContext>
    {
        // Force cache a gecko image
        [Command("load")]
        public async Task load(int name)
        {
            await ReplyAsync($"Cached at `{DriveUtils.ImagePath(name)}`");
        }
        
        //gets daily gecko image
        [Command("gec")]
        public async Task gec()
        {
            //gets day of the year
            DateTime date = DateTime.Today;
            string final = (date.DayOfYear - 1).ToString();

            //sends file with exception for leap years
            await Context.Channel.SendFileAsync(
                DriveUtils.ImagePath(date.DayOfYear - 1), 
                $"Today is {date.ToString("d")}. Day {date.DayOfYear} of the year {date.Year} (gecko #{final})");
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
            refreshHighestGec();
            
            //gets random value
            Random random = new Random();
            int numb = random.Next(0,Globals.HighestGecko);
            string final = DriveUtils.addZeros(numb);

            //sends file
            await Context.Channel.SendFileAsync(
                DriveUtils.ImagePath(numb), 
                $"gecko #{final}");
        }

        //finds a gecko
        [Command("fgec")]
        public async Task fgec(int value)
        {
            //converts int to string
            string final = DriveUtils.addZeros(value);

            //sends files
            await Context.Channel.SendFileAsync(
                DriveUtils.ImagePath(value), 
                $"gecko #{final}");
        }
        
        // Gets the highest number gecko
        [Command("hgec")]
        public async Task hgec()
        {
            refreshHighestGec();
            int num = Globals.HighestGecko;
            
            //sends file
            await Context.Channel.SendFileAsync(
                DriveUtils.ImagePath(num), 
                $"gecko #{num}");
        }

        // Gets the filename of the highest number gecko off of drive, then updates the Global value
        void refreshHighestGec()
        {
            DriveService driveService = DriveUtils.AuthenticateServiceAccount(
                "geckobotfileretriever@geckobot.iam.gserviceaccount.com", 
                "../../../GeckoBot-af43fa71833e.json");
            var listRequest = driveService.Files.List();
            //listRequest.Fields = "nextPageToken, files(id, name)";
            listRequest.PageSize = 1; // Only fetch one
            listRequest.OrderBy = "name desc"; // Name descending gets the highest number gecko
            listRequest.Q = "mimeType contains 'image'"; // Filter out folders or other non image types
            
            IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute().Files;
            Globals.HighestGecko = int.Parse(Regex.Replace(files[0].Name, @"_.+", ""));
        }
    }
}