using System.Collections.Generic;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GeckoBot.Preconditions;
using System.Linq;
using System.Threading.Tasks;
using GeckoBot.Utils;

namespace GeckoBot.Commands
{
    [Summary("Cross server emote commands.")]
    public class Emotes : ModuleBase<SocketCommandContext>
    {
        //emote dictionary
        public static Dictionary<string, string> EmoteDict = new();

        //emote dictionary
        public static List<string> cooldown = new();

        //sends message
        [Command("te")]
        [Summary("Sends a message with words replaced by emotes from the dictionary to the target channel.")]
        public async Task send([Summary("The channel id, or user id prefaced by 'dm'")] string target, [Summary("The message content.")] [Remainder]string message)
        {
            if (target.Contains("dm"))
            {
                if (cooldown.Contains(target.Remove(0, 2) + Context.User.Id.ToString()))
                {
                    await ReplyAsync("this user is still on cooldown!");
                    return;
                }

                var user = Context.Client.GetUser(ulong.Parse(target.Remove(0, 2)));
                await user.SendMessageAsync(Context.User + ": " + EmoteUtils.emoteReplace(message), allowedMentions: Globals.allowed);
                
                cooldown.Add(target.Remove(0, 2) + Context.User.Id.ToString());

                //starts a timer with desired amount of time
                System.Timers.Timer t = new(3600000);
                t.Elapsed += (sender, e) => cooldownUp(target.Remove(0, 2) + Context.User.Id.ToString(), t);
                t.Start();
            }
            else
            {
                //gets current client
                DiscordSocketClient client = Context.Client;

                //parses channel id provided and gets channel from client
                var chnl = client.GetChannel(ulong.Parse(target)) as IMessageChannel;

                await chnl.SendMessageAsync(Context.User + ": " + EmoteUtils.emoteReplace(message), allowedMentions: Globals.allowed);
            }

            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

        //the task that is activated when time is up
        private void cooldownUp(string key, System.Timers.Timer timer2)
        {
            cooldown.Remove(key);

            //stops timer
            timer2.Stop();
        }

        //sends message
        [RequireGeckobotAdmin]
        [Command("ate")]
        [Summary("Sends a message with words replaced by emotes from the dictionary to the target channel but is anonymous")]
        public async Task asend([Summary("The channel id, or user id prefaced by 'dm'")] string target, [Summary("The message content.")][Remainder] string message)
        {
            if (target.Contains("dm"))
            {
                var user = Context.Client.GetUser(ulong.Parse(target.Remove(0,2)));
                await user.SendMessageAsync(EmoteUtils.emoteReplace(message), allowedMentions: Globals.allowed);
            }
            else
            {
                //gets current client
                DiscordSocketClient client = Context.Client;

                //parses channel id provided and gets channel from client
                var chnl = client.GetChannel(ulong.Parse(target)) as IMessageChannel;

                await chnl.SendMessageAsync(EmoteUtils.emoteReplace(message), allowedMentions: Globals.allowed);
            }
        }

        //simple retrieval function
        [Command("e")]
        [Summary("Sends a message with words replaced by emotes from the dictionary.")]
        public async Task e([Summary("The message content.")] [Remainder]string yes)
        {
            await ReplyAsync(EmoteUtils.emoteReplace(yes), allowedMentions: Globals.allowed);
        }

        //finds emote
        [Command("ge")]
        [Summary("Looks up a key in the dictionary given the value; the reverse of `e.")]
        public async Task ge([Summary("The value to get the key for.")] [Remainder]string input)
        {
            EmoteUtils.RefreshEmoteDict();
            if (EmoteDict.ContainsValue(input))
            {
                string key = EmoteDict.FirstOrDefault(x => x.Value == input).Key;
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

        //sends the file for emote storage
        [Command("ea")]
        [Summary("gets length of emote list")]
        public async Task ea()
        {
            EmoteUtils.RefreshEmoteDict();
            await ReplyAsync("there are " + EmoteDict.Count.ToString() + " objects stored");
        }

        //removal function
        [Command("er")]
        [Summary("Removes a key from the dictionary.")]
        public async Task er([Summary("The key to remove.")] [Remainder]string yes1)
        {
            await EmoteRemove(yes1, false);
        }

        //save function
        [Command("es")]
        [Summary("Saves a key to the dictionary.")]
        public async Task es([Summary("The key of the value.")] string yes1, [Summary("The value to add.")] [Remainder]string yes)
        {
            await EmoteSave(yes1, yes, false);
        }

        //big save function
        [Command("ess")]
        [Summary("Saves all emotes from all guilds into the dictionary.")]
        public async Task ess()
        {
            EmoteUtils.RefreshEmoteDict();
            
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
                    bool isAnim = System.Text.RegularExpressions.Regex.IsMatch(c.ToString(), @"^<a:");
                    
                    //gets number of total characters in the emote name
                    int count = c.ToString().Length;

                    //shortened name of the emote
                    string name = c.ToString().Remove(count - 20, 20).Remove(0, (isAnim ? 3 : 2));

                    //if the emote dictionary already contains a key or emote contains banned characters
                    if (!EmoteDict.ContainsKey(name))
                    {
                        string cstring = c.ToString();
                        //escapes forbidden
                        name = EmoteUtils.escapeforbidden(name);
                        cstring = EmoteUtils.escapeforbidden(cstring);

                        //adds to emote dictionary
                        EmoteDict.Add(name, cstring);

                        //adds to counter
                        emotesAdded++;
                    }
                }
            }
            
            //converts dictionary to string and saves
            FileUtils.Save(Globals.DictToString(EmoteDict, "{0} ⁊ {1} ҩ "), @"..\..\Cache\gecko2.gek");

            //replies with number of new emotes added
            await ReplyAsync(emotesAdded + " new emotes added");
        }
        
        //emote react function
        [Command("re")]
        [Summary("Reacts to a message with the specified emotes.")]
        public async Task ReactCustomAsync([Summary("The emotes to react with, seperated by '$'")] string emote, [Summary("First input, either link or channel id.")] string input, [Summary("Second input, message id or leave blank for link.")] string input2 = null)
        {
            EmoteUtils.RefreshEmoteDict();

            string channel = "";
            string message = "";

            if (input2 == null)
            {
                input = input.Remove(0, 8);
                string[] final = input.Split("/");

                channel = final[3];
                message = final[4];
            }
            else
            {
                channel = input;
                message = input2;
            }

            //parses message id provided and gets message from channel
            var message2 = await (Context.Client.GetChannel(ulong.Parse(channel)) as IMessageChannel).GetMessageAsync(ulong.Parse(message));

            //splits based on $
            string[] yesnt = emote.Split("$");

            foreach (string em in yesnt)
            {
                //if the emote dictionary contains the key
                if (EmoteDict.ContainsKey(em))
                {
                    var emote2 = Emote.Parse(EmoteDict[em]);
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
        public async Task aes([Summary("The key of the value.")] string yes1, [Summary("The value to add.")] string yes)
        {
            await EmoteSave(yes1, yes, true);
        }

        //admin removal function
        [RequireGeckobotAdmin]
        [Command("aer")]
        [Summary("Removes a key from the dictionary with admin access (can remove admin keys).")]
        public async Task aer([Summary("The key to remove.")] string yes1)
        {
            await EmoteRemove(yes1, true);
        }
        
        
        // Saves an emote to the dictionary
        private async Task EmoteSave(string key, string value, bool withAdmin)
        {
            EmoteUtils.RefreshEmoteDict();

            //if emote dictionary already has a definition for the new key
            if (EmoteDict.ContainsKey(key))
            {
                await ReplyAsync("this name is taken, use a different name!");
            }
            else
            {
                //escapes forbidden
                key = EmoteUtils.escapeforbidden(key);
                value = EmoteUtils.escapeforbidden(value);

                //removes ::: for animated saving
                string[] temp = System.Text.RegularExpressions.Regex.Split(value, @"(?<!\\)\:::");

                //joins the split string and saves to emote dictionary
                EmoteDict.Add(
                    key, 
                    (withAdmin ? "@फΉ̚ᐼㇶ⤊" : "") + string.Join("", temp.Select(p => p.ToString())));

                //converts dictionary to string and saves
                FileUtils.Save(Globals.DictToString(EmoteDict, "{0} ⁊ {1} ҩ "), @"..\..\Cache\gecko2.gek");

                //adds reaction
                await Context.Message.AddReactionAsync(new Emoji("✅"));
            }
        }
        
        // Removes an emote from the dictionary
        private async Task EmoteRemove(string key, bool withAdmin)
        {
            EmoteUtils.RefreshEmoteDict();

            //if key is found
            if (EmoteDict.ContainsKey(key))
            {
                // If the key is an admin key
                if (EmoteDict[key].Contains("@फΉ̚ᐼㇶ⤊") && !withAdmin)
                {
                    await ReplyAsync("that is an admin command and cannot be removed");
                    return;
                }
                
                //removes key
                EmoteDict.Remove(key);

                //converts dictionary to string and saves
                FileUtils.Save(Globals.DictToString(EmoteDict, "{0} ⁊ {1} ҩ "), @"..\..\Cache\gecko2.gek");

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