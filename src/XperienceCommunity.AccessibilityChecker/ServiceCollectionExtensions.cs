using XperienceCommunity.AccessibilityChecker.Controllers;
using XperienceCommunity.AccessibilityChecker.Scanning;

namespace XperienceCommunity.AccessibilityChecker
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAccessibilityChecker(
            this IServiceCollection services)
        {
            services.AddControllers()
                .AddApplicationPart(
                    typeof(AccessibilityScanController).Assembly);

            services.AddSingleton<IAccessibilityScanService, PlaywrightAccessibilityScanService>();
            services.AddSingleton<IScanResultRepository, ScanResultRepository>();

            return services;
        }
    }
}
