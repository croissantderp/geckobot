using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
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

        [Command("command count")]
        [Summary("Identifies the total amount of commands.")]
        public async Task cc()
        {
            await ReplyAsync(_commands.Commands.ToList().Count.ToString());
        }

        [Command("links")]
        [Summary("Sends the relevant geckobot links.")]
        public async Task links()
        {
            var embed = new EmbedBuilder
            {
                Title = "geckobot links:",
                Description = "links: [trello](https://trello.com/invite/b/cFS33M13/8fddf3ac42bd0fe419e482c6f4414e01/gecko-bot-todo) [github](https://github.com/croissantderp/geckobot) [invite](https://discord.com/oauth2/authorize?client_id=766064505079726140&scope=bot&permissions=3525696)"
            };

            embed.WithColor(180, 212, 85);

            await ReplyAsync(embed: embed.Build());
        }
        
        [Command("help")]
        [Summary("Dynamic help command.")]
        public async Task help(
            [Summary("The specific command or module to send info about.")] string target = null,
            [Summary("The index of the result.")] int result = 1)
        {
            result--;

            List<CommandInfo> commands = _commands.Commands.ToList();
            List<ModuleInfo> modules = _commands.Modules.ToList();
            EmbedBuilder embedBuilder = new ();
            
            // If there was an argument given, send info about that argument
            if (target != null)
            {
                var matchedCommands = commands.FindAll(cmd => cmd.Aliases.Contains(target, StringComparer.InvariantCultureIgnoreCase) || FormatCommand(cmd) == target);
                var matchedModules = modules.FindAll(m => m.Name.Equals(target, StringComparison.InvariantCultureIgnoreCase));
                
                if (matchedCommands.Count > 0 && matchedCommands.Count > result) // Try matching a command first
                {
                    var command = matchedCommands[result]; 
                    
                    var fields = command.Parameters;

                    embedBuilder.Title = FormatCommand(command); // Command name
                    embedBuilder.Description = command.Summary; // Command description

                    embedBuilder.AddField("Usage:",
                        $"{command.Name} {string.Join(" ", fields.Select(FormatParameter))}"); // Command usage
                    if (fields.Count > 0)
                        embedBuilder.AddField("Parameters:",
                            string.Join("\n", fields.Select(FormatParameterLong))); // Detailed parameter explanations
                    embedBuilder.AddField("Module:", command.Module.Name); // Parent module
                } 
                else if (matchedModules.Count > 0 && matchedModules.Count > result - matchedCommands.Count) // Check modules
                {
                    var module = matchedModules[result - matchedCommands.Count];
                        
                    embedBuilder.Title = module.Name; // Module name
                    embedBuilder.Description = module.Summary; // Module description

                    embedBuilder.AddField("Commands:",
                        string.Join(", ", module.Commands.Select(FormatCommand))); // Children commands
                }
                else if (matchedModules.Count > 0 || matchedCommands.Count > 0) // If something was found, but index was out of range
                {
                    await ReplyAsync($"Matched {matchedCommands.Count} command(s) and {matchedModules.Count} module(s), but index {result} was out of range");
                    return;
                }
                else // Otherwise, nothing was found
                {
                    await Context.Channel.SendFileAsync(@"..\..\Cache\message.gif");
                    return;
                }
            } 
            else // If no argument was given, send a list of commands
            {
                
                //embedBuilder.Title = "Command List:";
                //embedBuilder.Description = "The prefix for this server is " + Prefix.returnPrefix(Context.Guild != null ? Context.Guild.Id.ToString() : ""); //+ "\n" + string.Join(", ", desc);
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
        
        // Formats the parameter by adding type and default value to the parameter name
        private static string FormatParameter(ParameterInfo param)
        {
            string defaultValue = param.IsOptional ? $" = {param.DefaultValue ?? "null"}" : "";
            
            return $"[{param.Type.Name} {param.Name}{defaultValue}]";
        }
        
        // A longer version of FormatParameter that includes the summary, used in the Parameters embed section
        private static string FormatParameterLong(ParameterInfo param)
        {
            return $"{param.Name}: {param.Summary}";
        }

        //instructions
        [Command("what do you do?")]
        [Summary(@"I gave up maintaining this ¯\_(ツ)_/¯, use `help.")]
        public async Task instructions()
        {
            //embed
            var embed = new EmbedBuilder
            {
            Title = "geckobot" + Globals.names[Globals.CurrentName] + " 4/13/2021 instruction manual",
            Description = "my prefix is " + Prefix.returnPrefix(Context.Guild != null ? Context.Guild.Id.ToString() : "") + " and [prefix]i for inline commands." + System.Environment.NewLine +
                "if there's a problem, ping a geckobot admin " + System.Environment.NewLine +
                "links: [trello](https://trello.com/invite/b/cFS33M13/8fddf3ac42bd0fe419e482c6f4414e01/gecko-bot-todo) [github](https://github.com/croissantderp/geckobot) [invite](https://discord.com/oauth2/authorize?client_id=766064505079726140&scope=bot&permissions=379968)" + System.Environment.NewLine +
                "'[prefix]what do you do?' quick start guide" + System.Environment.NewLine +
                "'[prefix]help [command]' cool help command"
            };
            embed.WithFooter("It has been " + (DateTime.Now - Globals.lastReset).ToString() + " since this bot has been restarted");
            embed.WithColor(180, 212, 85);

            var embed2 = embed.Build();

            await ReplyAsync(embed: embed2);
        }

        [Command("command list")]
        [Summary("Sends a list of all the commands.")]
        public async Task cl()
        {
            List<CommandInfo> commands = _commands.Commands.ToList();

            List<string> desc = commands.Select(a => FormatCommand(a).Trim()).ToList();

            string final = "";

            for (int i = 0; i < desc.Count(); i++)
            {
                if (i + 1 == desc.Count())
                {
                    final += desc[i];
                    Console.WriteLine("end");
                    break;
                }

                final += desc[i] + ", ";

                if ((i + 1) % 5 == 0)
                {
                    final += "\n";
                }
            }

            using (StreamWriter file = new(@"../../cache/commands.txt"))
            {
                await file.WriteAsync(
                    "𝗖𝗼𝗺𝗺𝗮𝗻𝗱 𝗟𝗶𝘀𝘁:\n" +
                    "The prefix for this server is " + Prefix.returnPrefix(Context.Guild != null ? Context.Guild.Id.ToString() : "") + "\n" +
                    final
                    );
            }

            await Context.Channel.SendFileAsync(@"../../cache/commands.txt");
        }

        //instructions
        [Command("admins")]
        [Summary("Gets a list of current geckobot admins.")]
        public async Task admins()
        {
            ulong[] adminsId = GeckoBot.Preconditions.RequireGeckobotAdmin.GeckobotAdmins.ToArray();

            string final = "";

            foreach (ulong a in adminsId)
            {
                final += MentionUtils.MentionUser(a) + "\n";
            }

            //embed
            var embed = new EmbedBuilder
            {
                Title = "geckobot" + Globals.names[Globals.CurrentName] + " admins",
                Description = final
            };

            embed.WithColor(180, 212, 85);

            var embed2 = embed.Build();

            await ReplyAsync(embed: embed2, allowedMentions: Globals.notAllowed);
        }
    }
}