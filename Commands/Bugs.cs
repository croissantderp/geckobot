using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace GeckoBot.Commands
{
    public class Bugs : ModuleBase<SocketCommandContext>
    {
        [Command("report")]
        [Summary("Reports a bug.")]
        public async Task report(string reason)
        {
            FileUtils.checkForExistance();
            
            //clears
            Globals.bugs.Clear();

            //gets info
            string[] temp = FileUtils.Load(@"..\..\Cache\gecko1.gek").Split(",");

            //adds info to list
            foreach (string a in temp)
            {
                Globals.bugs.Add(a);
            }

            Globals.bugs.Add(reason);

            //saves info
            FileUtils.Save(string.Join(",", Globals.bugs.ToArray()), @"..\..\Cache\gecko1.gek");

            //adds reaction
            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

        [Command("bugs")]
        [Summary("Lists current bugs.")]
        public async Task bugs(string passcode)
        {
            if (passcode == Top.Secret)
            {
                FileUtils.checkForExistance();
                
                //clears
                Globals.bugs.Clear();

                //gets info
                string[] temp = FileUtils.Load(@"..\..\Cache\gecko1.gek").Split(",");

                //adds info to list
                foreach (string a in temp)
                {
                    Globals.bugs.Add(a);
                }
                
                await ReplyAsync(string.Join(", ", Globals.bugs));
            }
        }

        [Command("clear bugs")]
        [Summary("Clears all bugs.")]
        public async Task clearBugs(string passcode)
        {
            if (passcode == Top.Secret)
            {
                FileUtils.checkForExistance();
                
                //clears
                Globals.bugs.Clear();

                //saves info
                FileUtils.Save(string.Join(",", Globals.bugs.ToArray()), @"..\..\Cache\gecko1.gek");

                //adds reaction
                await Context.Message.AddReactionAsync(new Emoji("✅"));
            }
        }
    }
}