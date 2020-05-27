using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Polly.Contrib.WaitAndRetry
{
    /// <summary>
    /// Helper methods for creating backoff strategies.
    /// </summary>
    public static partial class Backoff
    {
        private static IEnumerable<TimeSpan> Empty()
        {
            yield break;
        }
    }

    partial class Backoff // .Linear
    {
        /// <summary>
        /// Generates sleep durations in an linear manner.
        /// The formula used is: Duration = <paramref name="initialDelay"/> x (1 + <paramref name="factor"/> x iteration).
        /// For example: 100ms, 200ms, 300ms, 400ms, ...
        /// </summary>
        /// <param name="initialDelay">The duration value for the first retry.</param>
        /// <param name="factor">The linear factor to use for increasing the duration on subsequent calls.</param>
        /// <param name="retryCount">The maximum number of retries to use, in addition to the original call.</param>
        /// <param name="fastFirst">Whether the first retry will be immediate or not.</param>
        public static IEnumerable<TimeSpan> LinearBackoff(TimeSpan initialDelay, int retryCount, double factor = 1.0, bool fastFirst = false)
        {
            if (initialDelay < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(initialDelay), initialDelay, "should be >= 0ms");
            if (retryCount < 0) throw new ArgumentOutOfRangeException(nameof(retryCount), retryCount, "should be >= 0");
            if (factor < 0) throw new ArgumentOutOfRangeException(nameof(factor), factor, "should be >= 0");

            if (retryCount == 0)
                return Empty();

            return Enumerate(initialDelay, retryCount, fastFirst, factor);

            IEnumerable<TimeSpan> Enumerate(TimeSpan initial, int retry, bool fast, double f)
            {
                int i = 0;
                if (fast)
                {
                    i++;
                    yield return TimeSpan.Zero;
                }

                double ms = initial.TotalMilliseconds;
                double ad = f * ms;

                for (; i < retry; i++, ms += ad)
                {
                    yield return TimeSpan.FromMilliseconds(ms);
                }
            }
        }
    }

    partial class Backoff // .Exponential
    {
        /// <summary>
        /// Generates sleep durations in an exponential manner.
        /// The formula used is: Duration = <paramref name="initialDelay"/> x 2^iteration.
        /// For example: 100ms, 200ms, 400ms, 800ms, ...
        /// </summary>
        /// <param name="initialDelay">The duration value for the wait before the first retry.</param>
        /// <param name="factor">The exponent to multiply each subsequent duration by.</param>
        /// <param name="retryCount">The maximum number of retries to use, in addition to the original call.</param>
        /// <param name="fastFirst">Whether the first retry will be immediate or not.</param>
        public static IEnumerable<TimeSpan> ExponentialBackoff(TimeSpan initialDelay, int retryCount, double factor = 2.0, bool fastFirst = false)
        {
            if (initialDelay < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(initialDelay), initialDelay, "should be >= 0ms");
            if (retryCount < 0) throw new ArgumentOutOfRangeException(nameof(retryCount), retryCount, "should be >= 0");
            if (factor < 1.0) throw new ArgumentOutOfRangeException(nameof(factor), factor, "should be >= 1.0");

            if (retryCount == 0)
                return Empty();

            return Enumerate(initialDelay, retryCount, fastFirst, factor);

            IEnumerable<TimeSpan> Enumerate(TimeSpan initial, int retry, bool fast, double f)
            {
                int i = 0;
                if (fast)
                {
                    i++;
                    yield return TimeSpan.Zero;
                }

                double ms = initial.TotalMilliseconds;
                for (; i < retry; i++, ms *= f)
                {
                    yield return TimeSpan.FromMilliseconds(ms);
                }
            }
        }
    }

    partial class Backoff // .Constant
    {
        /// <summary>
        /// Generates sleep durations as a constant value.
        /// The formula used is: Duration = <paramref name="delay"/>.
        /// For example: 200ms, 200ms, 200ms, ...
        /// </summary>
        /// <param name="delay">The constant wait duration before each retry.</param>
        /// <param name="retryCount">The maximum number of retries to use, in addition to the original call.</param>
        /// <param name="fastFirst">Whether the first retry will be immediate or not.</param>
        public static IEnumerable<TimeSpan> ConstantBackoff(TimeSpan delay, int retryCount, bool fastFirst = false)
        {
            if (delay < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(delay), delay, "should be >= 0ms");
            if (retryCount < 0) throw new ArgumentOutOfRangeException(nameof(retryCount), retryCount, "should be >= 0");

            if (retryCount == 0)
                return Empty();

            return Enumerate(delay, retryCount, fastFirst);

            IEnumerable<TimeSpan> Enumerate(TimeSpan timeSpan, int retry, bool fast)
            {
                int i = 0;
                if (fast)
                {
                    i++;
                    yield return TimeSpan.Zero;
                }

                for (; i < retry; i++)
                {
                    yield return timeSpan;
                }
            }
        }
    }

    partial class Backoff // .AwsDecorrelatedJitter
    {
        /// <summary>
        /// Generates sleep durations in an jittered manner, making sure to mitigate any correlations.
        /// For example: 117ms, 236ms, 141ms, 424ms, ...
        /// Per the formula from https://aws.amazon.com/blogs/architecture/exponential-backoff-and-jitter/.
        /// </summary>
        /// <param name="minDelay">The minimum duration value to use for the wait before each retry.</param>
        /// <param name="maxDelay">The maximum duration value to use for the wait before each retry.</param>
        /// <param name="retryCount">The maximum number of retries to use, in addition to the original call.</param>
        /// <param name="seed">An optional <see cref="Random"/> seed to use.
        /// If not specified, will use a shared instance with a random seed, per Microsoft recommendation for maximum randomness.</param>
        /// <param name="fastFirst">Whether the first retry will be immediate or not.</param>
        public static IEnumerable<TimeSpan> AwsDecorrelatedJitterBackoff(TimeSpan minDelay, TimeSpan maxDelay, int retryCount, int? seed = null, bool fastFirst = false)
        {
            if (minDelay < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(minDelay), minDelay, "should be >= 0ms");
            if (maxDelay < minDelay) throw new ArgumentOutOfRangeException(nameof(maxDelay), maxDelay, $"should be >= {minDelay}");
            if (retryCount < 0) throw new ArgumentOutOfRangeException(nameof(retryCount), retryCount, "should be >= 0");

            if (retryCount == 0)
                return Empty();

            return Enumerate(minDelay, maxDelay, retryCount, fastFirst, new ConcurrentRandom(seed));

            IEnumerable<TimeSpan> Enumerate(TimeSpan min, TimeSpan max, int retry, bool fast, ConcurrentRandom random)
            {
                int i = 0;
                if (fast)
                {
                    i++;
                    yield return TimeSpan.Zero;
                }

                // https://github.com/aws-samples/aws-arch-backoff-simulator/blob/master/src/backoff_simulator.py#L45
                // self.sleep = min(self.cap, random.uniform(self.base, self.sleep * 3))

                // Formula avoids hard clamping (which empirically results in a bad distribution)
                double ms = min.TotalMilliseconds;
                for (; i < retry; i++)
                {
                    double ceiling = Math.Min(max.TotalMilliseconds, ms * 3);
                    ms = random.Uniform(min.TotalMilliseconds, ceiling);

                    yield return TimeSpan.FromMilliseconds(ms);
                }
            }
        }
    }

    partial class Backoff // .DecorrelatedJitterV2
    {
        /// <summary>
        /// Generates sleep durations in an exponentially backing-off, jittered manner, making sure to mitigate any correlations.
        /// For example: 850ms, 1455ms, 3060ms.
        /// Per discussion in Polly issue 530, the jitter of this implementation exhibits fewer spikes and a smoother distribution than the AWS jitter formula.
        /// </summary>
        /// <param name="medianFirstRetryDelay">The median delay to target before the first retry, call it f (= f * 2^0).
        /// Choose this value both to approximate the first delay, and to scale the remainder of the series.
        /// Subsequent retries will (over a large sample size) have a median approximating retries at time f * 2^1, f * 2^2 ... f * 2^t etc for try t.
        /// The actual amount of delay-before-retry for try t may be distributed between 0 and f * (2^(t+1) - 2^(t-1)) for t >= 2;
        /// or between 0 and f * 2^(t+1), for t is 0 or 1.</param>
        /// <param name="retryCount">The maximum number of retries to use, in addition to the original call.</param>
        /// <param name="seed">An optional <see cref="Random"/> seed to use.
        /// If not specified, will use a shared instance with a random seed, per Microsoft recommendation for maximum randomness.</param>
        /// <param name="fastFirst">Whether the first retry will be immediate or not.</param>
        public static IEnumerable<TimeSpan> DecorrelatedJitterBackoffV2(TimeSpan medianFirstRetryDelay, int retryCount, int? seed = null, bool fastFirst = false)
        {
            if (medianFirstRetryDelay < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(medianFirstRetryDelay), medianFirstRetryDelay, "should be >= 0ms");
            if (retryCount < 0) throw new ArgumentOutOfRangeException(nameof(retryCount), retryCount, "should be >= 0");

            if (retryCount == 0)
                return Empty();

            return Enumerate(medianFirstRetryDelay, retryCount, fastFirst, new ConcurrentRandom(seed));

            // The original author/credit for this jitter formula is @george-polevoy . Jitter formula used with permission as described at https://github.com/App-vNext/Polly/issues/530#issuecomment-526555979 
            // Minor adaptations (pFactor = 4.0 and rpScalingFactor = 1 / 1.4d) by @reisenberger, to scale the formula output for easier parameterisation to users.

            IEnumerable<TimeSpan> Enumerate(TimeSpan scaleFirstTry, int maxRetries, bool fast, ConcurrentRandom random)
            {
                // A factor used within the formula to help smooth the first calculated delay.
                const double pFactor = 4.0;

                // A factor used to scale the median values of the retry times generated by the formula to be _near_ whole seconds, to aid Polly user comprehension.
                // This factor allows the median values to fall approximately at 1, 2, 4 etc seconds, instead of 1.4, 2.8, 5.6, 11.2.
                const double rpScalingFactor = 1 / 1.4d;

                int i = 0;
                if (fast)
                {
                    i++;
                    yield return TimeSpan.Zero;
                }

                long targetTicksFirstDelay = scaleFirstTry.Ticks;

                double prev = 0.0;
                for (; i < maxRetries; i++)
                {
                    double t = (double)i + random.NextDouble();
                    double next = Math.Pow(2, t) * Math.Tanh(Math.Sqrt(pFactor * t));

                    double formulaIntrinsicValue = next - prev;
                    yield return TimeSpan.FromTicks((long)(formulaIntrinsicValue * rpScalingFactor * targetTicksFirstDelay));

                    prev = next;
                }

            }
        }
    }

    /// <summary>
    /// A random number generator with a Uniform distribution that is thread-safe (via locking).
    /// Can be instantiated with a custom <see cref="int"/> seed to make it emit deterministically.
    /// </summary>
    internal sealed class ConcurrentRandom
    {
        // Singleton approach is per MS best-practices.
        // https://docs.microsoft.com/en-us/dotnet/api/system.random?view=netframework-4.7.2#the-systemrandom-class-and-thread-safety
        // https://stackoverflow.com/a/25448166/
        // Also note that in concurrency testing, using a 'new Random()' for every thread ended up
        // being highly correlated. On NetFx this is maybe due to the same seed somehow being used
        // in each instance, but either way the singleton approach mitigated the problem.

        // For more discussion of different approaches to randomization in concurrent scenarios: https://github.com/App-vNext/Polly/issues/530#issuecomment-439680613

        private static readonly Random s_random = new Random();
        private readonly Random _random;

        /// <summary>
        /// Creates an instance of the <see cref="ConcurrentRandom"/> class.
        /// </summary>
        /// <param name="seed">An optional <see cref="Random"/> seed to use.
        /// If not specified, will use a shared instance with a random seed, per Microsoft recommendation for maximum randomness.</param>
        public ConcurrentRandom(int? seed = null)
        {
            _random = seed == null
                ? s_random // Do not use 'new Random()' here; in concurrent scenarios they could have the same seed
                : new Random(seed.Value);
        }

        /// <summary>
        /// Returns a random floating-point number that is greater than or equal to 0.0,
        /// and less than 1.0.
        /// This method uses locks in order to avoid issues with concurrent access.
        /// </summary>
        public double NextDouble()
        {
            // It is safe to lock on _random since it's not exposed
            // to outside use so it cannot be contended.
            lock (_random)
            {
                return _random.NextDouble();
            }
        }

        /// <summary>
        /// Returns a random floating-point number that is greater than or equal to <paramref name="a"/>,
        /// and less than <paramref name="b"/>.
        /// </summary>
        /// <param name="a">The minimum value.</param>
        /// <param name="b">The maximum value.</param>
        public double Uniform(double a, double b)
        {
            Debug.Assert(a <= b);

            if (a == b) return a;

            return a + (b - a) * NextDouble();
        }
    }
}