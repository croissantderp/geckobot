using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using System.Text.RegularExpressions;

namespace GeckoBot.Commands
{
    //significant figures
    [Group("sigfig")]
    [Summary("Math commands with significant figures.")]
    public class SigFig : ModuleBase<SocketCommandContext>
    {
        
        private string[] figures(string number)
        {
            //splits the number into two strings, one of the digits before the decimal, one of them after
            string[] numberArray = number.Split(".");

            bool negative = false;

            if (numberArray[0].Contains("-"))
            {
                negative = true;
                numberArray[0] = numberArray[0].Remove(0,1);
            }

            //a list of the numbers after the decimal point
            List<string> Decimal = new ();

            //number of sigfigs in number
            int SigNum = 0;

            //a list of numbers before the decimal point
            List<string> numberList = new ();

            int temp2electricboogaloo;
            if (int.TryParse(numberArray[0].ToString(), out temp2electricboogaloo))
            {
                numberList.Add(temp2electricboogaloo.ToString());
            }

            //if a decimal exists
            if (numberArray.Length == 2)
            {
                //adds the decimal point to the list
                Decimal.Add(".");
                string zeros = "";

                if (decimal.Parse(number) == 0)
                {
                    Decimal.Add(numberArray[1]);
                    SigNum = 0;
                }
                else if (numberArray[1] == "")
                {
                    //adds ** which bolds text
                    Decimal.Add(numberArray[1]);

                    //bolds everythings
                    numberList.Add("**");
                    numberList.Insert(0, "**");

                    //counts total number of sigfigs
                    SigNum = numberList[1].Length + Decimal[1].Length;
                }
                //if there are numbers before the decimal
                else if (numberList[0] != ("0") && numberList[0] != ("-0"))
                {
                    //adds ** which bolds text
                    Decimal.Add("**");
                    Decimal.Add(numberArray[1]);
                    Decimal.Add("**");

                    //bolds everythings
                    numberList.Add("**");
                    numberList.Insert(0, "**");

                    //counts total number of sigfigs
                    SigNum = numberList[1].Length + Decimal[2].Length;
                }
                else
                {
                    //gets zeros before
                    while (true)
                    {
                        if (numberArray[1][0] == '0')
                        {
                            numberArray[1] = numberArray[1].Remove(0, 1);
                            zeros += "0";
                        }
                        else
                        {
                            break;
                        }
                    }

                    //adds zeros
                    Decimal.Add(zeros);

                    //adds ** which bolds text
                    Decimal.Add("**");

                    //adds the number
                    Decimal.Add(numberArray[1]);

                    Decimal.Add("**");

                    //counts total number of sigfigs
                    SigNum = Decimal[3].Length;
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

                        if (Value.ToString(new string('#', 339)).Contains("-"))
                        {
                            SigNum--;
                        }

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
            string[] final = { (negative ? "-" : "") + string.Join("", numberList.Select(p => p.ToString())), string.Join("", Decimal.Select(p => p.ToString())), SigNum.ToString()};
            return final;
        }

        //general sigfig command
        [Command("")]
        [Summary("Tells you how many significant figures your number has.")]
        public async Task sigfigbase([Summary("The number to show sigfigs.")] string number)
        {
            var figs = figures(number);
            
            //constructs reply
            await ReplyAsync(figs[0] + figs[1] + " " + figs[2]);
        }

        //sigfig add
        [Command("add")]
        [Summary("Adds two numbers with sigfigs in mind.")]
        public async Task sigAdd([Summary("First number.")] decimal number1, [Summary("Number to add to the first number.")] decimal number2)
        {
            decimal finalNum = number1 + number2;
            decimal final = 0;

            int accuracy1 = int.Parse(figures(number1.ToString())[2]);
            int accuracy2 = int.Parse(figures(number2.ToString())[2]);

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
                accuracy1 = check1[1].Length;
                accuracy2 = check2[1].Length;

                if (accuracy1 <= accuracy2)
                {
                    accuracy1 += finalNum.ToString().Split(".")[0] != "0" ? finalNum.ToString().Split(".")[0].Length : 0;
                    final = Utils.Utils.RoundNum(finalNum, accuracy1);

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
                    accuracy2 += finalNum.ToString().Split(".")[0] != "0" ? finalNum.ToString().Split(".")[0].Length : 0;
                    final = Utils.Utils.RoundNum(finalNum, accuracy2);

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
                await ReplyAsync(string.Join("", figures(final2.ToString())[0] + figures(final2.ToString())[1] + " " + figures(final2.ToString())[2]));
            }
            else
            {
                decimal e = decimal.Parse(final.ToString() + string.Join("", extra));

                await ReplyAsync(figures(e.ToString())[0] + figures(e.ToString())[1] + " " + figures(e.ToString())[2]);
            }
        }

        //sigfig subtract
        [Command("subtract")]
        [Summary("Subtracts two numbers with sigfigs in mind.")]
        public async Task sigSubtract([Summary("First number.")] decimal number1, [Summary("Number to subtract from first number.")] decimal number2)
        {
            decimal finalNum = number1 - number2;
            decimal final = 0;
            int accuracy1 = int.Parse(figures(number1.ToString())[2]);
            int accuracy2 = int.Parse(figures(number2.ToString())[2]);

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
                accuracy1 = check1[1].Length;
                accuracy2 = check2[1].Length;

                if (accuracy1 <= accuracy2)
                {
                    accuracy1 += finalNum.ToString().Split(".")[0].Length - 1;
                    final = Utils.Utils.RoundNum(finalNum, accuracy1);

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
                    accuracy2 += finalNum.ToString().Split(".")[0].Length - 1;
                    final = Utils.Utils.RoundNum(finalNum, accuracy2-2);

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
                await ReplyAsync(figures(final2.ToString())[0] + figures(final2.ToString())[1] + " " + figures(final2.ToString())[2]);
            }
            else
            {
                decimal e = decimal.Parse(final.ToString() + string.Join("", extra));

                await ReplyAsync(figures(e.ToString())[0] + figures(e.ToString())[1] + " " + figures(e.ToString())[2]);
            }
        }

        //sigfig multiply
        [Command("multiply")]
        [Summary("Multiplies two numbers with sigfigs in mind.")]
        public async Task sigMultiply([Summary("First number.")] decimal number1, [Summary("Number to multiply first number by.")] decimal number2)
        {
            decimal finalNum = number1 * number2;

            int accuracy1 = int.Parse(figures(number1.ToString())[2]);
            int accuracy2 = int.Parse(figures(number2.ToString())[2]);

            decimal final = 0;

            string[] extra = new string[final.ToString().Length+1];

            if (accuracy1 <= accuracy2)
            {
                final = Utils.Utils.RoundNum(finalNum, accuracy1);
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
                final = Utils.Utils.RoundNum(finalNum, accuracy2);
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
                await ReplyAsync(string.Join("", figures(f.ToString())[0] + figures(f.ToString())[1] + " " + figures(f.ToString())[2]));
            }
            else
            {
                await ReplyAsync(figures(e.ToString())[0] + figures(e.ToString())[1] + " " + figures(e.ToString())[2]);
            }
        }

        //sigfig divide
        [Command("divide")]
        [Summary("Divides two numbers with sigfigs in mind.")]
        public async Task sigDivide([Summary("First number.")] decimal number1, [Summary("Number to divide first number by.")] decimal number2)
        {
            decimal finalNum = number1 / number2;
            int accuracy1 = int.Parse(figures(number1.ToString())[2]);
            int accuracy2 = int.Parse(figures(number2.ToString())[2]);

            decimal final = 0;

            string[] extra = new string[final.ToString().Length + 1];

            if (accuracy1 <= accuracy2)
            {
                final = Utils.Utils.RoundNum(finalNum, accuracy1);
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
                final = Utils.Utils.RoundNum(finalNum, accuracy2);
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
                await ReplyAsync(string.Join("", figures(f.ToString())[0] + figures(f.ToString())[1] + " " + figures(f.ToString())[2]));
            }
            else
            {
                await ReplyAsync(figures(e.ToString())[0] + figures(e.ToString())[1] + " " + figures(e.ToString())[2]);
            }
        }
    }
}