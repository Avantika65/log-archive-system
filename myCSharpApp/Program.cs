using myCSharpApp;
using myCSharpApp.Services;
using myCSharpApp.Models;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<ValidationService>();
builder.Services.Configure<WorkerSettings>(builder.Configuration.GetSection("WorkerSettings"));
builder.Services.AddSingleton<FileProcessingService>();

var host = builder.Build();
host.Run();
