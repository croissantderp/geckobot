using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using Discord;
using Discord.Commands;
using GeckoBot.Utils;
using GeckoBot.Preconditions;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;

namespace GeckoBot.Commands
{
    [Summary("Integration of the [gecko collection](https://drive.google.com/drive/folders/1Omwv0NNV0k_xlECZq3d4r0MbSbuHC_Og?usp=sharing).")]
    public class Gec : ModuleBase<SocketCommandContext>
    {
        public static int _highestGecko = FetchHighestGec().Result;
        public static Dictionary<string, string> geckos = new();
        static bool runnin = false;
        
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
        
        public static void RefreshGec()
        {
            geckos = Regex.Split(FileUtils.Load(@"..\..\Cache\gecko7.gek"), @"\s(?<!\\)ҩ\s")
            .Select(part => Regex.Split(part, @"\s(?<!\\)\⁊\s"))
            .Where(part => part.Length == 2)
            .ToDictionary(sp => sp[0], sp => sp[1]);
        }

        // Force updates
        [RequireGeckobotAdmin]
        [Command("set highest")]
        [Summary("Sets highest gecko to something else.")]
        public async Task seth(int num)
        {
            RefreshGec();

            await RefreshHighestGec();

            if (num > _highestGecko)
            {
                await ReplyAsync("new highest gecko cannot be higher than the actual one");
                return;
            }
            _highestGecko = num;

            await RefreshHighestGec();

            await ReplyAsync("force highest gecko to " + _highestGecko);
        }

        //gets daily gecko image
        [Command("ygec")]
        [Summary("Sends all geckos for a specified day.")]
        public async Task ygec([Summary("The day to search for.")] int dayNum)
        {
            RefreshGec();

            await RefreshHighestGec();

            string final = "";
            int i = 0;
            while (dayNum + (i * 367) < _highestGecko)
            {
                final += $"{EmoteUtils.removeforbidden(geckos[DriveUtils.addZeros(dayNum + (i * 367))])} ";
                i++;
            }

            await ReplyAsync(final);
        }

        //gets daily gecko image
        [Command("ycgec")]
        [Summary("Calculates the gecko number for a specific year.")]
        public async Task ycgec([Summary("The specific year.")] int year, [Summary("The day to search for.")] int dayNum)
        {
            RefreshGec();

            string final = "";

            for (int i = 0; i < year; i++)
            {
                final += "year " + (i + 1) + ": " + (dayNum + (i * 367)).ToString() + "\n";
            }

            final += $"year range: {(year - 1) * 367} - {year * 367}";

            await ReplyAsync(final);
        }

        //gets daily gecko image
        [Command("gec")]
        [Summary("Sends the daily gecko.")]
        public async Task gec([Summary("Optional year.")] int year = 0)
        {
            //gets day of the year
            DateTime date = DateTime.UtcNow;

            RefreshGec();

            //await RefreshHighestGec();

            if ((date.DayOfYear - 1) + ((year-1) * 367) > _highestGecko)
            {
                await ReplyAsync("year does not exist yet");
                return;
            }

            year = year == 0 ? int.Parse(FileUtils.Load(@"..\..\Cache\gecko4.gek").Split("$")[0]) : year;


            string final = $"Today is {date.ToString("d")}. Day {date.DayOfYear} of the year {date.Year} (gecko: {EmoteUtils.removeforbidden(geckos[DriveUtils.addZeros(((year - 1) * 367) + (date.DayOfYear - 1))])})\n" +
                $"Other geckos of today include: ";
            int i = 0;
            while ((date.DayOfYear - 1) + (i * 367) < _highestGecko)
            {
                final += $" {EmoteUtils.removeforbidden(geckos[DriveUtils.addZeros((date.DayOfYear - 1) + (i * 367))])}";
                i++;
            }

            //sends file
            await Context.Channel.SendFileAsync(
                DriveUtils.ImagePath(((year - 1) * 367) + (date.DayOfYear - 1), false), final
                );
        }
        
