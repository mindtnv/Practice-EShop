using Catalog.API.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services
       .CustomConfigure(builder.Configuration)
       .AddCustomMvc()
       .AddCustomDbContext(builder.Configuration)
       .AddCustomSwagger();

var app = builder.Build();
app.UseSwagger().UseSwaggerUI();
app.UseRouting();
app.MapControllers();
app.Run();