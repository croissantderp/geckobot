using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using GeckoBot.Utils;

namespace GeckoBot.Commands
{
    [Summary("Help with geckobot.")]
    public class Help : ModuleBase<SocketCommandContext>
    {
        // Receive the CommandService via dependency injection
        public CommandService _commands { get; set; }
        
        //identifies the version of geckobot
        [Command("id")]
        [Summary("Identifies the current version of geckobot.")]
        public async Task identify()
        {
            await ReplyAsync("geckobot" + Globals.names[Globals.CurrentName]);
        }

        [Command("links")]
        [Summary("Sends the relevant geckobot links.")]
        public async Task links()
        {
            var embed = new EmbedBuilder
            {
                Title = "geckobot links:",
                Description = "links: [trello](https://trello.com/invite/b/cFS33M13/8fddf3ac42bd0fe419e482c6f4414e01/gecko-bot-todo) [github](https://github.com/croissantderp/geckobot) [invite](https://discord.com/oauth2/authorize?client_id=766064505079726140&scope=bot&permissions=379968)"
            };

            embed.WithColor(180, 212, 85);

            await ReplyAsync(embed: embed.Build());
        }
        
        [Command("help")]
        [Summary("Dynamic help command.")]
        public async Task help(string target = null)
        {
            List<CommandInfo> commands = _commands.Commands.ToList();
            List<ModuleInfo> modules = _commands.Modules.ToList();
            EmbedBuilder embedBuilder = new EmbedBuilder();
            
            // If there was an argument given, send info about that argument
            if (target != null)
            {
                var command = commands.Find(cmd => cmd.Aliases.Contains(target) || FormatCommand(cmd) == target);
                var module = modules.Find(m => m.Name == target);
                
                if (command != null) // Try matching a command first
                {
                    embedBuilder.Title = FormatCommand(command);
                    embedBuilder.Description = command.Summary;
                    embedBuilder.AddField("Module:", command.Module.Name);
                }
                else if (module != null) // If not a command, check modules
                {
                    embedBuilder.Title = module.Name;
                    embedBuilder.Description = module.Summary;
                    
                    embedBuilder.AddField("Commands:", 
                        string.Join(", ", module.Commands.Select(FormatCommand)));
                }
                else // Otherwise, nothing was found
                {
                    await ReplyAsync("Target not found.");
                    return;
                }
            } 
            else // If no argument was given, send a list of commands
            {
                List<string> desc = commands.Select(FormatCommand).ToList();

                embedBuilder.Title = "Command List:";
                embedBuilder.Description = string.Join(", ", desc);
            }

            embedBuilder.WithColor(180, 212, 85);

            await ReplyAsync(embed: embedBuilder.Build());
        }
        
        // Formats the command by adding the module prefix to the command name
        private static string FormatCommand(CommandInfo command)
        {
            ModuleInfo module = command.Module;

            string space = command.Name != "" && module.Group != "" ? " " : ""; // Unfortunate but oh well

            return $"{module.Group}{space}{command.Name}";
        }

