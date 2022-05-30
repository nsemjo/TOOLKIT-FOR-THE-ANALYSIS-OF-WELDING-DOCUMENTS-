using System.Linq;
using CertificateRecognizer.Classes;
using Google.Cloud.Vision.V1;

namespace CertificateRecognizer.Model
{
    internal class SimpleText
    {
        #region fields



        #endregion

        #region constructors

        public SimpleText(TextAnnotation text) : this(text.Pages.Select(PageSimplifier.Simplify).ToArray())
        {
        }

        public SimpleText(SimplePage[] pages)
        {
            Pages = pages;
        }

        #endregion

        #region properties

        public SimplePage[] Pages { get; set; }

        #endregion

        #region public methods



        #endregion

        #region protected methods



        #endregion

        #region private methods



        #endregion
    }
}