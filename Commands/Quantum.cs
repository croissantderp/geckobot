using System.Threading.Tasks;
using Discord.Commands;

using Quantum.qeckoBot;

using Microsoft.Quantum.Simulation.Simulators;

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

