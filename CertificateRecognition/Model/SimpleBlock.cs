using System.Linq;
using CertificateRecognizer.Extensions;
using Google.Cloud.Vision.V1;

namespace CertificateRecognizer.Model
{
    internal class SimpleBlock : PositionedObject
    {
        #region fields



        #endregion

        #region constructors

        public SimpleBlock(Block block) : this(block.Paragraphs.Select(p => new SimpleParagraph(p)).ToArray())
        {
            Bounds = block.BoundingBox.ToRect();
        }

        public SimpleBlock(SimpleParagraph[] paragraphs)
        {
            Paragraphs = paragraphs;
        }

        #endregion

        #region properties

        public SimpleParagraph[] Paragraphs { get; set; }

        #endregion

        #region public methods

        public override string ToString()
        {
            return string.Join(" ", Paragraphs.Select(p => p.ToString()));
        }

        #endregion

        #region protected methods



        #endregion

        #region private methods



        #endregion
    }
}