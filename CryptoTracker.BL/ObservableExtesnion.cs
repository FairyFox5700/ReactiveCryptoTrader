using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace CryptoTracker.BL
{
    public static class ObservableExtensions
    {
        public static IObservable<T> RetryWithBackoffStrategy<T>(
            this IObservable<T> source, 
            int retryCount = 3,
            Func<Exception, bool> retryOnError = null,
            IScheduler scheduler = null)
        {
            scheduler ??= Scheduler.Default;

            if (retryOnError == null)
                retryOnError = e => true;

            int attempt = 0;

            return Observable.Defer(() =>
                {
                    return ((++attempt == 1) ? source : source.DelaySubscription(TimeSpan.FromSeconds(Math.Pow((attempt-1), 2)), scheduler))
                        .Select(item => new Tuple<bool, T, Exception>(true, item, null))
                        .Catch<Tuple<bool, T, Exception>, Exception>(e => retryOnError(e)
                            ? Observable.Throw<Tuple<bool, T, Exception>>(e)
                            : Observable.Return(new Tuple<bool, T, Exception>(false, default(T), e)));
                })
                .Retry(retryCount)
                .SelectMany(t => t.Item1
                    ? Observable.Return(t.Item2)
                    : Observable.Throw<T>(t.Item3));
        }
    }
}