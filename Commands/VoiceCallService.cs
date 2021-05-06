using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace GeckoBot.Commands
{

    public class VoiceCallService
    {
        public static readonly ConcurrentDictionary<ulong, IAudioClient> ConnectedChannels = new ConcurrentDictionary<ulong, IAudioClient>();
        public static Dictionary<ulong, (IVoiceChannel, AudioOutStream)> channels = new Dictionary<ulong, (IVoiceChannel, AudioOutStream)>();
        public static Dictionary<ulong, Process> ffmpegs = new Dictionary<ulong, Process>();
        public static Dictionary<ulong, ulong> captureChannels = new Dictionary<ulong, ulong>();

        public async Task JoinAudio(System.Timers.Timer timer, IGuild guild, IVoiceChannel target)
        {
            timer.Close();

            var audioClient = await target.ConnectAsync(selfDeaf: true);
            ConnectedChannels.TryAdd(guild.Id, audioClient);

            var stream = audioClient.CreatePCMStream(AudioApplication.Mixed);
            channels.Add(guild.Id, (target, stream));

        }

        public async Task LeaveAudio(IGuild guild)
        {
            if (ConnectedChannels.ContainsKey(guild.Id))
            {
                if (captureChannels.ContainsKey(guild.Id))
                {
                    captureChannels.Remove(guild.Id);
                }

                IAudioClient client;
                ConnectedChannels.Remove(guild.Id, out client);
                await channels[guild.Id].Item2.DisposeAsync();
                await channels[guild.Id].Item1.DisconnectAsync();
                channels.Remove(guild.Id);
            }
        }

        public void skip(IGuild guild)
        {
            try
            {
                ffmpegs[guild.Id].Kill();
            }
            catch
            {

            }
        }

        public async Task SendAudioAsync(IGuild guild)
        {
            IAudioClient client;

            string path = VoiceCall.queue[guild.Id][0];

            ConnectedChannels.TryGetValue(guild.Id, out client);

            try
            {
                using (var ffmpeg = CreateProcess(path))
                {
                    ffmpegs.Add(guild.Id, ffmpeg);
                    await ffmpeg.StandardOutput.BaseStream.CopyToAsync(channels[guild.Id].Item2);
                    await channels[guild.Id].Item2.FlushAsync();
                    ffmpegs.Remove(guild.Id);
                }
            }
            catch
            {

            }

            //timer for deletion
            System.Timers.Timer timer = new(1000);
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
