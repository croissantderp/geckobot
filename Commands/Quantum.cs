using System.Threading.Tasks;
using System.Linq;
using Discord.Commands;
using Discord;
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

        //absolutely fair coin flip
        [Command("coin flip")]
        [Summary("Simple coin flip game, you win if the coin is heads, computer wins if coins is tails")]
        public async Task coinAgainstComputer([Summary("Your move, true is to flip, false is to not.")] bool move)
        {
            using var sim = new QuantumSimulator();

            var result = await UnfairCoinFlip.Run(sim, move);

            await ReplyAsync("the bot Hadamard'd the coin, it is in |+⟩ \n you " + (move ? "flipped" : "did not flip") + " the coin, it is in |+⟩ \n the bot Hadamard'd the coin, it is in |0⟩ \n the coin is observed, it is in " + (result.ToString() == "1" ? "heads, the bot wins" : "tails, the bot wins"));
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
                await ReplyAsync("please use 15 or under qubits");
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

        private bool classicalSearch(int arraySize, long[] markedElements)
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
            string result = input switch
            {
                1 => "constant zero " + (await IsConstantZeroConstant.Run(sim) ? "is" : "is not") + " constant",
                2 => "constant one " + (await IsConstantOneConstant.Run(sim) ? "is" : "is not") + " constant",
                3 => "identity " + (await IsIdentityConstant.Run(sim) ? "is" : "is not") + " constant",
                4 => "negation " + (await IsNegationConstant.Run(sim) ? "is" : "is not") + " constant",
                _ => "The black box to pass in, 1 is constant zero, 2 is constant one, 3 is identity, 4 is negation.",
            };
            await ReplyAsync(result);
        }

        [Command("voodoo")]
        [Summary("Use the power of black magic to determine the outcome of your coin flip")]
        public async Task voodoo([Summary("Whether your coin is heads or tails (true is heads)")]bool coin)
        {
            var message = "```css\n";
            var numPhrases = new Random().Next(4, 7);
            List<string> phrases = new();
            List<string> states = new();
            
            // Populate phrases and states lists
            for (int i = 0; i < numPhrases; i++)
            {
                phrases.Add(_voodooPhrases[new Random().Next(0, _voodooPhrases.Count)]);
                states.Add(_voodooStates[new Random().Next(0, _voodooStates.Count)]);
            }

            var spaces = phrases.Select(x => x.Length).Max() + 3;
            message += "[ OPERATION: ]".PadRight(spaces) + "[ STATE: ]\n";
            
            // Append lists to message
            for (int i = 0; i < numPhrases; i++)
                message += $"{phrases[i].PadRight(spaces)}{states[i]}\n";

            var coinString = coin ? "HEADS" : "TAILS";
            message += $"[ DETERMINED: ] COIN was {coinString}\n```";
            
            await ReplyAsync(message);
        }

        private readonly List<string> _voodooPhrases = new()
        {
            "Expanding Matrix Quandaries",
            "Reciting Ancient Ritual",
            "Offering Tasty Sacrifice",
            "Solving For X",
            "Rationalizing Pi",
            "Engaging Quantum Oracle",
            "Constructing Additional Pylons",
            "Applying Hadamard Cascade",
            "Simulating Bloch Sphere",
            "Flipping Separate Yet Quantumly Orthogonal Coin",
            "Allocating Qubit Array",
            "Consuming Computer RAM",
            "Consuming Computer",
            "Heating Up CPU",
            "Applying Bitwise XOR",
            "Spawning Child Process",
            "Determining Inverse Square Root",
            "Communing With The Deep Ones",
            "Applying PauliZ rotation of PI() / 4 radians",
            "Finding answer with high probability",
            "Computing Oracle Synthesis algorithm",
            "Factoring your private key",
            "Reflect About var life",
            "Obtaining Artoo's token",
            "Obtaining RBot's token",
            "Generating circuit diagram",
            "Quantum state lost. All systems failed, attempting reboot...",
            "rotating characters in mspaint",
            "Plotting vectors",
            "Listening to music",
            "Accessing FBI backdoor",
            "Amplifying amplified amplitude",
            "Calculating arcsin of state",
            "Ising coupling qubits",
            "Applying Toffoli gate",
            "Applying Ravioli gate",
            "Swapping qubit matrices",
            "ApplyToAll(CCCCNOT)",
            "Reversing operation, generating reversable black box oracle",
            "Doing this thing 𝜙",
            "Reverse engineering your connection",
            "",
        };

        private readonly List<string> _voodooStates = new()
        {
            "|+⟩",
            "|-⟩",
            "|1⟩",
            "|0⟩",
            "|01⟩",
            "|10⟩",
            "|0110⟩",
            "|42⟩",
            "[ UNKNOWN ]",
            "[ REDACTED ]",
            "|ψ〉〈ψ|",
            "|k〉",
            "|z ⊕ xₖ〉",
            "|ψ〉",
            "|z〉 |k〉 = |z ⊕ xₖ〉 |k〉",
            "|z〉 |k〉 ↦ |z ⊕ f(k)〉 |k〉",
            "DU|0〉|0〉",
            "(RS · RM)^M |s〉",
            "sin((2M+1)θ) |1〉|N-1〉",
            "cos((2M+1)θ) |0〉(|0〉+|1〉+...+|N-2〉)",
            "O(1/√N)",
            "|𝜙〉"
        };
    }
}

