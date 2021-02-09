using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;

namespace GeckoBot.Commands
{
    public class Music : ModuleBase<SocketCommandContext>
    {
        //music generation function
        [Command("generate")]
        [Summary("Generates a tune based on note count.")]
        public async Task generate(int length)
        {
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
                                ? inKey - Globals.major[newNumber] 
                                : inKey + Globals.major[newNumber];
                        }
                        else
                        {
                            number = judge == 2 
                                ? inKey - Globals.minor[newNumber] 
                                : inKey + Globals.minor[newNumber];
                        }
                    }
                    else
                    {
                        if (majorMinor == 2)
                        {
                            number = judge == 2 
                                ? inKey - Globals.major[newNumber] 
                                : inKey + Globals.major[newNumber];
                        }
                        else
                        {
                            number = judge == 2 
                                ? inKey - Globals.minor[newNumber] 
                                : inKey + Globals.minor[newNumber];
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
                
                noteNames.Add(Globals.notes[number] + $" **{dure}**");

                if (measures <= 0)
                {
                    noteNames.RemoveAt(noteNames.Count - 1);
                    break;
                }
            }

            //joins stuff and sends
            string final = $"{Globals.notes[inKey]}{(majorMinor == 2 ? " major, in " : " minor, in ")}{time}/{time2}" +
                           $"{Environment.NewLine}{string.Join(", ", noteNames.Select(p => p.ToString()))}";
            await ReplyAsync(final);
        }
    }
}