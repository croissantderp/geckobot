using System.Threading.Tasks;
using System.Linq;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;

using static System.Math;

using Quantum.qeckoBot;

using Microsoft.Quantum.Simulation.Simulators;
using Microsoft.Quantum.Simulation.Core;

namespace GeckoBot.Commands
{
    [Summary("Quantum commands using Q# and the Quantum Simulator")]
    public class QuantumCommands : ModuleBase<SocketCommandContext>
    {
        //rng but qUaNtUm
        [Command("qrng")]
        [Summary("random function using quantum bits")]
        public async Task qrng([Summary("Max value that can be generated.")] int min = 0, [Summary("Max value that can be generated.")] int max = 100)
        {
            using var sim = new QuantumSimulator();

            max -= min;
            
            long num = await SampleRandomNumberInRange.Run(sim, long.Parse(max.ToString()));
            await ReplyAsync((min + num).ToString());
        }

        //g ro v e r
        [Command("grover")]
        [Summary("basic search in sqrt(n) time")]
        public async Task grover([Summary("number of qubits to use")] int qubits, [Summary("the pattern to search for (in binary seperated by '$')")] string matcher)
        {
            using var sim = new QuantumSimulator();
            if (qubits > 10)
            {
                await ReplyAsync("please use 10 or under qubits");
                return;
            }

            var matcherInts = matcher.Split("$").Select(a => (int.Parse(a) == 0 ? true : false));

            var temp = await SearchForMarkedInput.Run(sim, qubits, new QArray<bool>(matcherInts));
            await ReplyAsync(temp.ToString());
        }

        //https://github.com/microsoft/Quantum/blob/main/samples/algorithms/database-search/Program.cs
        [Command("grover search")]
        [Summary("search more things in sqrt(n) time")]
        public async Task grover2([Summary("number of qubits to use")] int qubits,  [Summary("the indices (seperated by '$') of the marked values")] string matcher, [Summary("number of search iterations")] int iterations = 0, [Summary("number of search repetitions")] int repetitions = 0)
        {

            if (qubits > 15)
            {
                await ReplyAsync("please use 12 or under qubits");
                return;
            }
            if (repetitions > 50)
            {
                await ReplyAsync("please use 50 or less repetitions");
                return;
            }
            if (iterations > 100)
            {
                await ReplyAsync("please use 100 or less iterations");
                return;
            }

            var sim = new QuantumSimulator(throwOnReleasingQubitsNotInZeroState: true);

            var nDatabaseQubits = qubits;

            var markedElements = matcher.Split("$").Select(a => long.Parse(a)).ToArray();
            var nMarkedElements = markedElements.Length;

            var databaseSize = Pow(2.0, nDatabaseQubits);
            var nIterations = 0;

            nIterations = (iterations == 0 ? int.Parse(Round(PI / 4 / Asin(1 / Sqrt(databaseSize / nMarkedElements)) - 0.5).ToString()) : iterations);   //int.Parse(Round(await NIterations.Run(sim, nDatabaseQubits) * Sqrt(markedElements.Length)).ToString());

            var successCount = 0;

            var queries = nIterations * 2 + 1;

            var classicalSuccessProbability = (nMarkedElements) / databaseSize;
            var quantumSuccessProbability = Pow(Sin((2.0 * nIterations + 1.0) * Asin(Sqrt(nMarkedElements) / Sqrt(databaseSize))), 2.0);
            var repeats = (repetitions == 0 ? 10 : repetitions);

            double averageSuccess = 0;
            double averageProbability = 0;
            double averageSpeedup = 0;
            decimal averageTimeInSeconds = 0;
            double averageProbabilityClassical = 0;
            decimal averageTimeInSecondsClassical = 0;

            foreach (var idxAttempt in Enumerable.Range(0, repeats))
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                var task = ApplyGroverSearch.Run(sim, new QArray<long>(markedElements), nIterations, nDatabaseQubits);
                stopwatch.Stop();
                averageTimeInSeconds += (decimal)stopwatch.Elapsed.TotalSeconds;

                var data = task.Result;

                var markedQubit = data.Item1;
                var databaseRegister = data.Item2;

                successCount += markedQubit == Result.One ? 1 : 0;
                 
                if ((idxAttempt + 1) % 1 == 0)
                {
                    var empiricalSuccessProbability = Round(successCount / ((double)idxAttempt + 1), 3);

                    var speedupFactor = Round(empiricalSuccessProbability / classicalSuccessProbability / queries, 3);

                    averageSuccess += (markedQubit.ToString() == "One" ? 1 : 0);
                    averageProbability += empiricalSuccessProbability;
                    averageSpeedup += speedupFactor;
                }

                Stopwatch stopwatch2 = new Stopwatch();
                stopwatch2.Start();
                bool result = classicalSearch((int)databaseSize, markedElements);
                stopwatch2.Stop();
                averageTimeInSecondsClassical += (decimal)stopwatch2.Elapsed.TotalSeconds;

                averageProbabilityClassical += result ? 1 : 0;
            }
            await ReplyAsync($"Quantum search for marked element in database.\n" +
                $"Database size: {databaseSize}\n" +
                $"Marked elements: {string.Join(",", markedElements.Select(x => x.ToString()).ToArray())}\n" +
                $"Classical success probability: {classicalSuccessProbability}\n" +
                $"Queries per search: {queries} \n" +
                $"Iterations per search: {nIterations} \n" +
                $"Quantum success probability: {quantumSuccessProbability}\n" +
                $"Average success: {averageSuccess / repeats} \n" +
                $"Average probability: {averageProbability / repeats} \n" +
                $"Average classsical probability: {averageProbabilityClassical / repeats} \n" +
                $"Average speedup: {averageSpeedup / repeats} \n" +
                $"Average time: {averageTimeInSeconds / repeats} \n" +
                $"Average classical time: {averageTimeInSecondsClassical / repeats} \n");
        }

        public bool classicalSearch(int arraySize, long[] markedElements)
        {
            int[] database = new int[arraySize];
            foreach (int i in markedElements) database[i] = 1;
            
            List<int> matches = new ();

            for (int i = 0; i < database.Length; i++)
            {
                if (database[i] == 1)
                {
                    matches.Add(i);
                }
            }
            bool[] matched = matches.Select(a => markedElements.Contains(a)).ToArray();

            return matched.All(a => a);
        }

        [Command("blackBox")]
        [Summary("Deutsch oracle thing")]
        public async Task blackBox([Summary("The black box to pass in, 1 is constant zero, 2 is constant one, 3 is identity, 4 is negation.")] int input)
        {
            using var sim = new QuantumSimulator();
            string result = "";
            switch (input)
            {
                case 1:
                    result = "constant zero " + (await IsConstantZeroConstant.Run(sim) ? "is" : "is not") + " constant";
                    break;
                case 2:
                    result = "constant one " + (await IsConstantOneConstant.Run(sim) ? "is" : "is not") + " constant";
                    break;
                case 3:
                    result = "identity " + (await IsIdentityConstant.Run(sim) ? "is" : "is not") + " constant";
                    break;
                case 4:
                    result = "negation " + (await IsNegationConstant.Run(sim) ? "is" : "is not") + " constant";
                    break;
                default:
                    result = "The black box to pass in, 1 is constant zero, 2 is constant one, 3 is identity, 4 is negation.";
                    break;
            }
            await ReplyAsync(result);
        }
    }
}

