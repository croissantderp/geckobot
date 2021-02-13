using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using GeckoBot.Utils;

namespace GeckoBot.Commands
{
    public class Embed : ModuleBase<SocketCommandContext>
    {
        //custom embed builder
        [Command("embed")]
        [Summary("Builds an embed from the provided arguments.")]
        public async Task embed(string title, string field, string footer2, string hex)
        {
            //converts hex to rgb
            if (hex.IndexOf('#') != -1)
            {
                hex = hex.Replace("#", "");
            }

            int r, g, b = 0;

            r = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
            g = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
            b = int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.AllowHexSpecifier);

            //starts embed building procedure
            var embed = new EmbedBuilder
            {
                Title = title
            };

            //splits fields by $$
            string[] fields = field.Split("$$");

            foreach(string a in fields)
            {
                //splits subfields by %%
                string[] subfields = a.Split("%%");

                //checks lengths of subfields
                if (subfields.Length == 1)
                {
                    //adds subfield as description if there isn't one already
                    if (embed.Description == null)
                    {
                        embed.Description = subfields[0];
                    }
                    else
                    {
                        embed.Description = "​";

                        //adds subfield with blank title
                        embed.AddField("​", subfields[0]);
                    }

                }
                else
                {
                    //adds field with title and description
                    embed.AddField(subfields[0], subfields[1]);
                }
            }

            //adds color
            embed.WithColor(r,g,b);

            //adds author of message
            embed.WithAuthor(Context.User);

            //assigns footer
            embed.WithFooter(footer => footer.Text = footer2);

            //assigns time
            embed.WithCurrentTimestamp();

            await ReplyAsync("", embed: embed.Build(), allowedMentions: Globals.allowed);
        }
    }
}