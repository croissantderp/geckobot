using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using Discord;
using Discord.Commands;
using GeckoBot.Utils;
using GeckoBot.Preconditions;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;

namespace GeckoBot.Commands
{
    //number guessing game
    [Summary("A gecko guessing game.")]
    public class GuessingGame : ModuleBase<SocketCommandContext>
    {
        public static Dictionary<ulong, (int, string, List<ulong>, System.Timers.Timer)> games = new Dictionary<ulong, (int, string, List<ulong>, System.Timers.Timer)>();

        public static Dictionary<ulong, int> scores = new Dictionary<ulong, int>();

        public static List<int> alreadyDone = new List<int>();

        //loads poll dictionary as string and converts it back into dictionary
        private static Dictionary<ulong, int> RefreshScoreDict()
        {
            FileUtils.checkForExistance();

            scores = Regex.Split(FileUtils.Load(@"..\..\Cache\gecko10.gek"), @"\s(?<!\\)ҩ\s")
                .Select(part => Regex.Split(part, @"\s(?<!\\)\⁊\s"))
                .Where(part => part.Length == 2)
                .ToDictionary(sp => ulong.Parse(sp[0]), sp => int.Parse(sp[1]));

            return scores;
        }

        [Command("left")]
        [Summary("Gets the amount of geckoimages left in ggg.")]
        public async Task test()
        {
            await ReplyAsync(alreadyDone.Count().ToString());
        }

        [Command("leaderboard")]
        [Summary("displays the leaderboard fot the gecko guessing game")]
        public async Task ldrbrd()
        {
            RefreshScoreDict();

            Console.WriteLine("refreshed");

            var unnamed = scores.OrderByDescending(a => a.Value);
            var unnamed2 = unnamed.Select(a => Context.Client.GetUser(a.Key).Username + " score: " + a.Value).Take(10).ToList();

            Console.WriteLine("initiated");

            for (int i = 0; i < unnamed2.Count(); i++)
            {
                unnamed2[i] = (i + 1) + ". " + unnamed2[i];
            }

            Console.WriteLine("counted");

            if (!unnamed.Select(a => a.Key).Take(10).ToList().Contains(Context.User.Id) && scores.Keys.Contains(Context.User.Id))
            {
                int index = unnamed.Select(a => a.Key).ToList().FindIndex(a => a == Context.User.Id);
                unnamed2.Add("...");
                unnamed2.Add(index + ". " + Context.User.Username + " score: " + scores[Context.User.Id]);
                unnamed2.Add("...");
            }

            Console.WriteLine("selected");

            //buils an embed
            var embed = new EmbedBuilder
            {
                Title = "gecko guessing leaderboard",
                Description = string.Join("\n", unnamed2)
            };

            embed.WithColor(180, 212, 85);

            var embed2 = embed.Build();

            Console.WriteLine("built");

            await ReplyAsync(embed: embed2);
        }

        //generates new number
        [Command("ggg")]
        [Summary("Starts a new Gecko Guessing Game.")]
        public async Task newGame()
        {
            if (games.ContainsKey(Context.Channel.Id))
            {
                await ReplyAsync("There is already an ongoing game!");
                return;
            }

            Gec.RefreshGec();

            if (alreadyDone.Count == 0)
            {
                for (int i = 0; i <= Gec._highestGecko; i++)
                {
                    alreadyDone.Add(i);
                }
            }

            //gets random value
            Random random = new Random();
            int indexNumb = random.Next(0, alreadyDone.Count);
            int numb = alreadyDone[indexNumb];

            alreadyDone.Remove(numb);

            List<string> array = EmoteUtils.removeforbidden(Gec.geckos[DriveUtils.addZeros(numb)]).Split("_").ToList();
            array.RemoveAt(0);
            List<string> array2 = string.Join("", array).Split(".").ToList();
            string extension = array2[array2.Count - 1];
            array2.RemoveAt(array2.Count - 1);
            string final = string.Join(".", array2);

            string stFormD = final.Normalize(NormalizationForm.FormD);
            int len = stFormD.Length;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < len; i++)
            {
                System.Globalization.UnicodeCategory uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(stFormD[i]);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(stFormD[i]);
                }
            }
            final = sb.ToString();

            FileInfo file = new FileInfo(DriveUtils.ImagePath(numb, false));

            string newPath = $"../../Cache/{random.Next(0, 100000)}_ggg.{extension}";

            FileInfo newFile = file.CopyTo(newPath);

            //sends file
            await Context.Channel.SendFileAsync(
                newPath,
                $"Guess the name or number of this gecko using 'g' (spacing and underscores do not matter, remove diacritics)");

            newFile.Delete();

