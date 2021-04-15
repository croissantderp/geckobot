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
    //no I am not commenting this

    [Summary("A quantum coin flipping game to see if you can trust your friends!")]
    [RequireContext(ContextType.DM)]
    public class QuantumCoinFlip : ModuleBase<SocketCommandContext>
    {
        private static readonly Dictionary<string, string> usersInGame = new Dictionary<string, string>();
        private static readonly List<string> justMoved = new List<string>();

        //coin flip but cheating is not allowed
        [Command("coin")]
        [Summary("**Info**\nA game where you encrypt five bits in rectilinear or diagonal fashion. Somebody else then has to guess the type of encryption by randomly testing the bits. (See https://en.wikipedia.org/wiki/Quantum_coin_flipping for more reference).\n\n" +
            "**Instructions**\nThe sender encrypts the bits. The recipitant will guess using '`guess [rect/diag]'. The sender would then '`confirm' or '`deny' the guess. The recipitant will recieve that result and '`confirm' or '`deny' it. The bot then sends the winner and other statistics.")]
        public async Task coin([Summary("The id of your opponent, playing against yourself is supported if you put your own id")] string recipitantid, [Summary("A string of bits (0 or 1) seperated by $")] string bits, [Summary("Type of encoding used, rectilinear or diagonal. ('rect' or 'diag')")] string encoding)
        {
            var recipitant = Context.Client.GetUser(ulong.Parse(recipitantid));

            if (usersInGame.Keys.Any(a => a.Contains(recipitant.Id.ToString())))
            {
                await ReplyAsync("person is already in a game");
                return;
            }
            IUser sender = Context.User;

            if (usersInGame.Keys.Any(a => a.Contains(sender.Id.ToString())))
            {
                await ReplyAsync("cannot start a new game while in a game");
                return;
            }

            usersInGame.Add(recipitant.Id.ToString() + "$" + sender.Id.ToString(), "");

            if (encoding != "rect" && encoding != "diag")
            {
                await ReplyAsync("use arguments 'rect' and 'diag' only for the encryption field");
                return;
            }

            using var sim = new QuantumSimulator();

            if (bits.Split("$").Any(a => a != "1" && a != "0"))
            {
                await ReplyAsync("make sure bits only consist of 1s and 0s and seperators ($)");
                return;
            }

            var elements = bits.Split("$").Select(a => a == "1");


            if (elements.Count() != 5)
            {
                await ReplyAsync("please enter exactly 5 bits");
                return;
            }

            Random rng = new Random();
            bool[] decoding = new bool[5];

            for (int i = 0; i < rng.Next((int)Round((double)5 / 2, MidpointRounding.ToZero), (int)Round((double)5 / 2, MidpointRounding.AwayFromZero) + 1); i++)
            {
                decoding[i] = true;
            }

            int n = decoding.Length;
            for (int i = 0; i < (n - 1); i++)
            {
                int r = i + rng.Next(n - i);
                var t = decoding[r];
                decoding[r] = decoding[i];
                decoding[i] = t;
            }

            var result = await CoinFlip.Run(sim, new QArray<bool>(elements), encoding == "rect", new QArray<bool>(decoding));

            string[] final = new string[5];

            for (int i = 0; i < 5; i++)
            {
                if (decoding[i])
                {
                    final[i] = $"{(result[i] == Result.One ? "1" : "0")}| ";
                }
                else
                {
                    final[i] = $" |{(result[i] == Result.One ? "1" : "0")}";
                }
            }

            usersInGame.Remove(recipitant.Id.ToString() + "$" + sender.Id.ToString());
            usersInGame.Add(recipitant.Id.ToString() + "$" + sender.Id.ToString(),encoding + "%" + bits + "%" + string.Join("\n", final));

            await recipitant.SendMessageAsync("decoding results from " + sender.ToString() + ":\n```R|D\n" + string.Join("\n", final) + "``` Use '\\`guess [rect/diag]' to guess the type of encryption used.");
            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

        [Command("guess")]
        [Summary("Guess in the quantum coin game.")]
        public async Task coinLie([Summary("Type of encoding used, rectilinear or diagonal.")] string encoding)
        {
            if (!usersInGame.Keys.Any(a => a.Contains(Context.User.Id.ToString())))
            {
                await ReplyAsync("you are not in a game");
                return;
            }


            if (encoding != "rect" && encoding != "diag")
            {
                await ReplyAsync("use arguments 'rect' and 'diag' only");
                return;
            }

            string temp = usersInGame.Keys.First(a => a.Contains(Context.User.Id.ToString()));
            List<string> users = temp.Split("$").ToList();

            if (users[1] == Context.User.Id.ToString() && users[0] != users[1])
            {
                await ReplyAsync("wait for your teammate to respond");
                return;
            }

            users.Remove(Context.User.Id.ToString());

            string temp2 = usersInGame[temp];
            usersInGame.Remove(temp);
            usersInGame.Add(temp, encoding + "%" + temp2);

            justMoved.Add(Context.User.Id.ToString());

            await Context.Client.GetUser(ulong.Parse(users[0])).SendMessageAsync("your opponent guessed " + encoding + ", were they correct? Use \\`confirm or \\`deny. \nThis is an opportunity to lie, you are not required to tell the truth. You can also chose to send them a new string of bits");
            await Context.Message.AddReactionAsync(new Emoji("✅"));
        }

        [Command("confirm")]
        [Summary("Confirm a result in the quantum coin flip game.")]
        public async Task confirm([Summary("Optional new string of bits to send to your opponent")] string optionalBits = null)
        {
            if (!usersInGame.Keys.Any(a => a.Contains(Context.User.Id.ToString())))
            {
                await ReplyAsync("you are not in a game");
                return;
            }

            string temp = usersInGame.Keys.First(a => a.Contains(Context.User.Id.ToString()));
            string[] users = temp.Split("$");

            if (justMoved.Contains(Context.User.Id.ToString()) && users[0] != users[1])
            {
                await ReplyAsync("wait for your teammate to respond");
                return;
            }

            if (usersInGame[temp].Split("%").Length == 4)
            {
                if (optionalBits != null)
                {
                    if (optionalBits.Split("$").Any(a => a != "1" && a != "0"))
                    {
                        await ReplyAsync("make sure bits only consist of 1s and 0s and seperators ($)");
                        return;
                    }

                    if (optionalBits.Split("$").Count() != 5)
                    {
                        await ReplyAsync("please enter exactly 5 bits");
                        return;
                    }
                }

                string[] results = usersInGame[temp].Split("%")[3].Split("\n");
                string[] original = optionalBits != null ? optionalBits.Split("$") : usersInGame[temp].Split("%")[2].Split("$");
                string[] final = new string[results.Length];

                for (int i = 0; i < results.Length; i++)
                {
                    final[i] = original[i] + " -> " + results[i];
                }

                bool fakeBits = string.Join("$", original) != usersInGame[temp].Split("%")[2];

                await Context.Client.GetUser(ulong.Parse(users[0])).SendMessageAsync("your opponent said your guess was correct. Do you agree?\n" +
                    "the original bits (O) should match with the correct decryption method (R or D). If nothing matches, it means your opponent sent you fake bits." +
                    $"```O    R|D\n{string.Join("\n", final)}``` " +
                    "Use \\`confirm or \\`deny.\nThis is an opportunity to lie, you are not required to tell the truth.");

                string temp2 = usersInGame[temp];
                usersInGame.Remove(temp);
                usersInGame.Add(temp, (fakeBits ? "fake" : "real") + "%confirm%" + temp2);
                Console.WriteLine(fakeBits);
                justMoved.Remove(users[0]);
                justMoved.Add(Context.User.Id.ToString());

                await Context.Message.AddReactionAsync(new Emoji("✅"));
            }
            else if (usersInGame[temp].Split("%").Length > 4)
            {
                bool p1lie = false;
                bool p2lie = false;

                bool actualCorrect = usersInGame[temp].Split("%")[2] == usersInGame[temp].Split("%")[3];

                p1lie = (usersInGame[temp].Split("%")[1] == (actualCorrect ? "deny" : "confirm")) || (usersInGame[temp].Split("%")[0] != "real");

                p2lie = (usersInGame[temp].Split("%")[1] == "deny") == actualCorrect;

                string winner = (actualCorrect ? Context.Client.GetUser(ulong.Parse(users[0])).ToString() : Context.Client.GetUser(ulong.Parse(users[1])).ToString());

                string liars = (p1lie, p2lie) switch
                {
                    (true, true) => Context.Client.GetUser(ulong.Parse(users[1])).ToString() + " lied, but " + Context.Client.GetUser(ulong.Parse(users[0])).ToString() + " accepted it",
                    (true, false) => Context.Client.GetUser(ulong.Parse(users[1])).ToString() + " is a liar but got away with it",
                    (false, true) => Context.Client.GetUser(ulong.Parse(users[0])).ToString() + " is a liar",
                    (false, false) => "both players were being truthful",
                };

                foreach (string user in users)
                {
                    await Context.Client.GetUser(ulong.Parse(user)).SendMessageAsync(
                        winner + " won the coin toss" + (actualCorrect ? " by guessing " + usersInGame[temp].Split("%")[2] : "") + "\n" +
                        liars + "\nThe game has ended."
                        );
                }
                usersInGame.Remove(temp);

                justMoved.Remove(users[0]);
                justMoved.Remove(users[1]);
            }
            else
            {
                await ReplyAsync("your game has not progressed to this point yet");
                return;
            }
        }

        //coin flip but cheating is not allowed
        [Command("deny")]
        [Summary("Deny a result in the quantum coin flip game.")]
        public async Task deny([Summary("Optional new string of bits to send to your opponent")] string optionalBits = null)
        {
            if (!usersInGame.Keys.Any(a => a.Contains(Context.User.Id.ToString())))
            {
                await ReplyAsync("you are not in a game");
                return;
            }

            string temp = usersInGame.Keys.First(a => a.Contains(Context.User.Id.ToString()));
            string[] users = temp.Split("$");

            if (justMoved.Contains(Context.User.Id.ToString()) && users[0] != users[1])
            {
                await ReplyAsync("wait for your teammate to respond");
                return;
            }

            if (usersInGame[temp].Split("%").Length == 4)
            {
                if (optionalBits != null)
                {
                    if (optionalBits.Split("$").Any(a => a != "1" && a != "0"))
                    {
                        await ReplyAsync("make sure bits only consist of 1s and 0s and seperators ($)");
                        return;
                    }

                    if (optionalBits.Split("$").Count() != 5)
                    {
                        await ReplyAsync("please enter exactly 5 bits");
                        return;
                    }
                }

                string[] results = usersInGame[temp].Split("%")[3].Split("\n");
                string[] original = optionalBits != null ? optionalBits.Split("$") : usersInGame[temp].Split("%")[2].Split("$");
                string[] final = new string[results.Length];

                for (int i = 0; i < results.Length; i++)
                {
                    final[i] = original[i] + " -> " + results[i];
                }

                bool fakeBits = string.Join("$", original) != usersInGame[temp].Split("%")[2];

                await Context.Client.GetUser(ulong.Parse(users[0])).SendMessageAsync("your opponent said your guess was incorrect. Do you agree?\n" +
                    "the original bits (O) should match with the correct decryption method (R or D). If nothing matches, it means your opponent sent you fake bits." +
                    $"```O    R|D\n{string.Join("\n", final)}``` " +
                    "Use \\`confirm or \\`deny.\nThis is an opportunity to lie, you are not required to tell the truth.");

                string temp2 = usersInGame[temp];
                usersInGame.Remove(temp);
                usersInGame.Add(temp, (fakeBits ? "fake" : "real") + "%deny%" + temp2);

                justMoved.Remove(users[0]);
                justMoved.Add(Context.User.Id.ToString());

                await Context.Message.AddReactionAsync(new Emoji("✅"));
            }
            else if(usersInGame[temp].Split("%").Length > 4)
            {
                bool p1lie = false;
                bool p2lie = false;

                bool actualCorrect = usersInGame[temp].Split("%")[2] == usersInGame[temp].Split("%")[3];

                p1lie = (usersInGame[temp].Split("%")[1] == (actualCorrect ? "deny": "confirm")) || (usersInGame[temp].Split("%")[0] != "real");

                p2lie = (usersInGame[temp].Split("%")[1] == "confirm") == actualCorrect;

                string winner = (actualCorrect ? Context.Client.GetUser(ulong.Parse(users[0])).ToString() : Context.Client.GetUser(ulong.Parse(users[1])).ToString());

                string liars = (p1lie, p2lie) switch
                {
                    (true, true) => Context.Client.GetUser(ulong.Parse(users[1])).ToString() + " was caught lying",
                    (true, false) => Context.Client.GetUser(ulong.Parse(users[1])).ToString() + " was caught lying " + (!actualCorrect ? "though" : ""),
                    (false, true) => Context.Client.GetUser(ulong.Parse(users[0])).ToString() + " lied about " + (actualCorrect ? "winning" : "losing"),
                    (false, false) => "both players were being truthful",
                };

                foreach (string user in users)
                {
                    await Context.Client.GetUser(ulong.Parse(user)).SendMessageAsync(
                        winner + " won the coin toss" + (actualCorrect ? " by guessing " + usersInGame[temp].Split("%")[2] : "") + "\n" +
                        liars + "\nThe game has ended."
                        );
                }
                usersInGame.Remove(temp);

                justMoved.Remove(users[0]);
                justMoved.Remove(users[1]);
            }
            else
            {
                await ReplyAsync("your game has not progressed to this point yet");
                return;
            }
        }

        //coin flip but cheating is not allowed
        [Command("quit")]
        [Summary("Quit a game of coin.")]
        public async Task quit()
        {
            if (!usersInGame.Keys.Any(a => a.Contains(Context.User.Id.ToString())))
            {
                await ReplyAsync("you are not in a game");
                return;
            }

            string temp = usersInGame.Keys.First(a => a.Contains(Context.User.Id.ToString()));
            string[] users = temp.Split("$");
            
            foreach (string user in users)
            {
                await Context.Client.GetUser(ulong.Parse(user)).SendMessageAsync("you/opponent has quit the game");
            }

            usersInGame.Remove(temp);

            justMoved.Remove(users[0]);
            justMoved.Remove(users[1]);
        }
    }
}
