using System;
using System.Collections.Generic;
using System.Text;
using Prism.Commands;

namespace WindowUI
{
    internal class ModuleCommands
    {
        public static CompositeCommand Navigate { get; } = new CompositeCommand();
    }
}
