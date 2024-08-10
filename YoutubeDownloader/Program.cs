using YoutubeDownloaderCS.Helpers.Repositories;
using YoutubeDownloaderCS.Helpers.Services;
using YoutubeDownloaderCS.Repositories;
using YoutubeDownloaderCS.Services;
using YoutubeExplode;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<IVideoService, VideoRepository>();
builder.Services.AddTransient<IAudioService, AudioRepository>();
builder.Services.AddTransient<IVideoHelper, VideoHelper>();


builder.Services.AddSingleton<IResponseStatus, ResponseStatusRepository>();
builder.Services.AddSingleton<YoutubeClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
