using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using CSharpMath.SkiaSharp;
using Discord;
using Discord.Commands;
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
            if (!TryRender(input, out var originalImage, out var errorMessage))
            {
                await new EmbedBuilder()
                     .WithDescription(Format.Code(errorMessage))
                     .WithColor(Discord.Color.Red)
                     .Build()
                     .SendToChannel(Context.Channel);
                return;
            }
            
            using var image = AddPadding(originalImage);
            await using var stream = GetImageStream(image);

            await Context.Channel.SendFileAsync(stream, "equation.png");
            image.Dispose();
        }

        private static Stream GetImageStream(Image image)
        {
            var stream = new MemoryStream();
            image.Save(stream, ImageFormat.Png);
            stream.Position = 0;
            return stream;
        }

        private static bool TryRender(string input, out Image image, out string errorMessage)
        {
            var painter = new MathPainter { LaTeX = input, FontSize = 25, DisplayErrorInline = false };
            try
            {
                var stream = painter.DrawAsStream(format: SKEncodedImageFormat.Png);
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
            var bitmap = new Bitmap(original.Width + padding * 2, original.Height + padding * 2);
            using var graphics = Graphics.FromImage(bitmap);
            graphics.Clear(System.Drawing.Color.White);
            graphics.DrawImage(original, new Point(padding, padding));
            graphics.Flush();
            return bitmap;
        }
    }
}
