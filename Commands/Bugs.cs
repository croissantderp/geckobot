using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using GeckoBot.Preconditions;
using GeckoBot.Utils;

namespace GeckoBot.Commands
{
    [Summary("Geckobot's bug reporting module.")]
    public class Bugs : ModuleBase<SocketCommandContext>
    {
        //bug list
        public static readonly List<string> BugList = new();
        
        [Command("report")]
        [Summary("Reports a bug.")]
        public async Task report(string reason)
        {
            RefreshBugs();

            BugList.Add(reason);

            //saves info
            FileUtils.Save(string.Join(",", BugList.ToArray()), @"..\..\Cache\gecko1.gek");

            //adds reaction
            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }
        
        [RequireGeckobotAdmin]
        [Command("bugs")]
        [Summary("Lists current bugs.")]
        public async Task bugs()
        {
            RefreshBugs();
            
            await ReplyAsync(string.Join(", ", BugList));
        }
        
        [RequireGeckobotAdmin]
        [Command("clear bugs")]
        [Summary("Clears all bugs.")]
        public async Task clearBugs()
        {
            FileUtils.checkForExistance();
            
            //clears
            BugList.Clear();

            //saves info
            FileUtils.Save(string.Join(",", BugList.ToArray()), @"..\..\Cache\gecko1.gek");

            //adds reaction
            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }
    
        // Refresh bugs list
        // Is this necessary?
        private void RefreshBugs()
        {
            FileUtils.checkForExistance();
            
            //clears
            BugList.Clear();

            //gets info
            string[] temp = FileUtils.Load(@"..\..\Cache\gecko1.gek").Split(",");

            //adds info to list
            foreach (string a in temp)
            {
                BugList.Add(a);
            }
        }
    }
}