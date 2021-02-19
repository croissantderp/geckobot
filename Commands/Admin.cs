using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using GeckoBot.Preconditions;
using GeckoBot.Utils;

namespace GeckoBot.Commands
{
    [RequireGeckobotAdmin]
    [Summary("Admin only GeckoBot commands.")]
    public class Admin : ModuleBase<SocketCommandContext>
    {
        //termination command
        [Command("terminate")]
        [Summary("Terminates the bot process.")]
        public async Task terminate(string name)
        {
            if (name == Globals.names[0] || name == "all")
            {
                await ReplyAsync("terminating...");
                System.Environment.Exit(0);
            }
        }
        
        //temporary command to set name
        [Command("set")]
        [Summary("Sets geckobot's internal name from the array of names.")]
        public async Task set(string name, int value)
        {
            if (name == Top.SecretName)
            {
                //changes current value to 
                Globals.CurrentName = value;

                //adds reaction
                await Context.Message.AddReactionAsync(new Emoji("✅"));
            }
        }

        //temporary command to set name
        [Command("change")]
        [Summary("Changes geckobot's internal name to the specified string.")]
        public async Task change(int value, string newName)
        {
            if (value != 0)
            {
                Globals.names[value] = newName;
                //adds reaction
                await Context.Message.AddReactionAsync(new Emoji("✅"));
            }
            else
            {
                await ReplyAsync("can't change original name");
            }
        }

        //temporary command to set profile
        [Command("profile")]
        [Summary("Sets geckobot's profile to the specified image path.")]
        public async Task profile(string path)
        {
            Utils.Utils.changeProfile(Context.Client, path);
            await ReplyAsync("profile changed");
        }

        [Command("name")]
        [Summary("Sets geckobot's name")]
        public async Task name(string name)
        {
            Utils.Utils.changeName(Context.Client, name);
            await ReplyAsync("name changed");
        }
    }
}