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
        // This functionality is accomplished by fgec already, consider removing
        /*
        [Command("load")]
        [Summary("Force caches a gecko image.")]
        public async Task load(int name)
        {
            await ReplyAsync($"Cached at `{DriveUtils.ImagePath(name)}`");
        }
        */
        
        //gets daily gecko image
        [Command("gec")]
        [Summary("Sends the daily gecko.")]
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
        [Summary("Links the gecko collection Google Drive folder.")]
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

        [Command("gecko gang")]
        [Summary("Lists people who are part of the gecko gang.")]
        public async Task gang()
        {
            //if file exists, load it
            if (FileUtils.Load(@"..\..\Cache\gecko3.gek") != null)
            {
                //clears
                Globals.dmUsers.Clear();

                //gets info
                string[] temp = FileUtils.Load(@"..\..\Cache\gecko3.gek").Split(",");

                //adds info to list
                foreach (string a in temp)
                {
                    Globals.dmUsers.Add(ulong.Parse(a));
                }
            }

            List<string> names = new List<string>();

            foreach(ulong a in Globals.dmUsers)
            {
                names.Add(Context.Client.GetUser(a).Username);
            }

            //buils an embed
            var embed = new EmbedBuilder
            {
                Title = "join the gecko gang",
                Description = (string.Join("\n", names))
            };

            embed.WithColor(180, 212, 85);

            await ReplyAsync("", embed: embed.Build(), allowedMentions: Globals.allowed);
        }

        [Command("submit")]
        [Summary("Links the gecko submission form.")]
        public async Task submit()
        {
            //buils an embed
            var embed = new EmbedBuilder
            {
                Title = "gecko submissions",
                Description = ("[submit a geckoimage here](https://forms.gle/CeNkM2aHcdrcidvX6)")
            };

            embed.WithColor(180, 212, 85);

            var embed2 = embed.Build();

            await ReplyAsync(embed: embed2);
        }

        //sends a random geckoimage
        [Command("rgec")]
        [Summary("Sends a random gecko.")]
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
        [Summary("Sends the specified gecko.")]
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
        [Summary("Sends the latest gecko.")]
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
        // Is inefficient; should update soon
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