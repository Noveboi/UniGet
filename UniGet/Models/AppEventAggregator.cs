using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniGet.Models
{
    public static class AppEventAggregator
    {
        private static readonly Dictionary<Type, List<object>> _subscribersByType = new();

        /// <summary>
        /// Subscribe to an event <typeparamref name="T"/> and trigger handler <see cref="Action{T}"/> when event is published.
        /// </summary>
        /// <typeparam name="T">The event to subscribe to</typeparam>
        /// <param name="action">The handler of the event when it's fired</param>
        public static void Subscribe<T>(Action<T> action)
        {
            // If T is not in the Dictionary, assign 
            if (!_subscribersByType.TryGetValue(typeof(T), out List<object>? subscribers))
            {
                subscribers = new List<object>();

                // If key does not exist, it is automatically handled here
                _subscribersByType[typeof(T)] = subscribers;
            }

            subscribers.Add(action);
        }

        /// <summary>
        /// Publish an event <typeparamref name="T"/> and notify the subscribers of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of event to publish</typeparam>
        /// <param name="obj">The event to be published</param>
        public static void Publish<T>(T obj)
        {
            if (_subscribersByType.TryGetValue(typeof(T), out List<object>? subscribers))
            {
                // For each subscriber, call the designated Action delegate.
                foreach (Action<T>? subscriberHandler in subscribers.OfType<Action<T>>())
                {
                    subscriberHandler(obj);
                }
            }
        }
    }
}
