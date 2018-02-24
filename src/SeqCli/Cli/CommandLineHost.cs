﻿// Copyright 2018 Datalust Pty Ltd
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac.Features.Metadata;

namespace SeqCli.Cli
{
    class CommandLineHost
    {
        readonly List<Meta<Lazy<Command>, CommandMetadata>> _availableCommands;

        public CommandLineHost(IEnumerable<Meta<Lazy<Command>, CommandMetadata>> availableCommands)
        {
            _availableCommands = availableCommands.ToList();
        }

        public async Task<int> Run(string[] args)
        {
            var ea = Assembly.GetEntryAssembly();
            var name = ea.GetName().Name;

            if (args.Length > 0)
            {
                var amountToSkip = 1;
                var norm = args[0].ToLowerInvariant();
                Meta<Lazy<Command>, CommandMetadata> cmd; 
                if (!args[1].Contains("-"))
                {
                    amountToSkip = 2;
                    cmd = _availableCommands.SingleOrDefault(c => c.Metadata.Name == norm && c.Metadata.SubCommand == args[1].ToLowerInvariant());
                }
                else
                {
                    cmd = _availableCommands.SingleOrDefault(c => c.Metadata.Name == norm);
                }
                if (cmd != null)
                {
                    return await cmd.Value.Value.Invoke(args.Skip(amountToSkip).ToArray());
                }
            }

            Console.WriteLine($"Usage: {name} <command> [<args>]");
            Console.WriteLine($"Type `{name} help` for available commands");
            return -1;
        }
    }
}
