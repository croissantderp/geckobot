using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Discord;
using Discord.Audio;
using Discord.Commands;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace GeckoBot.Commands
{
    public class VoiceCall : ModuleBase<SocketCommandContext>
    {
        private readonly Dictionary<ulong, IAudioClient> ConnectedChannels = new Dictionary<ulong, IAudioClient>();

        private static bool firstTime = true; 

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

        [Command("sa", RunMode = RunMode.Async)]
        [Summary("Synthesizes an audio file using DECtalk, credit for this feature goes to [this](https://github.com/freddyGiant/study-bot).")]
        public async Task say([Remainder][Summary("the text that DECtalk will synthesize, also could work with an attached text file.")] string text = "")
        {
            string fileName = @"../../../dectalk/" + Context.Message.Id.ToString() + ".wav";

            if (Context.Message.Attachments.Count != 0)
            {
                IAttachment attach = Context.Message.Attachments.First();

                string[] suffixs = attach.Filename.Split(".");

                string suffix = "." + suffixs[suffixs.Length - 1];
                
                if (suffix == ".txt")
                {
                    using (var client = new WebClient())
                    {
                        client.DownloadFile(new Uri(attach.Url), @"..\..\Cache\" + Context.Message.Id.ToString() + suffix);

                        

                        text = Utils.FileUtils.Load(@"..\..\Cache\" + Context.Message.Id.ToString() + suffix);
                        
                        File.Delete(@"..\..\Cache\" + Context.Message.Id.ToString() + suffix);
                    }
                }
            }

            string cleanText = text.Replace("'", "").Replace("\n", "");

            DecTalk(@"./" + Context.Message.Id.ToString() + ".wav", cleanText);

            Console.WriteLine(firstTime);
            //starts a timer with desired amount of time
            System.Timers.Timer t = new(firstTime ? 5000 : 1000);
            t.Elapsed += async (sender, e) => await dttimer(t, fileName);
            t.Start();

            if (firstTime) firstTime = false;
            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

        public async Task dttimer(System.Timers.Timer timer, string fileName)
        {
            //await SendAudioAsync(Context.Guild, Context.Channel, fileName);
            await Context.Channel.SendFileAsync(fileName);

            File.Delete(fileName);

            timer.Close();
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
                File.Delete(path);
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

        private Process DecTalk(string filePath, string content)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"sl ../../../dectalk | ../../../dectalk/say.exe -w {filePath} -p [:phoneme on] '{content}'",
                UseShellExecute = false
            });
        }
    }
}
