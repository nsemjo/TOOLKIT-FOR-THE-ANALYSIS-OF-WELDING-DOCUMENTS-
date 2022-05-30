using System.Collections.Generic;
using CertificateRecognizer.Model;
using CertificateRecognizer.RecognitionPatterns.Patterns;
using CertificateService;
using Domain.Model.Certificates;

namespace CertificateRecognizer.RecognitionPatterns
{
    internal class CompositePattern : IRecognitionPattern
    {
        #region fields

        private List<IRecognitionPattern> _patterns;

        private readonly IWeldingProcessStorage _processStorage;

        #endregion

        #region constructors

        public CompositePattern(IWeldingProcessStorage processStorage)
        {
            _processStorage = processStorage;
            InitializePatterns();
        }

        #endregion

        #region properties



        #endregion

        #region public methods

        public Certificate TryRecognize(SimpleText text)
        {
            if (text == null) return null;
            
            foreach (var pattern in _patterns)
            {
                var cert = pattern.TryRecognize(text);
                if (cert != null && cert.IsValid)
                    return cert;
            }

            return null;
        }

        #endregion

        #region protected methods



        #endregion

        #region private methods

        private void InitializePatterns()
        {
            _patterns = new List<IRecognitionPattern>
            {
                new BureauVeritasPattern(_processStorage),
                new VerificatorPattern(_processStorage),
                new BtiPattern(_processStorage),
                new TuvPattern(_processStorage),
                new EestiPattern(_processStorage),
                new InspectaPattern(_processStorage),
                new DnvGlPattern(_processStorage),
            };
        }

        #endregion
    }
}