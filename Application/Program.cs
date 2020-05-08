using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using CommonServiceLocator;
using Infrastructure;
using Prism.Events;

namespace Application
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            var application = new App();
            application.InitializeComponent();
            application.Run();
        }
    }
}