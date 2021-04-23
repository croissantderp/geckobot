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

        public async Task JoinAudio(IGuild guild, IVoiceChannel target)
        {
            var audioClient = await target.ConnectAsync();
            ConnectedChannels.TryAdd(guild.Id, audioClient);

            var stream = audioClient.CreatePCMStream(AudioApplication.Mixed);
            channels.Add(guild.Id, (target, stream));
        }

        public async Task LeaveAudio(IGuild guild)
        {
            IAudioClient client;
            ConnectedChannels.Remove(guild.Id, out client);
            await channels[guild.Id].Item1.DisconnectAsync();
            channels.Remove(guild.Id);
        }

        public async Task SendAudioAsync(IGuild guild)
        {
            IAudioClient client;

            string path = VoiceCall.queue[guild.Id][0];

            ConnectedChannels.TryGetValue(guild.Id, out client);

            using (var ffmpeg = CreateProcess(path))
            {
                await ffmpeg.StandardOutput.BaseStream.CopyToAsync(channels[guild.Id].Item2);
                await channels[guild.Id].Item2.FlushAsync();

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
