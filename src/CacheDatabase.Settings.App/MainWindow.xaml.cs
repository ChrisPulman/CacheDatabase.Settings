using CP.CacheDatabase.Settings.Locator;
using ReactiveMarbles.Locator;
using System.Reactive.Linq;
using System.Windows;

namespace CacheDatabase.Settings.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var locator = new ServiceLocator();
            Observable.Start(async () =>
            {
                await locator.SetupSettingsStore<ViewSettings>();

                var viewSettings = locator.GetService<ViewSettings>(nameof(ViewSettings));
                if (viewSettings != null)
                {
                    var a = viewSettings.FloatTest;
                    var b = viewSettings.StringTest;
                    var c = viewSettings.IntTest;
                    var d = viewSettings.BoolTest;
                }
            });
        }
    }
}
