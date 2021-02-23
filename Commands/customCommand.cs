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
    public class customCommand : ModuleBase<SocketCommandContext>
    {
        //emote dictionary
        public static Dictionary<string, string> cDict = new ();

        //loads emote dictionary as string and converts it back into dictionary
        public static void RefreshCDict()
        {
            cDict = Regex.Split(FileUtils.Load(@"..\..\Cache\gecko6.gek"), @"\s(?<!\\)ҩ\s")
                .Select(part => Regex.Split(part, @"\s(?<!\\)\⁊\s"))
                .Where(part => part.Length == 2)
                .ToDictionary(sp => sp[0], sp => sp[1]);
        }

        [Command("cs")]
        [Summary("Creates a custom command.")]
        public async Task cs(string title, [Remainder] string content)
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
        public async Task c(string title, [Remainder] string content)
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
                final = rgx.Replace(final, " " + inserts[i] + " ", 1);
            }

            await ReplyAsync(EmoteUtils.removeforbidden(final), allowedMentions: Globals.allowed);
        }

        [Command("cr")]
        [Summary("Remove a custom command.")]
        public async Task cr([Remainder] string title)
        {
            RefreshCDict();

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
