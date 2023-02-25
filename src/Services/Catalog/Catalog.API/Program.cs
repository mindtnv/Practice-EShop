using Catalog.API.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
builder.Services
       .CustomConfigure(builder.Configuration)
       .AddConfiguredMvc()
       .AddConfiguredDbContext(builder.Configuration)
       .AddConfiguredSwagger()
       .AddConfiguredMasstransit(builder.Configuration);

var app = builder.Build();
app.UseSwagger().UseSwaggerUI();
app.UseRouting();
app.MapControllers();
app.Run();