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
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;

namespace GeckoBot.Commands
{

    public sealed class VoiceCall : ModuleBase<SocketCommandContext>
    {
        //private readonly VoiceCallService _service;
        public static Dictionary<ulong, List<string>> queue = new Dictionary<ulong, List<string>>();

        private readonly VoiceCallService _service;

        // Remember to add an instance of the AudioService
        // to your IServiceCollection when you initialize your bot
        public VoiceCall(VoiceCallService service) => _service = service;

        // You *MUST* mark these commands with 'RunMode.Async'
        // otherwise the bot will not respond until the Task times out.
        [Command("join", RunMode = RunMode.Async)]
        public async Task JoinCmd()
        {
            var voiceState = Context.User as IVoiceState;

            if (!_service.channels.ContainsKey(Context.Guild.Id))//!_lavaNode.HasPlayer(Context.Guild))
            {
                if (voiceState?.VoiceChannel == null)
                {
                    await ReplyAsync("You must be connected to a voice channel!");
                    return;
                }
                else
                {
                    await _service.JoinAudio(Context.Guild, voiceState.VoiceChannel);
                    //await _lavaNode.LeaveAsync(voiceState.VoiceChannel);

                    await Context.Message.AddReactionAsync(new Emoji("⏫"));
                }
            }
            else
            {
                await ReplyAsync("I am already in a voice channel");
            }
        }

        // Remember to add preconditions to your commands,
        // this is merely the minimal amount necessary.
        // Adding more commands of your own is also encouraged.
        [Command("leave", RunMode = RunMode.Async)]
        public async Task LeaveCmd()
        {
            var voiceState = Context.User as IVoiceState;

            if (_service.channels.ContainsKey(Context.Guild.Id))//!_lavaNode.HasPlayer(Context.Guild))
            {
                if (voiceState?.VoiceChannel == null)
                {
                    await ReplyAsync("You must be connected to a voice channel!");
                    return;
                }
                else
                {
                    //await _service.JoinAudio(Context.Guild, voiceState.VoiceChannel);
                    //await _lavaNode.LeaveAsync(voiceState.VoiceChannel);
                    await _service.LeaveAudio(Context.Guild);

                    await Context.Message.AddReactionAsync(new Emoji("⏬"));
                }
            }
            else
            {
                await ReplyAsync("I am not in a voice channel");
            }
        }



        //private readonly LavaNode _lavaNode;

        //public VoiceCall(LavaNode lavaNode) => _lavaNode = lavaNode;


        // You *MUST* mark these commands with 'RunMode.Async'
        // otherwise the bot will not respond until the Task times out.
        /*[Command("join2", RunMode = RunMode.Async)]
        public async Task Join()
        {
            if (_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("I'm already connected to a voice channel!");
                return;
            }

            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }

            await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

        // Remember to add preconditions to your commands,
        // this is merely the minimal amount necessary.
        // Adding more commands of your own is also encouraged.
        [Command("leave2")]
        public async Task Leave()
        {
            var voiceState = Context.User as IVoiceState;

            if (_lavaNode.HasPlayer(Context.Guild))
            {
                if (voiceState?.VoiceChannel == null)
                {
                    await ReplyAsync("You must be connected to a voice channel!");
                }
                else
                {
                    Console.WriteLine(voiceState.VoiceChannel.Id);


                    await Context.Message.AddReactionAsync(new Emoji("✅"));
                }
                return;
            }
            await ReplyAsync("I am not in a voice channel!");
        }*/

