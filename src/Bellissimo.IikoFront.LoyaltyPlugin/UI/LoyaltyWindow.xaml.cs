using System.Windows;

namespace Bellissimo.IikoFront.LoyaltyPlugin.UI
{
    public partial class LoyaltyWindow : Window
    {
        public LoyaltyWindow(LoyaltyViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
            Loaded += (s, e) => Activate(); // TODO(iiko-sdk): verify owner/focus workaround in iikoFront shell.
        }
    }
}
