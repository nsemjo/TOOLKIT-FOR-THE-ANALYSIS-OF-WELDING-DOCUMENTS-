using System.Linq;
using CertificateRecognizer.Extensions;
using Google.Cloud.Vision.V1;

namespace CertificateRecognizer.Model
{
    internal class SimpleWord : PositionedObject
    {
        #region fields



        #endregion

        #region constructors

        public SimpleWord(Word word) : this(word.FromSymbols())
        {
            Bounds = word.BoundingBox.ToRect();
        }

        public SimpleWord(string word)
        {
            Word = word;
        }

        #endregion

        #region properties

        public string Word { get; set; }

        #endregion

        #region public methods

        public override string ToString()
        {
            return Word;
        }

        #endregion

        #region protected methods



        #endregion

        #region private methods



        #endregion
    }
}