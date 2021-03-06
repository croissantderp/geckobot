﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using GeckoBot.Utils;

namespace GeckoBot.Commands
{
    //in general all these commands get an object either by name or id then format and send a message based on it

    [Group("get")]
    [Summary("Gets information about things you are stalking")]
    public class get : ModuleBase<SocketCommandContext>
    {
        [Command("guild")]
        [Summary("Gets guild id.")]
        public async Task getGuild([Summary("A guild.")][Remainder] string input)
        {
            var guild = Context.Client.Guilds.Where(a => a.Name.Equals(input, StringComparison.InvariantCultureIgnoreCase)).First();
            
            await ReplyAsync("guild: `" + guild.Name + "`, id: `" + guild.Id + "`");
        }

        [Command("channel")]
        [Summary("gets channel id")]
        public async Task getChannel([Summary("A channel, either name or the #channel of it.")][Remainder] IChannel channel)
        {
            await ReplyAsync("guild: `"+ (channel as IGuildChannel).Guild.Name + "`, id: `" + (channel as IGuildChannel).GuildId + "`\n" +
                "channel : `" + channel.Name + "`, id: `<#" + channel.Id + ">`", allowedMentions: Globals.notAllowed);
        }

        [Command("message")]
        [Summary("gets ids from a message link")]
        public async Task getMessage([Summary("A message link.")] string input)
        {
            input = input.Remove(0, 8);
            string[] final = input.Split("/");

            var channel = Context.Client.GetChannel(ulong.Parse(final[3])) as IMessageChannel;
            var message = await channel.GetMessageAsync(ulong.Parse(final[4])) as IUserMessage;
            var user = message.Author;

            await ReplyAsync("guild: `" + (channel as IGuildChannel).Guild.Name + "`, id: `" + final[2] + "`\n" + 
                "channel: `" + channel.Name + "`, id: `<#" + final[3] + ">`\n" +
                "message: \"" + (message.Content.Length > 20 ? message.Content.Remove(20) + "..." : message.Content).Replace("`", "\\`") + "\", id: `" + final[4] + "`\n" +
                "author: " + MentionUtils.MentionUser(user.Id) + ", id: `<@" + user.Id + ">`", allowedMentions: Globals.notAllowed);
        }

        [Command("message content")]
        [Summary("gets content from a message link")]
        public async Task getMessageContent(string input)
        {
            input = input.Remove(0, 8);
            string[] final = input.Split("/");

            var channel = Context.Client.GetChannel(ulong.Parse(final[3])) as IMessageChannel;

            var message = await channel.GetMessageAsync(ulong.Parse(final[4]));

            //embed
            var embed = new EmbedBuilder
            {
                Description = message.Content,
                Author = new EmbedAuthorBuilder().WithName(message.Author.ToString() + " (" + message.Author.Id + ")").WithIconUrl(message.Author.GetAvatarUrl())
            };

            if (message.Attachments.Count != 0)
            {
                embed.WithImageUrl(message.Attachments.First().Url);
            }

            embed.WithTimestamp(message.Timestamp);

            embed.WithColor(180, 212, 85);

            await ReplyAsync("", embed: embed.Build(), allowedMentions: Globals.notAllowed);
        }

        [Command("user")]
        [Summary("gets info about a user using a mention")]
        public async Task getUser([Summary("A @ mention")][Remainder] IUser user)
        {
            await ReplyAsync("user: " + MentionUtils.MentionUser(user.Id) + ", id: `<@" + user.Id + ">`\n" + user.GetAvatarUrl(), allowedMentions: Globals.notAllowed);
        }

        [Command("emote")]
        [Summary("gets info about an emote using name or emote")]
        public async Task getEmote([Remainder] string emote)
        {
            try
            {
                IEmote emote2 = Emote.Parse(emote);
                
                await ReplyAsync("guild: `" + (emote2 as IGuild).Name + "`, id: `" + (emote2 as IGuild).Id + "`\n" + 
                    "emote: `" + emote2 + " `id: `" + emote2 + "`", allowedMentions: Globals.notAllowed);
            }
            catch
            {
                foreach (IGuild a in Context.Client.Guilds)
                {
                    foreach (IEmote e in a.Emotes)
                    {
                        if (Globals.FuzzyMatch(e.Name, emote, out int value))
                        {
                            await ReplyAsync("guild: `" + a.Name + "`, id: `" + a.Id + "`\n" +
                                "emote: " + e + " `id: " + e + "`", allowedMentions: Globals.notAllowed);
                            return;
                        }
                    }
                }
                await ReplyAsync("emote not found", allowedMentions: Globals.notAllowed);
            }
        }

