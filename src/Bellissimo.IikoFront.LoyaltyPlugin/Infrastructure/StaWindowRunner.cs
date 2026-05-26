using System;
using System.Threading;
using System.Windows;

namespace Bellissimo.IikoFront.LoyaltyPlugin.Infrastructure
{
    public static class StaWindowRunner
    {
        public static void Run(Func<Window> factory)
        {
            var thread = new Thread(() =>
            {
                var window = factory();
                window.ShowDialog();
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
        }
    }
}
