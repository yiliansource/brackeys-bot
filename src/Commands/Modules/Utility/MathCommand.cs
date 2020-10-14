using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using CSharpMath.SkiaSharp;
using Discord;
using Discord.Commands;
using Humanizer;
using Humanizer.Localisation;
using SkiaSharp;
using Image = System.Drawing.Image;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace BrackeysBot.Commands
{
    public partial class UtilityModule : BrackeysBotModule
    {
        [Command("math"), Alias("maths", "eq", "equation", "tex", "latex")]
        [Summary("Renders LaTeX input to an image, and attaches the result.")]
        [Remarks("math <input>")]
        public async Task MathCommandAsync([Summary("The LaTeX input."), Remainder] string input)
        {
            if (FilterService.ContainsBlockedWord(input))
            {
                return;
            }
            
            bool canOverride = (Context.User as IGuildUser).GetPermissionLevel(Context) >= PermissionLevel.Moderator;
            int remaining = MathService.LatexTimeoutRemaining(Context.User);
            
            if (!canOverride && remaining > 0)
                throw new TimeoutException($"You need to wait {TimeSpan.FromMilliseconds(remaining).Humanize(2, minUnit: TimeUnit.Second)} before you can use this command again!");
            
            MathService.UpdateLatexTimeout(Context.User);
            
            if (!TryRender(input, out Image originalImage, out string errorMessage))
            {
                await new EmbedBuilder()
                     .WithDescription(Format.Code(errorMessage))
                     .WithColor(Discord.Color.Red)
                     .Build()
                     .SendToChannel(Context.Channel);
                return;
            }
            
            using Image image = AddPadding(originalImage);
            await using Stream stream = GetImageStream(image);

            await Context.Channel.SendFileAsync(stream, "equation.png");
            image.Dispose();
        }

        private static Stream GetImageStream(Image image)
        {
            MemoryStream stream = new MemoryStream();
            image.Save(stream, ImageFormat.Png);
            stream.Position = 0;
            return stream;
        }

        private static bool TryRender(string input, out Image image, out string errorMessage)
        {
            MathPainter painter = new MathPainter { LaTeX = input, FontSize = 25, DisplayErrorInline = false };
            try
            {
                Stream stream = painter.DrawAsStream(format: SKEncodedImageFormat.Png);
                image = Image.FromStream(stream);
                errorMessage = null;
                return true;
            }
            catch (NullReferenceException)
            {
                image = null;
                errorMessage = painter.ErrorMessage;
                return false;
            }
        }

        private static Image AddPadding(Image original, int padding = 10)
        {
            Bitmap bitmap = new Bitmap(original.Width + padding * 2, original.Height + padding * 2);
            using Graphics graphics = Graphics.FromImage(bitmap);
            graphics.Clear(System.Drawing.Color.White);
            graphics.DrawImage(original, new Point(padding, padding));
            graphics.Flush();
            return bitmap;
        }
    }
}
