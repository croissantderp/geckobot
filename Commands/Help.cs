using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace GeckoBot.Commands
{
    public class Help : ModuleBase<SocketCommandContext>
    {
        //identifies the version of geckobot
        [Command("id")]
        public async Task identify()
        {
            await ReplyAsync("geckobot" + Globals.names[Globals.currentValue]);
        }

        [Command("links")]
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

        //instructions
        [Command("what do you")]
        public async Task instructions(string section)
        {
            //if info is found
            bool found = true;

            //embed
            var embed = new EmbedBuilder
            {
            Title = "geckobot" + Globals.names[Globals.currentValue] + System.Environment.NewLine + "1/31/2020 instruction manual"
            };

            //changes based on sections
            switch (section)
            {
                case "do?":
                    embed.Description = ("my prefix is \\`." + System.Environment.NewLine +
                                         "(highly recommended to have developer mode on to easily use)" + System.Environment.NewLine +
                                         "if there's a problem, ping my owner croissantderp#4167 " + System.Environment.NewLine + System.Environment.NewLine +
                                         "links: [trello](https://trello.com/invite/b/cFS33M13/8fddf3ac42bd0fe419e482c6f4414e01/gecko-bot-todo) [github](https://github.com/croissantderp/geckobot) [invite](https://discord.com/oauth2/authorize?client_id=766064505079726140&scope=bot&permissions=379968)" + System.Environment.NewLine + System.Environment.NewLine + 
                                         "'\\`what do you do?' help command, sections (replace 'do?' with these): general | gecko | random | sigfig | e | embed | edit | za_warudo | count | admin" + System.Environment.NewLine + System.Environment.NewLine +
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
                        "'\\`gec' shows the daily gecko image" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`load [int]' caches a gecko image" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`rgec' shwos a random gecko" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`fgec [int]' finds a gecko where int is the gecko#" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`dm [true/false]' activates daily gecko nonifs where true is to sign up and false is to cancel" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`start' starts the timer for geckobot daily functions" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`last checked' shows when the bot last checked" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`check [passcode for force-checking]' checks if it is a new day, use a blank space surrounded by quotes for regular checking"
                    );
                    break;
                case "random":
                    embed.AddField("**random shenanigans:**",
                        "'\\`rng [min value] [max value]' to generate a random number." + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`generate [number of measures in sequence]' generate random music" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`lottery' tries a 1/100^6 lottery" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`contest [opponent name]' pits you in a contest with opponent given"
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
                        "'\\`te [channel id or 'dm'] [emote string]' is targeted send where the channel id is the target channel" + System.Environment.NewLine + System.Environment.NewLine +
                        "(escape an emote or '$' by prefacing it with '\\\\')"
                    );
                    embed.AddField("**e system manage:**",
                        "can save anything including emotes which can be used globally." + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`es [key to save as] [anything]' is the simple save function" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`er [key]' will remove a key and related info" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`ess' is super save and saves all the emotes of every server the bot can use" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`el' will send the list of all the saved keys and items"
                    );
                    break;
                case "embed":
                    embed.AddField("**embed builder:**",
                        "build an embed without breaking discord tos!" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`embed [title of embed] [fields, fields are separated by '$$' title and description are separated by '%%'] [footer] [hexidecimal color]' got that? Good, you will be quized on this" + System.Environment.NewLine + System.Environment.NewLine +
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
                        "'\\`alarm [message to send after alarm] [alarm date in mm/dd/yyyy] [alarm time in (24 hr style) hh:mm:ss format]' sets an alarm" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`countdown' [passcode] [target channel id] [is it a timer or alarm (true/false)]] [message, insert '[time]' wherever you want the time to show up and insert [end] to divide the main message from what message to show at the end ][date in mm/dd/yyyy for alarm or number of days for timer] [time, either time until, or alarm time]' visible timer feature, creates a viewable updating countdown, ask a geckobot admin to make one because only one instance can exist at a time and it's expensive. " + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`end countdown [passcode]' aborts the countdown" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`pause [passcode]' pauses the countdown" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`unpause [passcode]' unpauses the countdown"
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
                        "'\\`terminate' terminates the current instance of geckobot" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`set [passcode] [original name of instance you want to change] [name to change it to]' changes the name of geckobot to something in a predetermined matrix" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`change [passcode] [value to change] [name to change it to]' changes names in predetermined matrix" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`profile [passcode] [local filepath]' to change the bot's profile" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`aes [passcode] [key] [anything]' to save as a undeletable emote" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`aer [passcode] [key]' to remove an undeletable emote" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`bugs [passcode]' shows current bug reports" + System.Environment.NewLine + System.Environment.NewLine +
                        "'\\`clear bugs [passcode]' clears all bug reports"
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