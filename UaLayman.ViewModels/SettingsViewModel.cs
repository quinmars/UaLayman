using Akavache;
using DynamicData;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using UaLayman.Common;
using Workstation.ServiceModel.Ua;

namespace UaLayman.ViewModels
{
    public class SettingsViewModel : RoutableViewModelBase
    {
        private const string _themeString = "Theme";

        public IEnumerable<string> AvailableThemes { get; } = new[]
        {
            "Light",
            "Dark",
            "Windows default mode",
        };

        private string _selectedTheme;
        public string SelectedTheme
        {
            get => _selectedTheme;
            set => this.RaiseAndSetIfChanged(ref _selectedTheme, value);
        }

        public SettingsViewModel(IScreen screen, IThemeService themeService, IBlobCache blobCache) : base(screen, "Settings")
        {
            SelectedTheme = AvailableThemes.Last();

            this.WhenAnyValue(x => x.SelectedTheme)
                .Select(x =>
                {
                    switch (x)
                    {
                        case "Dark":
                            return ThemeMode.Dark;
                        case "Light":
                            return ThemeMode.Light;
                        default:
                            return ThemeMode.Default;
                    }
                })
                .DistinctUntilChanged()
                .Subscribe(x => themeService.RequestTheme(x));

            this.WhenAnyValue(x => x.SelectedTheme)
                .Subscribe(s => blobCache.InsertObject(_themeString, s));

            blobCache
                .GetObject<string>(_themeString)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(s => SelectedTheme = s);
        }
    }
}
