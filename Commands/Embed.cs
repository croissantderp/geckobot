using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using GeckoBot.Utils;
using System.Text.RegularExpressions;

namespace GeckoBot.Commands
{
    [Summary("Embed building.")]
    public class Embed : ModuleBase<SocketCommandContext>
    {
        //custom embed builder
        [Command("embed")]
        [Summary("Builds an embed from the provided arguments.")]
        public async Task embed([Summary("Title of the embed.")] string title, [Summary("Fields seperated by '$$', title and and descriptions seperated by '%%'.")] string field, [Summary("Optional thumbnail url.")] string thumbnail = null, [Summary("Optional footer.")] string footer2 = null, [Summary("Optional color in hexidecimal.")] string hex = null)
        {
            //starts embed building procedure
            var embed = new EmbedBuilder
            {
                Title = title
            };

            //splits fields by $$
            string[] fields = Regex.Split(field, @"(?<!\\)\$\$");

            foreach(string a in fields)
            {
                //splits subfields by %%
                string[] subfields = Regex.Split(a, @"(?<!\\)\%\%");

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

            if (hex != null)
            {
                //converts hex to rgb
                if (hex.IndexOf('#') != -1)
                {
                    hex = hex.Replace("#", "");
                }

                int r, g, b;

                r = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
                g = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
                b = int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.AllowHexSpecifier);

                //adds color
                embed.WithColor(r, g, b);
            }

            //adds author of message
            embed.WithAuthor(Context.User);

            if (thumbnail != null)
            {
                embed.WithThumbnailUrl(thumbnail);
            }

            if (footer2 != null)
            {
                //assigns footer
                embed.WithFooter(footer => footer.Text = footer2);
            }
            //assigns time
            embed.WithCurrentTimestamp();

            await ReplyAsync("", embed: embed.Build(), allowedMentions: Globals.allowed);
        }
    }
}