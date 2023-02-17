using Catalog.API.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services
       .AddCustomMvc()
       .AddCustomDbContext(builder.Configuration)
       .AddSwaggerGen();

var app = builder.Build();
app.UseSwagger().UseSwaggerUI();
app.UseRouting();
app.MapControllers();
app.Run();