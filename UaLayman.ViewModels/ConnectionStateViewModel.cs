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
    public class ConnectionStateViewModel : ReactiveObject
    {
        private readonly ObservableAsPropertyHelper<CommunicationState> _State;
        public CommunicationState State => _State.Value;

        private readonly ObservableAsPropertyHelper<bool> _isConnected;
        public bool IsConnected => _isConnected.Value;

        private readonly ObservableAsPropertyHelper<bool> _faulted;
        public bool Faulted => _faulted.Value;

        public ConnectionStateViewModel(IChannelService channelService)
        {
            channelService.State
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, x => x.State, out _State);
            
            this.WhenAnyValue(x => x.State)
                .Select(x => x is CommunicationState.Opened)
                .ToProperty(this, x => x.IsConnected, out _isConnected);

            this.WhenAnyValue(x => x.State)
                .Select(x => x is CommunicationState.Faulted)
                .ToProperty(this, x => x.Faulted, out _faulted);
        }
    }
}
