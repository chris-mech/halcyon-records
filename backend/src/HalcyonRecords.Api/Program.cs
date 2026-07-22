var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.LicenseKey = builder.Configuration["MediatR:LicenseKey"];
});

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapGet("/", () => "Hello World!");

app.Run();