        [Command("sa")]
        [Summary("Synthesizes an audio file using DECtalk, credit for this feature goes to [this](https://github.com/freddyGiant/study-bot).")]
        public async Task say([Remainder][Summary("the text that DECtalk will synthesize, also could work with an attached text file.")] string text = null)
        {
            string fileName = @"../../../dectalk/" + Context.Message.Id.ToString() + ".wav";

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

            //cleans strings
            string cleanText = text.Replace("'", "").Replace("\n", "");

            DecTalk(@"./" + Context.Message.Id.ToString() + ".wav", cleanText);

            //starts a timer with desired amount of time
            System.Timers.Timer t = new(1000);
            t.Elapsed += async (sender, e) => await dttimer(t, fileName);
            t.Start();

            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

        [Command("s", RunMode = RunMode.Async)]
        [Summary("Plays dectalk in a voicecall using DECtalk, credit for this feature goes to [this](https://github.com/freddyGiant/study-bot).")]
        public async Task s([Remainder][Summary("the text that DECtalk will synthesize, also could work with an attached text file.")] string text = null)
        {
            var voiceState = Context.User as IVoiceState;

            if (!_service.channels.ContainsKey(Context.Guild.Id))//!_lavaNode.HasPlayer(Context.Guild))
            {
                if (voiceState?.VoiceChannel == null)
                {
                    await ReplyAsync("You must be connected to a voice channel!");
                    return;
                }
                else
                {
                    await _service.JoinAudio(Context.Guild, voiceState.VoiceChannel);
                    //await _lavaNode.LeaveAsync(voiceState.VoiceChannel);

                    await Context.Message.AddReactionAsync(new Emoji("⏫"));
                }
            }

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

            string fullPath = new FileInfo(fileName).FullName;

            //starts a timer with desired amount of time
            System.Timers.Timer t = new(1000);
            t.Elapsed += async (sender, e) => await vcdttimer(t, fullPath);
            t.Start();

            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

        public async Task dttimer(System.Timers.Timer timer, string fileName)
        {
            timer.Close();

            //await SendAudioAsync(Context.Guild, Context.Channel, fileName);
            try
            {
                await Context.Channel.SendFileAsync(fileName);

            }
            catch
            {
                await ReplyAsync("error, final recording file size too large or other error, try again");

                //resets timer for deletion
                timer = new(10000);
                timer.Elapsed += (sender, e) => delayDelete(timer, fileName);
                timer.Start();
            }

            //resets timer for deletion
            timer = new(10000);
            timer.Elapsed += (sender, e) => delayDelete(timer, fileName);
            timer.Start();
        }

        /*[Command("Play")]
        public async Task PlayAsync([Remainder] string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                await ReplyAsync("Please provide search terms.");
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }

            var queries = searchQuery.Split(' ');
            foreach (var query in queries)
            {
                var searchResponse = await _lavaNode.SearchAsync(query);
                if (searchResponse.LoadStatus == LoadStatus.LoadFailed ||
                    searchResponse.LoadStatus == LoadStatus.NoMatches)
                {
                    await ReplyAsync($"I wasn't able to find anything for `{query}`.");
                    return;
                }

                var player = _lavaNode.GetPlayer(Context.Guild);

                if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
                {
                    if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
                    {
                        foreach (var track in searchResponse.Tracks)
                        {
                            player.Queue.Enqueue(track);
                        }

                        await ReplyAsync($"Enqueued {searchResponse.Tracks.Count} tracks.");
                    }
                    else
                    {
                        var track = searchResponse.Tracks[0];
                        player.Queue.Enqueue(track);
                        await ReplyAsync($"Enqueued: {track.Title}");
                    }
                }
                else
                {
                    var track = searchResponse.Tracks[0];

                    if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
                    {
                        for (var i = 0; i < searchResponse.Tracks.Count; i++)
                        {
                            if (i == 0)
                            {
                                await player.PlayAsync(track);
                                await ReplyAsync($"Now Playing: {track.Title}");
                            }
                            else
                            {
                                player.Queue.Enqueue(searchResponse.Tracks[i]);
                            }
                        }

                        await ReplyAsync($"Enqueued {searchResponse.Tracks.Count} tracks.");
                    }
                    else
                    {
                        await player.PlayAsync(track);
                        await ReplyAsync($"Now Playing: {track.Title}");
                    }
                }
            }
        }*/

        public async Task vcdttimer(System.Timers.Timer timer, string fileName)
        {
            timer.Close();
            //var player = _lavaNode.GetPlayer(Context.Guild);
            
            //var track = await _lavaNode.SearchAsync(fileName);

            if (!queue.ContainsKey(Context.Guild.Id))
            {
                queue.Add(Context.Guild.Id, new List<string>() { fileName });
                await _service.SendAudioAsync(Context.Guild);
            }
            else
            {
                queue[Context.Guild.Id].Add(fileName);
            }
            //player.Queue.Enqueue(track.Tracks.First());
        }

        public static void delayDelete(System.Timers.Timer timer, string fileName)
        {
            File.Delete(fileName);
            timer.Close();
        }

        /*public static async Task OnTrackEnded(TrackEndedEventArgs args)
        {
            if (!args.Reason.ShouldPlayNext())
            {
                return;
            }

            var player = args.Player;
            if (!player.Queue.TryDequeue(out var queueable))
            {
                return;
            }

            if (!(queueable is LavaTrack track))
            {
                return;
            }

            //timer for deletion
            System.Timers.Timer timer = new(10000);
            timer.Elapsed += (sender, e) => delayDelete(timer, queue[player.VoiceServer.Guild.Id][0]);
            timer.Start();

            queue[player.VoiceServer.Guild.Id].RemoveAt(0);

            await args.Player.PlayAsync(track);
        }*/

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
