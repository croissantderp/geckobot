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
        //opens dm with user
        [Command("dmme")]
        public async Task DMme()
        {
            await Context.User.SendMessageAsync("blorp");
        }

        //creates new files if there are none
        [Command("instantiate")]
        public async Task instantiate()
        {
            //checks if files exist
            if (!File.Exists(@"..\..\Cache\gecko2.gek"))
            {
                File.Create(@"..\..\Cache\gecko2.gek");
            }

            if (!File.Exists(@"..\..\Cache\gecko3.gek"))
            {
                File.Create(@"..\..\Cache\gecko3.gek");
            }
            //adds reaction
            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

    }
}
