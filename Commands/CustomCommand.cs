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
    [Summary("Epic custom commands.")]
    public class CustomCommand : ModuleBase<SocketCommandContext>
    {
        //emote dictionary
        private static Dictionary<string, string> cDict = new ();

        //loads emote dictionary as string and converts it back into dictionary
        private static void RefreshCDict()
        {
            cDict = Regex.Split(FileUtils.Load(@"..\..\Cache\gecko6.gek"), @"\s(?<!\\)ҩ\s")
                .Select(part => Regex.Split(part, @"\s(?<!\\)\⁊\s"))
                .Where(part => part.Length == 2)
                .ToDictionary(sp => sp[0], sp => sp[1]);
        }

        [Command("cs")]
        [Summary("Creates a custom command.")]
        public async Task cs([Summary("The name of the custom command.")] string title, [Summary("The command content, fields are marked by '$'.")] [Remainder] string content)
        {
            if (!Regex.IsMatch(content, @"(?<!\\)\$"))
            {
                await ReplyAsync("please have at least one field");
                return;
            }

            RefreshCDict();

            cDict.Add(EmoteUtils.escapeforbidden(title), EmoteUtils.escapeforbidden(content));

            FileUtils.Save(Globals.DictToString(cDict, "{0} ⁊ {1} ҩ "), @"..\..\Cache\gecko6.gek");

            //adds check emote after done
            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

        [Command("c")]
        [Summary("Use a custom command.")]
        public async Task c([Summary("The name of the custom command")] string title, [Summary("The fields of the command, seperate with '$'.")] [Remainder] string content)
        {
            RefreshCDict();
            int amount = Regex.Matches(cDict[title], @"(?<!\\)\$").Count;
            string[] inserts = Regex.Split(content, @"(?<!\\)\$");

            if (inserts.Length != amount)
            {
                await ReplyAsync("there are " + amount + " fields in this command");
            }

            Regex rgx = new Regex(@"(?<!\\)\$");

            string final = cDict[title];

            for (int i = 0; i < amount; i++)
            {
                final = rgx.Replace(final, "" + inserts[i] + "", 1);
            }

            await ReplyAsync(EmoteUtils.removeforbidden(final), allowedMentions: Globals.allowed);
        }

        [Command("cf")]
        [Summary("See the custom command fields.")]
        public async Task cf([Summary("The name of the custom command to find.")] string title)
        {
            RefreshCDict();

            string final = Regex.Replace(cDict[title], @"(?<!\\)\$", "[field]");

            await ReplyAsync(EmoteUtils.removeforbidden(final), allowedMentions: Globals.allowed);
        }

        [Command("cr")]
        [Summary("Remove a custom command.")]
        public async Task cr([Summary("The name of the custom command to remove.")] [Remainder] string title)
        {
            RefreshCDict();

            if (!cDict.ContainsKey(title))
            {
                await ReplyAsync("command not found");
                return;
            }

            cDict.Remove(title);

            FileUtils.Save(Globals.DictToString(cDict, "{0} ⁊ {1} ҩ "), @"..\..\Cache\gecko6.gek");

            //adds check emote after done
            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

        [Command("cl")]
        [Summary("List of all commands.")]
        public async Task cl()
        {
            await Context.Channel.SendFileAsync(@"..\..\Cache\gecko6.gek");
        }
    }
}
