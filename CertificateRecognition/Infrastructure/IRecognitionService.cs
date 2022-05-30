using System.Threading.Tasks;
using CertificateRecognizer.Model;
using Domain.Model.Certificates;

namespace CertificateRecognizer.Infrastructure
{
    public interface IRecognitionService
    {
        Task<Certificate> RecognizeFileAsync(string path);
    }
}