using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using GeckoBot.Utils;

namespace GeckoBot.Commands
{
    [Group("get")]
    [Summary("Gets information about things you are stalking")]
    public class get : ModuleBase<SocketCommandContext>
    {
        [Command("channel")]
        [Summary("gets channel id")]
        public async Task getChannel(IChannel channel)
        {
            await ReplyAsync("guild id: \n`" + (channel as IGuildChannel).GuildId + "`\n" + 
                "channel id: \n`" + channel.Id + "`", allowedMentions: Globals.allowed);
        }

        [Command("message")]
        [Summary("gets ids from a message link")]
        public async Task getMessage(string input)
        {
            input = input.Remove(0, 29);
            string[] final = input.Split("/");

            await ReplyAsync("guild id: \n`" + final[0] + "`\n" + 
                "channel id: \n`" + final[1] + "`\n" + 
                "message id: \n`" + final[2] + "`", allowedMentions: Globals.allowed);
        }

        [Command("message content")]
        [Summary("gets ids from a message link")]
        public async Task getMessageContent(string input)
        {
            input = input.Remove(0, 29);
            string[] final = input.Split("/");

            var channel = Context.Client.GetChannel(ulong.Parse(final[1])) as IMessageChannel;

            var message = await channel.GetMessageAsync(ulong.Parse(final[2]));

            //embed
            var embed = new EmbedBuilder
            {
                Description = message.Content,
                Author = new EmbedAuthorBuilder().WithName(message.Author.ToString()).WithIconUrl(message.Author.GetAvatarUrl())
            };

            embed.WithTimestamp(message.Timestamp);

            embed.WithColor(180, 212, 85);

            await ReplyAsync("", embed: embed.Build(), allowedMentions: Globals.allowed);
        }

        [Command("user")]
        [Summary("gets info about a user using a mention")]
        public async Task getUser(IUser user)
        {
            await ReplyAsync("user id: \n`" + user.Id + "`", allowedMentions: new AllowedMentions(Discord.AllowedMentionTypes.None));
        }

        [Command("emote")]
        [Summary("gets info about an emote using name or emote")]
        public async Task getEmote(string emote)
        {
            try
            {
                IEmote emote2 = Emote.Parse(emote);
                
                await ReplyAsync(emote2 + "\n emote id: \n`" + emote2 + "`", allowedMentions: new AllowedMentions(AllowedMentionTypes.None));
            }
            catch
            {
                foreach (IGuild a in Context.Client.Guilds)
                {
                    foreach (IEmote e in a.Emotes)
                    {
                        if (e.Name == emote)
                        {
                            await ReplyAsync(e + "\n emote id: \n`" + e + "`", allowedMentions: new AllowedMentions(AllowedMentionTypes.None));
                            return;
                        }
                    }
                }
                await ReplyAsync("emote not found", allowedMentions: new AllowedMentions(AllowedMentionTypes.None));
            }
        }

        [Command("all emote")]
        [Summary("gets all emotes organized into pages of 5")]
        public async Task getAllEmote(string emote, int page = 1)
        {
            List<string> final = new ();

            int counter = 0;

            int total = 0;

            int pageCounter = 1;

            final.Add("temp");

            foreach (IGuild a in Context.Client.Guilds)
            {
                foreach (IEmote e in a.Emotes)
                {
                    if (e.Name == emote)
                    {
                        if (counter >= 5)
                        {
                            pageCounter++;
                            counter = 0;
                        }
                        if (pageCounter == page)
                        {
                            final.Add(e.ToString() + ":\n`" + e.ToString() + "`\n");
                        }
                        counter++;
                        total++;
                    }
                }
            }

            final[0] = "page " + page + " of " + pageCounter + " of results for " + emote + " (result " + (page * 5 - 4) + " - " + (page != pageCounter ? (page * 5) : page * 5 - 5 + (total % 5)) + " of " + total + ")" + "\n";

            if (final.Count > 1)
            {
                await ReplyAsync(string.Join("", final), allowedMentions: new AllowedMentions(Discord.AllowedMentionTypes.None));
            }
            else
            {
                await ReplyAsync(pageCounter > 1 ? "no more results, only " + pageCounter + " pages" : "emote not found");
            }
        }
    }

    [Group("find")]
    [Summary("finds various stuff, usually from ids")]
    public class find : ModuleBase<SocketCommandContext>
    {
        [Command("channel")]
        [Summary("finds channel from an id")]
        public async Task findChannel(string input)
        {
            await ReplyAsync("<#" + input + "> \n`<#" + input + ">`", allowedMentions: Globals.allowed);
        }

        [Command("message")]
        [Summary("generates a message link from given ids")]
        public async Task findMessage(string input, string input2)
        {
            var channel2 = Context.Client.GetChannel(ulong.Parse(input)) as IGuildChannel;

            await ReplyAsync("https://discord.com/channels/" + channel2.GuildId + "/" + input + "/" + input2, allowedMentions: Globals.allowed);
        }

        [Command("message content")]
        [Summary("gets ids from channel and message ids")]
        public async Task findMessageContent(string input, string input2)
        {
            var channel = Context.Client.GetChannel(ulong.Parse(input)) as IMessageChannel;

            var message = await channel.GetMessageAsync(ulong.Parse(input2));

            //embed
            var embed = new EmbedBuilder
            {
                Description = message.Content,
                Author = new EmbedAuthorBuilder().WithName(message.Author.ToString()).WithIconUrl(message.Author.GetAvatarUrl())
            };

            embed.WithTimestamp(message.Timestamp);

            embed.WithColor(180, 212, 85);

            await ReplyAsync("", embed: embed.Build(), allowedMentions: Globals.allowed);
        }

        [Command("user")]
        [Summary("gets info about a user using id")]
        public async Task findUser(string input)
        {
            var user = Context.Client.GetUser(ulong.Parse(input));

            await ReplyAsync(user.ToString() + "\n" + MentionUtils.MentionUser(user.Id) + "\n`" + MentionUtils.MentionUser(user.Id) + "`", allowedMentions: new AllowedMentions(Discord.AllowedMentionTypes.None));
        }

        [Command("emote")]
        [Summary("gets an emote using id (takes a while)")]
        public async Task findEmote(string emote)
        {
            foreach (IGuild a in Context.Client.Guilds)
            {
                try
                {
                    var emote2 = await a.GetEmoteAsync(ulong.Parse(emote));
                    await ReplyAsync(emote2.ToString() + "\n emote id: \n`" + emote2.ToString() + "`", allowedMentions: new AllowedMentions(Discord.AllowedMentionTypes.None));
                    return;
                }
                catch
                {
                    continue;
                }
            }
            await ReplyAsync("emote not found");
        }
    }
}
