using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;
using System.Threading.Tasks;
using System.Threading;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Discord;
using Discord.Audio;
using Discord.Commands;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net;
using Victoria;
using Victoria.Enums;

namespace GeckoBot.Commands
{

    public class VoiceCallService : ModuleBase<SocketCommandContext>
    {
        public static readonly ConcurrentDictionary<ulong, IAudioClient> ConnectedChannels = new ConcurrentDictionary<ulong, IAudioClient>();
        public static Dictionary<ulong, IVoiceChannel> channels = new Dictionary<ulong, IVoiceChannel>();
        public static  Dictionary<ulong, AudioOutStream> streams = new Dictionary<ulong, AudioOutStream>();

        public async Task JoinAudio(IGuild guild, IVoiceChannel target)
        {
            var audioClient = await target.ConnectAsync();
            ConnectedChannels.TryAdd(guild.Id, audioClient);
            channels.Add(guild.Id, target);

            var stream = audioClient.CreatePCMStream(AudioApplication.Mixed);
            streams.Add(guild.Id, stream);
        }

        public async Task LeaveAudio(IGuild guild)
        {
            IAudioClient client;
            ConnectedChannels.Remove(guild.Id, out client);
            await channels[guild.Id].DisconnectAsync();
            channels.Remove(guild.Id);
            streams.Remove(guild.Id);
        }

        public async Task SendAudioAsync(IGuild guild)
        {
            IAudioClient client;

            string path = VoiceCall.queue[guild.Id][0];

            ConnectedChannels.TryGetValue(guild.Id, out client);

            using (var ffmpeg = CreateProcess(path))
            {
                await ffmpeg.StandardOutput.BaseStream.CopyToAsync(streams[guild.Id]);
                await streams[guild.Id].FlushAsync();

                //await stream.DisposeAsync();
            }

            //timer for deletion
            System.Timers.Timer timer = new(10000);
            timer.Elapsed += (sender, e) => VoiceCall.delayDelete(timer, path);
            timer.Start();

            VoiceCall.queue[guild.Id].RemoveAt(0);

            if (VoiceCall.queue[guild.Id].Count > 0)
            {
                await SendAudioAsync(guild);
            }
            else
            {
                VoiceCall.queue.Remove(guild.Id);
            }
        }

        private Process CreateProcess(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = @"..\..\..\ffmpeg-4.4-full_build\bin\ffmpeg.exe",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
        }
    }
}
