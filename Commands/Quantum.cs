using System.Threading.Tasks;
using System.Linq;
using Discord.Commands;
using Discord;
using System;
using System.Text.RegularExpressions;
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

        [Command("circuit")]
        [Summary("Construct quantum circuits using up to 5 qubits, current supported operations are " +
            "H(qubit index) \n" +
            "I(qubit index) \n" +
            "X(qubit index) \n" +
            "Y(qubit index) \n" +
            "Z(qubit index) \n" +
            "T(qubit index) \n" +
            "RX(theta, qubit index) \n" +
            "RY(theta, qubit index) \n" +
            "RZ(theta, qubit index) \n" +
            "S(qubit index) \n" +
            "SWAP(qubit1 index, qubit2 index) \n" +
            "CNOT(control qubit index, target qubit index) \n" +
            "CY(control qubit index, qubit index) \n" +
            "CZ(control qubit index, qubit index) \n" +
            "CCNOT(control qubit1 index, control qubit2 index, target qubit index) \n" +
            "R1(theta, qubit index) \n" +
            "CRX(control qubit index, theta, target qubit index) \n" +
            "CRY(control qubit index, theta, target qubit index) \n" +
            "CRZ(control qubit index, theta, target qubit index) \n" +
            "CR1(control qubit index, theta, target qubit index) \n" +
            "CH(control qubit index, target qubit index) \n" +
            "CI(control qubit index, target qubit index) \n" +
            "CT(control qubit index, target qubit index) \n" +
            "CS(control qubit index, target qubit index) \n" +
            "CSWAP(control qubit index, qubit1 index, qubit2 index) \n" +
            "M(qubit index) \n" +
            "RESET(qubit index) ")]
        public async Task circuit([Summary("The number of qubits to use, has to be 5 or under.")] int qubits, [Summary("Commands to use, seperated by '$' (ex. 'H(0)$M(0)').")] [Remainder] string commands)
        {
            if (qubits > 5)
            {
                await ReplyAsync("use 5 or under qubits");
                return;
            }

            string[] parsedCommands = commands.Split("$");

            List<string> Oqubits = new List<string>();

            List<string> Operations = new List<string>();

            foreach (string c in parsedCommands)
            {
                string c2 = Regex.Replace(c, @"\s+", "");
                if (c2 == "")
                {
                    continue;
                }

                string[] operationArray = c2.ToUpper().Split("(");

                operationArray[0] = operationArray[0] switch
                {
                    "CX" => "CNOT",
                    "CCX" => "CCNOT",
                    "TOFF" => "CCNOT",
                    _ => operationArray[0]
                };

                Operations.Add(operationArray[0]);

                string[] argumentArray = operationArray[1].Replace(")", "").Split(",");

                Oqubits.AddRange(argumentArray);

                bool NotError = true;
                NotError = operationArray[0] switch
                {
                    "H" => argumentArray.Length == 1,
                    "I" => argumentArray.Length == 1,
                    "X" => argumentArray.Length == 1,
                    "Y" => argumentArray.Length == 1,
                    "Z" => argumentArray.Length == 1,
                    "T" => argumentArray.Length == 1,
                    "RX" => argumentArray.Length == 2,
                    "RY" => argumentArray.Length == 2,
                    "RZ" => argumentArray.Length == 2,
                    "S" => argumentArray.Length == 1,
                    "SWAP" => argumentArray.Length == 2,
                    "CNOT" => argumentArray.Length == 2,
                    "CY" => argumentArray.Length == 2,
                    "CZ" => argumentArray.Length == 2,
                    "CCNOT" => argumentArray.Length == 3,
                    "R1" => argumentArray.Length == 2,
                    "CRX" => argumentArray.Length == 3,
                    "CRY" => argumentArray.Length == 3,
                    "CRZ" => argumentArray.Length == 3,
                    "CR1" => argumentArray.Length == 3,
                    "CH" => argumentArray.Length == 2,
                    "CI" => argumentArray.Length == 2,
                    "CT" => argumentArray.Length == 2,
                    "CS" => argumentArray.Length == 2,
                    "CSWAP" => argumentArray.Length == 3,
                    "M" => argumentArray.Length == 1,
                    "RESET" => argumentArray.Length == 1,
                    _ => false
                };

                if (!NotError)
                {
                    await ReplyAsync("There is an error in the arguments");
                    return;
                }
            }

            var sim = new QuantumSimulator(throwOnReleasingQubitsNotInZeroState: true);

            double[] finalArguments = Oqubits.Select(a => double.Parse(a)).ToArray();
            
            var results = await COperation.Run(sim, qubits, new QArray<double>(finalArguments), new QArray<string>(Operations));

            await ReplyAsync(results.ToString());
        } 

        //https://github.com/microsoft/Quantum/blob/main/samples/algorithms/database-search/Program.cs
        [Command("grover")]
        [Summary("search in sqrt(n) time")]
        public async Task grover([Summary("number of qubits to use")] int qubits,  [Summary("the indices (seperated by '$') of the marked values")] string matcher, [Summary("number of search iterations")] int iterations = 0, [Summary("number of search repetitions")] int repetitions = 0)
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
        public async Task voodoo([Summary("Whether your coin is heads or tails (true is heads)")]bool coin, [Summary("Optional seed to influence coin")] int seed = 0)
        {
            var r = new Random();
            
            var message = "```css\n";
            var numPhrases = r.Next(4, 9);
            List<string> phrases = new();
            List<string> states = new();

            seed = seed == 0 ? r.Next(0, 1000) : seed;

            //secret statements
            states.AddRange((numPhrases, seed) switch
            { 
                (7, 102) => new List<string> { "[ THEY   ]", "[ ARE    ]", "[ COMING ]", "[ RUN    ]", "[ WHILE  ]", "[ YOU    ]", "[ CAN    ]" },
                (7, 058) => new List<string> { "[ PREPARE ]", "[ FOR     ]", "[ THE     ]", "[ END     ]", "[ IT      ]", "[ HAS     ]", "[ BEGUN   ]" },
                (6, 358) => new List<string> { "[ It       ]", "[ IS       ]", "[ A        ]", "[ PLEASURE ]", "[ TO       ]", "[ BURN     ]" },
                (6, 111) => new List<string> { "[ MAY       ]", "[ I         ]", "[ HAVE      ]", "[ YOUR      ]", "[ ATTENTION ]", "[ PLEASE    ]" },
                (8, 111) => new List<string> {"[ A        ]", "[ FIRE     ]", "[ HAS      ]", "[ BEEN     ]", "[ REPORTED ]", "[ IN       ]", "[ THE      ]", "[ BUILDING ]"},
                (6, 112) => new List<string> {"[ PLEASE  ]", "[ PROCEED ]", "[ TO      ]", "[ THE     ]", "[ NEAREST ]", "[ EXIT    ]" },
                (5, 9999999) => new List<string> { "[ h ]", "[ E ]", "[ h ]", "[ E ]", "[ . ]"},
                (7, 66) => new List<string> { "[ BEWARE ]", "[ THE    ]", "[ MAN    ]", "[ WHO    ]", "[ SPEAKS ]", "[ IN     ]", "[ HANDS  ]" },
                (7, 0) => new List<string> { "[ BEWARE  ]", "[ THE     ]", "[ GECKO   ]", "[ YOU     ]", "[ WILL    ]", "[ NOT     ]", "[ SURVIVE ]" },
                (5, -1) => new List<string> { "[ GOODBYE ]", "[ WE      ]", "[ WILL    ]", "[ MEET    ]", "[ AGAIN   ]" },
                (5, 101) => new List<string> { "[ THE    ]", "[ LAST   ]", "[ WORD   ]", "[ IS     ]", "[ NOT    ]", "[ VERY   ]", "[ NICE    ]" },

                _ => new List<string>()
            });

            // Populate phrases and states lists
            for (int i = 0; i < numPhrases; i++)
            {
                phrases.Add(_voodooPhrases[r.Next(0, _voodooPhrases.Count)]);
                states.Add(_voodooStates[r.Next(0, _voodooStates.Count)]);
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
            "Finding Answer With High Probability",
            "Computing Oracle Synthesis Algorithm",
            "Factoring your private key",
            "Reflecting About Variable [life]",
            "Reflecting about superposition",
            "Obtaining Artoo's token",
            "Obtaining RBot's token",
            "Generating Circuit Diagram",
            "[ERR] Quantum state lost. All systems failed, attempting reboot...",
            "Rotating Text in MSPaint",
            "Plotting Vectors",
            "Accessing FBI backdoor",
            "Amplifying Amplified Amplitude",
            "Calculating arcsin of state",
            "Ising coupling qubits",
            "Applying Toffoli Gate",
            "Applying Ravioli Gate",
            "Swapping Qubit Matrices",
            "ApplyToAll(CCCCNOT)",
            "Reversing Operation",
            "Generating Reversable Black Box Oracle",
            "Qubits Entering Black Box...",
            "Reversing equations",
            "Preparing State of Emergency",
            "Preparing Railgun For Fire",
            "Preparing Large Hadron Collider For Fire",
            "Aquiring Funds from Nigerian Prince",
            "Performing Hug Operator on Qubits",
            "Calculating 𝜙",
            "Reverse Engineering Your Connection",
            "[object Object]",
            "Gutting Sacrificial Pig",
            "Entering Fourth Dimension",
            "Compressing Dark Matter",
            "Opening Dimensional Rift",
            "Finding Inner Peace",
            "null",
            "Reading TOS",
            "Generating predictive model",
            "Consulting Wikipedia",
            "Pasting Snippet From Stack Overflow",
            "Moving three parallel universes ahead",
            "Counting geckoimages",
            "Querying Drive API",
            "Pushing to GitHub",
            "Resolving Merge Conflicts",
            "Contacting Gandalf",
            "Contacting Dumbledore",
            "Obtaining a Soul",
            "Simulating Gecko in 4D Space",
            "Dealing With Null Reference Exception On Line 45",
            "Dealing With Null Reference Exception On Line ???",
            "Compiling...",
            "Powering On Fusion Reactor",
            "Contacting SCP Foundation",
            "Generating Frequencies",
            "Powering On Vacuum Tubes",
            "Generating Voronoi Pattern",
            "Obtaining Coronavirus Sample",
            "Priming Sculk Sensor",
            "Priming Rabbit-based Proximity Sensors",
            "Processing gcode",
            "Constructing 3D Printer",
            "Calculating Minimal Surface Egg",
            "Inviting Geckobot to Discord Servers",
            "[WARNING] This Connection Is No Longer Private",
            "Rerouting Connection Through Secure Server",
            "Simplifying Fractions",
            "Approximating Pi = 5",
            "Generating Quantum GeckoImage",
            "Uploading Status to Server",
            "Contacting AI-COM/RSPN",
            "Stockpiling Weapons",
            "Discussing With GBot",
        };

        private readonly List<string> _voodooStates = new()
        {
            "|+⟩",
            "|-⟩",
            "|+-⟩",
            "|1⟩",
            "|0⟩",
            "|01⟩",
            "|10⟩",
            "|0110⟩",
            "|42⟩",
            "[ UNKNOWN ]",
            "[ REDACTED ]",
            "[ OBSCURED ]",
            "[ LOADING... ]",
            "[ RENDERING... ]",
            "[ IN PROGRESS ]",
            "[ NULL ]",
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
            "|𝜙〉",
        };
    }
}

