﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoPatterns.Tests.Examples;

namespace AutoPatterns.PerfTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var test = new ExampleTests();
            test.ExampleAutomaticPropertyAndDataContract();
        }
    }
}
