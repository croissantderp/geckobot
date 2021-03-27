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
    }
}

