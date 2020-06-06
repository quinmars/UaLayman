using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;

namespace UaLayman.ViewModels
{
    public static class ObservableEx
    {
        public static IObservable<T> DisposeLast<T>(this IObservable<T> source)
            where T : IDisposable
        {
            return Observable.Create((IObserver<T> obs) =>
            {
                var disposable = new SerialDisposable();
                source.Subscribe(Observer.Create<T>(
                    onNext: v =>
                    {
                        disposable.Disposable = v;
                        obs.OnNext(v);
                    },
                    onError: obs.OnError,
                    onCompleted: obs.OnCompleted
                    ));

                return disposable;
            });
        }
    }
}
