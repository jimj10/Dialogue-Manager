/*
 * This code comes from Rachel Lim's blog (thank-you)
 * https://rachel53461.wordpress.com/2011/10/09/simplifying-prisms-eventaggregator/
 * 
 */

using Prism.Events;
using System;

namespace DialogueManager
{
    public static class EventSystem
    {
        private static IEventAggregator _current;
        public static IEventAggregator Current
        {
            get
            {
                return _current ?? (_current = new EventAggregator());
            }
        }

        private static PubSubEvent<TEvent> GetEvent<TEvent>()
        {
            return Current.GetEvent<PubSubEvent<TEvent>>();
        }

        public static void Publish<TEvent>()
        {
            Publish<TEvent>(default(TEvent));
        }

        public static void Publish<TEvent>(TEvent @event)
        {
            GetEvent<TEvent>().Publish(@event);
        }

        public static SubscriptionToken Subscribe<TEvent>(Action action, ThreadOption threadOption = ThreadOption.PublisherThread, bool keepSubscriberReferenceAlive = false)
        {
            return Subscribe<TEvent>(e => action(), threadOption, keepSubscriberReferenceAlive);
        }

        public static SubscriptionToken Subscribe<TEvent>(Action<TEvent> action, ThreadOption threadOption = ThreadOption.PublisherThread, bool keepSubscriberReferenceAlive = false, Predicate<TEvent> filter = null)
        {
            return GetEvent<TEvent>().Subscribe(action, threadOption, keepSubscriberReferenceAlive, filter);
        }

        public static void Unsubscribe<TEvent>(SubscriptionToken token)
        {
            GetEvent<TEvent>().Unsubscribe(token);
        }
        public static void Unsubscribe<TEvent>(Action<TEvent> subscriber)
        {
            GetEvent<TEvent>().Unsubscribe(subscriber);
        }
    }

    // Subscription classes

    public class RulesetsLoaded { }

    public class SessionsLoaded { }

    public class AudioclipsLoaded { }

    public class OnlineVoicesLoaded { }

    public class AudioUpdated
    {
        public float RecordingLevel { get; set; }
    }

    public class AudioMuted { }

    public class SessionChanged
    {
        public string SessionName { get; set; }
    }

    public class SessionsInventoryChanged { }

    public class SessionInventoryChanged
    {
        public string SessionName { get; set; }
    }

    public class AudioClipsInventoryChanged { }

    public class EnableCastDisplayChanged
    {
        public string SessionName { get; set; }
        public bool CastEnable { get; set; }
    }

    class TabChanged
    {
        public string TabName { get; set; }
        public string UserCtrlType { get; set; }
    }

}