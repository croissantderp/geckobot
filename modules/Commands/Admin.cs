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
    }
}