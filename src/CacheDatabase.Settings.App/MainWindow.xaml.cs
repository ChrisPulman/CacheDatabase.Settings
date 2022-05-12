using CP.CacheDatabase.Settings;
using CP.CacheDatabase.Settings.Core;
using ReactiveMarbles.Locator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            locator.SetupSettingsStore<ViewSettings>();

            var viewSettings = locator.GetService<ViewSettings>(nameof(ViewSettings));
            if (viewSettings != null)
            {
                var a = viewSettings.FloatTest;
                var b = viewSettings.StringTest;
                var c = viewSettings.IntTest;
                var d = viewSettings.BoolTest;
            }
        }
    }
}
