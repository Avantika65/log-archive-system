using myCSharpApp;
using myCSharpApp.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<ValidationService>();

var host = builder.Build();
host.Run();
