using CertificateRecognizer.Model;
using CertificateRecognizer.Orientation;
using Google.Cloud.Vision.V1;

namespace CertificateRecognizer.Classes
{
    internal static class PageSimplifier
    {
        #region fields



        #endregion

        #region constructors

        

        #endregion

        #region properties



        #endregion

        #region public methods

        public static SimplePage Simplify(Page page)
        {
            var result = new SimplePage(page);
            
            var orientation = OrientationHelper.GetOrientation(page);

            OrientationHelper.CorrectOrientation(result, orientation);

            return result;
        }

        #endregion

        #region protected methods



        #endregion

        #region private methods



        #endregion
    }
}