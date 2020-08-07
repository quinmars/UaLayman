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
using Workstation.ServiceModel.Ua;

namespace UaLayman.ViewModels
{
    public class ConnectionViewModel : RoutableViewModelBase
    {
        public class ConnectionConfiguration
        {
            public string ConnectionString { get; set; }
            public string SecurityPolicy { get; set; }
            public MessageSecurityMode SecurityMode { get; set; }
        }

        private string _connectionString;
        public string ConnectionString
        {
            get => _connectionString;
            set => this.RaiseAndSetIfChanged(ref _connectionString, value);
        }

        private ObservableAsPropertyHelper<bool> _isConnectionStringValidating;
        public bool IsConnectionStringValidating => _isConnectionStringValidating.Value;

        private ObservableAsPropertyHelper<bool> _isConnectionStringFailed;
        public bool IsConnectionStringFailed => _isConnectionStringFailed.Value;

        private ObservableAsPropertyHelper<bool> _isConnectionStringOk;
        public bool IsConnectionStringOk => _isConnectionStringOk.Value;

        private ObservableAsPropertyHelper<EndpointDescription[]> _availableEndpoints;
        public EndpointDescription[] AvailableEndpoints => _availableEndpoints.Value;

        private ObservableAsPropertyHelper<EndpointDescription> _selectedEndpoint;
        public EndpointDescription SelectedEndpoint => _selectedEndpoint.Value;

        private ObservableAsPropertyHelper<IEnumerable<string>> _availableSecurityPolicies;
        public IEnumerable<string> AvailableSecurityPolicies => _availableSecurityPolicies.Value;

        private string _selectedSecurityPolicy;
        public string SelectedSecurityPolicy
        {
            get => _selectedSecurityPolicy;
            set => this.RaiseAndSetIfChanged(ref _selectedSecurityPolicy, value);
        }

        private ObservableAsPropertyHelper<IEnumerable<string>> _availableSecurityModes;
        public IEnumerable<string> AvailableSecurityModes => _availableSecurityModes.Value;

        private string _selectedSecurityMode;
        public string SelectedSecurityMode
        {
            get => _selectedSecurityMode;
            set => this.RaiseAndSetIfChanged(ref _selectedSecurityMode, value);
        }

        private ObservableAsPropertyHelper<bool> _isSearchingForSecurityPolicies;
        public bool IsSearchingForSecurityPolicies => _isSearchingForSecurityPolicies.Value;

        private ObservableAsPropertyHelper<string> _executionError;
        public string ExecutionError => _executionError.Value;

        private ReactiveCommand<string, EndpointDescription[]> Discover {get;}

        public ReactiveCommand<Unit, Unit> Disconnect { get; } 
        public ReactiveCommand<Unit, Unit> Connect { get; }

        private ObservableAsPropertyHelper<bool> _IsConnectingOrDisconnecting;
        public bool IsConnectingOrDisconnecting => _IsConnectingOrDisconnecting.Value;

        private ReadOnlyObservableCollection<ConnectionConfiguration> _connectionSuggestions;
        public ReadOnlyObservableCollection<ConnectionConfiguration> ConnectionSuggestions => _connectionSuggestions;

        private readonly ISubject<Unit> _startDiscover;
        public void StartDiscover() => _startDiscover.OnNext(default);