        //sends a message with a link to the gecko collection
        [Command("GecColle")]
        [Alias("gecko collection")]
        [Summary("Links the gecko collection Google Drive folder.")]
        public async Task gecColle()
        {
            //buils an embed
            var embed = new EmbedBuilder
            {
                Title = "gecko collection",
                Description = (
                "[Website](https://geckos.web.app)\n" +
                "[Google Drive](https://drive.google.com/drive/folders/1Omwv0NNV0k_xlECZq3d4r0MbSbuHC_Og?usp=sharing)\n" +
                "[official](https://discord.gg/Jgf6e3xaWf) [server](https://tinyurl.com/geckoimages) [invite](http://www.5z8.info/hack-outlook_n3e7iu_--INITIATE-CREDIT-CARD-XFER--)")
            };

            embed.WithColor(180, 212, 85);

            var embed2 = embed.Build();

            await ReplyAsync(embed: embed2);
        }

        [Command("gecko gang")]
        [Alias("subscribers")]
        [Summary("Lists people who are part of the gecko gang.")]
        public async Task gang()
        {
            DailyDM.RefreshUserDict();

            List<string> names = new List<string>();
            names.AddRange(DailyDM.DmUsers.Keys.Where(a => !DailyDM.DmUsers[a].Item1).Select(user => Context.Client.GetUser(user).Username));

            List<string> channels = new List<string>();
            channels.AddRange(DailyDM.DmUsers.Keys.Where(a => DailyDM.DmUsers[a].Item1).Select(a => Context.Client.GetChannel(a).ToString()));

            //buils an embed
            var embed = new EmbedBuilder
            {
                Title = "join the gecko gang",
                Description = string.Join("\n", names) + "\n\n" + string.Join("\n", channels)
            };

            embed.WithColor(180, 212, 85);

            await ReplyAsync("", embed: embed.Build(), allowedMentions: Globals.allowed);
        }

        //sends a random geckoimage
        [Command("rgec")]
        [Summary("Sends a random gecko.")]
        public async Task rgec()
        {
            RefreshGec();

            //gets random value
            Random random = new Random();
            int numb = random.Next(0, _highestGecko + 1);
            string final = DriveUtils.addZeros(numb);

            //sends file
            await Context.Channel.SendFileAsync(
                DriveUtils.ImagePath(numb, false), 
                $"gecko: {EmoteUtils.removeforbidden(geckos[final])}");
        }

        //finds a gecko
        [Command("fgec")]
        [Summary("Sends the specified gecko.")]
        public async Task fgec([Summary("The value of the gecko.")] int value)
        {
            RefreshGec();

            //converts int to string
            string final = DriveUtils.addZeros(value);

            //sends files
            await Context.Channel.SendFileAsync(
                DriveUtils.ImagePath(value, false), 
                $"gecko: {EmoteUtils.removeforbidden(geckos[final])}");
        }

        //finds a gecko
        [Command("rfgec")]
        [Summary("Deletes and redownloads the specified gecko.")]
        public async Task rfgec([Summary("The value of the gecko.")] int value)
        {
            //converts int to string
            string final = DriveUtils.addZeros(value);

            //sends files
            await Context.Channel.SendFileAsync(
                DriveUtils.ImagePath(value, false, true),
                $"refreshed gecko: {EmoteUtils.removeforbidden(geckos[final])}");
        }

        //finds a gecko
        [Command("sgec")]
        [Summary("Searches for the specified gecko.")]
        public async Task sgec([Summary("the search input.")] string input, [Summary("the result number.")] int index = 1)
        {
            RefreshGec();

            int tempInt = 0;
            List<int> tempIntList = new List<int>();

            IEnumerable<KeyValuePair<string, string>> temp = geckos.Where(a => Globals.FuzzyMatch(EmoteUtils.removeforbidden(a.Value), input.Replace(" ", ""), out tempInt)).OrderByDescending(a => Globals.FuzzyMatchScore(EmoteUtils.removeforbidden(a.Value), input.Replace(" ", ""))); //a.Value.Contains(input)

            IEnumerable<KeyValuePair<string, string>> temp2 = temp.Take(index);
            string final = temp2.Last().Key;

            bool isAlt = false;

            if (final.Contains("b"))
            {
                isAlt = true;
                final = final.Remove(0, 1);
            }

            //converts int to string
            int value = int.Parse(final);

            //sends files
            await Context.Channel.SendFileAsync(
                DriveUtils.ImagePath(value, isAlt),
                $"result {temp2.Count()} of {temp.Count()}, score: {Globals.FuzzyMatchScore(EmoteUtils.removeforbidden(geckos[final]), input.Replace(" ", ""))}, gecko: {EmoteUtils.removeforbidden(geckos[final])}");
        }

     
        //finds an alternate gecko
        [Command("ogec")]
        [Summary("Sends an alternate of a gecko")]
        public async Task ogec([Summary("The value of the gecko.")] int value)
        {
            RefreshGec();
            //converts int to string
            string final = "b" + DriveUtils.addZeros(value);

            //sends files
            await Context.Channel.SendFileAsync(
                DriveUtils.ImagePath(value, true),
                $"gecko: {EmoteUtils.removeforbidden(geckos[final])}");
        }