            //starts a timer with desired amount of time
            System.Timers.Timer t = new(60 * 1000);
            t.Elapsed += async (sender, e) => await timerUp(Context.Channel.Id, t);
            t.Start();

            games.Add(Context.Channel.Id, (numb, final, new List<ulong>(), t));
        }

        async Task timerUp(ulong channel, System.Timers.Timer t)
        {
            t.Dispose();
            await (Context.Client.GetChannel(channel) as IMessageChannel).SendMessageAsync("Time is up, the gecko was #" + games[channel].Item1 + ": " + games[channel].Item2);

            games.Remove(channel);
        }

        [Command("g")]
        [Summary("Guesses the picked gecko.")]
        public async Task attempt([Summary("The value to guess.")][Remainder] string value)
        {
            if (!games.ContainsKey(Context.Channel.Id))
            {
                await ReplyAsync("No ongoing game, use 'ggg' to start a game.");
                return;
            }

            if (games[Context.Channel.Id].Item3.Contains(Context.User.Id))
            {
                await ReplyAsync("You have already guessed correctly! Use 'gend' to end the game");
                return;
            }

            value = value.Replace("_", "").Replace(" ", "");

            int score = 0;
            int bonus = 0;
            
            int num = games[Context.Channel.Id].Item1;
            string name = games[Context.Channel.Id].Item2;

            double scoreScale = 100.0 / Globals.FuzzyMatchScore(name, name);

            int temp;

            if (int.TryParse(value, out temp) && Math.Abs(num - temp) < 10)
            {
                RefreshScoreDict();

                bonus = 30 - (games[Context.Channel.Id].Item3.Count * 10);
                score = 100 - (int)Math.Round(Math.Log10(Math.Abs(num - temp) + 1) * 100);
                score = score == 0 ? 1 : score;
                await ReplyAsync(Context.User.Username + " guessed correctly with a score of " + score + " and " + bonus + " bonus" + (score >= 50 ? ", the gecko was #" + games[Context.Channel.Id].Item1 + ": " + games[Context.Channel.Id].Item2 : ", keep guessing or use 'gend' to end the game!"));

                games[Context.Channel.Id].Item3.Add(Context.User.Id);

                int finalScore = score + bonus;

                if (scores.ContainsKey(Context.User.Id))
                {
                    finalScore += scores[Context.User.Id];
                    scores.Remove(Context.User.Id);
                }
                scores.Add(Context.User.Id, finalScore);

                if (score >= 50)
                {
                    games[Context.Channel.Id].Item4.Dispose();
                    games.Remove(Context.Channel.Id);
                }

                //converts dictionary to string and saves
                FileUtils.Save(Globals.DictToString(scores, "{0} ⁊ {1} ҩ "), @"..\..\Cache\gecko10.gek");
            }
            else if ((Globals.FuzzyMatch(name, value, out temp) && !temp.ToString().Contains("-")) || name == value)
            {
                RefreshScoreDict();

                bonus = 30 - (games[Context.Channel.Id].Item3.Count * 10);
                score = name == value ? 100 : int.Parse(Math.Round(temp * scoreScale).ToString());
                await ReplyAsync(Context.User.Username + " guessed correctly with a score of " + score + " and " + bonus + " bonus" + (score >= 50 ? ", the gecko was #" + games[Context.Channel.Id].Item1 + ": " + games[Context.Channel.Id].Item2 : ", keep guessing or use 'gend' to end the game!"));

                games[Context.Channel.Id].Item3.Add(Context.User.Id);

                int finalScore = score + bonus;

                if (scores.ContainsKey(Context.User.Id))
                {
                    finalScore += scores[Context.User.Id];
                    scores.Remove(Context.User.Id);
                }
                scores.Add(Context.User.Id, finalScore);

                if (score >= 50)
                {
                    games[Context.Channel.Id].Item4.Dispose();
                    games.Remove(Context.Channel.Id);
                }

                //converts dictionary to string and saves
                FileUtils.Save(Globals.DictToString(scores, "{0} ⁊ {1} ҩ "), @"..\..\Cache\gecko10.gek");
            }
            else
            {
                //adds reaction
                await Context.Message.AddReactionAsync(new Emoji("❌"));
            }
        }

        [Command("gend")]
        [Summary("Ends the current game.")]
        public async Task end()
        {
            if (!games.ContainsKey(Context.Channel.Id))
            {
                await ReplyAsync("No ongoing game, use 'ggg' to start a game.");
                return;
            }

            await ReplyAsync("game ended. The gecko was #" + games[Context.Channel.Id].Item1 + ": " + games[Context.Channel.Id].Item2);


            games[Context.Channel.Id].Item4.Dispose();
            games.Remove(Context.Channel.Id);
        }
    }
}