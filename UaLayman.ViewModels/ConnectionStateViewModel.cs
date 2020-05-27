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
        private readonly IChannelService _channelService;

        private readonly ObservableAsPropertyHelper<CommunicationState> _State;
        public CommunicationState State => _State.Value;

        public ConnectionStateViewModel(IChannelService channelService)
        {
            _channelService = channelService;

            _channelService.State
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, x => x.State, out _State);
        }
    }
}
