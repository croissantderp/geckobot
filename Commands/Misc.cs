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

        [Command("delete")]
        public async Task delete(string channel, string message)
        {
            //gets current client
            DiscordSocketClient client = Context.Client;

            //parses channel id provided and gets channel from client
            var chnl = client.GetChannel(ulong.Parse(channel)) as IMessageChannel;

            //parses message id provided and gets message from channel
            var message2 = await chnl.GetMessageAsync(ulong.Parse(message));

            if (message2.Author.Id == Context.Client.CurrentUser.Id)
            {
                if (!Globals.undeletable.Contains(message2.Id))
                {
                    await message2.DeleteAsync();

                    //adds reaction
                    await Context.Message.AddReactionAsync(new Emoji("✅"));
                }
                else
                {
                    await ReplyAsync("message is undeletable");
                }
            }
            else
            {
                await ReplyAsync("can only delete messages sent by geckobot");
            }
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

        [Command("clear bugs")]
        public async Task clearBugs(string passcode)
        {
            if (passcode == Top.Secret)
            {
                FileUtils.checkForExistance();

                //if file exists, load it
                if (FileUtils.Load(@"..\..\Cache\gecko1.gek") != null)
                {
                    //clears
                    Globals.bugs.Clear();
                }

                //saves info
                FileUtils.Save(string.Join(",", Globals.bugs.ToArray()), @"..\..\Cache\gecko1.gek");

                //adds reaction
                await Context.Message.AddReactionAsync(new Emoji("✅"));
            }
        }
    }
}
