using System.Linq;
using CertificateRecognizer.Extensions;
using Google.Cloud.Vision.V1;

namespace CertificateRecognizer.Model
{
    internal class SimpleParagraph : PositionedObject
    {
        #region fields



        #endregion

        #region constructors

        public SimpleParagraph(Paragraph paragraph) : this(paragraph.Words.Select(w => new SimpleWord(w)).ToArray())
        {
            Bounds = paragraph.BoundingBox.ToRect();
        }

        public SimpleParagraph(SimpleWord[] words)
        {
            Words = words;
        }

        #endregion

        #region properties

        public SimpleWord[] Words { get; set; }

        #endregion

        #region public methods

        public override string ToString()
        {
            return string.Join(" ", Words.Select(w => w.ToString()));
        }

        #endregion

        #region protected methods



        #endregion

        #region private methods



        #endregion
    }
}