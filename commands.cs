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
using System.Text.RegularExpressions;

namespace geckoBot.modules
{
    //global variables
    public class Globals
    {
        //guessing game variables
        public static int gNumber;
        public static int attempts;
        public static bool easyMode;

        //emote dictionary
        public static Dictionary<string, string> emoteDict = new Dictionary<string, string>();

        //people to dm for daily gecko images
        public static List<ulong> dmUsers = new List<ulong>();

        //last time bot was run and daily geckoimage was sent
        public static int lastrun = DateTime.Now.DayOfYear;

        //days since bot was reset
        public static int daysSinceReset = 0;

        //is timer is running
        public static bool started = false;

        //various dictionaries for music generation
        public static Dictionary<int, string> notes = new Dictionary<int, string>();
        public static Dictionary<int, int> major = new Dictionary<int, int>();
        public static Dictionary<int, int> minor = new Dictionary<int, int>();

        public static Dictionary<int, int> timeDict = new Dictionary<int, int>();
        public static Dictionary<int, int> timeDict2 = new Dictionary<int, int>();

        public static string[] names = { " 1: A Game Of Tokens", " 2: Electric boogaloo", " 3: return of the rbot", " Act 4: flight of the paradox bots", " V: Artoo Strikes Back", " 6: The Undiscovered Server", " and the deathly nitros", " part 8: Geckolion", " IX: Rise of Moofy" };
        public static int currentValue = 0;
    }

