using Splat;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;
using ReactiveUI;
using System.Reactive.Concurrency;
using ReactiveUI.Testing;

namespace UaLayman.ViewModels.Tests
{
    public class GeneralTests : TestBase
    {
        [Fact]
        public void SplatModeDetection()
        {
            ModeDetector.InUnitTestRunner()
                .Should().BeTrue();
        }

        [Fact]
        public void MainThreadScheduler()
        {
            RxApp.MainThreadScheduler
                .Should().BeOfType<CurrentThreadScheduler>();
        }
    }
}
