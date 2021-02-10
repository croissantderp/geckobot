using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace GeckoBot.Commands
{
    public class Emotes : ModuleBase<SocketCommandContext>
    {
        //sends message
        [Command("te")]
        [Summary("Sends a message with words replaced by emotes from the dictionary to the target channel.")]
        public async Task send(string target, string message)
        {
            if (target == "dm")
            {
                await Context.User.SendMessageAsync(Utils.emoteReplace(message));
            }
            else
            {
                //gets current client
                DiscordSocketClient client = Context.Client;

                //parses channel id provided and gets channel from client
                var chnl = client.GetChannel(ulong.Parse(target)) as IMessageChannel;

                await chnl.SendMessageAsync(Context.User + ": " +Utils.emoteReplace(message), allowedMentions: Globals.allowed);
            }
        }

        //simple retrieval function
        [Command("e")]
        [Summary("Sends a message with words replaced by emotes from the dictionary.")]
        public async Task e(string yes)
        {
            await ReplyAsync(Utils.emoteReplace(yes), allowedMentions: Globals.allowed);
        }

        //finds emote
        [Command("ge")]
        [Summary("Looks up a key in the dictionary given the value; the reverse of `e.")]
        public async Task ge(string input)
        {
            Globals.RefreshEmoteDict();
            if (Globals.emoteDict.ContainsValue(input))
            {
                string key = Globals.emoteDict.FirstOrDefault(x => x.Value == input).Key;
                await ReplyAsync(key, allowedMentions: Globals.allowed);
            }
            else
            {
                await ReplyAsync(input + " not found", allowedMentions: Globals.allowed);
            }
        }

        //sends the file for emote storage
        [Command("el")]
        [Summary("Sends the raw emote storage text file.")]
        public async Task el()
        {
            await Context.Channel.SendFileAsync(@"..\..\Cache\gecko2.gek");
        }
        
        //removal function
        [Command("er")]
        [Summary("Removes a key from the dictionary.")]
        public async Task er(string yes1)
        {
            Globals.RefreshEmoteDict();

            //if key is found
            if (Globals.emoteDict.ContainsKey(yes1))
            {
                if (Globals.emoteDict[yes1].Contains("@फΉ̚ᐼㇶ⤊"))
                {
                    await ReplyAsync("that is an admin command and cannot be removed");
                }
                else
                {
                    //removes key
                    Globals.emoteDict.Remove(yes1);

                    //converts dictionary to string and saves
                    FileUtils.Save(Globals.DictToString(Globals.emoteDict, "{0}⁊{1}ҩ"), @"..\..\Cache\gecko2.gek");

                    //adds reaction
                    await Context.Message.AddReactionAsync(new Emoji("✅"));
                }
            }
            else
            {
                //if emote is not found
                await ReplyAsync("emote not found!");
            }
        }

        //save function
        [Command("es")]
        [Summary("Saves a key to the dictionary.")]
        public async Task es(string yes1, string yes)
        {
            Globals.RefreshEmoteDict();

            //if emote name contains banned characters
            string combined = yes + yes1;
            if (Utils.containsForbidden(combined))
            {
                await ReplyAsync("saved things cannot contain 'ҩ' or '⁊'!");
            }
            else
            {
                //if emote dictionary already has a definition for the new key
                if (Globals.emoteDict.ContainsKey(yes1))
                {
                    await ReplyAsync("this name is taken, use a different name!");
                }
                else
                {
                    //removes ::: for animated saving
                    string[] temp = yes.Split(":::");

                    //joins the split string and saves to emote dictionary
                    Globals.emoteDict.Add(yes1, string.Join("", temp.Select(p => p.ToString())));

                    //converts dictionary to string and saves
                    FileUtils.Save(Globals.DictToString(Globals.emoteDict, "{0}⁊{1}ҩ"), @"..\..\Cache\gecko2.gek");

                    //adds reaction
                    await Context.Message.AddReactionAsync(new Emoji("✅"));
                }
            }
        }

        //big save function
        [Command("ess")]
        [Summary("Saves all emotes from all guilds into the dictionary.")]
        public async Task ess()
        {
            Globals.RefreshEmoteDict();
            
            //gets guilds
            IGuild[] guilds = Context.Client.Guilds.ToArray();

            //number of emotes added
            int emotesAdded = 0;

            foreach (IGuild a in guilds)
            {
                //gets every emote in a guild
                IEmote[] b = a.Emotes.ToArray();

                foreach (IEmote c in b)
                {
                    //gets number of total characters in the emote name
                    int count = c.ToString().Length;

                    //shortened name of the emote
                    string name = c.ToString().Remove(count - 20, 20).Remove(0, 2);

                    //if the emote dictionary already contains a key or emote contains banned characters
                    if (!Globals.emoteDict.ContainsKey(name) && !Utils.containsForbidden(name))
                    {
                        //adds to emote dictionary
                        Globals.emoteDict.Add(name, c.ToString());

                        //adds to counter
                        emotesAdded += 1;
                    }
                }
            }

            //converts dictionary to string and saves
            FileUtils.Save(Globals.DictToString(Globals.emoteDict, "{0}⁊{1}ҩ"), @"..\..\Cache\gecko2.gek");

            //replies with number of new emotes added
            await ReplyAsync(emotesAdded.ToString() + " new emotes added");
        }
        
        //emote react function
        [Command("re")]
        [Summary("Reacts to a message with the specified emotes.")]
        public async Task ReactCustomAsync(IMessageChannel channel, string message, string emote)
        {
            Globals.RefreshEmoteDict();
            
            //parses message id provided and gets message from channel
            var message2 = await channel.GetMessageAsync(ulong.Parse(message));

            //splits based on $
            string[] yesnt = emote.Split("$");

            foreach (string em in yesnt)
            {
                //if the emote dictionary contains the key
                if (Globals.emoteDict.ContainsKey(em))
                {
                    var emote2 = Emote.Parse(Globals.emoteDict[em]);
                    await message2.AddReactionAsync(emote2);
                }
                else
                {
                    //trys to parse emotes 2 different ways
                    try
                    {
                        var emote2 = Emote.Parse(em);
                        await message2.AddReactionAsync(emote2);
                    }
                    catch
                    {
                        var emote2 = new Emoji(em);
                        await message2.AddReactionAsync(emote2);
                    }
                }

                //adds check emote after done
                await Context.Message.AddReactionAsync(new Emoji("✅"));
            }
        }

        //admin save function
        [Command("aes")]
        [Summary("Saves a key to the dictionary which cannot be removed by non admins.")]
        public async Task aes(string passcode, string yes1, string yes)
        {
            if (passcode == Top.Secret)
            {
                Globals.RefreshEmoteDict();

                //if emote name contains banned characters
                string combined = yes + yes1;
                if (Utils.containsForbidden(combined))
                {
                    await ReplyAsync("saved things cannot contain 'ҩ' or '⁊'!");
                }
                else
                {
                    //if emote dictionary already has a definition for the new key
                    if (Globals.emoteDict.ContainsKey(yes1))
                    {
                        await ReplyAsync("this name is taken, use a different name!");
                    }
                    else
                    {
                        //removes ::: for animated saving
                        string[] temp = yes.Split(":::");

                        //joins the split string and saves to emote dictionary
                        Globals.emoteDict.Add(yes1, "@फΉ̚ᐼㇶ⤊" + string.Join("", temp.Select(p => p.ToString())));

                        //converts dictionary to string and saves
                        FileUtils.Save(Globals.DictToString(Globals.emoteDict, "{0}⁊{1}ҩ"), @"..\..\Cache\gecko2.gek");

                        //adds reaction
                        await Context.Message.AddReactionAsync(new Emoji("✅"));
                    }
                }
            }
        }

        //admin removal function
        [Command("aer")]
        [Summary("Removes a key from the dictionary with admin access (can remove admin keys).")]
        public async Task aer(string passcode, string yes1)
        {
            if (passcode == Top.Secret)
            {
                Globals.RefreshEmoteDict();

                //if key is found
                if (Globals.emoteDict.ContainsKey(yes1))
                {
                    //removes key
                    Globals.emoteDict.Remove(yes1);

                    //converts dictionary to string and saves
                    FileUtils.Save(Globals.DictToString(Globals.emoteDict, "{0}⁊{1}ҩ"), @"..\..\Cache\gecko2.gek");

                    //adds reaction
                    await Context.Message.AddReactionAsync(new Emoji("✅"));
                }
                else
                {
                    //if emote is not found
                    await ReplyAsync("emote not found!");
                }
            }
        }
    }
}