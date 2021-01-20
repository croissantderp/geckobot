using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace GeckoBot.Commands
{
    public class Admin : ModuleBase<SocketCommandContext>
    {
        //termination command
        [Command("terminate")]
        public async Task terminate(string passcode)
        {
            //if password is correct
            if (passcode == Top.Secret)
            {
                await ReplyAsync("terminating...");
                System.Environment.Exit(0);
            }
        }
        
        //temporary command to set name
        [Command("set")]
        public async Task set(string password, int value)
        {
            if (password == Top.Secret)
            {
                //changes current value to 
                Globals.currentValue = value;

                //adds reaction
                await Context.Message.AddReactionAsync(new Emoji("✅"));
            }
        }

        //temporary command to set name
        [Command("profile")]
        public async Task profile(string password, string path)
        {
            if (password == Top.Secret)
            {
                Utils.changeProfile(Context.Client, path);
                await ReplyAsync("profile changed");
            }
        }

        //admin save function
        [Command("aes")]
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