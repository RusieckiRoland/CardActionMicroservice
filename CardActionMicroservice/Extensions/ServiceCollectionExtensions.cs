using CardActionMicroservice.Business.Services;
using CardActionMicroservice.Business.Strategies;
using CardActionMicroservice.DataProviders;
using CardActionMicroservice.Infrastructure;

namespace CardActionService.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            
            services.AddSingleton(provider =>
            {
                var ruleLoader = provider.GetRequiredService<IRuleLoader>();              

                return BusinessStrategiesFactory.CreateStrategies(ruleLoader);
            });
            
            services.AddSingleton<AllowedActionsService>();
            services.AddSingleton<ICardProvider, InMemoryCardProvider>();

            return services;
        }

    }
}



