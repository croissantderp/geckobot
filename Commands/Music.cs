using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;

namespace GeckoBot.Commands
{
    [Summary("Music generation.")]
    public class Music : ModuleBase<SocketCommandContext>
    {
        //music generation function
        [Command("generate")]
        [Summary("Generates a tune based on measure count.")]
        public async Task generate(int length)
        {
            //rAnDoM
            Random random = new Random();
            
            //generated a bunch of random values from generation dictionaries
            int measures = length;
            int majorMinor = random.Next(1,3);
            int number = random.Next(12, 25);
            int inKey = number;
            
            int time = TimeDict[random.Next(1, 7)];
            int time2 = TimeDict2[random.Next(1, 4)];
            int timeRemain = time;
            int dure = 0;

            //list that stores notes
            List<string> noteNames = new ();

            //I'm far too lazy to explain this, deal with it.
            while (measures > 0)
            {
                int noteChance = random.Next(1, 8);

                int newNumber = random.Next(1, 8);

                int judge = random.Next(1, 3);
                int rest = random.Next(1, 15);

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
                            number = judge == 2 
                                ? inKey - Major[newNumber] 
                                : inKey + Major[newNumber];
                        }
                        else
                        {
                            number = judge == 2 
                                ? inKey - Minor[newNumber] 
                                : inKey + Minor[newNumber];
                        }
                    }
                    else
                    {
                        if (majorMinor == 2)
                        {
                            number = judge == 2 
                                ? inKey - Major[newNumber] 
                                : inKey + Major[newNumber];
                        }
                        else
                        {
                            number = judge == 2 
                                ? inKey - Minor[newNumber] 
                                : inKey + Minor[newNumber];
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
                
                noteNames.Add(Notes[number] + $" **{dure}**");

                if (measures <= 0)
                {
                    noteNames.RemoveAt(noteNames.Count - 1);
                    break;
                }
            }

            //joins stuff and sends
            string final = $"{Notes[inKey]}{(majorMinor == 2 ? " major, in " : " minor, in ")}{time}/{time2}" +
                           $"{Environment.NewLine}{string.Join(", ", noteNames.Select(p => p.ToString()))}";
            await ReplyAsync(final);
        }
        
        //various dictionaries for music generation
        private static readonly Dictionary<int, string> Notes = new()
        {
            {0, "c"},
            {1, "c#"},
            {2, "d"},
            {3, "d#"},
            {4, "e"},
            {5, "f"},
            {6, "f#"},
            {7, "g"},
            {8, "g#"},
            {9, "a"},
            {10, "a#"},
            {11, "b"},
            {12, "c2"},
            {13, "c#2"},
            {14, "d2"},
            {15, "d#2"},
            {16, "e2"},
            {17, "f2"},
            {18, "f#2"},
            {19, "g2"},
            {20, "g#2"},
            {21, "a2"},
            {22, "a#2"},
            {23, "b2"},
            {24, "c3"},
            {25, "c#3"},
            {26, "d3"},
            {27, "d#3"},
            {28, "e3"},
            {29, "f3"},
            {30, "f#3"},
            {31, "g3"},
            {32, "g#3"},
            {33, "a3"},
            {34, "a#3"},
            {35, "b3"},
            {36, "c4"},
            {37, "rest"},
        };
        private static readonly Dictionary<int, int> Major = new()
        {
            {1, 2},
            {2, 4},
            {3, 5},
            {4, 7},
            {5, 9},
            {6, 11},
            {7, 12},
        };
        private static readonly Dictionary<int, int> Minor = new()
        {
            {1, 2},
            {2, 3},
            {3, 5},
            {4, 7},
            {5, 8},
            {6, 10},
            {7, 12},
        };
        private static readonly Dictionary<int, int> TimeDict = new()
        {
            {1, 2},
            {2, 3},
            {3, 4},
            {4, 5},
            {5, 8},
            {6, 16},
        };
        private static readonly Dictionary<int, int> TimeDict2 = new()
        {
            {1, 2},
            {2, 4},
            {3, 8},
        };
    }
}