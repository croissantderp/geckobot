using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace GeckoBot.Commands
{
    public class Help : ModuleBase<SocketCommandContext>
    {
        //instructions
        [Command("what do you")]
        public async Task instructions(string section)
        {
            //if info is found
            bool found = true;

            //embed
            var embed = new EmbedBuilder
            {
            Title = "geckobot" + Globals.names[Globals.currentValue] + System.Environment.NewLine + "1/18/2020 instruction manual"
            };

            //changes based on sections
            switch (section)
            {
                case "do?":
                    embed.Description = ("my prefix is \\`." + System.Environment.NewLine +
                                         "(highly recommended to have developer mode on to easily use)" + System.Environment.NewLine +
                                         "if there's a problem, ping my owner croissantderp#4167 " + System.Environment.NewLine + System.Environment.NewLine +
                                         "links: [trello](https://trello.com/invite/b/cFS33M13/8fddf3ac42bd0fe419e482c6f4414e01/gecko-bot-todo) [github](https://github.com/croissantderp/geckobot) [invite](https://discord.com/oauth2/authorize?client_id=766064505079726140&scope=bot&permissions=379968)" + System.Environment.NewLine + System.Environment.NewLine + 
                                         "sections (replace 'do?' with these): general | gecko | random | sigfigs | emote | embed | edit | za_warudo" + System.Environment.NewLine + System.Environment.NewLine +
                                         "**In order to not make admins angery, consider using a spam channel for these commands as they are lengthy.**");
                    break;
                case "general":
                    embed.AddField("**general**",
                        "I can do math with '\\`add [value] [value]' replace add with subtract, multiply and divide to do other operations." + System.Environment.NewLine + System.Environment.NewLine +
                        "To play rock paper scissors, enter '\\`rps [scissors/rock/paper]'." + System.Environment.NewLine + System.Environment.NewLine +
                        "to open a dm with me, use '\\`dm'" + System.Environment.NewLine + System.Environment.NewLine
                    );
                    break;
                case "gecko":
                    embed.AddField("**gecko images:**",
                        "to see the entire collection try '\\`GecColle' [go here](https://drive.google.com/drive/folders/1Omwv0NNV0k_xlECZq3d4r0MbSbuHC_Og?usp=sharing)" + System.Environment.NewLine + System.Environment.NewLine +
                        "to see the daily gecko profile picture, try '\\`gec' " + System.Environment.NewLine + System.Environment.NewLine +
                        "to see a random gecko profile picture, try '\\`rgec'" + System.Environment.NewLine + System.Environment.NewLine +
                        "to see a find a gecko profile picture, try '\\`fgec [int]' where int is the gecko#" + System.Environment.NewLine + System.Environment.NewLine +
                        "to receive daily gecko dms, use '\\`dm [true/false]' where true is to sign up and false is to cancel" + System.Environment.NewLine + System.Environment.NewLine +
                        "to start the daily clock use '\\`start', manually check by '`check [passcode for force-checking]' use a blank space surrounded by quotes for regular checking" + System.Environment.NewLine + System.Environment.NewLine
                    );
                    break;
                case "random":
                    embed.AddField("**random shenanigans:**",
                        "To generate a random number enter '\\`rng [min value] [max value]'." + System.Environment.NewLine + System.Environment.NewLine +
                        "To generate a random sequence of music, enter '\\`generate [number of measures in sequence]'" + System.Environment.NewLine + System.Environment.NewLine +
                        "Enter '\\`lottery' to try a 1/100^6 lottery" + System.Environment.NewLine + System.Environment.NewLine +
                        "To do a random contest thingy, enter '\\`contest [opponent name]'" + System.Environment.NewLine + System.Environment.NewLine +
                        "To play a number guessing game, first generate a number by entering '\\`g new [min value] [max value]' then enter '\\`g [value]' to guess" + System.Environment.NewLine + System.Environment.NewLine
                    );
                    break;
                case "sigfigs":
                    embed.AddField("**sigfigs:**",
                        "To see the significant figures of a number, enter '\\`sigfig [number]'" + System.Environment.NewLine + System.Environment.NewLine +
                        "To do sigfig math, do '\\`sigfig add [value] [value]' replace with other operations for those" + System.Environment.NewLine + System.Environment.NewLine
                    );
                    break;
                case "emote":
                    embed.AddField("**global emotes:**",
                        "Strange discord oversight/intentional design lets bots use emotes globally!" + System.Environment.NewLine + System.Environment.NewLine +
                        "To use an emote use '\\`e [emote name]' this command is also integrated into other commands (e.g. '\\`flip')" + System.Environment.NewLine + System.Environment.NewLine +
                        "To log an emote so it can be used globally, type '\\`es [common name] [actual emote here]' make sure the actual emote actually sends as an emote or it won't work. " + System.Environment.NewLine + System.Environment.NewLine +
                        "To save an animated emote, get the emote id (formatted like this: <a:[name]:[id]>) by entering backslash before the emote and copying the message, then paste it and the id will be there, remember to remove the backslash when saving. Then insert a random ':::' anywhere in the id and type '\\`es [common name] [emote id with random ::: ]'" + System.Environment.NewLine + System.Environment.NewLine +
                        "geckobot can also log every emote of every server that it is in by '\\`ess'" + System.Environment.NewLine + System.Environment.NewLine +
                        "To remove an emote, type '\\`er [common name]'" + System.Environment.NewLine + System.Environment.NewLine +
                        "To react, type '\\`re [channel id] [message id] [emote or common name]' global emote thing works too and you can string many together with '$'" + System.Environment.NewLine + System.Environment.NewLine +
                        "(escape an emote by prefacing it with '\\\\')"
                    );
                    break;
                case "embed":
                    embed.AddField("**embed builder:**",
                        "build an embed without breaking discord tos!" + System.Environment.NewLine + System.Environment.NewLine +
                        "the command is '\\`embed [title of embed] [fields, fields are separated by '$$' title and description are separated by '%%'] [footer] [hexidecimal color]'" + System.Environment.NewLine + System.Environment.NewLine +
                        "it supports hyperlink markdown, example: '\\[hyperlink markdown](https://example.com)' would make [hyperlink markdown](https://example.com)"
                    );
                    break;
                case "edit":
                    embed.AddField("**cursed edits**",
                        "do cursed things with (edited) messages" + System.Environment.NewLine + System.Environment.NewLine +
                        "to insert edited randomly with a message use '\\`edit [text before edit] [text after edit]'" + System.Environment.NewLine + System.Environment.NewLine +
                        "to flip the parens of the (edited) use '\\`flip [line one text] [line two text]'"
                    );
                    break;
                case "za_warudo":
                    embed.AddField("**timer and alarm**",
                        "to set a timer '\\`timer [message to send after timer] [amount of time in hh:mm:ss format]'" + System.Environment.NewLine + System.Environment.NewLine +
                        "to set an alarm '\\`alarm [message to send after alarm] [alarm time in (24 hr style) hh:mm:ss format]'"
                    );
                    break;
                default:
                    await Context.Channel.SendFileAsync(@"..\..\Cache\message.gif");
                    found = false;
                    break;
            }

            if (found)
            {
                embed.AddField("**counter**",
                    Globals.daysSinceReset + " days since this bot has been updated"
                    );

                embed.WithColor(180, 212, 85);

                var embed2 = embed.Build();

                await ReplyAsync(embed: embed2);
            }
        }
    }
}