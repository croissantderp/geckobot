using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GeckoBot.Preconditions;
using System.Linq;
using System.Threading.Tasks;

namespace GeckoBot.Commands
{
    [Summary("Cross server emote commands.")]
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
            await EmoteRemove(yes1, false);
        }

        //save function
        [Command("es")]
        [Summary("Saves a key to the dictionary.")]
        public async Task es(string yes1, string yes)
        {
            await EmoteSave(yes1, yes, false);
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
                    if (!Globals.emoteDict.ContainsKey(name))
                    {
                        string cstring = c.ToString();
                        //escapes forbidden
                        name = Utils.escapeforbidden(name);
                        cstring = Utils.escapeforbidden(cstring);

                        //adds to emote dictionary
                        Globals.emoteDict.Add(name, cstring);

                        //adds to counter
                        emotesAdded += 1;
                    }
                }
            }

            //converts dictionary to string and saves
            FileUtils.Save(Globals.DictToString(Globals.emoteDict, "{0} ⁊ {1} ҩ "), @"..\..\Cache\gecko2.gek");

            //replies with number of new emotes added
            await ReplyAsync(emotesAdded + " new emotes added");
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
                    //tries to parse emotes 2 different ways
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
        [RequireGeckobotAdmin]
        [Command("aes")]
        [Summary("Saves a key to the dictionary which cannot be removed by non admins.")]
        public async Task aes(string yes1, string yes)
        {
            await EmoteSave(yes1, yes, true);
        }

        //admin removal function
        [RequireGeckobotAdmin]
        [Command("aer")]
        [Summary("Removes a key from the dictionary with admin access (can remove admin keys).")]
        public async Task aer(string yes1)
        {
            await EmoteRemove(yes1, true);
        }
        
        
        // Saves an emote to the dictionary
        private async Task EmoteSave(string key, string value, bool withAdmin)
        {
            Globals.RefreshEmoteDict();

            //if emote dictionary already has a definition for the new key
            if (Globals.emoteDict.ContainsKey(key))
            {
                await ReplyAsync("this name is taken, use a different name!");
            }
            else
            {
                //escapes forbidden
                key = Utils.escapeforbidden(key);
                value = Utils.escapeforbidden(value);

                //removes ::: for animated saving
                string[] temp = System.Text.RegularExpressions.Regex.Split(value, @"(?<!\\)\:::");

                //joins the split string and saves to emote dictionary
                Globals.emoteDict.Add(
                    value, 
                    (withAdmin ? "@फΉ̚ᐼㇶ⤊" : "") + string.Join("", temp.Select(p => p.ToString())));

                //converts dictionary to string and saves
                FileUtils.Save(Globals.DictToString(Globals.emoteDict, "{0} ⁊ {1} ҩ "), @"..\..\Cache\gecko2.gek");

                //adds reaction
                await Context.Message.AddReactionAsync(new Emoji("✅"));
            }
        }
        
        // Removes an emote from the dictionary
        private async Task EmoteRemove(string key, bool withAdmin)
        {
            Globals.RefreshEmoteDict();

            //if key is found
            if (Globals.emoteDict.ContainsKey(key))
            {
                // If the key is an admin key
                if (Globals.emoteDict[key].Contains("@फΉ̚ᐼㇶ⤊") && !withAdmin)
                {
                    await ReplyAsync("that is an admin command and cannot be removed");
                    return;
                }
                
                //removes key
                Globals.emoteDict.Remove(key);

                //converts dictionary to string and saves
                FileUtils.Save(Globals.DictToString(Globals.emoteDict, "{0} ⁊ {1} ҩ "), @"..\..\Cache\gecko2.gek");

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