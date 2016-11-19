using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace BFF.MVVM
{
    /// <summary>
    /// Adopted from answer of Dalstroem: http://stackoverflow.com/questions/23798425/wpf-mvvm-communication-between-view-model
    /// </summary>
    public class Messenger
    {
        private static readonly object CreationLock = new object();
        private static readonly ConcurrentDictionary<(object recipient, object context, Type type), object> Dictionary = new ConcurrentDictionary<(object, object, Type), object>();

        #region Default property

        private static Messenger _instance;

        /// <summary>
        /// Gets the single instance of the Messenger.
        /// </summary>
        public static Messenger Default
        {
            get
            {
                if (_instance == null)
                {
                    lock (CreationLock)
                    {
                        if (_instance == null)
                        {
                            _instance = new Messenger();
                        }
                    }
                }

                return _instance;
            }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the Messenger class.
        /// </summary>
        private Messenger()
        {
        }

        /// <summary>
        /// Registers a recipient for a type of message T. The action parameter will be executed
        /// when a corresponding message is sent.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="recipient"></param>
        /// <param name="action"></param>
        public void Register<T>(object recipient, Action<T> action)
        {
            Register(recipient, action, null);
        }

        /// <summary>
        /// Registers a recipient for a type of message T and a matching context. The action parameter will be executed
        /// when a corresponding message is sent.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="recipient"></param>
        /// <param name="action"></param>
        /// <param name="context"></param>
        public void Register<T>(object recipient, Action<T> action, object context)
        {
            Dictionary.TryAdd((recipient, context, typeof(T)), action);
        }

        /// <summary>
        /// Unregisters a messenger recipient completely. After this method is executed, the recipient will
        /// no longer receive any messages.
        /// </summary>
        /// <param name="recipient"></param>
        public void Unregister<T>(object recipient)
        {
            Unregister<T>(recipient, null);
        }

        /// <summary>
        /// Unregisters a messenger recipient with a matching context completely. After this method is executed, the recipient will
        /// no longer receive any messages.
        /// </summary>
        /// <param name="recipient"></param>
        /// <param name="context"></param>
        public void Unregister<T>(object recipient, object context)
        {
            Dictionary.TryRemove((recipient, context, typeof(T)), out object action);
        }

        /// <summary>
        /// Sends a message to registered recipients. The message will reach all recipients that are
        /// registered for this message type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        public void Send<T>(T message)
        {
            Send(message, null);
        }

        /// <summary>
        /// Sends a message to registered recipients. The message will reach all recipients that are
        /// registered for this message type and matching context.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <param name="context"></param>
        public void Send<T>(T message, object context)
        {
            IEnumerable<KeyValuePair<(object, object, Type), object>> result;

            if (context == null)
            {
                // Get all recipients where the context is null.
                result = from r in Dictionary where r.Key.context == null select r;
            }
            else
            {
                // Get all recipients where the context is matching.
                result = from r in Dictionary where r.Key.context != null && r.Key.context.Equals(context) select r;
            }

            foreach (Action<T> action in result.Select(x => x.Value).OfType<Action<T>>())
            {
                // Send the message to all recipients.
                action(message);
            }
        }
    }
}