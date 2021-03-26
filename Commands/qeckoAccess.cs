using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using static System.Diagnostics.Debug;
using Discord;
using Discord.Commands;

using Quantum.qeckoBot;

using Microsoft.Quantum.Simulation.Core;
using Microsoft.Quantum.Simulation.Simulators;

namespace GeckoBot.Commands
{
    public class QuantumStart : ModuleBase<SocketCommandContext>
    {
        //rng but qUaNtUm
        [Command("qrng")]
        [Summary("random function using quantum bits")]
        public async Task qrng()
        {
            using var sim = new QuantumSimulator();

            long num = await Quantum.qeckoBot.SampleRandomNumber.Run(sim);
            await ReplyAsync(num.ToString());
        }
    }
}

