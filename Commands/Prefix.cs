using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using GeckoBot.Preconditions;
using GeckoBot.Utils;

namespace GeckoBot.Commands
{
    [Summary("Prefix related commands.")]
    public class Prefix : ModuleBase<SocketCommandContext>
    {
        //prefix dictionary
        private static Dictionary<string, string> prefixes = new();

        //loads poll dictionary as string and converts it back into dictionary
        private static void RefreshPrefixDict()
        {
            FileUtils.checkForExistance();

            prefixes = Regex.Split(FileUtils.Load(@"..\..\Cache\gecko8.gek"), @"\s(?<!\\)ҩ\s")
                .Select(part => Regex.Split(part, @"\s(?<!\\)\⁊\s"))
                .Where(part => part.Length == 2)
                .ToDictionary(sp => sp[0], sp => sp[1]);
        }

        [Command("change prefix")]
        [Summary("Changes the bot's prefix in the current server, only works in guilds.")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task change([Summary("The new prefix.")][Remainder]string prefix)
        {
            RefreshPrefixDict();

            if (prefixes.ContainsKey(Context.Guild.Id.ToString()))
            {
                prefixes.Remove(Context.Guild.Id.ToString());
            }

            prefixes.Add(Context.Guild.Id.ToString(), EmoteUtils.escapeforbidden(prefix));

            FileUtils.Save(Globals.DictToString(prefixes, "{0} ⁊ {1} ҩ "), @"..\..\Cache\gecko8.gek");

            //adds check emote after done
            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

        public static string returnPrefix(string guild)
        {
            RefreshPrefixDict();

            return prefixes.ContainsKey(guild) && guild != null ? EmoteUtils.removeforbidden(prefixes[guild]) : "`";
        }
    }
}
