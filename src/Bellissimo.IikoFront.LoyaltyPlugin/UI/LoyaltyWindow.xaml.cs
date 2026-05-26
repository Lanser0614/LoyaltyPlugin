using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Bellissimo.IikoFront.LoyaltyPlugin.UI
{
    public partial class LoyaltyWindow : Window
    {
        public LoyaltyWindow(LoyaltyViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
            Loaded += (s, e) =>
            {
                var helper = new WindowInteropHelper(this);
                SetForegroundWindow(helper.Handle);
            };
        }

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}
