﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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

        //gets daily gecko image
        [Command("ygec")]
        [Summary("Sends all geckos for a specified day")]
        public async Task ygec(int dayNum)
        {
            RefreshGec();

            string final = "";
            int i = 0;
            while (dayNum + (i * 367) < _highestGecko)
            {
                final += $"{geckos[DriveUtils.addZeros(dayNum + (i * 367))]} ";
                i++;
            }

            await ReplyAsync(final);
        }

        //gets daily gecko image
        [Command("gec")]
        [Summary("Sends the daily gecko.")]
        public async Task gec([Summary("Optional year.")] int year = 0)
        {
            //gets day of the year
            DateTime date = DateTime.Today;

            RefreshGec();

            _highestGecko = await FetchHighestGec();

            if ((date.DayOfYear - 1) + ((year-1) * 367) > _highestGecko)
            {
                await ReplyAsync("year does not exist yet");
                return;
            }

            year = year == 0 ? int.Parse(FileUtils.Load(@"..\..\Cache\gecko4.gek").Split("$")[0]) : year;


            string final = $"Today is {date.ToString("d")}. Day {date.DayOfYear} of the year {date.Year} (gecko: {geckos[DriveUtils.addZeros(((year - 1) * 367) + (date.DayOfYear - 1))]})\n" +
                $"Other geckos of today include: ";
            int i = 0;
            while ((date.DayOfYear - 1) + (i * 367) < _highestGecko)
            {
                final += $" {geckos[DriveUtils.addZeros((date.DayOfYear - 1) + (i * 367))]}";
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
                "[see the gecko collection here](https://drive.google.com/drive/folders/1Omwv0NNV0k_xlECZq3d4r0MbSbuHC_Og?usp=sharing)\n" +
                "[submit a geckoimage here](https://forms.gle/CeNkM2aHcdrcidvX6)\n" +
                "[about](https://docs.google.com/document/d/1cBOt6IL3leouEg90WItw769HLBea5JD1_8bIoKC7A9s/edit?usp=sharing)\n")
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
            //if file exists, load it
            if (FileUtils.Load(@"..\..\Cache\gecko3.gek") != null)
            {
                //clears
                DailyDM.DmUsers.Clear();

                //gets info
                string[] temp = FileUtils.Load(@"..\..\Cache\gecko3.gek").Split(",");

                //adds info to list
                DailyDM.DmUsers.AddRange(temp.Select(ulong.Parse));
            }

            List<string> names = new List<string>();
            names.AddRange(DailyDM.DmUsers.Select(user => Context.Client.GetUser(user).Username));

            //buils an embed
            var embed = new EmbedBuilder
            {
                Title = "join the gecko gang",
                Description = (string.Join("\n", names))
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
                $"gecko: {geckos[final]}");
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
                $"gecko: {geckos[final]}");
        }

        //finds a gecko
        [Command("sgec")]
        [Summary("Searches for the specified gecko.")]
        public async Task sgec([Summary("the search input.")] string input, [Summary("the result number.")] int index = 1)
        {
            RefreshGec();

            int tempInt = 0;
            List<int> tempIntList = new List<int>();

            IEnumerable<KeyValuePair<string, string>> temp = geckos.Where(a => Globals.FuzzyMatch(a.Value, input.Replace(" ", ""), out tempInt)).OrderByDescending(a => Globals.FuzzyMatchScore(a.Value, input.Replace(" ", ""))); //a.Value.Contains(input)

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
                $"result {temp2.Count()} of {temp.Count()}, score: {Globals.FuzzyMatchScore(geckos[final], input.Replace(" ", ""))}, gecko: {geckos[final]}");
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
                $"gecko: {geckos[final]}");
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
        public async Task hgec()
        {
            RefreshGec();

            _highestGecko = await FetchHighestGec();

            //sends file
            await Context.Channel.SendFileAsync(
                DriveUtils.ImagePath(_highestGecko, false), 
                $"gecko: {geckos[DriveUtils.addZeros(_highestGecko)]}");
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
            listRequest.OrderBy = "name desc"; // Name descending gets the highest number gecko
            listRequest.Q = "mimeType contains 'image'"; // Filter out folders or other non image types
            while (true)
            {
                FileList files2 = await listRequest.ExecuteAsync();
                IList<File> files = files2.Files;

                foreach (File a in files)
                {
                    if (Regex.IsMatch(a.Name, @"^\d"))
                    {
                        return int.Parse(Regex.Replace(a.Name, @"_.+", ""));
                    }
                }
                listRequest.PageToken = files2.NextPageToken;
            }
        }

        // Fetches the highest gecko, then updates the HighestGecko value and alerts image subscribers. Also sends multiple geckoimages if there have been multiple added since last checked.
        public async Task RefreshHighestGec()
        {
            var fetched = await FetchHighestGec();
            if (fetched == _highestGecko) return;
            
            List<string> paths = new List<string>();

            for (int i = 0; i < fetched - _highestGecko; i++)
            {
                paths.Add(DriveUtils.ImagePath(_highestGecko + i + 1, false));
            }

            for (int i = 0; i < fetched - _highestGecko; i++)
            {
                await Program.ddm.DmGroup(
                    paths[i],
                    $"new gecko image: {geckos[DriveUtils.addZeros(_highestGecko + i + 1)]}");
            }

                _highestGecko = fetched;
        }
    }
}