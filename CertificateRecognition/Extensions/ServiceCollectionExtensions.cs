using CertificateRecognizer.Classes;
using CertificateRecognizer.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace CertificateRecognizer.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddRecognitionService(this IServiceCollection services)
        {
            services.AddSingleton<IRecognitionService, RecognitionService>();
        }
    }
}