    public class commands : ModuleBase<SocketCommandContext>
    {
        //file save function
        static void Save(string data, string path)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(path))
            {
                file.Write(data);
            }
        }

        //file load function
        static string Load(string path)
        {
            string line = "";
            using (System.IO.StreamReader file = new System.IO.StreamReader(path))
            {
                line = file.ReadToEnd();
            }
            return line;
        }

        //dictionary to string
        public string DictToString<T, V>(IEnumerable<KeyValuePair<T, V>> items, string format)
        {
            format = String.IsNullOrEmpty(format) ? "{0}='{1}' " : format;

            StringBuilder itemString = new StringBuilder();
            foreach (var item in items)
                itemString.AppendFormat(format, item.Key, item.Value);

            return itemString.ToString();
        }

        //big save function
        [Command("ess")]
        public async Task ess()
        {
            //loads emote dictionary as string and converts it back into dictionary
            Globals.emoteDict = Load("C:\\gecko2.gek").ToString().Split('ҩ').Select(part => part.Split('⁊')).Where(part => part.Length == 2).ToDictionary(sp => sp[0], sp => sp[1]);

            //gets client
            IDiscordClient client = Context.Client;
            
            //gets guilds
            IGuild[] guilds = Context.Client.Guilds.ToArray();

            //number of emotes added
            int emotesAdded = 0;

            foreach (IGuild a in guilds)
            {
                //gets every emote in a guild
                IEmote[] b = a.Emotes.ToArray();

                foreach (IEmote c in b)
                {
                    //gets number of total characters in the emote name
                    int count = c.ToString().Length;

                    //shortened name of the emote
                    string name = c.ToString().Remove(count - 20, 20).Remove(0, 2);

                    //if the emote dictionary already contains a key or emote contains banned characters
                    if (!Globals.emoteDict.ContainsKey(name) && !name.Contains("ҩ") && !name.Contains("⁊"))
                    {
                        //adds to emote dictionary
                        Globals.emoteDict.Add(name, c.ToString());

                        //adds to counter
                        emotesAdded += 1;
                    }
                }
            }

            //converts dictionary to string and saves
            Save(DictToString(Globals.emoteDict, "{0}⁊{1}ҩ"), "C:\\gecko2.gek");

            //replies with number of new emotes added
            await ReplyAsync(emotesAdded.ToString() + " new emotes added");
        }

        //save function
        [Command("es")]
        public async Task es(string yes1, string yes)
        {
            //loads emote dictionary as string and converts it back into dictionary
            Globals.emoteDict = Load("C:\\gecko2.gek").ToString().Split('ҩ').Select(part => part.Split('⁊')).Where(part => part.Length == 2).ToDictionary(sp => sp[0], sp => sp[1]);

            //if emote name contains banned characters
            if (yes1.Contains("⁊") || yes1.Contains("ҩ") || yes.Contains("⁊") || yes.Contains("ҩ"))
            {
                await ReplyAsync("saved things cannot contain 'ҩ' or '⁊'!");
            }
            else
            {
                //if emote dictionary already has a definition for the new key
                if (Globals.emoteDict.ContainsKey(yes1))
                {
                    await ReplyAsync("this name is taken, use a different name!");
                }
                else
                {
                    //removes ::: for animated saving
                    string[] temp = yes.Split(":::");

                    //joins the split string and saves to emote dictionary
                    Globals.emoteDict.Add(yes1, string.Join("", temp.Select(p => p.ToString())));

                    //converts dictionary to string and saves
                    Save(DictToString(Globals.emoteDict, "{0}⁊{1}ҩ"), "C:\\gecko2.gek");

                    //adds reaction
                    await Context.Message.AddReactionAsync(new Emoji("✅"));
                }
            }

        }

        //removal function
        [Command("er")]
        public async Task er(string yes1)
        {
            //loads emote dictionary as string and converts it back into dictionary
            Globals.emoteDict = Load("C:\\gecko2.gek").ToString().Split('ҩ').Select(part => part.Split('⁊')).Where(part => part.Length == 2).ToDictionary(sp => sp[0], sp => sp[1]);

            //if key is found
            if (Globals.emoteDict.ContainsKey(yes1))
            {
                //removes key
                Globals.emoteDict.Remove(yes1);

                //converts dictionary to string and saves
                Save(DictToString(Globals.emoteDict, "{0}⁊{1}ҩ"), "C:\\gecko2.gek");

                //adds reaction
                await Context.Message.AddReactionAsync(new Emoji("✅"));
            }
            else
            {
                //if emote is not found
                await ReplyAsync("emote not found!");
            }
        }

        //simple retrival function
        [Command("e")]
        public async Task e(string yes)
        {
            await ReplyAsync(emoteReplace(yes));
        }

        //replaces strings with emotes
        public string emoteReplace(string stuff)
        {
            //loads emote dictionary as string and converts it back into dictionary
            Globals.emoteDict = Load("C:\\gecko2.gek").ToString().Split('ҩ').Select(part => part.Split('⁊')).Where(part => part.Length == 2).ToDictionary(sp => sp[0], sp => sp[1]);

            //splits string by $
            string[] yesnt = Regex.Split(stuff, @"(?<!\\)\$");

            //removes empty strings which would break this command
            yesnt = yesnt.Where(x => !string.IsNullOrEmpty(x)).ToArray();

            //final string array with converted segments
            string[] final = new string[yesnt.Length];

            for (int i = 0; i < yesnt.Length; i++)
            {
                //checks if a key exists for the segment and if it is prefaced by \
                if (Globals.emoteDict.ContainsKey(yesnt[i]) && !(yesnt[i][0] == '\\'))
                {
                    final[i] = Globals.emoteDict[yesnt[i]];
                }
                else
                {
                    //if segment is prefaced by \
                    if (yesnt[i][0] == '\\')
                    {
                        //removes \
                        final[i] = yesnt[i].Remove(0, 1);
                    }
                    else
                    {
                        final[i] = yesnt[i];
                    }
                }
            }

            //returns joined segments
            return string.Join("", final.Select(p => p.ToString()));
        }

        //emote react function
        [Command("re")]
        public async Task ReactCustomAsync(string channel, string message, string emote)
        {
            //loads emote dictionary as string and converts it back into dictionary
            Globals.emoteDict = Load("C:\\gecko2.gek").ToString().Split('ҩ').Select(part => part.Split('⁊')).Where(part => part.Length == 2).ToDictionary(sp => sp[0], sp => sp[1]);

            //gets current client
            DiscordSocketClient client = Context.Client;

            //parses channel id provided and gets channel from client
            var chnl = client.GetChannel(ulong.Parse(channel)) as IMessageChannel;

            //parses message id provided and gets message from channel
            var message2 = await chnl.GetMessageAsync(ulong.Parse(message));

            //splits based on $
            string[] yesnt = emote.Split("$");

            for (int i = 0; i < yesnt.Length; i++)
            {
                //if the emote dictionary contains the key
                if (Globals.emoteDict.ContainsKey(yesnt[i]))
                {
                    var emote2 = Emote.Parse(Globals.emoteDict[yesnt[i]]);
                    await message2.AddReactionAsync(emote2);
                }
                else
                {
                    //trys to parse emotes 2 different ways
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

                //adds check emote after done
                await Context.Message.AddReactionAsync(new Emoji("✅"));
            }
        }

        //adds stuff for music generation
        void add()
        {
            Globals.notes.Add(0, "c");
            Globals.notes.Add(1, "c#");
            Globals.notes.Add(2, "d");
            Globals.notes.Add(3, "d#");
            Globals.notes.Add(4, "e");
            Globals.notes.Add(5, "f");
            Globals.notes.Add(6, "f#");
            Globals.notes.Add(7, "g");
            Globals.notes.Add(8, "g#");
            Globals.notes.Add(9, "a");
            Globals.notes.Add(10, "a#");
            Globals.notes.Add(11, "b");
            Globals.notes.Add(12, "c2");
            Globals.notes.Add(13, "c#2");
            Globals.notes.Add(14, "d2");
            Globals.notes.Add(15, "d#2");
            Globals.notes.Add(16, "e2");
            Globals.notes.Add(17, "f2");
            Globals.notes.Add(18, "f#2");
            Globals.notes.Add(19, "g2");
            Globals.notes.Add(20, "g#2");
            Globals.notes.Add(21, "a2");
            Globals.notes.Add(22, "a#2");
            Globals.notes.Add(23, "b2");
            Globals.notes.Add(24, "c3");
            Globals.notes.Add(25, "c#3");
            Globals.notes.Add(26, "d3");
            Globals.notes.Add(27, "d#3");
            Globals.notes.Add(28, "e3");
            Globals.notes.Add(29, "f3");
            Globals.notes.Add(30, "f#3");
            Globals.notes.Add(31, "g3");
            Globals.notes.Add(32, "g#3");
            Globals.notes.Add(33, "a3");
            Globals.notes.Add(34, "a#3");
            Globals.notes.Add(35, "b3");
            Globals.notes.Add(36, "c4");
            Globals.notes.Add(37, "rest");

            Globals.minor.Add(1, 2);
            Globals.minor.Add(2, 3);
            Globals.minor.Add(3, 5);
            Globals.minor.Add(4, 7);
            Globals.minor.Add(5, 8);
            Globals.minor.Add(6, 10);
            Globals.minor.Add(7, 12);

            Globals.major.Add(1, 2);
            Globals.major.Add(2, 4);
            Globals.major.Add(3, 5);
            Globals.major.Add(4, 7);
            Globals.major.Add(5, 9);
            Globals.major.Add(6, 11);
            Globals.major.Add(7, 12);

            Globals.timeDict.Add(1, 2);
            Globals.timeDict.Add(2, 3);
            Globals.timeDict.Add(3, 4);
            Globals.timeDict.Add(4, 5);
            Globals.timeDict.Add(5, 8);
            Globals.timeDict.Add(6, 16);

            Globals.timeDict2.Add(1, 2);
            Globals.timeDict2.Add(2, 4);
            Globals.timeDict2.Add(3, 8);
        }

        //music generation function
        [Command("generate")]
        public async Task generate(int length)
        {
            //if generation dictionaries are unpopulated
            if (!Globals.notes.ContainsKey(0))
            {
                add();
            }

            //rAnDoM
            Random random = new Random();
            
            //generated a bunch of random values from generation dictionaries
            int measures = length;
            int majorMinor = random.Next(1,3);
            int number = random.Next(12, 25);
            int inKey = number;
            
            int time = Globals.timeDict[random.Next(1, 7)];
            int time2 = Globals.timeDict2[random.Next(1, 4)];
            int timeRemain = time;
            int dure = 0;

            //list that stores notes
            List<string> noteNames = new List<string>();

            //I'm far too lazy to explain this, deal with it.
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
                            number = (judge == 2 ? inKey - Globals.major[newNumber] : inKey + Globals.major[newNumber]);
                        }
                        else
                        {
                            number = (judge == 2 ? inKey - Globals.minor[newNumber] : inKey + Globals.minor[newNumber]);
                        }
                    }
                    else
                    {
                        if (majorMinor == 2)
                        {
                            number = (judge == 2 ? inKey - Globals.major[newNumber] : inKey + Globals.major[newNumber]);
                        }
                        else
                        {
                            number = (judge == 2 ? inKey - Globals.minor[newNumber] : inKey + Globals.minor[newNumber]);
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
                
                noteNames.Add(Globals.notes[number] + " **" + dure.ToString() + "**");

                if (measures <= 0)
                {
                    noteNames.RemoveAt(noteNames.Count - 1);
                    break;
                }
            }

            //joins stuff and sends
            string final = Globals.notes[inKey] + (majorMinor == 2 ? " major, in " : " minor, in ") + time.ToString() + "/" + time2.ToString() + System.Environment.NewLine + string.Join(", ", noteNames.Select(p => p.ToString()));
            await ReplyAsync(final);
        }

        //contest function
        [Command("contest")]
        public async Task contest(string user)
        {
            //generates a random number
            Random random = new Random();
            int number = random.Next(1, 3);

            //decides who wins
            if (number == 1)
            {
                await ReplyAsync(Context.User + " wins!");
            }
            else if (number == 2)
            {
                await ReplyAsync(user + " wins!");
            }
        }

        //how to stonks
        [Command("lottery")]
        public async Task lottery()
        {
            //variables
            string results = " ";
            string[] number2 = new string[6];
            string[] key2 = new string[6];
            int matches = 0;
            Random random = new Random();

            //generates 6 values and matches them
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

            //generates results and replies
            results = "numbs: " + string.Join(" ", number2.Select(p => p.ToString())) + "\nresults: " + string.Join(" ", key2.Select(p => p.ToString())) + "\nmatches: " + matches.ToString();
            await ReplyAsync(results);
        }

        //gets daily gecko image
        [Command("gec")]
        public async Task gec()
        {
            //gets day of the year
            DateTime date = DateTime.Today;
            string final = (date.DayOfYear - 1).ToString();

            //adds 0s as needed
            while (final.Length < 3)
            {
                final = "0" + final;
            }
            //sends file with exception for leap years
            await Context.Channel.SendFileAsync(filePath: @"D:\Documents\stuff\GeckoImages_for_bot\" + final + "_icon" + (date.DayOfYear == 366 ? ".gif" : ".png"), text: "Today is " + date.ToString("d") + ". Day " + date.DayOfYear + " of the year " + date.Year + " (gecko #" + final + ")");
            if (date.DayOfYear == 366)
            {
                await Context.Channel.SendFileAsync(filePath: @"D:\Documents\stuff\GeckoImages_for_bot\366_icon.gif", text: "Today is " + date.ToString("d") + ". Day " + date.DayOfYear + " of the year " + date.Year + "(gecko #366)");
            }
        }

        //sends a message with a link to the gecko collection
        [Command("GecColle")]
        public async Task gecColle()
        {
            //buils an embed
            var embed = new EmbedBuilder
            {
                Title = "gecko collection",
                Description = ("[see the gecko collection here](https://drive.google.com/drive/folders/1Omwv0NNV0k_xlECZq3d4r0MbSbuHC_Og?usp=sharing)")
            };

            embed.WithColor(180, 212, 85);

            var embed2 = embed.Build();

            await ReplyAsync(embed: embed2);
        }

        //sends a random geckoimage
        [Command("rgec")]
        public async Task rgec()
        {
            //gets random value
            Random random = new Random();
            int numb = random.Next(0,367);
            string final = (numb).ToString();

            //adds 0s as needed
            while (final.Length < 3)
            {
                final = "0" + final;
            }

            //sends file
            await Context.Channel.SendFileAsync(filePath: @"D:\Documents\stuff\GeckoImages_for_bot\" + final + "_icon" + (numb == 365 || numb ==366 ? ".gif" : ".png"), text: "gecko #" + final);
        }

        //finds a gecko
        [Command("fgec")]
        public async Task fgec(int value)
        {
            //converts int to string
            string final = value.ToString();

            //adds 0s as needed
            while (final.Length < 3)
            {
                final = "0" + final;
            }

            //sends files
            await Context.Channel.SendFileAsync(filePath: @"D:\Documents\stuff\GeckoImages_for_bot\" + final + "_icon" + (value == 365 || value == 366 ? ".gif" : ".png"), text: "gecko #" + final);
        }

        //sets up daily dms
        [Command("dm")]
        public async Task dmgec(bool yes)
        {
            if (yes)
            {
                //if file exists, load it
                if (Load("C:\\gecko3.gek") != null)
                {
                    //clears
                    Globals.dmUsers.Clear();

                    //gets info
                    string[] temp = Load("C:\\gecko3.gek").Split(",");

                    //adds info to list
                    foreach (string a in temp)
                    {
                        Globals.dmUsers.Add(ulong.Parse(a));
                    }
                }

                //gets current user
                IUser user = Context.User;

                //if they are already signed up
                if (Globals.dmUsers.Contains(user.Id))
                {
                    await ReplyAsync("you are aleady signed up!");
                }
                else
                {
                    //adds id
                    Globals.dmUsers.Add(user.Id);

                    //saves info
                    Save(string.Join(",", Globals.dmUsers.ToArray()), "C:\\gecko3.gek");

                    //DMs the user
                    await Discord.UserExtensions.SendMessageAsync(user, "hi, daily gecko updates have been set up, cancel by '\\`dm false'");
                }

            }
            else
            {
                //loads things the same way as above
                if (Load("C:\\gecko3.gek") != null)
                {
                    Globals.dmUsers.Clear();
                    string[] temp = Load("C:\\gecko3.gek").Split(",");
                    foreach (string a in temp)
                    {
                        Globals.dmUsers.Add(ulong.Parse(a));
                    }
                }
                
                //gets current user
                IUser user = Context.User;

                //if the are already not signed up
                if (Globals.dmUsers.Contains(user.Id))
                {
                    //removes user form list
                    Globals.dmUsers.Remove(user.Id);

                    //saves info
                    Save(string.Join(",", Globals.dmUsers.ToArray()), "C:\\gecko3.gek");

                    //DMs the user
                    await Discord.UserExtensions.SendMessageAsync(user, "hi, daily gecko updates have been canceled");

                }
                else
                {
                    await ReplyAsync("you are already not signed up!");
                }
            }
        }

        //starts timer for various checks
        [Command("start")]
        public async Task test()
        {
            //if started or not
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

        //checks
        [Command("check")]
        public async Task check(string passcode)
        {
            //if password matches secret password
            if (passcode == Top.Secret)
            {
                //forces update by subtracting one from day of the year
                Globals.lastrun = DateTime.Now.DayOfYear - 1;

                await dailydm();

                await ReplyAsync("checked and force updated");
            }
            else if (Globals.lastrun != DateTime.Now.DayOfYear)
            {
                //checks
                await dailydm();

                await ReplyAsync("checked and updated");
            }
            else
            {
                await ReplyAsync("checked");
            }
        }

        //actually starts the timer
        public void Start()
        {
            //one hour looping timer for checking
            Timer timer = new Timer(1000*60*60);
            timer.Elapsed += new System.Timers.ElapsedEventHandler(daily);
            timer.Start();

            //makes sure there is only one tiemr
            Globals.started = true;
        }

        //checks when timer runs out
        public async void daily(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Globals.lastrun != DateTime.Now.DayOfYear)
            {
                Globals.daysSinceReset += 1;
                await dailydm();
            }
        }

        //sends daily dm
        public async Task dailydm()
        {
            //loads file in same way as described above
            if (Load("C:\\gecko3.gek") != null)
            {
                Globals.dmUsers.Clear();
                string[] temp = Load("C:\\gecko3.gek").Split(",");

                foreach (string a in temp)
                {
                    Globals.dmUsers.Add(ulong.Parse(a));
                }
            }

            //generates statement to send
            DateTime date = DateTime.Today;
            string final = (date.DayOfYear - 1).ToString();
            while (final.Length < 3)
            {
                final = "0" + final;
            }

            //gets client
            DiscordSocketClient client = Context.Client;

            //DMs everybody on the list
            foreach (ulong a in Globals.dmUsers)
            {
                //gets user from id
                IUser b = client.GetUser(a);

                //sends files
                await Discord.UserExtensions.SendFileAsync(b, filePath: @"D:\Documents\stuff\GeckoImages_for_bot\" + final + "_icon" + (date.DayOfYear == 366 ? ".gif" : ".png"), text: "Today is " + date.ToString("d") + ". The " + date.DayOfYear + " day of the year (gecko #" + final + ")");
                if (date.DayOfYear == 366)
                {
                    await Discord.UserExtensions.SendFileAsync(b, filePath: @"D:\Documents\stuff\GeckoImages_for_bot\366_icon" + ".gif", text: "Today is " + date.ToString("d") + ". The " + date.DayOfYear + " day of the year (gecko #366)");
                }
            }

            //updates last run counter
            Globals.lastrun = DateTime.Now.DayOfYear;
        }

        //timer
        [Command("timer")]
        public async Task timer(string message, string time)
        {
            //parses time in hh:mm:ss format
            string[] times1 = time.Split(":");
            int[] times2 = new int[3];
            for (int i = 0; i < 3; i++)
            {
                times2[i] = int.Parse(times1[i]);
            }

            //gets user
            IUser user = Context.User;

            //starts a timer with desired amount of time
            Timer timer = new Timer(((times2[0] * 60 * 60) + (times2[1] * 60) + times2[2])* 1000);
            timer.Elapsed += async (sender, e) => await timerUp(user, message, timer);
            timer.Start();

            //adds reaction
            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

        //alarm
        [Command("alarm")]
        public async Task alarm(string message, string time)
        {
            //parses time in hh:mm:ss format
            string[] times1 = time.Split(":");
            int[] times2 = new int[3];
            for (int i = 0; i < 3; i++)
            {
                times2[i] = int.Parse(times1[i]);
            }

            //gets current time
            int hour = DateTime.Now.Hour;
            int minute = DateTime.Now.Minute;
            int second = DateTime.Now.Second;

            //does maths and subtracts the current time from the alarm time
            int final = (((times2[0] - hour) * 60 * 60) + ((times2[1] - minute) * 60) + (times2[2] - second)) * 1000;

            //gets user
            IUser user = Context.User;

            //sets timer to exact amount of time
            Timer timer = new Timer(final);
            timer.Elapsed += async (sender, e) => await timerUp(user, message, timer);
            timer.Start();

            //adds reaction
            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

        //the task that is activated when time is up
        public async Task timerUp(IUser user, string message, Timer timer2)
        {
            //dms user
            await Discord.UserExtensions.SendMessageAsync(user, emoteReplace(message));

            //stops timer
            timer2.Stop();
        }

        //tem
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

        //temporary command to set name
        [Command("set")]
        public async Task set(string password, int value)
        {
            if (password == Top.Secret)
            {
                //changes current value to 
                Globals.currentValue = value;

                //adds reaction
                await Context.Message.AddReactionAsync(new Emoji("✅"));
            }
        }

        //instructions
        [Command("what do you")]
        public async Task instructions(string section)
        {
            //if info is found
            bool found = true;

            //embed
            var embed = new EmbedBuilder
            {
            Title = "geckobot" + Globals.names[Globals.currentValue] + System.Environment.NewLine + "1/16/2020 instruction manual"
            };

            //changes based on sections
            if (section == "do?")
            {
                embed.Description = ("my prefix is \\`." + System.Environment.NewLine +
                "(highly recommended to have developer mode on to easily use)" + System.Environment.NewLine +
                "if there's a problem, ping my owner croissantderp#4167 " + System.Environment.NewLine + System.Environment.NewLine +
                "links: [trello](https://trello.com/invite/b/cFS33M13/8fddf3ac42bd0fe419e482c6f4414e01/gecko-bot-todo) [github](https://github.com/croissantderp/geckobot) [invite](https://discord.com/oauth2/authorize?client_id=766064505079726140&scope=bot&permissions=379968)" + System.Environment.NewLine + System.Environment.NewLine + 
                "sections (replace 'do?' with these): general | gecko | random | sigfigs | emote | embed | edit | za_warudo" + System.Environment.NewLine + System.Environment.NewLine +
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
                    "to see the entire collection try '\\`GecColle' [go here](https://drive.google.com/drive/folders/1Omwv0NNV0k_xlECZq3d4r0MbSbuHC_Og?usp=sharing)" + System.Environment.NewLine + System.Environment.NewLine +
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
            else if (section == "emote")
            {
                embed.AddField("**global emotes:**",
                    "Strange discord oversight/intentional design lets bots use emotes globally!" + System.Environment.NewLine + System.Environment.NewLine +
                    "To use an emote use '\\`e [emote name]' this command is also integrated into other commands (e.g. '\\`flip')" + System.Environment.NewLine + System.Environment.NewLine +
                    "To log an emote so it can be used globally, type '\\`es [common name] [actual emote here]' make sure the actual emote actually sends as an emote or it won't work. " + System.Environment.NewLine + System.Environment.NewLine +
                    "To save an animated emote, get the emote id (formated like this: <a:[name]:[id]>) by entering backslash before the emote and copying the message, then paste it and the id will be there, remember to remove the backslash when saving. Then insert a random ':::' anywhere in the id and type '\\`es [common name] [emote id with random ::: ]'" + System.Environment.NewLine + System.Environment.NewLine +
                    "geckobot can also log every emote of every server that it is in by '\\`ess'" + System.Environment.NewLine + System.Environment.NewLine +
                    "To remove an emote, type '\\`er [common name]'" + System.Environment.NewLine + System.Environment.NewLine +
                    "To react, type '\\`re [channel id] [message id] [emote or common name]' global emote thing works too and you can string many together with '$'" + System.Environment.NewLine + System.Environment.NewLine +
                    "(escape an emote by prefacing it with '\\\\')"
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

        //custom embed builder
        [Command("embed")]
        public async Task embed(string title, string field, string footer2, string hex)
        {
            //converts hex to rgb
            if (hex.IndexOf('#') != -1)
            {
                hex = hex.Replace("#", "");
            }

            int r, g, b = 0;

            r = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
            g = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
            b = int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.AllowHexSpecifier);

            //starts embed building proceedure
            var embed = new EmbedBuilder
            {
                Title = title
            };

            //splits fields by $$
            string[] fields = field.Split("$$");

            foreach(string a in fields)
            {
                //splits subfields by %%
                string[] subfields = a.Split("%%");

                //checks lengths of subfields
                if (subfields.Length == 1)
                {
                    //adds subfield as description if there isn't one already
                    if (embed.Description == null)
                    {
                        embed.Description = subfields[0];
                    }
                    else
                    {
                        embed.Description = "​";
                    }

                    //adds subfield with blank title
                    embed.AddField("​", subfields[0]);
                }
                else
                {
                    //adds field with title and description
                    embed.AddField(subfields[0], subfields[1]);
                }
            }

            //adds color
            embed.WithColor(r,g,b);

            //adds author of message
            embed.WithAuthor(Context.User);

            //assigns footer
            embed.WithFooter(footer => footer.Text = footer2);

            //assigns time
            embed.WithCurrentTimestamp();

            await ReplyAsync(embed: embed.Build());
        }

        //generates a random order of characters
        [Command("fek")]
        public async Task fek()
        {
            //generates random
            Random random = new Random();

            //generate random length of the word
            int charNum = random.Next(2, 10);
            
            string[] charFinal = new string[charNum];

            //generates random characters
            for (int i = 0; i < charNum; i++)
            {
                int num = random.Next(0, 26);
                char let = (char)('a' + num);
                charFinal[i] = let.ToString();
            }

            //joins characters and sends
            await ReplyAsync(string.Join("", charFinal));
        }

        //flips the edited tag on messages
        [Command("flip")]
        public async Task flip(string text, string text2)
        {
            //joins text with dark magic
            string final = "؜" + emoteReplace(text) + "\n" + emoteReplace(text2) + "؜؜؜";

            //sends a placeholder message
            var Message1 = await ReplyAsync("ahaaha");

            //edits it with content
            await Message1.ModifyAsync(m => { m.Content = final; });
        }

        [Command("edit")]
        public async Task edit(string text, string text2)
        {
            //joins text with dark magic
            string final = "؜" + emoteReplace(text2) + "؜؜؜؜؜؜؜؜؜؜؜؜" + emoteReplace(text);

            //sends a placeholder message
            var Message1 = await ReplyAsync("ahaaha");

            //edits it with content
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

        //random number generator
        [Command("rng")]
        public async Task rng(int min, int max)
        {
            Random random = new Random();
            int number = random.Next(min, max + 1);
            await ReplyAsync(number.ToString());
        }
    }

    //significant figures
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

        //general sigfig command
        [Command("")]
        public async Task sigfigbase(decimal number)
        {
            //constructs reply
            await ReplyAsync(string.Join("", figures(number)[0] + figures(number)[1] + " " + figures(number)[2]));
        }

        //rounds values (found off of stack overflow I don't know how it works)
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

        //sigfig add
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

        //sigfig subtract
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

        //sigfig multiply
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

        //sigfig divide
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
        //generates new number
        [Command("new")]
        public async Task newNumber(int min, int max)
        {
            //generates number based on min and max value
            Random random = new Random();
            int number = random.Next(min, max + 1);

            //achievement :D
            await ReplyAsync(min == max ? "achievement get! play on the easiest difficulty!" : "new number generated between " + min.ToString() + " and " + max.ToString());
            
            //assigns variables
            Globals.gNumber = number;
            Globals.easyMode = (min == max);
            Globals.attempts = 0;
        }

        [Command("")]
        public async Task attempt(int value)
        {
            //gets value from global variables
            int gNumber = Globals.gNumber;

            //checks values and adds to attempts
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

                //achievemnt :D
                await ReplyAsync(Globals.easyMode && Globals.attempts > 1 ? "achievement get! lose the game on the easiest difficulty!" :  Context.User + " got it! The number was " + gNumber.ToString() + ". It took " + Globals.attempts.ToString() + " attempts!");
            }
        }
    }
}
