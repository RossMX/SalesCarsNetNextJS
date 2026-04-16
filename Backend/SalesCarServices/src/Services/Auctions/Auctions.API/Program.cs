using Auctions.API.Consumers;
using Auctions.API.Data;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

var dbConnection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AuctionDbContext>(opt =>
{
    opt.UseNpgsql(dbConnection);
});


builder.Services.AddAutoMapper(cfg => { }, AppDomain.CurrentDomain.GetAssemblies());

//builder.Services.AddMessageBroker(typeof(AuctionDbContext), Assembly.GetExecutingAssembly());
builder.Services.AddMassTransit(x =>
{
    x.AddEntityFrameworkOutbox<AuctionDbContext>(opt =>
    {
        opt.QueryDelay = TimeSpan.FromSeconds(10);
        opt.UsePostgres();
        opt.UseBusOutbox();
    });

    x.AddConsumersFromNamespaceContaining<AuctionCreatedFaultConsumer>();
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction", false));

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["MessageBroker:Host"], "/", h =>
        {
            h.Username(builder.Configuration.GetValue("MessageBroker:Username", "guest"));
            h.Password(builder.Configuration.GetValue("MessageBroker:Password", "guest"));
        });

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.Authority = builder.Configuration["IdentityServiceUrl"];
        opt.RequireHttpsMetadata = false;
        opt.TokenValidationParameters.ValidateAudience = false;
        opt.TokenValidationParameters.NameClaimType = "username";
    });


// Register services
builder.Services.AddScoped<IAuctionRepository, AuctionRepository>();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.UseSwagger();
    //app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


try
{
    await DbInitializer.Seed(app);
} catch (Exception e)
{
    Console.WriteLine($"An error occurred while seeding the database: {e.Message}");
}


app.Run();


public partial class Program { }