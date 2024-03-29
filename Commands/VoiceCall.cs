﻿using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace GeckoBot.Commands
{

    public sealed class VoiceCall : ModuleBase<SocketCommandContext>
    {
        //private readonly VoiceCallService _service;
        public static Dictionary<ulong, List<(string, bool)>> queue = new Dictionary<ulong, List<(string,bool)>>();

        private readonly VoiceCallService _service;

        // Remember to add an instance of the AudioService
        // to your IServiceCollection when you initialize your bot
        public VoiceCall(VoiceCallService service) => _service = service;

        // You *MUST* mark these commands with 'RunMode.Async'
        // otherwise the bot will not respond until the Task times out.
        [Command("join", RunMode = RunMode.Async)]
        [Alias("connect")]
        [Summary("Has the bot join a voice call you are currently in.")]
        public async Task JoinCmd()
        {
            var voiceState = Context.User as IVoiceState;

            if (!VoiceCallService.channels.ContainsKey(Context.Guild.Id))
            {
                if (voiceState?.VoiceChannel == null)
                {
                    await ReplyAsync("You must be connected to a voice channel!");
                    return;
                }
                else
                {
                    await _service.JoinAudio(Context.Guild, voiceState.VoiceChannel);
                    
                    await Context.Message.AddReactionAsync(new Emoji("⏫"));
                }
            }
            else
            {
                await ReplyAsync("I am already in a voice channel");
            }
        }

        [Command("skip", RunMode = RunMode.Async)]
        [Summary("Skips the current playing thing.")]
        public async Task skipCmd()
        {
            var voiceState = Context.User as IVoiceState;

            if (VoiceCallService.channels.ContainsKey(Context.Guild.Id))
            {
                if (voiceState?.VoiceChannel == null)
                {
                    await ReplyAsync("You must be connected to a voice channel!");
                    return;
                }
                else
                {
                    _service.skip(Context.Guild);
                    await Context.Message.AddReactionAsync(new Emoji("⏭️"));
                }
            }
            else
            {
                await ReplyAsync("I am not in a voice channel");
            }
        }

        [Command("clear", RunMode = RunMode.Async)]
        [Summary("Clears the server queue.")]
        public async Task superskipCmd()
        {
            var voiceState = Context.User as IVoiceState;

            if (VoiceCallService.channels.ContainsKey(Context.Guild.Id))
            {
                if (voiceState?.VoiceChannel == null)
                {
                    await ReplyAsync("You must be connected to a voice channel!");
                    return;
                }
                else
                {
                    _service.skip(Context.Guild);
                    queue.Remove(Context.Guild.Id);
                    await Context.Message.AddReactionAsync(new Emoji("⏏️"));
                }
            }
            else
            {
                await ReplyAsync("I am not in a voice channel");
            }
        }

        [Command("leave", RunMode = RunMode.Async)]
        [Alias("disconnect", "dc")]
        [Summary("Has the bot leave a voice call you are currently in.")]
        public async Task LeaveCmd()
        {
            var voiceState = Context.User as IVoiceState;

            if (VoiceCallService.channels.ContainsKey(Context.Guild.Id))
            {
                if (voiceState?.VoiceChannel == null)
                {
                    await ReplyAsync("You must be connected to a voice channel!");
                    return;
                }
                else
                {
                    await _service.LeaveAudio(Context.Guild);

                    await Context.Message.AddReactionAsync(new Emoji("⏬"));
                }
            }
            else
            {
                await ReplyAsync("I am not in a voice channel");
            }
        }

        [Command("reload", RunMode = RunMode.Async)]
        [Summary("Has the bot leave and then rejoin a voice call you are currently in.")]
        public async Task ReloadCmd()
        {
            var voiceState = Context.User as IVoiceState;

            if (VoiceCallService.channels.ContainsKey(Context.Guild.Id))
            {
                if (voiceState?.VoiceChannel == null)
                {
                    await ReplyAsync("You must be connected to a voice channel!");
                    return;
                }
                else
                {
                    await _service.LeaveAudio(Context.Guild);

                    await Context.Message.AddReactionAsync(new Emoji("⏬"));

                    await _service.JoinAudio(Context.Guild, voiceState.VoiceChannel);

                    await Context.Message.AddReactionAsync(new Emoji("⏫"));
                }
            }
            else
            {
                await ReplyAsync("I am not in a voice channel");
            }
        }

        [Command("start capture", RunMode = RunMode.Async)]
        [Alias("begin capture")]
        [Summary("Starts capturing all the text and transmitting it into a voicecall.")]
        public async Task capture()
        {
            var voiceState = Context.User as IVoiceState;

            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }


            if (!VoiceCallService.captureChannels.ContainsKey(Context.Guild.Id))
            {
                if (!VoiceCallService.channels.ContainsKey(Context.Guild.Id))
                {
                    await _service.JoinAudio(Context.Guild, voiceState.VoiceChannel);

                    await Context.Message.AddReactionAsync(new Emoji("⏫"));
                }

                VoiceCallService.captureChannels.Add(Context.Guild.Id, Context.Channel.Id);
                await ReplyAsync("Capturing channel, the capture will end either when 'end capture' is called or the bot leaves the voice channel");
            }
            else
            {
                await ReplyAsync("I am already capturing");
            }
        }

        [Command("clear dectalk cache")]
        [Summary("Clears the dectalk audio directory.")]
        public async Task clearFiles()
        {
            DirectoryInfo dir = new DirectoryInfo(@"../../../dectalk/audio/");

            //clears files in dectalk audio cache if some still exist for some reason
            foreach (FileInfo file in dir.GetFiles())
            {
                try
                {
                    file.Delete();
                }
                catch
                {
                    continue;
                }
            }

            await ReplyAsync("cleared");
        }

        [Command("capture")]
        [Summary("Gets the current capture channel if it exists.")]
        public async Task Currentcapture()
        {
            if (VoiceCallService.captureChannels.ContainsKey(Context.Guild.Id))
            {
                await ReplyAsync("Currently capturing <#" + VoiceCallService.captureChannels[Context.Guild.Id].ToString() + ">");
            }
            else
            {
                await ReplyAsync("No ongoing captures in this guild.");
            }
        }

        [Command("end capture")]
        [Alias("stop capture")]
        [Summary("Ends a current text to speech capture")]
        public async Task capturent()
        {
            var voiceState = Context.User as IVoiceState;

            if (VoiceCallService.channels.ContainsKey(Context.Guild.Id))
            {
                if (voiceState?.VoiceChannel == null)
                {
                    await ReplyAsync("You must be connected to a voice channel!");
                    return;
                }
                else
                {
                    if (VoiceCallService.captureChannels.ContainsKey(Context.Guild.Id))
                    {
                        VoiceCallService.captureChannels.Remove(Context.Guild.Id);
                        await ReplyAsync("Capture ended");
                    }
                    else
                    {
                        await ReplyAsync("I am already not capturing");
                    }
                }
            }
            else
            {
                await ReplyAsync("I am not in a voice channel");
            }
        }


        [Command("dectalk")]
        [Summary("Sends a desktop app version of dectalk.")]
        public async Task dectalk()
        {
            await Context.Channel.SendFileAsync(@"../../../dectalk/DECTALK 5.0.zip");
        }

        [Command("dectalk help")]
        [Alias("dhelp")]
        [Summary("Give some helpful information about DECtalk")]
        public async Task dhelp()
        {
            //buils an embed
            var embed = new EmbedBuilder
            {
                Title = "DECtalk help",
                Description = (
                "DECtalk is a simple speech synthesizer integrated into discord calls with geckobot and [studybot](https://github.com/freddyGiant/study-bot)" + "\n" +
                "DECtalk can also sing, be sure to add '[:phoneme on]' at the beginning though" + "\n" +
                "More information:" + "\n" +
                "[Wikipedia](https://en.wikipedia.org/wiki/DECtalk)" + "\n" +
                "Example songs for phoneme:" + "\n" +
                "[The flame of hope collection](http://theflameofhope.co/SONGS.html)" + "\n" +
                "[Steam collection](https://steamcommunity.com/sharedfiles/filedetails/?id=482628855)")
            };

            embed.WithColor(180, 212, 85);

            await Context.Channel.SendFileAsync(@"../../../dectalk/info.txt", embed: embed.Build());
        }

        string DectalkReplace(string original, DiscordSocketClient client)
        {
            Regex eregex = new Regex(@"\<a?\:\w*\:\d{18}\>");
            Regex cregex = new Regex(@"\<\#\d{18}\>");
            Regex mregex = new Regex(@"\<\@\!?\d{18}\>");
            Regex rregex = new Regex(@"\<\@\&\d{18}\>");

            original = eregex.Replace(original, a => a.ToString().Split(":")[1]);
            original = cregex.Replace(original, a => client.GetChannel(ulong.Parse(a.ToString().Remove(0, 2).Remove(18, 1))).ToString());
            original = mregex.Replace(original, a => client.GetUser(ulong.Parse(a.ToString().Replace("!", "").Remove(0, 2).Remove(18, 1))).Username);
            original = rregex.Replace(original, "role");

            return original.Replace("'", "''").Replace("\n", " ").Replace("’", "''").Replace("‘", "''").Replace("\t", " ").Replace("“", "\"").Replace("”", "\"");
        }

        [Command("sa")]
        [Summary("Synthesizes an audio file using DECtalk, credit for this feature goes to [this](https://github.com/freddyGiant/study-bot).")]
        public async Task say([Remainder][Summary("the text that DECtalk will synthesize, also could work with an attached text file.")] string text = null)
        {
            string fileName = @"../../../dectalk/audio/" + Context.Message.Id.ToString() + ".wav";

                //.txt file support
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

            if (text == null)
            {
                await Context.Message.AddReactionAsync(new Emoji("❎"));
                return;
            }
            //cleans strings
            string cleanText = DectalkReplace(text, Context.Client);

            await DecTalk(@"./audio/" + Context.Message.Id.ToString() + ".wav", cleanText).WaitForExitAsync();

            await dttimer(fileName);

            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

        [Command("s", RunMode = RunMode.Async)]
        [Alias("say")]
        [Summary("Plays dectalk in a voice call using DECtalk, credit for the idea behind this feature goes to [this](https://github.com/freddyGiant/study-bot).")]
        public async Task s([Remainder][Summary("the text that DECtalk will synthesize, also could work with an attached text file.")] string text = null)
        {
            var voiceState = Context.User as IVoiceState;

            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }

            if (!VoiceCallService.channels.ContainsKey(Context.Guild.Id))
            {
                await _service.JoinAudio(Context.Guild, voiceState.VoiceChannel);

                await Context.Message.AddReactionAsync(new Emoji("⏫"));
            }

            string fileName = @"../../../dectalk/audio/" + Context.Message.Id.ToString() + ".wav";

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

            if (text == null)
            {
                await Context.Message.AddReactionAsync(new Emoji("❎"));
                return;
            }

            string cleanText = DectalkReplace(text, Context.Client);

            await DecTalk(@"./audio/" + Context.Message.Id.ToString() + ".wav", cleanText).WaitForExitAsync();

            Console.WriteLine("e");

            string fullPath = new FileInfo(fileName).FullName;

            await vcdttimer(fullPath, Context.Guild);

            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

        [Command("audio list")]
        [Summary("Finds all current audio files loaded in geckobot.")]
        public async Task pList()
        {
            DirectoryInfo thing = new DirectoryInfo(@"..\..\..\audio\");

            string[] audioFiles = thing.GetFiles().Select(a => a.Name).ToArray();

            await ReplyAsync("Current playable audio:\n" + String.Join("\n", audioFiles));
        }

        [Command("p", RunMode = RunMode.Async)]
        [Summary("Plays some preset audio files in a voice call.")]
        public async Task p([Remainder][Summary("the audio file that will play")] string text)
        {
            var voiceState = Context.User as IVoiceState;

            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }

            if (!VoiceCallService.channels.ContainsKey(Context.Guild.Id))
            {
                await _service.JoinAudio(Context.Guild, voiceState.VoiceChannel);

                await Context.Message.AddReactionAsync(new Emoji("⏫"));
            }

            bool found = false;
            string file = "";

            DirectoryInfo thing = new DirectoryInfo(@"..\..\..\audio\");

            FileInfo[] audioFiles = thing.GetFiles();

            foreach (FileInfo audioFile in audioFiles)
            {
                if (audioFile.Name.Split(".")[0] == text)
                {
                    file = audioFile.FullName;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                await ReplyAsync("Effect not found.");
                return;
            }

            await vcdttimer(file, Context.Guild, false);

            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

        public async void dectalkcapture(ulong message, string text, IGuild guild, DiscordSocketClient client)
        {
            string fileName = @"../../../dectalk/audio/" + message + ".wav";

            string cleanText = DectalkReplace(text, client);

            await DecTalk(@"./audio/" + message + ".wav", cleanText).WaitForExitAsync();

            string fullPath = new FileInfo(fileName).FullName;

            await vcdttimer(fullPath, guild);
        }

        public async Task dttimer(string fileName)
        {
            try
            {
                await Context.Channel.SendFileAsync(fileName);

            }
            catch
            {
                await ReplyAsync("error, final recording file size too large or other error, try again");

                //resets timer for deletion
                System.Timers.Timer timer2 = new(10000);
                timer2.Elapsed += (sender, e) => delayDelete(timer2, fileName);
                timer2.Start();
            }

            //resets timer for deletion
            System.Timers.Timer timer = new(10000);
            timer.Elapsed += (sender, e) => delayDelete(timer, fileName);
            timer.Start();
        }

        public async Task vcdttimer(string fileName, IGuild guild, bool delete = true)
        {
            if (!queue.ContainsKey(guild.Id))
            {
                bool success = queue.TryAdd(guild.Id, new List<(string, bool)>() { (fileName, delete) });

                if (!success)
                {
                    Console.WriteLine("queue addition failed");
                }
                else
                {
                    await _service.SendAudioAsync(guild);
                }
            }
            else
            {
                queue[guild.Id].Add((fileName, delete));
            }
        }

        public static void delayDelete(System.Timers.Timer timer, string fileName)
        {
            File.Delete(fileName);
            timer.Close();
        }

        private Process DecTalk(string filePath, string content)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe",
                Arguments = $"sl ../../../dectalk | ../../../dectalk/say.exe -w {filePath} -p '{content}'",
                UseShellExecute = false
            });
        }
    }
}