        //caches all gecko descriptions
        [RequireGeckobotAdmin]
        [Command("cgec")]
        [Summary("caches all geckos and descriptions.")]
        public async Task cgec()
        {
            RefreshGec();
            int before = geckos.Count;
            int number = DriveUtils.saveAll(_highestGecko);

            await ReplyAsync(number + " items saved, " + (geckos.Count - before) + " descriptions saved");
            
        }

        // Gets the highest number gecko
        [Command("hgec")]
        [Summary("Sends the latest gecko.")]
        public async Task hgec([Summary("If hgec should check or not.")] bool check = false)
        {
            RefreshGec();

            if (check)
            {
                await RefreshHighestGec();
            }

            //sends file
            await Context.Channel.SendFileAsync(
                DriveUtils.ImagePath(_highestGecko, false), 
                $"gecko: {EmoteUtils.removeforbidden(geckos[DriveUtils.addZeros(_highestGecko)])}");
        }

        // Fetches the highest gecko from Google Drive
        private static async Task<int> FetchHighestGec()
        {
            DriveService driveService = DriveUtils.AuthenticateServiceAccount(
                "geckobotfileretriever@geckobot.iam.gserviceaccount.com", 
                "../../../GeckoBot-af43fa71833e.json");
            
            var listRequest = driveService.Files.List();
            //listRequest.Fields = "nextPageToken, files(id, name)";
            listRequest.PageSize = 100; // Only fetch one hundred
            listRequest.OrderBy = "name_natural desc"; // Name descending gets the highest number gecko
            listRequest.Q = "mimeType contains 'image'"; // Filter out folders or other non image types
            
            try
            {
                while (true)
                {
                    FileList files2 = await listRequest.ExecuteAsync();
                    IList<File> files = files2.Files;

                    foreach (File a in files)
                    {
                        Console.WriteLine(a.Name);
                        if (Regex.IsMatch(a.Name, @"^\d"))
                        {
                            return int.Parse(Regex.Replace(a.Name, @"_.+", ""));
                        }
                    }
                    listRequest.PageToken = files2.NextPageToken;
                }
            }
            catch
            {
                return _highestGecko;
            }
        }

        // Fetches the highest gecko, then updates the HighestGecko value and alerts image subscribers. Also sends multiple geckoimages if there have been multiple added since last checked.
        public async Task RefreshHighestGec()
        {
            if (runnin)
            {
                return;
            }
            runnin = true;

            var fetched = await FetchHighestGec();
            if (fetched == _highestGecko)
            {
                runnin = false;
                return;
            }
            
            List<string> paths = new List<string>();

            int total = fetched - _highestGecko;

            int baseline = _highestGecko;

            int trunicated = 0;

            if (total > 5)
            {
                trunicated = total - 5;
                baseline = fetched - 5;
                total = 5;
            }

            for (int i = 0; i < total; i++)
            {
                paths.Add(DriveUtils.ImagePath(baseline + i + 1, false));
            }

            for (int i = 0; i < total; i++)
            {
                await Program.hdm.DmGroup(
                    paths[i],
                    $"new gecko image: {EmoteUtils.removeforbidden(geckos[DriveUtils.addZeros(baseline + i + 1)])}");
                Thread.Sleep(10000);
            }

            if (trunicated != 0)
            {
                await Program.hdm.DmGroup("", $"{trunicated} more new geckoimages ({_highestGecko + 1} - {baseline})", false);
            }

            _highestGecko = fetched;
            runnin = false;
        }
    }
}