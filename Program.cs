using Microsoft.EntityFrameworkCore;
using GeckoimagesApi.Models;
using GeckoimagesApi.DriveService;
using Microsoft.Extensions.FileProviders;

if (Directory.Exists("./public")) Directory.CreateDirectory("./public");

Console.WriteLine(Directory.GetCurrentDirectory());

var options = new WebApplicationOptions { ContentRootPath = "./public" };

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

app.UseHttpsRedirection();

// using Microsoft.Extensions.FileProviders;
// using System.IO;
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(builder.Environment.ContentRootPath),
    RequestPath = ""
});

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

DriveCheck check = new(services.BuildServiceProvider().GetRequiredService<GeckoContext>());
check.setTimer().GetAwaiter().GetResult();

app.Run();