        //instructions
        [Command("what do you")]
        [Summary("a quick guide on every command, meant for quick reads. sections: | do? | general | gecko | random | sigfig | e | embed | edit | za_warudo | find | get | poll | admin | count")]
        public async Task instructions(string section)
        {
            //if info is found
            bool found = true;

            //embed
            var embed = new EmbedBuilder
            {
            Title = "geckobot" + Globals.names[Globals.CurrentName] + System.Environment.NewLine + "1/31/2020 instruction manual"
            };

            //changes based on sections
            switch (section)
            {
                case "do?":
                    embed.Description = ("my prefix is \\`." + System.Environment.NewLine +
                                         "(highly recommended to have developer mode on to easily use)" + System.Environment.NewLine +
                                         "if there's a problem, ping my owner croissantderp#4167 " + System.Environment.NewLine + System.Environment.NewLine +
                                         "links: [trello](https://trello.com/invite/b/cFS33M13/8fddf3ac42bd0fe419e482c6f4414e01/gecko-bot-todo) [github](https://github.com/croissantderp/geckobot) [invite](https://discord.com/oauth2/authorize?client_id=766064505079726140&scope=bot&permissions=379968)" + System.Environment.NewLine + System.Environment.NewLine +
                                         "'\\`what do you [section]' quick start guide (inputs prefaced by (?) are optional), sections: | **do?** | **general** | **gecko** | **random** | **sigfig** | **e** | **embed** | **edit** | **za_warudo** | **find** | **get** | **poll** | **admin** | **count** |" + System.Environment.NewLine + System.Environment.NewLine +
                                         "'\\`help [command]' cool help command" + System.Environment.NewLine + System.Environment.NewLine +
                                         "**In order to not make admins angery, consider using a spam channel for these commands as they are lengthy.**"
                                         );
                    break;
                case "general":
                    embed.AddField("**general**",
                        "'\\`add [value] [value]' does simple math, replace add with subtract, multiply and divide to do other operations." + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`rps [scissors/rock/paper]' to play rock paper scissors." + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`instantiate' creates the nessecary directories if they do not exist" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`id' displays current instance of geckobot." + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`report' logs a bug" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`delete [channel id] [message id]' can delete messages sent by geckobot" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`links' returns a list of helpful links"
                        );
                    break;
                case "gecko":
                    embed.AddField("**gecko images:**",
                        "'\\`GecColle' returns a link to the geckoimage archive, or [go here](https://drive.google.com/drive/folders/1Omwv0NNV0k_xlECZq3d4r0MbSbuHC_Og?usp=sharing)" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`submit' returns a link to submit a gecko, or [go here](https://forms.gle/CeNkM2aHcdrcidvX6)" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`gec' shows the daily gecko image" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`rgec' shwos a random gecko" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`fgec [int]' finds a gecko where int is the gecko#" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`ogec [int]' finds an alternate gecko where int is the gecko#"
                    );
                    embed.AddField("**daily dm:**",
                        "'\\`dm [true/false]' activates daily gecko nonifs where true is to sign up and false is to cancel" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`gecko gang' shows who is in the gecko gang (people who have signed up for daily dms)" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`start' starts the timer for geckobot daily functions" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`last checked' shows when the bot last checked" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`check' checks if it is a new day"
                    );
                    break;
                case "random":
                    embed.AddField("**random shenanigans:**",
                        "'\\`rng [min value] [max value]' to generate a random number." + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`generate [number of measures in sequence]' generate random music" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`lottery' tries a 1/100^6 lottery" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`contest [opponent name]' pits you in a contest with opponent given" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`fek (?)[length of word]' generates a random word, defaults to a random length between 2 and 10"
                    );
                    embed.AddField("**number guessing game**",
                        "'\\`g new [min value] [max value]' to generate a new number." + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`g [value]' to guess"
                    );
                    break;
                case "sigfig":
                    embed.AddField("**sigfigs:**",
                        "'\\`sigfig [number]' returns the significant figures of the number" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`sigfig add [value] [value]' does math, replace with other operations for those"
                    );
                    break;
                case "e":
                    embed.AddField("**e system use:**",
                        "can call items with a key, string multiple keys together with $, this also applies to many other functions (e.g. '\\`flip [emote string]')" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`e [string]' is the simple send function" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`ge [string]' is reverse lookup, where the emote string is the item you want to find the key for" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`re [channel id] [message id] [emote string (note: only emotes)]' is react function" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`fe [message link] [emote string (note: only emotes)]' is react function using links" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`te [channel id or 'dm'] [emote string]' is targeted send where the channel id is the target channel" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`ie [emote string]' is inline send, you can insert this anywhere in a message and it will treat everything after it as a command" + System.Environment.NewLine + System.Environment.NewLine +
                        "(escape an emote or '$' by prefacing it with '\\\\')"
                    );
                    embed.AddField("**e system manage:**",
                        "can save anything including emotes which can be used globally." + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`es [key to save as] [any string]' is the simple save function" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`er [key]' will remove a key and related info" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`ess' is super save and saves all the emotes of every server the bot can use" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`el' will send the list of all the saved keys and items" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`ea' will get the amount of items in the system"
                    );
                    break;
                case "embed":
                    embed.AddField("**embed builder:**",
                        "build an embed without breaking discord tos!" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`embed [title of embed] [fields, fields are separated by '$$' title and description are separated by '%%'] (?)[thumbnail url] (?)[footer] (?)[hexidecimal color]' got that? Good, you will be quized on this" + System.Environment.NewLine + System.Environment.NewLine +
                        "it supports hyperlink markdown, example: '\\[hyperlink markdown](https://example.com)' would make [hyperlink markdown](https://example.com)"
                    );
                    break;
                case "edit":
                    embed.AddField("**cursed edits**",
                        "do cursed things with (edited) messages" + System.Environment.NewLine + System.Environment.NewLine +
                        "\\`edit [text before edit] [text after edit]' inserts (edited) between texts" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`edit flip [line one text] [line two text]' flips the parans of the (edited)"
                    );
                    break;
                case "za_warudo":
                    embed.AddField("**timer and alarm**",
                        "'\\`timer [message to send after timer] [amount of time in hh:mm:ss format]' sets an timer" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`alarm [message to send after alarm] [alarm time in (24 hr style) hh:mm:ss format] (?)[alarm date in mm/dd/yyyy]' sets an alarm" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`countdown' [passcode] [target channel id] [is it a timer or alarm (true/false)]] [message, insert '[time]' wherever you want the time to show up and insert [end] to divide the main message from what message to show at the end ][date in mm/dd/yyyy for alarm or number of days for timer] [time, either time until, or alarm time]' visible timer feature, creates a viewable updating countdown, ask a geckobot admin to make one because only one instance can exist at a time and it's expensive. " + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`end countdown [passcode]' aborts the countdown" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`pause [passcode]' pauses the countdown" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`unpause [passcode]' unpauses the countdown"
                    );
                    break;
                case "find":
                    embed.AddField("**finds**",
                        "'\\`find channel [channel id]' gets a channel" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`find message [channel id] [message id]' generates a link for the message" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`find message content [channel id] [message id]' finds content of a message using ids" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`find user [user id]' gets info about a user" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`find emote [emote id]' gets an emote from id" 
                    );
                    break;
                case "get":
                    embed.AddField("**gets**",
                        "'\\`get channel' gets various id from a #channel" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`get message content [link]' finds content of a message using links" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`get message [link]' gets various ids from a message link" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`get user [mention]' gets info about a user using mention" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`get emote [emote name or emote]' gets an emote in it's text form" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`get all emote [emote name] (?)[page]' gets all emotes of a certain name organized in pages of 5, defaults to page 1"
                    );
                    break;
                case "poll":
                    embed.AddField("**poll**",
                        "'\\`create [poll name]' creates a poll" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`vote [poll name] [fraction in ##/##]' votes on a poll" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`remove [poll name]' removes polls, only creator of a poll can remove it" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`results [poll name]' gets current results of a poll" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`poll list' gets all polls"
                    );
                    break;
                case "count":
                    embed.AddField("**counter**",
                        Globals.daysSinceReset + " day(s) since this bot has been updated"
                        );
                    break;
                case "admin":
                    embed.AddField("**admin**",
                        "ask a geckobot admin to do something" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`terminate [name of isntance]' terminates the current instance of geckobot, put all as name to terminate every instance" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`set [original name of instance you want to change] [name to change it to]' changes the name of geckobot to something in a predetermined matrix" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`change [value to change] [name to change it to]' changes names in predetermined matrix" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`profile [local filepath]' to change the bot's profile" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`aes [key] [anything]' to save as a undeletable emote" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`aer [key]' to remove an undeletable emote" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`bugs' shows current bug reports" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`clear bugs' clears all bug reports" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`fremove [poll name]' force removes a specified poll" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`fcheck' forces bot to update"
                        );
                    break;
                default:
                    await Context.Channel.SendFileAsync(@"..\..\Cache\message.gif");
                    found = false;
                    break;
            }

            if (found)
            {
                embed.WithColor(180, 212, 85);

                var embed2 = embed.Build();

                await ReplyAsync(embed: embed2);
            }
        }
    }
}