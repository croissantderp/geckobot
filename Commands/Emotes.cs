using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace GeckoBot.Commands
{
    public class Emotes : ModuleBase<SocketCommandContext>
    {
        //simple retrival function
        [Command("e")]
        public async Task e(string yes)
        {
            await ReplyAsync(Utils.emoteReplace(yes));
        }
        
        //removal function
        [Command("er")]
        public async Task er(string yes1)
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
        
        //save function
        [Command("es")]
        public async Task es(string yes1, string yes)
        {
            Globals.RefreshEmoteDict();

            //if emote name contains banned characters
            string combined = yes + yes1;
            if (combined.Contains("⁊") || combined.Contains("ҩ"))
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
        public async Task ess()
        {
            Globals.RefreshEmoteDict();

            //gets client
            IDiscordClient client = Context.Client;
            
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
                    if (!Globals.emoteDict.ContainsKey(name) && !name.Contains("ҩ") && !name.Contains("⁊"))
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
        public async Task ReactCustomAsync(string channel, string message, string emote)
        {
            Globals.RefreshEmoteDict();

            //gets current client
            DiscordSocketClient client = Context.Client;

            //parses channel id provided and gets channel from client
            var chnl = client.GetChannel(ulong.Parse(channel)) as IMessageChannel;

            //parses message id provided and gets message from channel
            var message2 = await chnl.GetMessageAsync(ulong.Parse(message));

            //splits based on $
            string[] yesnt = emote.Split("$");

            for (int i = 0; i < yesnt.Length; i++)
            {
                //if the emote dictionary contains the key
                if (Globals.emoteDict.ContainsKey(yesnt[i]))
                {
                    var emote2 = Emote.Parse(Globals.emoteDict[yesnt[i]]);
                    await message2.AddReactionAsync(emote2);
                }
                else
                {
                    //trys to parse emotes 2 different ways
                    try
                    {
                        var emote2 = Emote.Parse(yesnt[i]);
                        await message2.AddReactionAsync(emote2);
                    }
                    catch
                    {
                        var emote2 = new Emoji(yesnt[i]);
                        await message2.AddReactionAsync(emote2);
                    }
                }

                //adds check emote after done
                await Context.Message.AddReactionAsync(new Emoji("✅"));
            }
        }
    }
}