        [Command("all emote")]
        [Summary("gets all emotes organized into pages of 5")]
        public async Task getAllEmote([Summary("The emote name.")] string emote, [Summary("The page of results.")] int page = 1)
        {
            List<(string, int)> final = new ();

            int counter = 0;

            int total = 0;

            int pageCounter = 1;

            foreach (IGuild a in Context.Client.Guilds)
            {
                foreach (IEmote e in a.Emotes)
                {
                    if (Globals.FuzzyMatch(e.Name, emote, out int value))
                    {
                        if (counter >= 5)
                        {
                            pageCounter++;
                            counter = 0;
                        }

                        final.Add((e + ":\n`" + e.ToString() + "`\n", value));

                        counter++;
                        total++;
                    }
                }
            }

            List<string> finalfinal = final.OrderByDescending(a => a.Item2).Select(a => a.Item1).ToList().GetRange((page-1)*5, page != pageCounter ? 5 : ((total % 5) == 0 ? 5 : (total % 5)));
            finalfinal.Insert(0, "page " + page + " of " + pageCounter + " of results for " + emote + " (result " + (page * 5 - 4) + " - " + (page != pageCounter ? (page * 5) : page * 5 - 5 + ((total % 5) == 0 ? 5 : (total % 5))) + " of " + total + ")" + "\n");
            
            if (final.Count > 0)
            {
                await ReplyAsync(string.Join("", finalfinal), allowedMentions: Globals.notAllowed);
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
        [Command("guild")]
        [Summary("Gets guild via id.")]
        public async Task getGuild([Summary("A guild id.")][Remainder] string input)
        {
            var guild = Context.Client.GetGuild(ulong.Parse(input));
            await ReplyAsync("guild: `" + guild.Name + "`, id: `" + guild.Id + "`");
        }

        [Command("channel")]
        [Summary("finds channel from an id")]
        public async Task findChannel([Summary("The channel id.")] string input)
        {
            var channel = Context.Client.GetChannel(ulong.Parse(input)) as IGuildChannel;
            await ReplyAsync("guild: `" + channel.Guild.Name + "`, id: `" + (channel as IGuildChannel).GuildId + "`\n" +
                "channel : `" + channel.Name + "`, id: `<#" + channel.Id + ">`", allowedMentions: Globals.notAllowed);
        }

        [Command("message")]
        [Summary("generates a message link from given ids")]
        public async Task findMessage([Summary("The channel id of the message.")] string input, [Summary("The message id")] string input2)
        {
            var channel2 = Context.Client.GetChannel(ulong.Parse(input)) as IGuildChannel;
            var message = await (channel2 as IMessageChannel).GetMessageAsync(ulong.Parse(input2));
            var user = message.Author;

            await ReplyAsync("guild: `" + channel2.Guild.Name + ", id: " + channel2.Guild.Id + "`\n" +
                "channel: `" + channel2.Name + ", id: <#" + channel2.Id + ">`\n" +
                "message: \"" + (message.Content.Length > 20 ? message.Content.Remove(20) + "..." : message.Content).Replace("`", "\\`") + "\", id: `" + message.Id + "`https://discord.com/channels/" + channel2.GuildId + "/" + input + "/" + input2 + "\n" +
                "author: " + MentionUtils.MentionUser(user.Id) + ", id: `<@" + user.Id + ">`", allowedMentions: Globals.notAllowed);
        }

        [Command("message content")]
        [Summary("gets ids from channel and message ids")]
        public async Task findMessageContent([Summary("The channel id of the message.")] string input, [Summary("The message id")] string input2)
        {
            var channel = Context.Client.GetChannel(ulong.Parse(input)) as IMessageChannel;

            var message = await channel.GetMessageAsync(ulong.Parse(input2));

            //embed
            var embed = new EmbedBuilder
            {
                Description = message.Content,
                Author = new EmbedAuthorBuilder().WithName(message.Author.ToString() + " (" + message.Author.Id + ")").WithIconUrl(message.Author.GetAvatarUrl())
            };

            if (message.Attachments.Count != 0)
            {
                embed.WithImageUrl(message.Attachments.First().Url);
            }

            embed.WithTimestamp(message.Timestamp);

            embed.WithColor(180, 212, 85);

            await ReplyAsync("", embed: embed.Build(), allowedMentions: Globals.notAllowed);
        }

        [Command("user")]
        [Summary("gets info about a user using id")]
        public async Task findUser([Summary("The user id.")] string input)
        {
            var user = Context.Client.GetUser(ulong.Parse(input));

            await ReplyAsync("user: " + MentionUtils.MentionUser(user.Id) + ", id: `<@" + user.Id + ">`\n" + user.GetAvatarUrl(), allowedMentions: new AllowedMentions(Discord.AllowedMentionTypes.None));
        }

        [Command("emote")]
        [Summary("gets an emote using id (for some reason discord's copy emote id is wack so the only way to obtain the emote id is through the escaped string)")]
        public async Task findEmote([Summary("The emote id.")] string emote)
        {
            List<(ulong, ulong)> emotes = new List<(ulong, ulong)>();

            foreach (IGuild b in Context.Client.Guilds)
            {
                emotes.AddRange(b.Emotes.Select(a => (a.Id, b.Id)));
            }

            Dictionary<ulong, ulong> emotes2 = emotes.ToDictionary(a => a.Item1, b => b.Item2);
            Console.WriteLine(emotes2.First());
            if (emotes2.ContainsKey(ulong.Parse(emote)))
            {
                var guild = Context.Client.GetGuild(emotes2[ulong.Parse(emote)]);

                var emote2 = await guild.GetEmoteAsync(ulong.Parse(emote));
                await ReplyAsync("guild: `" + guild.Name + "`, id: `" + guild.Id + "`\n" +
                    "emote: " + emote2 + " `id: " + emote2 + "`", allowedMentions: new AllowedMentions(AllowedMentionTypes.None));
            }
            else
            {
                await ReplyAsync("emote not found");
            }
        }
    }
}
