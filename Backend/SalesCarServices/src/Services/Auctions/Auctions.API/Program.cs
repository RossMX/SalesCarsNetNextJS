using Auctions.API.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

var dbConnection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AuctionDbContext>(opt => {
    opt.UseNpgsql(dbConnection);
});


builder.Services.AddAutoMapper(cfg => {}, AppDomain.CurrentDomain.GetAssemblies());


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

app.UseHttpsRedirection();

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
