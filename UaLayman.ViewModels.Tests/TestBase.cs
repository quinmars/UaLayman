using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;

namespace UaLayman.ViewModels.Tests
{
    public class TestBase
    {
        public TestBase()
        {
            RxApp.MainThreadScheduler = CurrentThreadScheduler.Instance;
        }
    }
}
