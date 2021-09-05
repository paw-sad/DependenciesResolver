﻿using System;

namespace DependenciesResolver
{
    public interface ILogger
    {
        void Log(string message);
    }

    public class ConsoleLogger : ILogger
    {
        public void Log(string message)
        {
            Console.WriteLine($"Info: {message}");
        }
    }
}