        public ConnectionViewModel(IScreen screen, IChannelService channelService, IDiscoveryService discoveryService)
            : base(screen, "Connection")
        {
            var connectionConfigurations = new SourceCache<ConnectionConfiguration, string>(x => x.ConnectionString);

            _startDiscover = new Subject<Unit>();
            _startDiscover
                .Select(_ => ConnectionString)
                .Where(s => s != null)
                .Select(s => Discover
                            .Execute(s)
                            .Catch(Observable.Return(new EndpointDescription[] { })))
                .Switch()
                .Subscribe();

            Discover = ReactiveCommand.CreateFromObservable((string s) => discoveryService.GetEndpoints(s));

            Observable.Merge(
                    Discover,
                    Discover.ThrownExceptions.Select(_ => default(EndpointDescription[]))
                )
                .ToProperty(this, x => x.AvailableEndpoints, out _availableEndpoints, null);

            Discover.IsExecuting
                .ToProperty(this, x => x.IsConnectionStringValidating,  out _isConnectionStringValidating, false);

            var typing = this.WhenAnyValue(x => x.ConnectionString)
                .Select(_ => false);

            Observable.Merge(
                Discover.Select(_ => true),
                typing)
                .ToProperty(this, x => x.IsConnectionStringOk, out _isConnectionStringOk);

            Observable.Merge(
                Discover.ThrownExceptions.Select(_ => true),
                typing)
                .ToProperty(this, x => x.IsConnectionStringFailed, out _isConnectionStringFailed);

            this.WhenAnyValue(x => x.IsConnectionStringValidating)
                .ToProperty(this, x => x.IsSearchingForSecurityPolicies, out _isSearchingForSecurityPolicies);

            this.WhenAnyValue(x => x.AvailableEndpoints)
                .Select(x => x?.Select(e => e.SecurityPolicyUri).Distinct())
                .ToProperty(this, x => x.AvailableSecurityPolicies, out _availableSecurityPolicies, null);
            
            this.WhenAnyValue(x => x.AvailableEndpoints, x => x.SelectedSecurityPolicy, (ends, pol) => (ends, pol))
                .Select(t =>
                {
                    if (t.ends is null)
                        return Enumerable.Empty<string>();
                    else
                        return t.ends
                                .Where(e => e.SecurityPolicyUri == t.pol)
                                .Select(e => e.SecurityMode.ToString())
                                .Distinct();
                })
                .ToProperty(this, x => x.AvailableSecurityModes, out _availableSecurityModes, null);

            connectionConfigurations
                .Connect()
                .Filter(s =>
                {
                    if (s == null)
                        return false;
                    if (string.IsNullOrEmpty(ConnectionString))
                        return true;
                    return s.ConnectionString.Contains(ConnectionString);
                })
                .Bind(out _connectionSuggestions)
                .Subscribe();

            this.WhenAnyValue(
                    x => x.AvailableEndpoints,
                    x => x.SelectedSecurityPolicy,
                    x => x.SelectedSecurityMode,
                    (ends, pol, mode) => (ends, pol, mode))
                .Select(t => t.ends?.FirstOrDefault(e => e.SecurityPolicyUri == t.pol && e.SecurityMode.ToString() == t.mode))
                .ToProperty(this, x => x.SelectedEndpoint, out _selectedEndpoint, null);

            var curConf = this.WhenAnyValue(x => x.ConnectionString, x => x.ConnectionSuggestions, (str, sug) => sug?.FirstOrDefault(s => s.ConnectionString == str));
            
            this.WhenAnyValue(x => x.AvailableSecurityPolicies)
                .WithLatestFrom(curConf, (pols, sel) => (pols, sel))
                .Where(t => t.sel != null && t.pols != null && t.pols.Any(m => m == t.sel.SecurityPolicy))
                .Subscribe(t => SelectedSecurityPolicy = t.sel.SecurityPolicy);

            this.WhenAnyValue(x => x.AvailableSecurityModes)
                .WithLatestFrom(curConf, (modes, sel) => (modes, sel))
                .Where(t => t.sel != null && t.modes != null && t.modes.Any(m => m == t.sel.SecurityMode.ToString()))
                .Select(t => t.sel.SecurityMode)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(m => SelectedSecurityMode = m.ToString(), ex => Debug.WriteLine(ex));
            /*
             * Connecting commands
             */
            var notConnected = channelService.State
                .Select(s => s == CommunicationState.Closed || s == CommunicationState.Faulted || s == CommunicationState.Created)
                .ObserveOn(RxApp.MainThreadScheduler);

            var canConnect = Observable.CombineLatest(
                this.WhenAnyValue(
                    x => x.IsSearchingForSecurityPolicies,
                    x => x.SelectedEndpoint,
                    (s, p) => !s && p != null),
                notConnected,
                (s, c) => s && c);

            Connect = ReactiveCommand.CreateFromObservable(() => channelService.Connect(SelectedEndpoint), canConnect);
            Connect.Subscribe();

            var canDisconnect = channelService.State
                .Select(s => s == CommunicationState.Opened || s == CommunicationState.Opening)
                .ObserveOn(RxApp.MainThreadScheduler);

            Disconnect = ReactiveCommand.CreateFromObservable(() => channelService.Disconnect(), canDisconnect);
            Disconnect.Subscribe();

            Observable.CombineLatest(Connect.IsExecuting, Disconnect.IsExecuting, (c, d) => c || d)
                .ToProperty(this, x => x.IsConnectingOrDisconnecting, out _IsConnectingOrDisconnecting, false);


            channelService.State
                .Where(s => s == CommunicationState.Opened)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(_ => NavigateTo.Execute("Browse"))
                .Switch()
                .Subscribe();
            /*
             * Akavache
             */
            Connect.IsExecuting
                .Where(x => x)
                .SelectMany(_ =>
                {
                    var cfg = new ConnectionConfiguration
                    {
                        ConnectionString = ConnectionString,
                        SecurityMode = Enum.TryParse<MessageSecurityMode>(SelectedSecurityMode, out var val) ? val : MessageSecurityMode.Invalid,
                        SecurityPolicy = SelectedSecurityPolicy
                    };
                    connectionConfigurations.AddOrUpdate(cfg);
                    return BlobCache.UserAccount.InsertObject(ConnectionString, cfg);
                })
                .Subscribe();

            BlobCache.UserAccount.GetAllObjects<ConnectionConfiguration>()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => connectionConfigurations.AddOrUpdate(x));


            /*
             * The error message
             */
            var noerr = this.WhenAnyValue(x => x.AvailableEndpoints)
                .Where(x => x != null && x.Any())
                .Select(_ => "");

            var err = Observable.Merge(
                    Discover.ThrownExceptions,
                    Connect.ThrownExceptions,
                    Disconnect.ThrownExceptions
                )
                .Select(ex =>
                {
                    switch (ex)
                    {
                        case ServiceResultException e:
                            var sc = e.StatusCode;
                            return $"{StatusCodes.GetDefaultMessage(sc)} ({sc})";
                        default:
                            return ex.Message;
                    };
                });

            Observable.Merge(err, noerr)
                .ToProperty(this, x => x.ExecutionError, out _executionError, null);
        }
    }
}
