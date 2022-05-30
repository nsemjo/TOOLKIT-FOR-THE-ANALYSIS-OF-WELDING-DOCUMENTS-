using System.Linq;
using Google.Cloud.Vision.V1;

namespace CertificateRecognizer.Model
{
    internal class SimplePage
    {
        #region fields



        #endregion

        #region constructors

        public SimplePage(Page page) : this(page.Blocks.Select(b => new SimpleBlock(b)).ToArray())
        {
            Width = page.Width;
            Height = page.Height;
        }

        public SimplePage(SimpleBlock[] blocks)
        {
            Blocks = blocks;
        }

        #endregion

        #region properties
        
        public int Width { get; set; }
        
        public int Height { get; set; }

        public double QuarterWidth => Width * 0.25;
        public double ThirdWidth => Width / 3.0;
        public double HalfWidth => Width * 0.5;
        public double TwoThirdsWidth => Width / 3.0 * 2;
        public double ThreeQuarterWidth => Width * 0.75;

        public double QuarterHeight => Height * 0.25;
        public double ThirdHeight => Height / 3.0;
        public double HalfHeight => Height * 0.5;
        public double TwoThirdsHeight => Height / 3.0 * 2;
        public double ThreeQuarterHeight => Height * 0.75;

        public SimpleBlock[] Blocks { get; set; }

        #endregion

        #region public methods



        #endregion

        #region protected methods



        #endregion

        #region private methods



        #endregion
    }
}