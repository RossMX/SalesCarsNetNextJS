using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BuildingBlocks.Messaging.MassTransit;

public static class Extensions
{
    public static IServiceCollection AddMessageBroker(this IServiceCollection services,
                                                      Assembly? assembly = null)
    {
        services.AddMassTransit(x =>
        {
            //x.SetKebabCaseEndpointNameFormatter();
            x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));

            //x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();
            if (assembly != null) 
                x.AddConsumers(assembly);

            x.UsingRabbitMq( (context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
                //cfg.Host(builder.Configuration["RabbitMQ:Host"], h =>
                //{
                //    h.Username(builder.Configuration["RabbitMQ:Username"]);
                //    h.Password(builder.Configuration["RabbitMQ:Password"]);
                //});
            });
        });

        return services;
    }
}
