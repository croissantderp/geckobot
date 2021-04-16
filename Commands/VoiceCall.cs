using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Discord.Commands;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;

namespace GeckoBot.Commands
{
    public class VoiceCall : ModuleBase<SocketCommandContext>
    {
        private readonly Dictionary<ulong, IAudioClient> ConnectedChannels = new Dictionary<ulong, IAudioClient>();

        // You *MUST* mark these commands with 'RunMode.Async'
        // otherwise the bot will not respond until the Task times out.
        [Command("join", RunMode = RunMode.Async)]
        public async Task JoinCmd()
        {
            await JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel);
        }

        // Remember to add preconditions to your commands,
        // this is merely the minimal amount necessary.
        // Adding more commands of your own is also encouraged.
        [Command("leave", RunMode = RunMode.Async)]
        public async Task LeaveCmd()
        {
            await LeaveAudio(Context.Guild);
        }

        [Command("play", RunMode = RunMode.Async)]
        public async Task PlayCmd([Remainder] string song)
        {
            await SendAudioAsync(Context.Guild, Context.Channel, song);
        }

        [Command("say", RunMode = RunMode.Async)]
        public async Task say([Remainder] string text)
        {
            string fileName = @"..\..\Cache\" + Context.Message.Id.ToString() + ".wav";

            File.Create(fileName);

            DecTalk(@"..\..\..\dectalk", fileName, "lol");

            Console.WriteLine("test1");

            //await SendAudioAsync(Context.Guild, Context.Channel, fileName);

            File.Delete(fileName);
        }

        public async Task JoinAudio(IGuild guild, IVoiceChannel target)
        {
            if (ConnectedChannels.ContainsKey(guild.Id))
            {
                return;
            }
            if (target.Guild.Id != guild.Id)
            {
                return;
            }

            var audioClient = await target.ConnectAsync();

            ConnectedChannels.Add(guild.Id, audioClient);
        }

        public async Task LeaveAudio(IGuild guild)
        {
            IAudioClient client = ConnectedChannels[guild.Id];
            ConnectedChannels.Remove(guild.Id);

            Console.WriteLine(client.ConnectionState);

            await client.StopAsync();
            Console.WriteLine("testest");
        }

        public async Task SendAudioAsync(IGuild guild, IMessageChannel channel, string path)
        {
            // Your task: Get a full path to the file if the value of 'path' is only a filename.
            if (!File.Exists(path))
            {
                await channel.SendMessageAsync("File does not exist.");
                return;
            }
            IAudioClient client;
            if (ConnectedChannels.TryGetValue(guild.Id, out client))
            {
                //await Log(LogSeverity.Debug, $"Starting playback of {path} in {guild.Name}");
                using (var ffmpeg = CreateProcess(path))
                using (var stream = client.CreatePCMStream(AudioApplication.Music))
                {
                    try { await ffmpeg.StandardOutput.BaseStream.CopyToAsync(stream); }
                    finally { await stream.FlushAsync(); }
                }
            }
        }

        private Process CreateProcess(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = @"..\..\Cache\ffmpeg-N-101953-g4e64c8fa29-win64-gpl-shared\bin\ffmpeg.exe",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
        }

        private Process DecTalk(string dectalkPath, string filePath, string content)
        {
            Console.WriteLine("test");
            return Process.Start(new ProcessStartInfo
            {
                FileName = @"..\..\..\dectalk\say.exe",
                Arguments = $"cd {dectalkPath} & say.exe -w {filePath} -p [:phoneme on] \"{content}\"",
                UseShellExecute = true
            });
        }
    }
}
