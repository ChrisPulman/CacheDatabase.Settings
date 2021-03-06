
using ReactiveMarbles.CacheDatabase.Settings;

namespace CacheDatabase.Settings.App
{
    public class ViewSettings : SettingsBase
    {
        public ViewSettings()
            : base(nameof(ViewSettings))
        { }
        public bool BoolTest
        {
            get => GetOrCreate(true); set => SetOrCreate(value);
        }

        public int IntTest
        {
            get => GetOrCreate(1); set => SetOrCreate(value);
        }

        public string? StringTest
        {
            get => GetOrCreate("TestString"); set => SetOrCreate(value);
        }

        public float FloatTest
        {
            get => GetOrCreate(2.2f); set => SetOrCreate(value);
        }
    }
}