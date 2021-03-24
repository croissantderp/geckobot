using System.Collections.Generic;
using System.Linq;
using System;
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
                Description = "links: [trello](https://trello.com/invite/b/cFS33M13/8fddf3ac42bd0fe419e482c6f4414e01/gecko-bot-todo) [github](https://github.com/croissantderp/geckobot) [invite](https://discord.com/oauth2/authorize?client_id=766064505079726140&scope=bot&permissions=3525696)"
            };

            embed.WithColor(180, 212, 85);

            await ReplyAsync(embed: embed.Build());
        }
        
        [Command("help")]
        [Summary("Dynamic help command.")]
        public async Task help(
            [Remainder]
            [Summary("The specific command or module to send info about.")]
            string target = null
            )
        {
            List<CommandInfo> commands = _commands.Commands.ToList();
            List<ModuleInfo> modules = _commands.Modules.ToList();
            EmbedBuilder embedBuilder = new ();
            
            // If there was an argument given, send info about that argument
            if (target != null)
            {
                var command = commands.Find(cmd => cmd.Aliases.Contains(target) || FormatCommand(cmd) == target);
                var module = modules.Find(m => m.Name == target);
                
                if (command != null) // Try matching a command first
                {
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
                else if (module != null) // If not a command, check modules
                {
                    embedBuilder.Title = module.Name; // Module name
                    embedBuilder.Description = module.Summary; // Module description
                    
                    embedBuilder.AddField("Commands:", 
                        string.Join(", ", module.Commands.Select(FormatCommand))); // Children commands
                }
                else // Otherwise, nothing was found
                {
                    await Context.Channel.SendFileAsync(@"..\..\Cache\message.gif");
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
            Title = "geckobot" + Globals.names[Globals.CurrentName] + System.Environment.NewLine + "1/31/2020 instruction manual",
            Description = ("my prefix is \\` and \\`i for inline commands." + System.Environment.NewLine +
                "(highly recommended to have developer mode on to easily use)" + System.Environment.NewLine +
                "if there's a problem, ping my owner croissantderp#4167 " + System.Environment.NewLine + System.Environment.NewLine +
                "links: [trello](https://trello.com/invite/b/cFS33M13/8fddf3ac42bd0fe419e482c6f4414e01/gecko-bot-todo) [github](https://github.com/croissantderp/geckobot) [invite](https://discord.com/oauth2/authorize?client_id=766064505079726140&scope=bot&permissions=379968)" + System.Environment.NewLine + System.Environment.NewLine +
                "'\\`what do you do?' quick start guide" + System.Environment.NewLine + System.Environment.NewLine +
                "'\\`help [command]' cool help command" + System.Environment.NewLine + System.Environment.NewLine +
                "**In order to not make admins angery, consider using a spam channel for these commands as they are lengthy.**"
                )
            };
            embed.WithFooter("It has been " + (DateTime.Now - Globals.lastReset).ToString() + " since this bot has been restarted");
            embed.WithColor(180, 212, 85);

            var embed2 = embed.Build();

            await ReplyAsync(embed: embed2);
        }
    }
}