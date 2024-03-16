using System;
using System.Diagnostics;

namespace Axis.Luna.Common
{
    public class EventTimer
    {
        public static void Measure(Action @event, out TimeSpan timeSpan)
        {
            var stopwatch = new Stopwatch();
            try
            {
                stopwatch.Start();
                @event.Invoke();
            }
            catch(Exception e)
            {
                // stop here so time spent creating and throwing exception isn't included
                stopwatch.Stop();
                throw new TimerException(stopwatch.Elapsed, e);
            }
            finally
            {
                stopwatch.Stop();
                timeSpan = stopwatch.Elapsed;
            }
        }

        public static TOut Measure<TOut>(Func<TOut> @event, out TimeSpan timeSpan)
        {
            var stopwatch = new Stopwatch();
            try
            {
                stopwatch.Start();
                return @event.Invoke();
            }
            catch (Exception e)
            {
                stopwatch.Stop();
                throw new TimerException(stopwatch.Elapsed, e);
            }
            finally
            {
                stopwatch.Stop();
                timeSpan = stopwatch.Elapsed;
            }
        }


        public class TimerException: Exception
        {
            public TimeSpan ElapsedTime { get; }

            public TimerException(TimeSpan timeSpan, Exception sourceException)
                :base("An exception occured while timing the event", sourceException)
            {
                ElapsedTime = timeSpan;
            }
        }
    }
}
