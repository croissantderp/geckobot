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
        //sends message
        [Command("send")]
        public async Task send(string target, string message)
        {
            if (target == "dm")
            {
                await Context.User.SendMessageAsync(message);
            }
            else
            {
                //gets current client
                DiscordSocketClient client = Context.Client;

                //parses channel id provided and gets channel from client
                var chnl = client.GetChannel(ulong.Parse(target)) as IMessageChannel;

                await chnl.SendMessageAsync(message);
            }
        }

        //creates new files if there are none
        [Command("instantiate")]
        public async Task instantiate()
        {
            //checks if files exist
            FileUtils.checkForExistance();

            //adds reaction
            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

    }
}
