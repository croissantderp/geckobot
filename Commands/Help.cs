﻿using System.Collections.Generic;
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
        [Summary("Identifies the total amount of modules and commands.")]
        public async Task cc()
        {
            await ReplyAsync(_commands.Modules.ToList().Count.ToString() + " modules \n" + _commands.Commands.ToList().Count.ToString() + " commands");
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
            [Summary("The specific command or module to send info about. Enter 'list' if you want a list of commands under their respective modules.")] string target = null,
            [Summary("The index of the result.")] int result = 1)
        {
            result--;

            List<CommandInfo> commands = _commands.Commands.ToList();
            List<ModuleInfo> modules = _commands.Modules.ToList();
            EmbedBuilder embedBuilder = new ();

            // If list is specified, send the command list
            if (target == "list")
            {
                embedBuilder.Title = "Command List:";
                embedBuilder.Description = "The prefix for this server is " + Prefix.returnPrefix(Context.Guild != null ? Context.Guild.Id.ToString() : "");
                
                foreach (var module in modules)
                {
                    EmbedFieldBuilder field = new()
                    {
                        Name = module.Name,
                        Value = string.Join(", ", module.Commands.Select(FormatCommand)),
                        IsInline = true
                    };
                    embedBuilder.AddField(field);
                }
            }
            else if (target != null) // If there was an argument given, send info about that argument
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
            else // If no argument was given, send the generic help text
            {
                embedBuilder.Title = "geckobot" + Globals.names[Globals.CurrentName] + " 4/13/2021 instruction manual";
                embedBuilder.Description =
                    "my prefix is " + Prefix.returnPrefix(Context.Guild != null ? Context.Guild.Id.ToString() : "") +
                    " and [prefix]i for inline commands." + Environment.NewLine +
                    "if there's a problem, ping a geckobot admin " + Environment.NewLine +
                    "links: [trello](https://trello.com/invite/b/cFS33M13/8fddf3ac42bd0fe419e482c6f4414e01/gecko-bot-todo) [github](https://github.com/croissantderp/geckobot) [invite](https://discord.com/oauth2/authorize?client_id=766064505079726140&scope=bot&permissions=379968)" +
                    Environment.NewLine +
                    "'[prefix]what do you do?' quick start guide" + Environment.NewLine +
                    "'[prefix]help [command]' cool help command";
            }

            embedBuilder.WithColor(180, 212, 85);

            await ReplyAsync(embed: embedBuilder.Build());

        }
        
        // Formats the command by adding the module prefix to the command name
        private static string FormatCommand(CommandInfo command)
        {
            ModuleInfo module = command.Module;

            string space = command.Name != "" && module.Group != null ? " " : ""; // Unfortunate but oh well

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

        [Command("what do you do?")]
        [Summary("Sends a load of info as a text file.")]
        public async Task instructions()
        {
            List<CommandInfo> commands = _commands.Commands.ToList();
            List<ModuleInfo> modules = _commands.Modules.ToList();

            //constructing long list
            string final = "";

            //logo
            final += 
                @"         _              _             _             _              _            _               _          _       " + "\n" +
                @"        /\ \           /\ \         /\ \           /\_\           /\ \         / /\            /\ \       /\ \     " + "\n" +
                @"       /  \ \         /  \ \       /  \ \         / / /  _       /  \ \       / /  \          /  \ \      \_\ \    " + "\n" +
                @"      / /\ \_\       / /\ \ \     / /\ \ \       / / /  /\_\    / /\ \ \     / / /\ \        / /\ \ \     /\__ \   " + "\n" +
                @"     / / /\/_/      / / /\ \_\   / / /\ \ \     / / /__/ / /   / / /\ \ \   / / /\ \ \      / / /\ \ \   / /_ \ \  " + "\n" +
                @"    / / / ______   / /_/_ \/_/  / / /  \ \_\   / /\_____/ /   / / /  \ \_\ / / /\ \_\ \    / / /  \ \_\ / / /\ \ \ " + "\n" +
                @"   / / / /\_____\ / /____/\    / / /    \/_/  / /\_______/   / / /   / / // / /\ \ \___\  / / /   / / // / /  \/_/ " + "\n" +
                @"  / / /  \/____ // /\____\/   / / /          / / /\ \ \     / / /   / / // / /  \ \ \__/ / / /   / / // / /        " + "\n" +
                @" / / /_____/ / // / /______  / / /________  / / /  \ \ \   / / /___/ / // / /____\_\ \  / / /___/ / // / /         " + "\n" +
                @"/ / /______\/ // / /_______\/ / /_________\/ / /    \ \ \ / / /____\/ // / /__________\/ / /____\/ //_/ /          " + "\n" +
                @"\/___________/ \/__________/\/____________/\/_/      \_\_\\/_________/ \/_____________/\/_________/ \_\/           " + "\n" +
                "\n\n";


            //intro
            final += "geckobot" + Globals.names[Globals.CurrentName] + " 4/22/2021 instruction manual:\n"+
                "   If there's a problem, ping a geckobot admin\n" +
                "   quick links: " +
                "       [trello](https://trello.com/invite/b/cFS33M13/8fddf3ac42bd0fe419e482c6f4414e01/gecko-bot-todo) \n" +
                "       [github](https://github.com/croissantderp/geckobot) \n" +
                "       [invite](https://discord.com/oauth2/authorize?client_id=766064505079726140&scope=bot&permissions=379968)\n" +
                "   '[prefix]what do you do?' quick start guide\n" +
                "   '[prefix]help [command]' cool help command\n" +
                "   The generic prefix is '`', the prefix set for this server is " + Prefix.returnPrefix(Context.Guild != null ? Context.Guild.Id.ToString() : "") + 
                ".\n\n";


            //quick module/command list
            final += "Modules and their respective commands:\n";

            foreach (var module in modules)
            {
                final += 
                    "   " + module.Name + ": ";
                final += 
                    "   " + string.Join(", ", module.Commands.Select(FormatCommand)) + "\n";
            }
            final += "\n";


            //long command list
            final += "Command List:\n";

            foreach (var command in commands)
            {
                var fields = command.Parameters;

                final += 
                    "   " + FormatCommand(command) + ": " + command.Summary + "\n" +
                    "       Usage: " + $"{command.Name} {string.Join(" ", fields.Select(FormatParameter))}\n"; 
                
                if (fields.Count > 0)
                    final += 
                        "       Parameters: " + string.Join(" | ", fields.Select(FormatParameterLong)) + "\n";

                final += 
                    "       Module: " + command.Module.Name + "\n\n";
            }


            //setup
            final += "setup:\n" + 
                "   Download the code from the github and FFmpeg which is linked below. \n" +
                "   Move the extracted FFmpeg folder into the main directory. \n" +
                "   Then open and build the solution in an ide (Visual Studios recommended). \n" +
                "   After building for the first time, copy libsodium.dll and opus.dll into your runtime folder for voice features to work. \n" +
                "   Recommended setup is to use Task Scheduler or something similar to run the bot in the background. \n" + 
                "\n";


            //references & full link list
            final += "Links and references:\n" +
                "   trello: https://trello.com/invite/b/cFS33M13/8fddf3ac42bd0fe419e482c6f4414e01/gecko-bot-todo \n" +
                "   github: https://github.com/croissantderp/geckobot \n" +
                "   discord bot invite: https://discord.com/oauth2/authorize?client_id=766064505079726140&scope=bot&permissions=379968 \n" +
                "   Quantum component library: https://github.com/microsoft/Quantum \n" +
                "   DECtalk credit: https://github.com/freddyGiant/study-bot \n" +
                "   Gecko Collection: https://drive.google.com/drive/folders/1Omwv0NNV0k_xlECZq3d4r0MbSbuHC_Og \n" +
                "   FFmpeg: https://ffmpeg.org" +
                "   FFmpeg download shortcut: https://drive.google.com/file/d/1Bk-Net7-vP3gdGsDYzeNKQy2dinyvoV3/view \n" +
                "   Discord.net: https://github.com/discord-net/Discord.Net \n" +
                "   Google Drive API: https://developers.google.com/drive/api/v3/about-sdk \n" +
                "   Tutorials: https://www.youtube.com/watch?v=e2iaRVf4sho https://www.youtube.com/watch?v=dQw4w9WgXcQ \n" +
                "   Text generator: https://ascii.co.uk/text \n" +
                "   Contributors: https://github.com/ky28059 https://github.com/croissantderp \n" +
                "   Other cool bots: https://github.com/ky28059/RBot https://github.com/BubbyBabur/PeepsBot https://github.com/freddyGiant/study-bot \n" +
                "\n";


            //ending
            final += "Thanks and credits: \n" + 
                "   Special thanks to :lemonthink:, crested geckos and viewers like you who have the attention span to scroll to the bottom of the page. \n" + 
                "   ";


            //saving and sending file
            using (StreamWriter file = new(@"../../cache/commands.txt"))
            {
                await file.WriteAsync(final);
            }

            await Context.Channel.SendFileAsync(@"../../cache/commands.txt");
        }

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