using Google.Apis.Drive.v3;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using GeckoBot.Commands;
using GeckoBot.Utils;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;

namespace GeckoBot
{
    class Program
    {
        static readonly string[] Scopes = { DriveService.Scope.DriveReadonly };
        static string ApplicationName = "GeckoBot";

        public static readonly DailyDM ddm = new ();
        public static readonly Gec gec = new ();

        static void Main(string[] args)
        {
            new Program().RunBotAsync().GetAwaiter().GetResult();
        }

        public DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;

        public async Task RunBotAsync()
        {
            var _config = new DiscordSocketConfig();

            _config.GatewayIntents = 
                  GatewayIntents.DirectMessageReactions 
                | GatewayIntents.DirectMessages 
                | GatewayIntents.DirectMessageTyping 
                //| GatewayIntents.GuildBans 
                | GatewayIntents.GuildEmojis
                //| GatewayIntents.GuildIntegrations
                //| GatewayIntents.GuildInvites
                | GatewayIntents.GuildMembers
                | GatewayIntents.GuildMessageReactions
                | GatewayIntents.GuildMessages
                | GatewayIntents.GuildMessageTyping
                | GatewayIntents.GuildPresences
                | GatewayIntents.Guilds
                | GatewayIntents.GuildVoiceStates
                //| GatewayIntents.GuildWebhooks
                ;

            _client = new DiscordSocketClient(_config);
            _commands = new CommandService();
            _services = new ServiceCollection()
                .AddSingleton(new VoiceCallService())
                .AddSingleton(new VoiceCall(new VoiceCallService()))
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            string token = Top.secret;

            _client.Log += _client_Log;

            await RegisterThingsAsync();

            await _client.LoginAsync(TokenType.Bot, token);

            await _client.StartAsync();

            _client.Ready += ReadyAsync;

            await Task.Delay(-1);
        }

        private async Task ReadyAsync()
        {
            ddm._client = _client;

            //sets activity
            await _client.SetGameAsync("`what do you do?");

            await ddm.initiatethings();
        }

        private Task _client_Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        public async Task RegisterThingsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            _client.UserVoiceStateUpdated += HandleDisconnectAsync;

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task HandleDisconnectAsync(SocketUser user, SocketVoiceState state, SocketVoiceState state2)
        {
            if (user.Id == _client.CurrentUser.Id && state.VoiceChannel != null)
            {
                if (!state.VoiceChannel.Users.Select(a => a.Id).Contains(_client.CurrentUser.Id) && VoiceCallService.ConnectedChannels.ContainsKey(state.VoiceChannel.Guild.Id))
                {
                    IAudioClient client;
                    VoiceCallService.captureChannels.Remove(state.VoiceChannel.Guild.Id);
                    VoiceCallService.ConnectedChannels.TryRemove(state.VoiceChannel.Guild.Id, out client);
                    await VoiceCallService.channels[state.VoiceChannel.Guild.Id].Item1.DisconnectAsync();
                    VoiceCallService.channels.Remove(state.VoiceChannel.Guild.Id);
                }
            }
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(_client, message);
            if (message.Author.IsBot) return;

            int argPos = 0;

            string prefix = Prefix.returnPrefix(context.Guild != null ? context.Guild.Id.ToString() : "");
            
            Regex regex = new Regex(@"(?<!(\\|\`))" + prefix + "i");

            if (message.HasStringPrefix(prefix, ref argPos) || message.HasStringPrefix("\\" + prefix, ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _services);
                if (!result.IsSuccess && !result.Error.Equals(CommandError.UnknownCommand)) await message.Channel.SendMessageAsync(result.ErrorReason, allowedMentions: Globals.allowed);
                else if (!result.Error.Equals(CommandError.UnknownCommand)) return;
                //if (result.Error.Equals(CommandError.UnmetPrecondition)) await message.Channel.SendMessageAsync(result.ErrorReason, allowedMentions: Globals.allowed);
            }
            if (regex.IsMatch(message.Content))
            {
                var temp2 = regex.Split(message.Content); //@"(?<!\\)\`i"
                argPos = temp2[0].Length + prefix.Length + 1;

                var result = await _commands.ExecuteAsync(context, argPos, _services);
                if (!result.IsSuccess && !result.Error.Equals(CommandError.UnknownCommand)) await message.Channel.SendMessageAsync(result.ErrorReason, allowedMentions: Globals.allowed);
                //if (result.Error.Equals(CommandError.UnmetPrecondition)) await message.Channel.SendMessageAsync(result.ErrorReason, allowedMentions: Globals.allowed);
                else if (!result.Error.Equals(CommandError.UnknownCommand)) return;
            }
            if (message.Content == "`what do you do?")
            {
                var result = await _commands.ExecuteAsync(context, 1, _services);
                if (!result.IsSuccess && !result.Error.Equals(CommandError.UnknownCommand)) await message.Channel.SendMessageAsync(result.ErrorReason, allowedMentions: Globals.allowed);
                return;
            }
            if (message.Content != "")
            {
                //refreshes the alert dictionary
                Alert.RefreshAlertsDict();

                //checks if message content matched with anything else
                var matches = Alert.alerts.Where(a => message.Content.Contains(a.Value));
                if (matches != null)
                {
                    foreach (var match in matches)
                    {
                        //gets users
                        var user = context.Guild.GetUser(ulong.Parse(match.Key));

                        //if the user can see the channel in the first place
                        if (user.GetPermissions(context.Channel as IGuildChannel).ViewChannel)
                        {
                            //sends message and then removes key to avoid spam
                            await user.SendMessageAsync($"Alert for '{match.Value}' triggered at https://discord.com/channels/" + context.Guild.Id.ToString() + "/" + context.Channel.Id.ToString() + "/" + context.Message.Id.ToString() + $"\nThe Alert was automatically deactivated, use the following to reactivate it. \n\n`as {match.Value}");

                            Alert.alerts.Remove(match.Key);
                        }
                    }
                    //saves updated info
                    FileUtils.Save(Globals.DictToString(Alert.alerts, "{0} ⁊ {1} ҩ "), @"..\..\Cache\gecko9.gek");
                }

                if (VoiceCallService.captureChannels.ContainsValue(context.Channel.Id))
                {
                    _services.GetService<VoiceCall>().dectalkcapture(context.Message.Id, context.Message.ToString(), context.Guild, _client);
                }
            }

        }
    }
}