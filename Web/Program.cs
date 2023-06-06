using Grpc.Core;
using Grpc.Net.Client.Configuration;
using Grpc.Net.Client;
using GrpcService;
using Microsoft.AspNetCore.ResponseCompression;
using Web.HostedServices;
using Web.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<IPipelinePublisher, PipelinePublisher>();

// background service for SignalR
builder.Services.AddHostedService<SignalRBackgroundService>();

builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});

builder.Services.AddGrpcClient<PipelineSubscriber.PipelineSubscriberClient>(options =>
{
    options.Address = new Uri("https://localhost:7284");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapHub<PipelineHub>("/pipelinehub");
app.MapFallbackToPage("/_Host");

app.Run();
