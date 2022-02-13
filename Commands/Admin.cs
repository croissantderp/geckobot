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
        [Command("block")]
        [Summary("Blocks a user.")]
        public async Task tempblock([Summary("The user's id to block")][Remainder] string id)
        {
            if (Globals.tempBlocked.Contains(ulong.Parse(id)))
            {
                await ReplyAsync("this user is already block'd");
            }
            else
            {
                Globals.tempBlocked.Add(ulong.Parse(id));
                await ReplyAsync("block'd");
            }
        }

        [Command("unblock")]
        [Summary("Unblocks a user.")]
        public async Task untempblock([Summary("The user's id to block")][Remainder] string id)
        {
            if (Globals.tempBlocked.Contains(ulong.Parse(id)))
            {
                Globals.tempBlocked.Remove(ulong.Parse(id));
                await ReplyAsync("unblock'd");
            }
            else
            {
                await ReplyAsync("this user is already unblock'd");
            }
        }

        //termination command
        [Command("terminate")]
        [Summary("Terminates the bot process.")]
        public async Task terminate([Summary("The name of the instance to terminate, use all for all instances.")][Remainder]string name = "all")
        {
            if (name == Globals.names[0] || name == "all")
            {
                await ReplyAsync("terminating...");
                System.Environment.Exit(0);
            }
        }
        
        //temporary command to set name
        [Command("set name")]
        [Summary("Sets geckobot's internal name from the array of names.")]
        public async Task set([Summary("The name of the instance to change.")] string name, [Summary("The value of the internal name array to change the instance name to.")] int value)
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
        [Command("change name")]
        [Summary("Changes geckobot's internal name to the specified string.")]
        public async Task change([Summary("The value of the internal name array to change.")] int value, [Summary("The new name to change that name to.")][Remainder]string newName)
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
        public async Task profile([Summary("The local path of the image.")] string path)
        {
            bool success = await Utils.Utils.changeProfile(Context.Client, path);
            DailyDM.allowedToPfp = false;
            await ReplyAsync(success ? "profile changed" : "profile change failed");
        }

        [Command("name")]
        [Summary("Sets geckobot's name")]
        public async Task name([Summary("The name to change the in-dicord name to.")] [Remainder]string name)
        {
            bool success = await Utils.Utils.changeName(Context.Client, name);
            await ReplyAsync(success ? "name changed" : "name change failed");
        }
    }
}