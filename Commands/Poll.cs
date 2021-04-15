using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GeckoBot.Preconditions;
using GeckoBot.Utils;
using System.Text.RegularExpressions;

namespace GeckoBot.Commands
{
    [Summary("Ask others about things.")]
    public class Poll : ModuleBase<SocketCommandContext>
    {
        //poll dictionary
        private static Dictionary<string, string> rateDict = new();

        //loads poll dictionary as string and converts it back into dictionary
        private static void RefreshRateDict()
        {
            rateDict = Regex.Split(FileUtils.Load(@"..\..\Cache\gecko5.gek"), @"\s(?<!\\)ҩ\s")
                .Select(part => Regex.Split(part, @"\s(?<!\\)\⁊\s"))
                .Where(part => part.Length == 2)
                .ToDictionary(sp => sp[0], sp => sp[1]);
        }

        [Command("pl")]
        [Summary("Sends the poll list.")]
        public async Task plist()
        {
            await Context.Channel.SendFileAsync(@"..\..\Cache\gecko5.gek");
        }

        [Command("pc")]
        [Summary("Creates a poll.")]
        public async Task create([Summary("The content of the poll.")] [Remainder]string poll)
        {
            RefreshRateDict();

            if (rateDict.ContainsKey(poll))
            {
                await ReplyAsync("a poll of that name already exists");
                return;
            }

            rateDict.Add(EmoteUtils.escapeforbidden(poll), "0/0;;" + Context.User.Id.ToString());

            FileUtils.Save(Globals.DictToString(rateDict, "{0} ⁊ {1} ҩ "), @"..\..\Cache\gecko5.gek");

            //adds check emote after done
            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

        [Command("pv")]
        [Summary("Votes on a poll.")]
        public async Task vote([Summary("The content of the poll to vote on.")] string poll, [Summary("The fraction to submit in ##/## (denominator has to be greatest and numerator has to be positive).")] string fraction)
        {
            RefreshRateDict();

            string[] fractions = fraction.Split("/");
            int[] fractionNums = { int.Parse(fractions[0]) , int.Parse(fractions[1]) };

            if (fractionNums[0] > fractionNums[1] || fractionNums[0] < 0)
            {
                await ReplyAsync("invalid fraction");
                return;
            }

            string[] parts = rateDict[poll].Split(";");

            string[] people = parts[1].Split(",");
            if (people.Contains(Context.User.Id.ToString()))
            {
                await ReplyAsync("you have already voted on this poll");
                return;
            }

            parts[1] += Context.User.Id + ",";

            string[] finalFracParts = parts[0].Split("/");
            finalFracParts[0] = (int.Parse(finalFracParts[0]) + fractionNums[0]).ToString();
            finalFracParts[1] = (int.Parse(finalFracParts[1]) + fractionNums[1]).ToString();

            parts[0] = finalFracParts[0] + "/" + finalFracParts[1];

            rateDict.Remove(poll);
            rateDict.Add(poll, parts[0] + ";" + parts[1] + ";" + parts[2]);

            FileUtils.Save(Globals.DictToString(rateDict, "{0} ⁊ {1} ҩ "), @"..\..\Cache\gecko5.gek");

            //adds check emote after done
            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

        [Command("ps")]
        [Summary("see results of a poll")]
        public async Task results([Summary("The content of the poll to see results for.")] [Remainder]string poll)
        {
            RefreshRateDict();

            string[] parts = rateDict[poll].Split(";");

            string[] fractions = parts[0].Split("/");

            string final = String.Format("{0:P2}.", float.Parse(fractions[0]) / float.Parse(fractions[1]));

            await ReplyAsync("rating on \"" + poll + "\": " + final, allowedMentions: Globals.allowed);
        }

        [Command("pr")]
        [Summary("removes polls, only creator of poll can remove")]
        public async Task remove([Summary("The content of the poll to remove.")] [Remainder]string poll)
        {
            await PollRemove(poll, false);
        }

        [RequireGeckobotAdmin]
        [Command("fpr")]
        [Summary("force removes polls, requires geckobot admin")]
        public async Task fremove([Summary("The content of the poll to remove.")] [Remainder]string poll)
        {
            await PollRemove(poll, true);
        }
        
        // Remove a poll from the dictionary
        private async Task PollRemove(string poll, bool withAdmin)
        {
            RefreshRateDict();

            string userid = rateDict[poll].Split(";")[2];

            if (withAdmin || Context.User.Id.ToString() == userid)
            {
                rateDict.Remove(poll);

                //adds check emote after done
                await Context.Message.AddReactionAsync(new Emoji("✅"));
            }

            FileUtils.Save(Globals.DictToString(rateDict, "{0} ⁊ {1} ҩ "), @"..\..\Cache\gecko5.gek");
        }
    }
}
