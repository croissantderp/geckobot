using System.Collections.Generic;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GeckoBot.Preconditions;
using System.Linq;
using System.Threading.Tasks;
using GeckoBot.Utils;
using System;
using System.Text.RegularExpressions;

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
        [Summary("Sends a message with words replaced by emotes from the dictionary to the target channel. DMs have a 1 hour cooldown.")]
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
                var chnl = client.GetChannel(ulong.Parse(target));
                
                var user = await (chnl as IGuildChannel).Guild.GetUserAsync(Context.User.Id);

                if (!user.GetPermissions(chnl as IGuildChannel).ViewChannel || !user.GetPermissions(chnl as IGuildChannel).SendMessages)
                {
                    await ReplyAsync("You are missing permission!");
                    return;
                }

                await (chnl as IMessageChannel).SendMessageAsync(Context.User + ": " + EmoteUtils.emoteReplace(message), allowedMentions: Globals.allowed);
            }

            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

        //the task that is activated when time is up
        private void cooldownUp(string key, System.Timers.Timer timer2)
        {
            cooldown.Remove(key);

            //stops timer
            timer2.Close();
        }

        //replies to message
        [Command("me")]
        [Summary("Sends a message with words replaced by emotes from the dictionary in a reply to a message")]
        public async Task mention([Summary("The message content.")] string message, [Summary("url or channel id")] string target = null, [Summary("message id")] string target2 = null)
        {
            string[] temp = (await Globals.getIds(target, target2, Context)).Split("$");

            //gets current client
            DiscordSocketClient client = Context.Client;
            
            //parses channel id provided and gets channel from client
            var chnl = client.GetChannel(ulong.Parse(temp[0])) as IMessageChannel;
            var message2 = await chnl.GetMessageAsync(ulong.Parse(temp[1])) as IUserMessage;

            var user = await (chnl as IGuildChannel).Guild.GetUserAsync(Context.User.Id);

            if (!user.GetPermissions(chnl as IGuildChannel).ViewChannel || !user.GetPermissions(chnl as IGuildChannel).SendMessages)
            {
                await ReplyAsync("You are missing permission!");
                return;
            }

            await message2.ReplyAsync(Context.User + ": " + EmoteUtils.emoteReplace(message), allowedMentions: Globals.allowed);

            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

        //replies to message
        [Command("ame")]
        [Summary("Sends a message with words replaced by emotes from the dictionary in a reply to a message but admin only")]
        public async Task amention([Summary("The message content.")] string message, [Summary("url or channel id")] string target = null, [Summary("message id")] string target2 = null)
        {
            string[] temp = (await Globals.getIds(target, target2, Context)).Split("$");

            //gets current client
            DiscordSocketClient client = Context.Client;

            //parses channel id provided and gets channel from client
            var chnl = client.GetChannel(ulong.Parse(temp[0])) as IMessageChannel;
            var message2 = await chnl.GetMessageAsync(ulong.Parse(temp[1])) as IUserMessage;

            await message2.ReplyAsync(EmoteUtils.emoteReplace(message), allowedMentions: Globals.allowed);

            await Context.Message.AddReactionAsync(new Emoji("✅"));
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

            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

        //simple retrieval function
        [Command("e")]
        [Summary("Sends a message with words replaced by emotes from the dictionary.")]
        public async Task e([Summary("The message content.")] [Remainder]string yes)
        {
            await ReplyAsync(EmoteUtils.emoteReplace(yes), allowedMentions: Globals.allowed);
        }

        [Command("se")]
        [Summary("Looks up data in the dictionary using fuzzy search.")]
        public async Task se([Summary("The input to search for")] string yes, [Summary("The result number.")] int index = 1)
        {
            EmoteUtils.RefreshEmoteDict();
            int tempInt = 0;
            IEnumerable<KeyValuePair<string, string>> temp = EmoteDict.Where(a => Globals.FuzzyMatch(a.Key, yes, out tempInt) || Globals.FuzzyMatch(a.Value, yes, out tempInt)).OrderByDescending(a => Globals.FuzzyMatchScore(a.Key + a.Value, yes)); //a.Value.Contains(input)
            IEnumerable<KeyValuePair<string, string>> temp2 = temp.Take(index);
            string final = temp2.Last().Key;

            await ReplyAsync($"result {temp2.Count()} of {temp.Count()}, score: {Globals.FuzzyMatchScore(final + EmoteDict[final], yes)}, key: " + final + ", Value: " + EmoteDict[final], allowedMentions: Globals.allowed);
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

        [Command("ee")]
        [Summary("Edits a key's value in the dictionary.")]
        public async Task ee([Summary("The key of the value.")] string yes1, [Summary("The value to change the previous value to.")][Remainder] string yes)
        {
            await EmoteRemove(yes1, false);
            await EmoteSave(yes1, yes, false);
        }

        [Command("en")]
        [Summary("Edits a key in the dictionary while maintaining the value.")]
        public async Task en([Summary("The key of the value.")] string yes1, [Summary("The new key to change the previous key to.")][Remainder] string yes)
        {
            string temp = EmoteDict[yes1];
            await EmoteRemove(yes1, false);
            await EmoteSave(yes, temp, false);
        }

        //big save function
        [Command("ess")]
        [Summary("Saves all emotes from all guilds into the dictionary. Geckobot will add repeat emotes with a '-(number)' at the end.")]
        public async Task ess()
        {
            EmoteUtils.RefreshEmoteDict();
            
            //gets guilds
            IGuild[] guilds = Context.Client.Guilds.ToArray();

            //number of emotes added
            int emotesAdded = 0;
            int emotesRemoved = 0;

            List<string> emotes = new List<string>();

            foreach (IGuild b in Context.Client.Guilds)
            {
                emotes.AddRange(b.Emotes.Select(a => a.ToString()));
            }

            Regex eregex = new Regex(@"^\<a?\:\w*\:\d{18}\>$");

            var matchedValues = EmoteDict.Where(a => eregex.IsMatch(a.Value)).Where(a => !emotes.Contains(a.Value)).Select(a => a.Key);

            foreach(string key in matchedValues)
            {
                EmoteDict.Remove(key);
                emotesRemoved++;
            }

            foreach (IGuild a in guilds)
            {
                //gets every emote in a guild
                IEmote[] b = a.Emotes.ToArray();

                foreach (IEmote c in b)
                {
                    bool isAnim = Regex.IsMatch(c.ToString(), @"^<a:");
                    
                    //gets number of total characters in the emote name
                    int count = c.ToString().Length;

                    //shortened name of the emote
                    string name = c.ToString().Remove(count - 20, 20).Remove(0, (isAnim ? 3 : 2));

                    string cstring = c.ToString();

                    Regex preRegex = new Regex(@"^" + name + @"(|-)\d*");

                    //if the emote dictionary already contains a key
                    if (!EmoteDict.ContainsKey(name))
                    {
                        //escapes forbidden
                        name = EmoteUtils.escapeforbidden(name);
                        cstring = EmoteUtils.escapeforbidden(cstring);

                        //adds to emote dictionary
                        EmoteDict.Add(name, cstring);

                        //adds to counter
                        emotesAdded++;
                    }
                    else if (EmoteDict.Keys.Where(a => preRegex.IsMatch(a)).All(a => EmoteDict[a] != cstring))
                    {
                        //escapes forbidden
                        name = EmoteUtils.escapeforbidden(name);
                        cstring = EmoteUtils.escapeforbidden(cstring);

                        Regex regex = new Regex(@"^" + name + @"-\d+$");

                        var arrayThing = EmoteDict.Keys.Where(a => regex.IsMatch(a));

                        var arrayThingButKeyValuePair = arrayThing.Select(a => new KeyValuePair<string, string>(a,EmoteDict[a]));

                        var duplicateKeys = arrayThingButKeyValuePair.GroupBy(s => s.Value).SelectMany(grp => grp.Skip(1)).Distinct().Select(a => a.Key);

                        foreach (string key in duplicateKeys)
                        {
                            Console.WriteLine(key);
                            EmoteDict.Remove(key);
                            emotesRemoved++;
                        }

                        arrayThing = EmoteDict.Keys.Where(a => regex.IsMatch(a));

                        int offset = 0;

                        if (arrayThing.Count() != 0)
                        {
                            var numArray = arrayThing.Select(a => a.Split("-").Last()).OrderBy(a => int.Parse(a)).Select(a => int.Parse(a));

                            foreach (int d in numArray)
                            {
                                if (!numArray.Contains(d + 1))
                                {
                                    offset = d;
                                    break;
                                }
                            }
                        }
                        //adds to emote dictionary
                        EmoteDict.Add(name + "-" + (offset + 1).ToString(), cstring);

                        //adds to counter
                        emotesAdded++;
                    }
                }
            }
            
            //converts dictionary to string and saves
            FileUtils.Save(Globals.DictToString(EmoteDict, "{0} ⁊ {1} ҩ "), @"..\..\Cache\gecko2.gek");

            //replies with number of new emotes added
            await ReplyAsync(emotesAdded + " new emotes added, " + emotesRemoved + " emotes removed");
        }
        
        //emote react function
        [Command("re")]
        [Summary("Reacts to a message with the specified emotes.")]
        public async Task ReactCustomAsync([Summary("The emotes to react with, seperated by '$'")] string emote, [Summary("First input, either link or channel id.")] string input = null, [Summary("Second input, message id or leave blank for link.")] string input2 = null)
        {
            EmoteUtils.RefreshEmoteDict();

            string[] ids = (await Globals.getIds(input, input2, Context)).ToString().Split("$");
            string channel = ids[0];
            string message = ids[1];

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

        [RequireGeckobotAdmin]
        [Command("aee")]
        [Summary("Edits a key's value in the dictionary.")]
        public async Task aee([Summary("New text for the message.")] string yes1, [Summary("First input, either link or channel id.")] string input = null, [Summary("Second input, message id or leave blank for link.")] string input2 = null)
        {
            string[] ids = (await Globals.getIds(input, input2, Context)).ToString().Split("$");
            string channel = ids[0];
            string message = ids[1];

            //parses message id provided and gets message from channel
            var message2 = await (Context.Client.GetChannel(ulong.Parse(channel)) as IMessageChannel).GetMessageAsync(ulong.Parse(message)) as IUserMessage;

            if (message2.Author.Id != Context.Client.CurrentUser.Id)
            {
                await ReplyAsync("cannot edit message not sent by geckobot.");
                return;
            }

            await message2.ModifyAsync(a => a.Content = yes1);

            //adds check emote after done
            await Context.Message.AddReactionAsync(new Emoji("✅"));
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