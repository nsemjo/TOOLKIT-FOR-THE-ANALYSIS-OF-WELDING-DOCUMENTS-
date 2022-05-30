using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using CertificateRecognizer.Extensions;
using Google.Cloud.Vision.V1;
using Image = Google.Cloud.Vision.V1.Image;

namespace CertificateRecognizer.Classes
{
    internal static class DebugDrawer
    {
        #region fields

        private const string DebugFilePath = "./debug_last.png";

        private static Color BlockColor = Color.Blue;
        private static Color ParagraphColor = Color.DarkGreen;
        private static Color WordColor = Color.Red;

        private static Pen BlockPen;
        private static Pen ParagraphPen;
        private static Pen WordPen;

        #endregion

        #region constructors

        static DebugDrawer()
        {
            BlockPen = new Pen(BlockColor, 1);
            ParagraphPen = new Pen(ParagraphColor, 1);
            WordPen = new Pen(WordColor, 1);
        }

        #endregion

        #region properties



        #endregion

        #region public methods

        public static void SaveToFile(Image image, TextAnnotation text)
        {
            if (text == null) return;
            var imageContent = image.Content.ToByteArray();
            using (var stream = new MemoryStream(imageContent, false))
            using (var debugImage = System.Drawing.Image.FromStream(stream))
            using (var graphics = Graphics.FromImage(debugImage))
            {
                var page = text.Pages.First();
                var blocks = page.Blocks.ToList();
                var paragraphs = blocks.SelectMany(b => b.Paragraphs).ToList();
                var words = paragraphs.SelectMany(p => p.Words).ToList();

                foreach (var block in blocks)
                    graphics.DrawPolygon(BlockPen, block.BoundingBox.Vertices.Select(v => v.ToPoint()).ToArray());

                foreach (var block in paragraphs)
                    graphics.DrawPolygon(ParagraphPen, block.BoundingBox.Vertices.Select(v => v.ToPoint()).ToArray());

                foreach (var block in words)
                    graphics.DrawPolygon(WordPen, block.BoundingBox.Vertices.Select(v => v.ToPoint()).ToArray());
                
                debugImage.Save(DebugFilePath, ImageFormat.Png);
            }
        }

        #endregion

        #region protected methods



        #endregion

        #region private methods



        #endregion
    }
}