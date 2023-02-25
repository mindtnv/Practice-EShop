using Basket.API.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
builder.Services
       .AddConfiguredSwagger()
       .AddConfiguredControllers()
       .AddConfiguredRedisBasketRepository(builder.Configuration)
       .AddConfiguredMassTransit(builder.Configuration);

var app = builder.Build();
app.UseRouting();
app.UseSwagger().UseSwaggerUI();
app.MapControllers();

app.MapGet("/", () => Results.LocalRedirect("/swagger", true));

app.Run();