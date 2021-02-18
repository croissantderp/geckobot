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
    [Summary("gets various stuff")]
    public class get : ModuleBase<SocketCommandContext>
    {
        [Command("channel")]
        [Summary("gets channel id")]
        public async Task getChannel(IChannel channel)
        {
            await ReplyAsync("guild id: `" + (channel as IGuildChannel).GuildId + "`\n" + 
                "channel id: `" + channel.Id.ToString() + "`", allowedMentions: Globals.allowed);
        }

        [Command("message")]
        [Summary("gets ids from a message link")]
        public async Task getMessage(string input)
        {
            input = input.Remove(0, 29);
            string[] final = input.Split("/");

            await ReplyAsync("guild id: `" + final[0] + "`\n" + 
                "channel id: `" + final[1] + "`\n" + 
                "message id: `" + final[2] + "`", allowedMentions: Globals.allowed);
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
            await ReplyAsync("user id: `" + user.Id.ToString() + "`", allowedMentions: new AllowedMentions(Discord.AllowedMentionTypes.None));
        }

        [Command("emote")]
        [Summary("gets info about an emote using name or emote")]
        public async Task getEmote(string emote)
        {
            try
            {
                IEmote emote2 = Emote.Parse(emote);
                
                await ReplyAsync(emote2.ToString() + "\n emote id: \n`" + emote2.ToString() + "`");
            }
            catch
            {
                foreach (IGuild a in Context.Client.Guilds)
                {
                    foreach (IEmote e in a.Emotes)
                    {
                        if (e.Name == emote)
                        {
                            await ReplyAsync(e.ToString() + "\n emote id: \n`" + e.ToString() + "`");
                            return;
                        }
                    }
                }
                await ReplyAsync("emote not found");
            }
        }

        [Command("all emote")]
        [Summary("gets all emotes organized into pages of 5")]
        public async Task getAllEmote(string emote, int page)
        {
            List<string> final = new ();

            int counter = 0;

            int pageCounter = 1;

            final.Add("page " + page + " of results for " + emote + "\n");

            foreach (IGuild a in Context.Client.Guilds)
            {
                foreach (IEmote e in a.Emotes)
                {
                    if (e.Name == emote)
                    {
                        if (pageCounter == page)
                        {
                            final.Add(e.ToString() + " `" + e.ToString() + "`\n");
                        }
                        if (counter >= 5)
                        {
                            pageCounter++;
                            counter = 0;
                        }
                        counter++;
                    }
                }
            }
            if (final.Count > 1)
            {
                await ReplyAsync(string.Join("", final));
            }
            else
            {
                await ReplyAsync("emote not found or no more results");
            }
        }
    }

    [Group("find")]
    [Summary("finds various stuff")]
    public class find : ModuleBase<SocketCommandContext>
    {
        [Command("channel")]
        [Summary("finds channel from an id")]
        public async Task findChannel(string input)
        {
            await ReplyAsync("<#" + input + ">", allowedMentions: Globals.allowed);
        }

        [Command("message")]
        [Summary("generates a message link from given ids")]
        public async Task findMessage(string input, string input2)
        {
            var channel2 = Context.Client.GetChannel(ulong.Parse(input)) as IGuildChannel;

            await ReplyAsync("https://discord.com/channels/" + channel2.GuildId + "/" + input + "/" + input2, allowedMentions: Globals.allowed);
        }

        [Command("message content")]
        [Summary("gets ids from channel adn message ids")]
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

            await ReplyAsync(user.ToString() + " " + MentionUtils.MentionUser(user.Id), allowedMentions: new AllowedMentions(Discord.AllowedMentionTypes.None));
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
                    await ReplyAsync(emote2.ToString() + "\n emote id: \n`" + emote2.ToString() + "`");
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
