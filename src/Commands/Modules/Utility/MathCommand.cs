using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using CSharpMath.SkiaSharp;
using Discord.Commands;
using SkiaSharp;


namespace BrackeysBot.Commands
{
    public partial class UtilityModule : BrackeysBotModule
    {
        [Command("math"), Alias("maths", "eq", "equation", "tex", "latex")]
        [Summary("Renders LaTeX input to an image, and attaches the result.")]
        [Remarks("math <input>")]
        public async Task MathCommandAsync([Summary("The LaTeX input."), Remainder] string input)
        {
            using var originalImage = Render(input);
            using var image = AddPadding(originalImage, 20);
            using var resized = Resize(image, image.Size / 2);
            await using var stream = GetImageStream(resized);

            await Context.Channel.SendFileAsync(stream, "equation.png");
        }

        private static Image Resize(Image image, Size newSize)
        {
            var bitmap = new Bitmap(newSize.Width, newSize.Height);
            using var graphics = Graphics.FromImage(bitmap);
            
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            
            graphics.DrawImage(image, new Rectangle(Point.Empty, newSize));
            graphics.Flush();
            return bitmap;
        }

        private static Stream GetImageStream(Image image)
        {
            var stream = new MemoryStream();
            image.Save(stream, ImageFormat.Png);
            stream.Position = 0;
            return stream;
        }

        private static Image Render(string input)
        {
            var painter = new MathPainter { LaTeX = input };
            var stream = painter.DrawAsStream(format: SKEncodedImageFormat.Png);
            return Image.FromStream(stream);
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
