﻿using Microsoft.Extensions.Logging;
using MQSample_Common;
using SlugEnt.MQStreamProcessor;
using Spectre.Console;

namespace ProducerApp;

public class MainMenu
{
    private readonly ILogger           _logger;
    private          IServiceProvider  _serviceProvider;
    private          bool              _started;
    private          string            _streamName = "";
    private          IMqStreamProducer _producer   = null;

    private FlightProducerEngine _flightProducerEngine;
    private DisplayStats         _displayStats;


    public MainMenu(ILogger<MainMenu> logger, IServiceProvider serviceProvider)
    {
        _logger          = logger;
        _serviceProvider = serviceProvider;
    }



    internal async Task Start()
    {
        bool keepProcessing = true;

        Display();
        while (keepProcessing)
        {
            if (Console.KeyAvailable)
            {
                keepProcessing = await ProcessUserInput();
            }
            else
                Thread.Sleep(1000);

            Display();
        }
    }



    internal void Display()
    {
        string engineStatus = _started == true ? "Running" : "Stopped";
        AnsiConsole.WriteLine($" Engine is currently {engineStatus}");
        AnsiConsole.WriteLine();
        Console.WriteLine(" ( S ) StartAsync / Stop Producing Flights");
        Console.WriteLine();
        if (_displayStats != null)
            _displayStats.Refresh();
    }



    /// <summary>
    /// Processes user input.  Returns True, if we should keep processing.  False if user choose to exit.
    /// </summary>
    /// <returns></returns>
    internal async Task<bool> ProcessUserInput()
    {
        if (Console.KeyAvailable)
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey();

            switch (keyInfo.Key)
            {
                case ConsoleKey.S:
                    if (!_started)
                    {
                        // StartAsync the engine
                        _flightProducerEngine                 = (FlightProducerEngine)_serviceProvider.GetService(typeof(FlightProducerEngine));
                        _flightProducerEngine.WaitBetweenDays = 30;
                        await _flightProducerEngine.StartEngine();
                        _displayStats = new DisplayStats(_flightProducerEngine.Stats);
                    }
                    else if (_started)
                    {
                        // Stop the engine
                        await _flightProducerEngine.StopEngine();

                        // TODO Dispose it in future.
                        _flightProducerEngine = null;
                    }

                    _started = !_started;
                    break;

                case ConsoleKey.X: return false;
            }
        }

        return true;
    }
}