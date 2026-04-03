using MassTransit;
using Polly;
using Polly.Extensions.Http;
using Search.API.Consumers;
using Search.API.Data;
using Search.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpClient<AuctionSvcHttpClient>()
    .AddPolicyHandler(GetRetryForeverPolicy());

builder.Services.AddAutoMapper((cfg) => { }, AppDomain.CurrentDomain.GetAssemblies());

//builder.Services.AddMessageBroker(Assembly.GetExecutingAssembly());
builder.Services.AddMassTransit(x =>
{
    x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));

    x.UsingRabbitMq((context, cfg) =>
    {
        // queue name to listen messages
        cfg.ReceiveEndpoint("search-auction-created-event", e =>
        {
            // options to retry
            e.UseMessageRetry(r => r.Interval(5, 5));
            e.ConfigureConsumer<AuctionCreatedConsumer>(context);
        });

        cfg.ConfigureEndpoints(context);
        //cfg.Host(builder.Configuration["RabbitMQ:Host"], h =>
        //{
        //    h.Username(builder.Configuration["RabbitMQ:Username"]);
        //    h.Password(builder.Configuration["RabbitMQ:Password"]);
        //});
    });
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.MapOpenApi();
//}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Lifetime.ApplicationStarted.Register(async () =>
{
    try
    {
        await DbInitializer.InitDb(app);
    } catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
});

app.Run();


static IAsyncPolicy<HttpResponseMessage> GetRetryForeverPolicy() =>
    HttpPolicyExtensions.HandleTransientHttpError()
                        .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() =>
    HttpPolicyExtensions.HandleTransientHttpError()
                        .WaitAndRetryAsync(3, retryAttempt =>
                            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));