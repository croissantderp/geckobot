using Discord.Commands;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Timers;

namespace geckoBot.modules
{
    //global variables
    public class Globals
    {
        public static int gNumber;
        public static int attempts;
        public static bool easyMode;

        public static string saveData;

        public static Dictionary<string, string> messageDict = new Dictionary<string, string>();

        public static Dictionary<string, string> emoteDict = new Dictionary<string, string>();

        public static List<ulong> dmUsers = new List<ulong>();

        public static int lastrun = DateTime.Now.DayOfYear;

        public static int daysSinceReset = 0;

        public static bool started = false;
    }

    public class commands : ModuleBase<SocketCommandContext>
    {
        static void Save(string data, string path)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(path))
            {
                file.Write(data);
            }
        }

        static string Load(string path)
        {
            string line = "";
            using (System.IO.StreamReader file = new System.IO.StreamReader(path))
            {
                line = file.ReadToEnd();
            }
            return line;
        }

        public string DictToString<T, V>(IEnumerable<KeyValuePair<T, V>> items, string format)
        {
            format = String.IsNullOrEmpty(format) ? "{0}='{1}' " : format;

            StringBuilder itemString = new StringBuilder();
            foreach (var item in items)
                itemString.AppendFormat(format, item.Key, item.Value);

            return itemString.ToString();
        }

        public Dictionary<int, string> notes = new Dictionary<int, string>();
        public Dictionary<int, int> major = new Dictionary<int, int>();
        public Dictionary<int, int> minor = new Dictionary<int, int>();

        public Dictionary<int, int> timeDict = new Dictionary<int, int>();
        public Dictionary<int, int> timeDict2 = new Dictionary<int, int>();

        [Command("send")]
        public async Task send(string sendData, string recipitant)
        {
            bool tooMany = false;

            Globals.messageDict = Load("C:\\gecko.gek").ToString().Split('ҩ').Select(part => part.Split('⁊')).Where(part => part.Length == 2).ToDictionary(sp => sp[0], sp => sp[1]);

            if (sendData.Length > 1000)
            {
                await ReplyAsync("please keep messages to under 1000 characters");
            }
            else
            {
                if (Globals.messageDict.ContainsKey(recipitant))
                {
                    if ((Globals.messageDict[recipitant] + "\n" + sendData + " from " + Context.User).Length <= 2000)
                    {
                        string temp = Globals.messageDict[recipitant];
                        Globals.messageDict.Remove(recipitant);
                        Globals.messageDict.Add(recipitant, temp + "\n" + sendData + " from " + Context.User);
                    }
                    else
                    {
                        tooMany = true;
                    }
                }
                else
                {
                    Globals.messageDict.Add(recipitant, sendData + " from " + Context.User);
                }
                Save(DictToString(Globals.messageDict, "{0}⁊{1}ҩ"), "C:\\gecko.gek");
                await ReplyAsync(tooMany ? "user has too many unread messages!" : "message sent to " + Context.User);
            }
        }

        [Command("messages")]
        public async Task recieve()
        {
            Globals.messageDict = Load("C:\\gecko.gek").ToString().Split('ҩ').Select(part => part.Split('⁊')).Where(part => part.Length == 2).ToDictionary(sp => sp[0], sp => sp[1]);

            if (Globals.messageDict.ContainsKey(Context.User.ToString()))
            {
                await ReplyAsync(Globals.messageDict[Context.User.ToString()]);
            }
            else
            {
                await ReplyAsync("no new messages");
            }
        }

        [Command("clear")]
        public async Task clear()
        {
            Globals.messageDict = Load("C:\\gecko.gek").ToString().Split('ҩ').Select(part => part.Split('⁊')).Where(part => part.Length == 2).ToDictionary(sp => sp[0], sp => sp[1]);
            Globals.messageDict.Remove(Context.User.ToString());
            Save(DictToString(Globals.messageDict, "{0}⁊{1}ҩ"), "C:\\gecko.gek");
            await ReplyAsync("cleared");
        }

        [Command("ess")]
        public async Task ess()
        {
            Globals.emoteDict = Load("C:\\gecko2.gek").ToString().Split('ҩ').Select(part => part.Split('⁊')).Where(part => part.Length == 2).ToDictionary(sp => sp[0], sp => sp[1]);

            IDiscordClient client = Context.Client;
            
            IGuild[] guilds = Context.Client.Guilds.ToArray();

            int emotesAdded = 0;

            foreach (IGuild a in guilds)
            {
                IEmote[] b = a.Emotes.ToArray();

                foreach (IEmote c in b)
                {
                    int count = c.ToString().Length;
                    if (!Globals.emoteDict.ContainsKey(c.ToString().Remove(count - 20, 20).Remove(0, 2)))
                    {
                        Globals.emoteDict.Add(c.ToString().Remove(count - 20, 20).Remove(0, 2), c.ToString());
                        emotesAdded += 1;
                    }
                }
            }
            Save(DictToString(Globals.emoteDict, "{0}⁊{1}ҩ"), "C:\\gecko2.gek");

            await ReplyAsync(emotesAdded.ToString() + " new emotes added");
        }

        [Command("es")]
        public async Task es(string yes1, string yes)
        {
            Globals.emoteDict = Load("C:\\gecko2.gek").ToString().Split('ҩ').Select(part => part.Split('⁊')).Where(part => part.Length == 2).ToDictionary(sp => sp[0], sp => sp[1]);

            if (Globals.emoteDict.ContainsKey(yes1))
            {
                await ReplyAsync("this name is taken, use a different name!");
            }
            else
            {
                string[] temp = yes.Split(":::");

                Globals.emoteDict.Add(yes1, string.Join("", temp.Select(p => p.ToString())));

                Save(DictToString(Globals.emoteDict, "{0}⁊{1}ҩ"), "C:\\gecko2.gek");

                await Context.Message.AddReactionAsync(new Emoji("✅"));
            }
        }

        [Command("er")]
        public async Task er(string yes1)
        {
            Globals.emoteDict = Load("C:\\gecko2.gek").ToString().Split('ҩ').Select(part => part.Split('⁊')).Where(part => part.Length == 2).ToDictionary(sp => sp[0], sp => sp[1]);

            Globals.emoteDict.Remove(yes1);

            Save(DictToString(Globals.emoteDict, "{0}⁊{1}ҩ"), "C:\\gecko2.gek");

            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

        [Command("e")]
        public async Task e(string yes)
        {
            Globals.emoteDict = Load("C:\\gecko2.gek").ToString().Split('ҩ').Select(part => part.Split('⁊')).Where(part => part.Length == 2).ToDictionary(sp => sp[0], sp => sp[1]);

            string[] yesnt = yes.Split("$");

            string[] final = new string[yesnt.Length];

            for (int i = 0; i < yesnt.Length; i++)
            {
                if (Globals.emoteDict.ContainsKey(yesnt[i]))
                {
                    final[i] = Globals.emoteDict[yesnt[i]];
                }
                else
                {
                    final[i] = yesnt[i];
                }
            }
            await ReplyAsync(string.Join("", final.Select(p => p.ToString())));
        }

        [Command("re")]
        public async Task ReactCustomAsync(string channel, string message, string emote)
        {
            Globals.emoteDict = Load("C:\\gecko2.gek").ToString().Split('ҩ').Select(part => part.Split('⁊')).Where(part => part.Length == 2).ToDictionary(sp => sp[0], sp => sp[1]);

            DiscordSocketClient client = Context.Client;
            var chnl = client.GetChannel(ulong.Parse(channel)) as IMessageChannel;
            var message2 = await chnl.GetMessageAsync(ulong.Parse(message));

            string[] yesnt = emote.Split("$");

            for (int i = 0; i < yesnt.Length; i++)
            {

                if (Globals.emoteDict.ContainsKey(yesnt[i]))
                {
                    var emote2 = Emote.Parse(Globals.emoteDict[yesnt[i]]);
                    await message2.AddReactionAsync(emote2);
                }
                else
                {
                    try
                    {
                        var emote2 = Emote.Parse(yesnt[i]);
                        await message2.AddReactionAsync(emote2);
                    }
                    catch
                    {
                        var emote2 = new Emoji(yesnt[i]);
                        await message2.AddReactionAsync(emote2);
                    }
                }
                await Context.Message.AddReactionAsync(new Emoji("✅"));
            }
        }

        void add()
        {
            notes.Add(0, "c");
            notes.Add(1, "c#");
            notes.Add(2, "d");
            notes.Add(3, "d#");
            notes.Add(4, "e");
            notes.Add(5, "f");
            notes.Add(6, "f#");
            notes.Add(7, "g");
            notes.Add(8, "g#");
            notes.Add(9, "a");
            notes.Add(10, "a#");
            notes.Add(11, "b");
            notes.Add(12, "c2");
            notes.Add(13, "c#2");
            notes.Add(14, "d2");
            notes.Add(15, "d#2");
            notes.Add(16, "e2");
            notes.Add(17, "f2");
            notes.Add(18, "f#2");
            notes.Add(19, "g2");
            notes.Add(20, "g#2");
            notes.Add(21, "a2");
            notes.Add(22, "a#2");
            notes.Add(23, "b2");
            notes.Add(24, "c3");
            notes.Add(25, "c#3");
            notes.Add(26, "d3");
            notes.Add(27, "d#3");
            notes.Add(28, "e3");
            notes.Add(29, "f3");
            notes.Add(30, "f#3");
            notes.Add(31, "g3");
            notes.Add(32, "g#3");
            notes.Add(33, "a3");
            notes.Add(34, "a#3");
            notes.Add(35, "b3");
            notes.Add(36, "c4");
            notes.Add(37, "rest");

            minor.Add(1, 2);
            minor.Add(2, 3);
            minor.Add(3, 5);
            minor.Add(4, 7);
            minor.Add(5, 8);
            minor.Add(6, 10);
            minor.Add(7, 12);

            major.Add(1, 2);
            major.Add(2, 4);
            major.Add(3, 5);
            major.Add(4, 7);
            major.Add(5, 9);
            major.Add(6, 11);
            major.Add(7, 12);

            timeDict.Add(1, 2);
            timeDict.Add(2, 3);
            timeDict.Add(3, 4);
            timeDict.Add(4, 5);
            timeDict.Add(5, 8);
            timeDict.Add(6, 16);

            timeDict2.Add(1, 2);
            timeDict2.Add(2, 4);
            timeDict2.Add(3, 8);
        }

        [Command("generate")]
        public async Task generate(int length)
        {
            if (!notes.ContainsKey(0))
            {
                add();
            }

            Random random = new Random();
            
            int measures = length;

            int majorMinor = random.Next(1,3);


            int number = random.Next(12, 25);
            int inKey = number;
            
            int time = timeDict[random.Next(1, 7)];
            int time2 = timeDict2[random.Next(1, 4)];
            int timeRemain = time;
            int dure = 0;

            List<string> noteNames = new List<string>();

            while (measures > 0)
            {
                int noteChance = random.Next(1, 8);

                int newNumber;

                int judge = random.Next(1, 3);
                int rest = random.Next(1, 15);
                newNumber = random.Next(1, 8);

                if (rest == 1 && number != 37)
                {
                    number = 37;
                }
                else
                {
                    if (noteChance == 1)
                    {
                        if (majorMinor == 1)
                        {
                            number = (judge == 2 ? inKey - major[newNumber] : inKey + major[newNumber]);
                        }
                        else
                        {
                            number = (judge == 2 ? inKey - minor[newNumber] : inKey + minor[newNumber]);
                        }
                    }
                    else
                    {
                        if (majorMinor == 2)
                        {
                            number = (judge == 2 ? inKey - major[newNumber] : inKey + major[newNumber]);
                        }
                        else
                        {
                            number = (judge == 2 ? inKey - minor[newNumber] : inKey + minor[newNumber]);
                        }
                    }
                }
                
                timeRemain -= dure;

                int judge2 = random.Next(1,3);

                if (timeRemain <= 0)
                {
                    timeRemain = time;
                    measures -= 1;

                    noteNames.Add("| ");
                }

                if (judge2 == 1)
                {
                    dure = random.Next(1, timeRemain + 1);
                }
                else
                {
                    dure = random.Next(1, (timeRemain + 1) / 2);
                }
                
                noteNames.Add(notes[number] + " **" + dure.ToString() + "**");

                if (measures <= 0)
                {
                    noteNames.RemoveAt(noteNames.Count - 1);
                    break;
                }
            }
            string final = notes[inKey] + (majorMinor == 2 ? " major, in " : " minor, in ") + time.ToString() + "/" + time2.ToString() + System.Environment.NewLine + string.Join(", ", noteNames.Select(p => p.ToString()));
            await ReplyAsync(final);
        }

        [Command("contest")]
        public async Task contest(string user)
        {
            Random random = new Random();
            int number = random.Next(1, 3);
            if (number == 1)
            {
                await ReplyAsync(Context.User + " wins!");
            }
            else if (number == 2)
            {
                await ReplyAsync(user + " wins!");
            }
        }

        [Command("lottery")]
        public async Task lottery()
        {
            string results = " ";
            string[] number2 = new string[6];
            string[] key2 = new string[6];
            int matches = 0;
            Random random = new Random();

            for (int i = 0; i < 6; i++)
            {
                int number = random.Next(1, 100);
                int key = random.Next(1, 100);
                if (number == key)
                {
                    number2[i] = "**" + number.ToString() + "**";
                    key2[i] = "**" + key.ToString() + "**";
                    matches += 1;
                }
                else
                {
                    number2[i] = number.ToString();
                    key2[i] = key.ToString();
                }
            }
            results = "numbs: " + string.Join(" ", number2.Select(p => p.ToString())) + "\nresults: " + string.Join(" ", key2.Select(p => p.ToString())) + "\nmatches: " + matches.ToString();

            await ReplyAsync(results);
        }

        [Command("gec")]
        public async Task gec()
        {
            DateTime date = DateTime.Today;
            string final = (date.DayOfYear - 1).ToString();
            while (final.Length < 3)
            {
                final = "0" + final;
            }
            await Context.Channel.SendFileAsync(filePath: @"D:\Documents\stuff\GeckoImages_for_bot\" + final + "_icon" + (date.DayOfYear == 366 ? ".gif" : ".png"), text: "Today is " + date.ToString("d") + ". Day " + date.DayOfYear + " of the year " + date.Year + " (gecko #" + final + ")");
            if (date.DayOfYear == 366)
            {
                await Context.Channel.SendFileAsync(filePath: @"D:\Documents\stuff\GeckoImages_for_bot\366_icon.gif", text: "Today is " + date.ToString("d") + ". Day " + date.DayOfYear + " of the year " + date.Year + "(gecko #366)");
            }
        }

        [Command("geccolle")]
        public async Task gecColle()
        {
            var embed = new EmbedBuilder
            {
                Title = "gecko collection",
                Description = ("[see the gecko collection here](https://drive.google.com/drive/folders/1Omwv0NNV0k_xlECZq3d4r0MbSbuHC_Og?usp=sharing)")
            };

            embed.WithColor(180, 212, 85);

            var embed2 = embed.Build();

            await ReplyAsync(embed: embed2);
        }

            [Command("rgec")]
        public async Task rgec()
        {
            Random random = new Random();
            int numb = random.Next(0,367);
            string final = (numb).ToString();
            while (final.Length < 3)
            {
                final = "0" + final;
            }
            await Context.Channel.SendFileAsync(filePath: @"D:\Documents\stuff\GeckoImages_for_bot\" + final + "_icon" + (numb == 365 || numb ==366 ? ".gif" : ".png"), text: "gecko #" + final);
        }

        [Command("fgec")]
        public async Task fgec(int value)
        {
            string final = value.ToString();
            while (final.Length < 3)
            {
                final = "0" + final;
            }
            await Context.Channel.SendFileAsync(filePath: @"D:\Documents\stuff\GeckoImages_for_bot\" + final + "_icon" + (value == 365 || value == 366 ? ".gif" : ".png"), text: "gecko #" + final);
        }

        [Command("dm")]
        public async Task dmgec(bool yes)
        {
            if (yes)
            {
                if (Load("C:\\gecko3.gek") != null)
                {
                    Globals.dmUsers.Clear();
                    string[] temp = Load("C:\\gecko3.gek").Split(",");
                    foreach (string a in temp)
                    {
                        Globals.dmUsers.Add(ulong.Parse(a));
                    }
                }
                IUser user = Context.User;
                if (Globals.dmUsers.Contains(user.Id))
                {
                    await ReplyAsync("you have aleady signed up!");
                }
                else
                {
                    Globals.dmUsers.Add(user.Id);

                    Save(string.Join(",", Globals.dmUsers.ToArray()), "C:\\gecko3.gek");

                    await Discord.UserExtensions.SendMessageAsync(user, "hi, daily gecko updates have been set up, cancel by '\\`dm false'");
                }

            }
            else
            {
                if (Load("C:\\gecko3.gek") != null)
                {
                    Globals.dmUsers.Clear();
                    string[] temp = Load("C:\\gecko3.gek").Split(",");
                    foreach (string a in temp)
                    {
                        Globals.dmUsers.Add(ulong.Parse(a));
                    }
                }
                
                IUser user = Context.User;

                if (Globals.dmUsers.Contains(user.Id))
                {
                    await Discord.UserExtensions.SendMessageAsync(user, "hi, daily gecko updates have been canceled");
                    Globals.dmUsers.Remove(user.Id);

                    Save(string.Join(",", Globals.dmUsers.ToArray()), "C:\\gecko3.gek");
                }
                else
                {
                    await ReplyAsync("you are already not signed up!");
                }
            }
        }

        [Command("start")]
        public async Task test()
        {
            if (Globals.started)
            {
                await ReplyAsync("already started");
            }
            else
            {
                Start();
                await ReplyAsync("started");
            }
        }

        [Command("check")]
        public async Task check(string passcode)
        {
            if (passcode == "crested gecko 2021")
            {
                Globals.lastrun = DateTime.Now.DayOfYear - 1;

                //await dailydm();

                await ReplyAsync("checked and force updated");
            }
            else if (Globals.lastrun != DateTime.Now.DayOfYear)
            {
                await dailydm();

                await ReplyAsync("checked and updated");
            }
            else
            {
                await ReplyAsync("checked");
            }
        }

        public void Start()
        {
            Timer timer = new Timer(1000*60*60);
            timer.Elapsed += new System.Timers.ElapsedEventHandler(daily);
            timer.Start();

            Globals.started = true;
        }

        public async void daily(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Globals.lastrun != DateTime.Now.DayOfYear)
            {
                Globals.daysSinceReset += 1;
                await dailydm();
            }
        }

        public async Task dailydm()
        {
            if (Load("C:\\gecko3.gek") != null)
            {
                Globals.dmUsers.Clear();
                string[] temp = Load("C:\\gecko3.gek").Split(",");

                foreach (string a in temp)
                {
                    Globals.dmUsers.Add(ulong.Parse(a));
                }
            }

            DateTime date = DateTime.Today;
            string final = (date.DayOfYear - 1).ToString();
            while (final.Length < 3)
            {
                final = "0" + final;
            }

            DiscordSocketClient client = Context.Client;

            foreach (ulong a in Globals.dmUsers)
            {
                IUser b = client.GetUser(a);
                await Discord.UserExtensions.SendFileAsync(b, filePath: @"D:\Documents\stuff\GeckoImages_for_bot\" + final + "_icon" + (date.DayOfYear == 366 ? ".gif" : ".png"), text: "Today is " + date.ToString("d") + ". The " + date.DayOfYear + " day of the year (gecko #" + final + ")");
                if (date.DayOfYear == 366)
                {
                    await Discord.UserExtensions.SendFileAsync(b, filePath: @"D:\Documents\stuff\GeckoImages_for_bot\366_icon" + ".gif", text: "Today is " + date.ToString("d") + ". The " + date.DayOfYear + " day of the year (gecko #366)");
                }
            }

            Globals.lastrun = DateTime.Now.DayOfYear;
        }

        [Command("timer")]
        public async Task timer(string message, string time)
        {
            string[] times1 = time.Split(":");
            int[] times2 = new int[3];
            for (int i = 0; i < 3; i++)
            {
                times2[i] = int.Parse(times1[i]);
            }

            IUser user = Context.User;

            Timer timer = new Timer((times2[0] * 60 * 60) + (times2[1] * 60) + (times2[2])* 1000);
            timer.Elapsed += async (sender, e) => await timerUp(user, message, timer);
            timer.Start(); 
            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

        [Command("alarm")]
        public async Task alarm(string message, string time)
        {
            string[] times1 = time.Split(":");
            int[] times2 = new int[3];
            for (int i = 0; i < 3; i++)
            {
                times2[i] = int.Parse(times1[i]);
            }
            int hour = DateTime.Now.Hour;
            int minute = DateTime.Now.Minute;
            int second = DateTime.Now.Second;

            int final = (((times2[0] - hour) * 60 * 60) + ((times2[1] - minute) * 60) + (times2[2] - second)) * 1000;

            IUser user = Context.User;

            Timer timer = new Timer(final);
            timer.Elapsed += async (sender, e) => await timerUp(user, message, timer);
            timer.Start();
            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

        public async Task timerUp(IUser user, string message, Timer timer2)
        {
            await Discord.UserExtensions.SendMessageAsync(user, message);
            timer2.Stop();
        }

        [Command("tem")]
        public async Task tem()
        {
            Random random = new Random();

            int number = random.Next(0, 1000);

            if (number == 0)
            {
                await ReplyAsync("░░░░░░░░░░▄▄░░░░░░░░░░░░░░░░░░░░░░░░░░░" + System.Environment.NewLine +
                "░░░░░░░░░██▀█▄░░▄██▀░░░░▄██▄░░░░░░░░░░░" + System.Environment.NewLine +
                "░░░░░░░░██▄▄▄████████▄▄█▀░▀█░░░░░░░░░░░" + System.Environment.NewLine +
                "░░░░░░░▄██████████████████▄█░░░░░░░░░░░" + System.Environment.NewLine +
                "░░░░░░▄██████████████████████░░░░░░░░░░" + System.Environment.NewLine +
                "░░░░░▄████████▀░▀█████████████░░░░░░░░░" + System.Environment.NewLine +
                "░░░░░███████▀░░░░▀████████████▄░░░░░░░░" + System.Environment.NewLine +
                "░░░░██████▀░░░░░░░░▀███████████▄▄▄▀▀▄░░" + System.Environment.NewLine +
                "░▄▀▀████▀░░░▄▄░░░░░░░░░████████░░░░░█░░" + System.Environment.NewLine +
                "█░░░▀█▀█░░░░▀▀░░░░░░██░░███░██▀░░░░▄▀░░" + System.Environment.NewLine +
                "█░░░░▀██▄░░░▄░░░▀░░░▄░░░███░██▄▄▄▀▀░░░░" + System.Environment.NewLine +
                "░▀▄▄▄▄███▄░░▀▄▄▄▀▄▄▄▀░░░███░█░░░░░░░░░░" + System.Environment.NewLine +
                "░░░░░░████▄░░░░░░░░░░░░▄██░█░░░░░░░░▄▀▀" + System.Environment.NewLine +
                "░░░░░░███▀▀█▀▄▄▄▄▄▄▄▄▄▀█████░░░░░░▄▀░░▄" + System.Environment.NewLine +
                "░░░░░░█▀░░█░▀▄▄▄▄▄▄▄▄▄▀░███░█▀▀▀▄▀░░▄▀░" + System.Environment.NewLine +
                "░░░░░░░░░▄▀░░░░░░░░░░░░░▀█▀░░█░░░░█▀░░░" + System.Environment.NewLine +
                "░░░░░░░░░█░░░█░░░░░░░░░░░░░░░█░░░░░█░░░" + System.Environment.NewLine +
                "░░░░░░░░░█▄▄█░▀▄░░█▄░░░░░░░░█░░▄█░░█░░░" + System.Environment.NewLine +
                "░░░░░░░░░▀▄▄▀░░█▀▀█░▀▀▀▀█▀▀█▀▀▀▀█░█░░░░" + System.Environment.NewLine);
            }
        }

        [Command("what do you")]
        public async Task instructions(string section)
        {
            bool found = true;

            var embed = new EmbedBuilder
            {
            Title = "geckobot 1/14/2020 instruction manual"
            };

            if (section == "do?")
            {
                embed.Description = ("my prefix is \\`." + System.Environment.NewLine +
                "(highly recommended to have developer mode on to easily use)" + System.Environment.NewLine +
                "if there's a problem, ping my owner croissantderp#4167 or leave a card on the [trello](https://trello.com/invite/b/cFS33M13/8fddf3ac42bd0fe419e482c6f4414e01/gecko-bot-todo)" + System.Environment.NewLine + System.Environment.NewLine + 
                "sections (replace 'do?' with these): general | gecko | random | sigfigs | message | emote | embed | edit | za_warudo" + System.Environment.NewLine + System.Environment.NewLine +
                "**In order to not make admins angery, consider using a spam channel for these commands as they are lengthy.**"
                );
            }
            else if (section == "general")
            {
                embed.AddField("**general**",
                    "I can do math with '\\`add [value] [value]' replace add with subtract, multiply and divide to do other operations." + System.Environment.NewLine + System.Environment.NewLine +
                    "To play rock paper scissors, enter '\\`rps [scissors/rock/paper]'." + System.Environment.NewLine + System.Environment.NewLine
                    );
            }
            else if (section == "gecko")
            {
                embed.AddField("**gecko images:**",
                    "to see the entire collection try '\\`geccolle' [go here](https://drive.google.com/drive/folders/1Omwv0NNV0k_xlECZq3d4r0MbSbuHC_Og?usp=sharing)" + System.Environment.NewLine + System.Environment.NewLine +
                    "to see the daily gecko profile picture, try '\\`gec' " + System.Environment.NewLine + System.Environment.NewLine +
                    "to see a random gecko profile picture, try '\\`rgec'" + System.Environment.NewLine + System.Environment.NewLine +
                    "to see a find a gecko profile picture, try '\\`fgec [int]' where int is the gecko#" + System.Environment.NewLine + System.Environment.NewLine +
                    "to recieve daily gecko dms, use '\\`dm [true/false]' where true is to sign up and false is to cancel" + System.Environment.NewLine + System.Environment.NewLine +
                    "to start the daily clock use '\\`start', manually check by '`check [passcode for force-checking]' use a blank space surrounded by quotes for regular checking" + System.Environment.NewLine + System.Environment.NewLine
                    );
            }
            else if (section == "random")
            {
                embed.AddField("**random shenanigans:**",
                    "To generate a random number enter '\\`rng [min value] [max value]'." + System.Environment.NewLine + System.Environment.NewLine +
                    "To generate a random sequence of music, enter '\\`generate [number of measures in sequence]'" + System.Environment.NewLine + System.Environment.NewLine +
                    "Enter '\\`lottery' to try a 1/100^6 lottery" + System.Environment.NewLine + System.Environment.NewLine +
                    "To do a random contest thingy, enter '\\`contest [opponent name]'" + System.Environment.NewLine + System.Environment.NewLine +
                    "To play a number guessing game, first generate a number by entering '\\`g new [min value] [max value]' then enter '\\`g [value]' to guess" + System.Environment.NewLine + System.Environment.NewLine
                    );
            }
            else if (section == "sigfigs")
            {
                embed.AddField("**sigfigs:**",
                    "To see the significant figures of a number, enter '\\`sigfig [number]'" + System.Environment.NewLine + System.Environment.NewLine +
                    "To do sigfig math, do '\\`sigfig add [value] [value]' replace with other operations for those" + System.Environment.NewLine + System.Environment.NewLine
                    );
            }
            else if (section == "message")
            {
                embed.AddField("**messaging:**",
                    "To send a message type '\\`send [string to send, no spaces] [recipitant, whole name, ex. gecko bot#4897 ]'" + System.Environment.NewLine + System.Environment.NewLine +
                    "To see messages type '\\`messages'" + System.Environment.NewLine + System.Environment.NewLine +
                    "To clear messages type '\\`clear'" + System.Environment.NewLine
                    );
            }
            else if (section == "emote")
            {
                embed.AddField("**global emotes:**",
                    "Strange discord oversight/intentional design lets bots use emotes globally!" + System.Environment.NewLine +
                    "To log an emote so it can be used globally, type '\\`es [common name] [actual emote here]' make sure the actual emote actually sends as an emote or it won't work. " + System.Environment.NewLine + System.Environment.NewLine +
                    "To save an animated emote, get the emote id (formated like this: <a:[name]:[id]>) by entering backslash before the emote and copying the message, then paste it and the id will be there, remember to remove the backslash when saving. Then insert a random ':::' anywhere in the id and type '\\`es [common name] [emote id with random ::: ]'" + System.Environment.NewLine + System.Environment.NewLine +
                    "geckobot can also log every emote of every server that it is in by '\\`ess'" + System.Environment.NewLine + System.Environment.NewLine +
                    "To remove an emote, type '\\`er [common name]'" + System.Environment.NewLine + System.Environment.NewLine +
                    "To react, type '\\`re [channel id] [message id] [emote or common name]' global emote thing works too and you can string many together with '$'"
                    );
            }
            else if (section == "embed")
            {
                embed.AddField("**embed builder:**",
                    "build an embed without breaking discord tos!" + System.Environment.NewLine + System.Environment.NewLine +
                    "the command is '\\`embed [title of embed] [fields, fields are seperated by '$$' title and description are seperated by '%%'] [footer] [hexidecimal color]'" + System.Environment.NewLine + System.Environment.NewLine +
                    "it supports hyperlink markdown, example: '\\[hyperlink markdown](https://example.com)' would make [hyperlink markdown](https://example.com)"
                    );
            }
            else if (section == "edit")
            {
                embed.AddField("**cursed edits**",
                    "do cursed things with (edited) messages" + System.Environment.NewLine + System.Environment.NewLine +
                    "to insert edited randomly with a message use '\\`edit [text before edit] [text after edit]'" + System.Environment.NewLine + System.Environment.NewLine +
                    "to flip the parans of the (edited) use '\\`flip [line one text] [line two text]'"
                    );
            }
            else if (section == "za_warudo")
            {
                embed.AddField("**timer and alarm**",
                    "to set a timer '\\`timer [message to send after timer] [amount of time in hh:mm:ss format]'" + System.Environment.NewLine + System.Environment.NewLine +
                    "to set an alarm '\\`alarm [message to send after alarm] [alarm time in (24 hr style) hh:mm:ss format]'"
                    );
            }
            else
            {
                await Context.Channel.SendFileAsync(filePath: @"D:\Documents\stuff\GeckoImages_for_bot\message.gif");
                found = false;
            }

            if (found)
            {
                embed.AddField("**counter**",
                    Globals.daysSinceReset + " days since this bot has been updated"
                    );

                embed.WithColor(180, 212, 85);

                var embed2 = embed.Build();

                await ReplyAsync(embed: embed2);
            }
        }

        [Command("embed")]
        public async Task embed(string title, string field, string footer2, string hex)
        {
            if (hex.IndexOf('#') != -1)
            {
                hex = hex.Replace("#", "");
            }

            int r, g, b = 0;

            r = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
            g = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
            b = int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.AllowHexSpecifier);

            var embed = new EmbedBuilder
            {
                Title = title
            };

            string[] fields = field.Split("$$");

            foreach(string a in fields)
            {
                string[] subfields = a.Split("%%");
                if (subfields.Length == 1)
                {
                    embed.AddField("​", subfields[0]);
                }
                else
                {
                    embed.AddField(subfields[0], subfields[1]);
                }
            }
            embed.WithColor(r,g,b);
            embed.WithAuthor(Context.User);
            embed.WithFooter(footer => footer.Text = footer2);
            embed.WithCurrentTimestamp();

            await ReplyAsync(embed: embed.Build());
        }

        [Command("fek")]
        public async Task fek()
        {
            Random random = new Random();
            int charNum = random.Next(2, 10);
            
            string[] charFinal = new string[charNum];
            for (int i = 0; i < charNum; i++)
            {
                int num = random.Next(0, 26);
                char let = (char)('a' + num);
                charFinal[i] = let.ToString();
            }
            await ReplyAsync(string.Join("", charFinal));
        }

        [Command("flip")]
        public async Task flip(string text, string text2)
        {
            string final = "؜" + text + "\n" + text2 + "؜؜؜";
            var Message1 = await ReplyAsync("ahaaha");
            await Message1.ModifyAsync(m => { m.Content = final; });
        }

        [Command("edit")]
        public async Task edit(string text, string text2)
        {
            string final = "؜" + text2 + "؜؜؜؜؜؜؜؜؜؜؜؜" + text;
            var Message1 = await ReplyAsync("ahaaha");
            await Message1.ModifyAsync(m => { m.Content = final; });
        }

        //maf
        [Command("add")]
        public async Task add(decimal num1, decimal num2)
        {
            await ReplyAsync((num1 + num2).ToString());
        }

        [Command("subtract")]
        public async Task subtract(decimal num1, decimal num2)
        {
            await ReplyAsync((num1 - num2).ToString());
        }

        [Command("multiply")]
        public async Task multiply(decimal num1, decimal num2)
        {
            await ReplyAsync((num1 * num2).ToString());
        }

        [Command("divide")]
        public async Task divide(decimal num1, decimal num2)
        {
            await ReplyAsync((num1 / num2).ToString());
        }

        [Command("rng")]
        public async Task rng(int min, int max)
        {
            Random random = new Random();
            int number = random.Next(min, max + 1);
            await ReplyAsync(number.ToString());
        }

        [Command("save")]
        public async Task save(string saved)
        {
            Globals.saveData = saved;
            await ReplyAsync("saved");
        }

        [Command("load")]
        public async Task load()
        {
            await ReplyAsync(Globals.saveData);
        }
    }

    [Group("sigfig")]
    public class sigfig : ModuleBase<SocketCommandContext>
    {
        public string[] figures(decimal number)
        {
            //converts number to a string
            string yes = number.ToString();
            //splits the number into two strings, one of the digits before the decimal, one of them after
            string[] numberArray = yes.Split(".");
            //a list of the numbers after the decimal point
            List<string> Decimal = new List<string>();
            //number of sigfigs in number
            int SigNum = 0;

            //a list of numbers before the decimal point
            List<string> numberList = new List<string>();
            //adds numbers
            numberList.Add(numberArray[0]);

            //if a decimal exists
            if (numberArray.Length == 2)
            {
                //adds the decimal point to the list
                Decimal.Add(".");
                //adds ** which bolds text
                Decimal.Add("**");
                //adds the number
                Decimal.Add(numberArray[1]);
                Decimal.Add("**");

                //if there are numbers before the decimal
                if (numberList[0] != ("0") && numberList[0] != ("-0"))
                {
                    //bolds everythings
                    numberList.Add("**");
                    numberList.Insert(0, "**");

                    //counts total number of sigfigs
                    SigNum = numberList[1].Length + Decimal[2].Length;
                }
                else
                {
                    //counts total number of sigfigs
                    SigNum = Decimal[2].Length;
                }
            }
            //if decimal does not exist
            else if (numberArray.Length == 1)
            {
                //temporary string
                string temp = numberList[0];

                //gets rid of exponential notation
                decimal Value = decimal.Parse(temp, System.Globalization.NumberStyles.Float);
                //number of 0s in the number
                int Zeros = 0;

                //temp string array
                string[] temp3 = new string[2];

                //divides by 10
                for (int i = 0; i < numberList[0].Length; i++)
                {
                    //if number is divisible by 10, do it
                    if (Value % 10 == 0)
                    {
                        Value /= 10;
                        Zeros += 1;
                    }
                    //if number is no longer divisible by 10
                    else
                    {
                        //new string array for adding values
                        string[] temp2 = new string[Zeros + 1];
                        //adds value to string array with bolding
                        temp2[0] = ("**" + Value.ToString(new string('#', 339)) + "**");

                        //counts total number of sigfigs
                        SigNum = Value.ToString(new string('#', 339)).Length;

                        //adds zeros to end of number
                        for (int j = 0; j < Zeros; j++)
                        {
                            temp2[j + 1] = "0";
                        }
                        //joins new string array to a single string
                        temp = string.Join("", temp2);

                        //passes value to final number list
                        numberList[0] = temp;
                        break;
                    }
                }
            }
            string[] final = { string.Join("", numberList.Select(p => p.ToString())), string.Join("", Decimal.Select(p => p.ToString())), SigNum.ToString()};
            return final;
        }

        [Command("")]
        public async Task sigfigbase(decimal number)
        {
            //constructs reply
            await ReplyAsync(string.Join("", figures(number)[0] + figures(number)[1] + " " + figures(number)[2]));
        }

        private decimal RoundNum(decimal d, int digits)
        {
            int neg = 1;
            if (d < 0)
            {
                d = d * (-1);
                neg = -1;
            }

            int n = 0;
            if (d > 1)
            {
                while (d > 1)
                {
                    d = d / 10;
                    n++;
                }
                d = Math.Round(d * (decimal)Math.Pow(10, digits));
                d = d * (decimal)Math.Pow(10, n - digits);
            }
            else
            {
                while ((double)d < 0.1)
                {
                    d = d * 10;
                    n++;
                }
                d = Math.Round(d * (decimal)Math.Pow(10, digits));
                d = d / (decimal)Math.Pow(10, n + digits);
            }

            return d * neg;
        }

        [Command("add")]
        public async Task sigAdd(decimal number1, decimal number2)
        {
            decimal finalNum = number1 + number2;
            decimal final = 0;

            int accuracy1 = int.Parse(figures(number1)[2]);
            int accuracy2 = int.Parse(figures(number2)[2]);

            bool isDecimal = false;

            string[] extra = new string[final.ToString().Length+1];

            string[] check1 = new string[2];
            string[] check2 = new string[2];
            check1 = number1.ToString().Split(".");
            check2 = number2.ToString().Split(".");

            if (number1.ToString().Contains(".") && number2.ToString().Contains("."))
            {
                isDecimal = true;
            }
            
            if (isDecimal)
            {
                if (accuracy1 < check2[0].Length)
                {
                    accuracy1 = check2[0].Length;
                }
                if (accuracy1 < check1[0].Length)
                {
                    accuracy1 = check1[0].Length;
                }
                if (accuracy2 < check1[0].Length)
                {
                    accuracy2 = check1[0].Length;
                }
                if (accuracy2 < check2[0].Length)
                {
                    accuracy2 = check2[0].Length;
                }

                if (accuracy1 <= accuracy2)
                {
                    final = RoundNum(finalNum, accuracy1);

                    if (accuracy1 > final.ToString().Length)
                    {
                        extra[0] = ".";
                        for (int i = 0; i < accuracy1 - final.ToString().Length; i++)
                        {
                            extra[i + 1] = "0";
                        }
                    }
                }
                else
                {
                    final = RoundNum(finalNum, accuracy2);

                    if (accuracy2 > final.ToString().Length)
                    {
                        extra[0] = ".";
                        for (int i = 0; i < accuracy2 - final.ToString().Length; i++)
                        {
                            extra[i + 1] = "0";
                        }
                    }
                }
            }
            else
            {
                final = finalNum;
            }

            if (!isDecimal)
            {
                int final2 = (int)final;
                await ReplyAsync(string.Join("", figures(final2)[0] + figures(final2)[1] + " " + figures(final2)[2]));
            }
            else
            {
                decimal e = decimal.Parse(final.ToString() + string.Join("", extra));

                await ReplyAsync(string.Join("", figures(e)[0] + figures(e)[1] + " " + figures(e)[2]));
            }
        }

        [Command("subtract")]
        public async Task sigSubtract(decimal number1, decimal number2)
        {
            decimal finalNum = number1 - number2;
            decimal final = 0;
            int accuracy1 = int.Parse(figures(number1)[2]);
            int accuracy2 = int.Parse(figures(number2)[2]);

            bool isDecimal = false;

            string[] extra = new string[final.ToString().Length + 1];

            string[] check1 = new string[2];
            string[] check2 = new string[2];
            check1 = number1.ToString().Split(".");
            check2 = number2.ToString().Split(".");

            if (number1.ToString().Contains(".") && number2.ToString().Contains("."))
            {
                isDecimal = true;
            }
            

            if (isDecimal)
            {
                if (accuracy1 < check2[0].Length)
                {
                    accuracy1 = check2[0].Length;
                }
                if (accuracy1 < check1[0].Length)
                {
                    accuracy1 = check1[0].Length;
                }
                if (accuracy2 < check1[0].Length)
                {
                    accuracy2 = check1[0].Length;
                }
                if (accuracy2 < check2[0].Length)
                {
                    accuracy2 = check2[0].Length;
                }

                if (accuracy1 <= accuracy2)
                {
                    final = RoundNum(finalNum, accuracy1-2);

                    if (accuracy1 > final.ToString().Length)
                    {
                        extra[0] = ".";
                        for (int i = 0; i < accuracy1 - final.ToString().Length; i++)
                        {
                            extra[i + 1] = "0";
                        }
                    }
                }
                else
                {
                    final = RoundNum(finalNum, accuracy2-2);

                    if (accuracy2 > final.ToString().Length)
                    {
                        extra[0] = ".";
                        for (int i = 0; i < accuracy2 - final.ToString().Length; i++)
                        {
                            extra[i + 1] = "0";
                        }
                    }
                }
            }
            else
            {
                final = finalNum;
            }

            if (!isDecimal)
            {
                int final2 = (int)final;
                await ReplyAsync(string.Join("", figures(final2)[0] + figures(final2)[1] + " " + figures(final2)[2]));
            }
            else
            {
                decimal e = decimal.Parse(final.ToString() + string.Join("", extra));

                await ReplyAsync(string.Join("", figures(e)[0] + figures(e)[1] + " " + figures(e)[2]));
            }
        }

        [Command("multiply")]
        public async Task sigMultiply(decimal number1, decimal number2)
        {
            decimal finalNum = number1 * number2;

            int accuracy1 = int.Parse(figures(number1)[2]);
            int accuracy2 = int.Parse(figures(number2)[2]);

            decimal final = 0;

            string[] extra = new string[final.ToString().Length+1];

            if (accuracy1 <= accuracy2)
            {
                final = RoundNum(finalNum, accuracy1);
                if (accuracy1 > final.ToString().Length)
                {
                    extra[0] = ".";
                    for (int i = 0; i < accuracy1 - final.ToString().Length; i++)
                    {
                        extra[i+1] = "0";
                    }
                }
            }
            else
            {
                final = RoundNum(finalNum, accuracy2);
                if (accuracy2 > final.ToString().Length)
                {
                    extra[0] = ".";
                    for (int i = 0; i < accuracy2 - final.ToString().Length; i++)
                    {
                        extra[i+1] = "0";
                    }
                }
            }
            Console.WriteLine();
            decimal e = decimal.Parse(final.ToString() + string.Join("", extra));

            if (e.ToString().Length > accuracy1 + 1 || e.ToString().Length > accuracy2 + 1)
            {
                double f = double.Parse(final.ToString() + string.Join("", extra));
                await ReplyAsync(string.Join("", figures((decimal)f)[0] + figures((decimal)f)[1] + " " + figures((decimal)f)[2]));
            }
            else
            {
                await ReplyAsync(string.Join("", figures(e)[0] + figures(e)[1] + " " + figures(e)[2]));
            }
        }

        [Command("divide")]
        public async Task sigDivide(decimal number1, decimal number2)
        {
            decimal finalNum = number1 / number2;
            int accuracy1 = int.Parse(figures(number1)[2]);
            int accuracy2 = int.Parse(figures(number2)[2]);

            decimal final = 0;

            string[] extra = new string[final.ToString().Length + 1];

            if (accuracy1 <= accuracy2)
            {
                final = RoundNum(finalNum, accuracy1);
                if (accuracy1 > final.ToString().Length)
                {
                    extra[0] = ".";
                    for (int i = 0; i < accuracy1 - final.ToString().Length; i++)
                    {
                        extra[i + 1] = "0";
                    }
                }
            }
            else
            {
                final = RoundNum(finalNum, accuracy2);
                if (accuracy2 > final.ToString().Length)
                {
                    extra[0] = ".";
                    for (int i = 0; i < accuracy2 - final.ToString().Length; i++)
                    {
                        extra[i + 1] = "0";
                    }
                }
            }

            decimal e = decimal.Parse(final.ToString() + string.Join("", extra));

            if (e.ToString().Length > accuracy1 + 1 || e.ToString().Length > accuracy2 + 1)
            {
                double f = double.Parse(final.ToString() + string.Join("", extra));
                await ReplyAsync(string.Join("", figures((decimal)f)[0] + figures((decimal)f)[1] + " " + figures((decimal)f)[2]));
            }
            else
            {
                await ReplyAsync(string.Join("", figures(e)[0] + figures(e)[1] + " " + figures(e)[2]));
            }
        }
    }

    //rock paper scissors
    [Group("rps")]
    public class rps : ModuleBase<SocketCommandContext>
    {
        [Command("scissors")]
        public async Task scissors()
        {
            await ReplyAsync("a rock crushes your scissors's hopes and dreams");
        }

        [Command("rock")]
        public async Task rock()
        {
            await ReplyAsync("paper envelops your rock, digesting it and consuming it's nutrients");
        }

        [Command("paper")]
        public async Task paper()
        {
            await ReplyAsync("scissors ruthlessly stab through your paper, leaving deep wounds and killing your paper");
        }

        [Command("gun")]
        public async Task gun()
        {
            await ReplyAsync("fine, you win. Now if you would lower the gun. please?");
        }
    }

    //number guessing game
    [Group("g")]
    public class guess : ModuleBase<SocketCommandContext>
    {
        [Command("new")]
        public async Task newNumber(int min, int max)
        {
            Random random = new Random();
            int number = random.Next(min, max + 1);
            await ReplyAsync(min == max ? "achievement get! play on the easiest difficulty!" : "new number generated between " + min.ToString() + " and " + max.ToString());
            Globals.gNumber = number;
            Globals.easyMode = (min == max);
            Globals.attempts = 0;
        }

        [Command("")]
        public async Task attempt(int value)
        {
            int gNumber = Globals.gNumber;
            if (value < gNumber)
            {
                await ReplyAsync("too low");
                Globals.attempts += 1;
            }
            else if (value > gNumber)
            {
                await ReplyAsync("too high");
                Globals.attempts += 1;
            }
            else
            {
                Globals.attempts += 1;
                await ReplyAsync(Globals.easyMode && Globals.attempts > 1 ? "achievement get! lose the game on the easiest difficulty!" :  Context.User + " got it! The number was " + gNumber.ToString() + ". It took " + Globals.attempts.ToString() + " attempts!");
            }
        }
    }
}
