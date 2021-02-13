using Google.Apis.Drive.v3;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GeckoBot.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace GeckoBot
{
    class Program
    {
        static readonly string[] Scopes = { DriveService.Scope.DriveReadonly };
        static string ApplicationName = "GeckoBot";
        
        static void Main(string[] args)
        {
            new Program().RunBotAsync().GetAwaiter().GetResult();
        }
        public DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;

        public async Task RunBotAsync()
        {
            _client = new DiscordSocketClient();
            _commands = new CommandService();

            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            string token = Top.secret;

            _client.Log += _client_Log;

            await RegisterCommandsAsync();

            await _client.LoginAsync(TokenType.Bot, token);

            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private Task _client_Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        public async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(_client, message);
            if (message.Author.IsBot) return;

            int argPos = 0;
            if (message.HasStringPrefix("`", ref argPos))
            {
                string temp = message.Content.Remove(0,1);
                if (!System.Text.RegularExpressions.Regex.IsMatch(temp, @"(?<!\\)\`"))
                {
                    var result = await _commands.ExecuteAsync(context, argPos, _services);
                    if (!result.IsSuccess) await message.Channel.SendMessageAsync(result.ErrorReason, allowedMentions: Globals.allowed);
                    if (result.Error.Equals(CommandError.UnmetPrecondition)) await message.Channel.SendMessageAsync(result.ErrorReason, allowedMentions: Globals.allowed);
                }
            }
            else if (message.HasStringPrefix("\\`", ref argPos))
            {
                string temp = message.Content.Remove(0, 2);
                if (!System.Text.RegularExpressions.Regex.IsMatch(temp, @"(?<!\\)\`"))
                {
                    var result = await _commands.ExecuteAsync(context, argPos, _services);
                    if (!result.IsSuccess) await message.Channel.SendMessageAsync(result.ErrorReason, allowedMentions: Globals.allowed);
                    if (result.Error.Equals(CommandError.UnmetPrecondition)) await message.Channel.SendMessageAsync(result.ErrorReason, allowedMentions: Globals.allowed);
                }
            }
        }
    }
}