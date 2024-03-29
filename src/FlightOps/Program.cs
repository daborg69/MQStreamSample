﻿using FlightOps;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using SlugEnt.MQStreamProcessor;
using System.Reflection;


public class Program
{
    static async Task Main(string[] args)
    {
        Serilog.ILogger Logger;
        Log.Logger = new LoggerConfiguration()
#if DEBUG
                     .MinimumLevel.Debug()
                     .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
#else
						 .MinimumLevel.Information()
			             .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
#endif
                     .Enrich.FromLogContext()
                     .WriteTo.Console()
                     .WriteTo.Debug()
                     .CreateLogger();

        Log.Debug("Starting " + Assembly.GetEntryAssembly().FullName);


        var      host     = CreateHostBuilder(args).Build();
        MainMenu mainMenu = host.Services.GetService<MainMenu>();
        await host.StartAsync();

        await mainMenu.Start();

        return;
    }


    /// <summary>
    /// Creates the Host
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((_, services) =>
            {
                services.AddTransient<MainMenu>();
                services.AddTransient<IMqStreamConsumer, MqStreamConsumer>();
                services.AddTransient<IMqStreamProducer, MqStreamProducer>();
                services.AddTransient<IMQStreamEngine, MQStreamEngine>();
                services.AddTransient<FlightOperationsEngine>();
            })
            .ConfigureLogging((_, logging) =>
            {
                logging.ClearProviders();
                logging.AddSerilog();
                logging.AddDebug();
                logging.AddConsole();

                //logging.AddSimpleConsole(options => options.IncludeScopes = true);
                //logging.AddEventLog();
            });
}