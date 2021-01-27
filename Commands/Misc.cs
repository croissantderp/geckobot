using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.IO;

namespace GeckoBot.Commands
{
    public class Misc : ModuleBase<SocketCommandContext>
    {
        //creates new files if there are none
        [Command("instantiate")]
        public async Task instantiate()
        {
            //checks if files exist
            FileUtils.checkForExistance();

            //adds reaction
            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

        [Command("report")]
        public async Task report(string reason)
        {
            FileUtils.checkForExistance();

            //if file exists, load it
            if (FileUtils.Load(@"..\..\Cache\gecko1.gek") != null)
            {
                //clears
                Globals.bugs.Clear();

                //gets info
                string[] temp = FileUtils.Load(@"..\..\Cache\gecko1.gek").Split(",");

                //adds info to list
                foreach (string a in temp)
                {
                    Globals.bugs.Add(a);
                }
            }

            Globals.bugs.Add(reason);

            //saves info
            FileUtils.Save(string.Join(",", Globals.bugs.ToArray()), @"..\..\Cache\gecko1.gek");

            //adds reaction
            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

        [Command("bugs")]
        public async Task bugs(string passcode)
        {
            if (passcode == Top.Secret)
            {
                FileUtils.checkForExistance();

                //if file exists, load it
                if (FileUtils.Load(@"..\..\Cache\gecko1.gek") != null)
                {
                    //clears
                    Globals.bugs.Clear();

                    //gets info
                    string[] temp = FileUtils.Load(@"..\..\Cache\gecko1.gek").Split(",");

                    //adds info to list
                    foreach (string a in temp)
                    {
                        Globals.bugs.Add(a);
                    }
                }
                await ReplyAsync(string.Join(", ", Globals.bugs));
            }
        }
    }
}
