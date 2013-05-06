using System;
using System.Collections.Generic;
using Errordite.Core.Configuration;
using System.Linq;

namespace Errordite.Core.Receive
{
    public class RateLimiterRule
    {
        public int Count { get; set; }
        public int Minutes { get; set; }
        public string Name  { get; set; }
    }

    public class MinuteCounter
    {
        public DateTime Start { get; set; }
        public int Count { get; set; }

        public MinuteCounter(DateTime start)
        {
            Start = start;
        }
    }

    public class TimeBasedRateLimiter
    {
        private readonly List<MinuteCounter> _counterQueue = new List<MinuteCounter>();
        private readonly int _minutesToRetainCount;
        private static readonly object _syncLock = new object();
        private readonly IEnumerable<RateLimiterRule> _rules;

        /// <summary>
        /// When the class is constructed assume we have received the first event, so start the current minute timer
        /// </summary>
        public TimeBasedRateLimiter(IEnumerable<RateLimiterRule> rules)
        {
            _minutesToRetainCount = rules.Max(r => r.Minutes);
            _rules = rules;
        }

        public RateLimiterRule Occur(IDateTime dateTime)
        {
            var current = _counterQueue.LastOrDefault();

            if (current == null || current.Start < dateTime.GetUtcNow().AddMinutes(-1))
            {
                current = new MinuteCounter(dateTime.GetUtcNow());

                lock (_syncLock)
                {
                    _counterQueue.Add(current);

                    while(_counterQueue.Count > _minutesToRetainCount)
                    {
                        _counterQueue.RemoveAt(0);
                    }
                }
            }

            foreach(var rule in _rules)
            {
                var count = _counterQueue
                    .Where(c => c.Start >= dateTime.GetUtcNow().AddMinutes(-rule.Minutes))
                    .Sum(c => c.Count);

                if (count >= rule.Count)
                    return rule;
            }

            current.Count++;
            return null;
        }
    }

    public interface IExceptionRateLimiter
    {
        RateLimiterRule Accept(string applicationId);
    }

    public class ExceptionRateLimiter : IExceptionRateLimiter
    {
        private static readonly object _syncLock = new object();
        private readonly IDateTime _dateTime;
        private readonly ErrorditeConfiguration _configuration;
        private static readonly Dictionary<string, TimeBasedRateLimiter> _perApplicationRateLimiters = new Dictionary<string, TimeBasedRateLimiter>();

        public ExceptionRateLimiter(ErrorditeConfiguration configuration, IDateTime dateTime)
        {
            _configuration = configuration;
            _dateTime = dateTime;
        }

        public RateLimiterRule Accept(string applicationId)
        {
            if (!_perApplicationRateLimiters.ContainsKey(applicationId))
            {
                lock (_syncLock)
                {
                    if (!_perApplicationRateLimiters.ContainsKey(applicationId))
                    {
                        _perApplicationRateLimiters.Add(applicationId, new TimeBasedRateLimiter(_configuration.RateLimiterRules));
                    }
                }
            }

            return _perApplicationRateLimiters[applicationId].Occur(_dateTime);
        }
    }

    public interface IDateTime
    {
        DateTime GetUtcNow();
        void SetUtcNow(DateTime dateTime);
    }

    public class UtcDateTime : IDateTime
    {
        public DateTime GetUtcNow()
        {
            return DateTime.UtcNow;
        }

        public void SetUtcNow(DateTime dateTime)
        {}
    }

    public class SettableDateTime : IDateTime
    {
        private DateTime _dateTime = DateTime.UtcNow;

        public DateTime GetUtcNow()
        {
            return _dateTime;
        }

        public void SetUtcNow(DateTime dateTime)
        {
            _dateTime = dateTime;
        }
    }
}
