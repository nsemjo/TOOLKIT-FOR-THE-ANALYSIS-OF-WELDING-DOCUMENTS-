using CertificateRecognizer.Model;
using Domain.Model.Certificates;

namespace CertificateRecognizer.RecognitionPatterns
{
    internal interface IRecognitionPattern
    {
        Certificate TryRecognize(SimpleText text);
    }
}