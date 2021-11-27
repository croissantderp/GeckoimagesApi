using Microsoft.EntityFrameworkCore;
using GeckoimagesApi.Models;
using GeckoimagesApi.DriveService;
using Microsoft.Extensions.FileProviders;

if (!Directory.Exists("./public")) Directory.CreateDirectory("./public");
//if (File.Exists("./public/index.html")) 

var options = new WebApplicationOptions { WebRootPath = "../public", ContentRootPath = "./public" };

var builder = WebApplication.CreateBuilder(options);

// Add services to the container.
builder.Services.AddControllers();

var services = builder.Services.AddDbContext<GeckoContext>(opt => opt.UseInMemoryDatabase("GeckoList"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseDefaultFiles(new DefaultFilesOptions 
{
    DefaultFileNames = new[] { "index.html" },
});

// using Microsoft.Extensions.FileProviders;
// using System.IO;
app.UseStaticFiles(new StaticFileOptions
{
    ServeUnknownFileTypes = true,
    FileProvider = new PhysicalFileProvider(app.Environment.ContentRootPath),
    RequestPath = ""
});

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

DriveCheck check = new(services.BuildServiceProvider().GetRequiredService<GeckoContext>());
check.setTimer().GetAwaiter().GetResult();

app.Run();
