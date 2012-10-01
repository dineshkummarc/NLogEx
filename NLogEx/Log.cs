//===========================================================================
// MODULE:  Log.cs
// PURPOSE: logging framework
// 
// Copyright © 2012
// Brent M. Spell. All rights reserved.
//
// This library is free software; you can redistribute it and/or modify it 
// under the terms of the GNU Lesser General Public License as published 
// by the Free Software Foundation; either version 3 of the License, or 
// (at your option) any later version. This library is distributed in the 
// hope that it will be useful, but WITHOUT ANY WARRANTY; without even the 
// implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
// See the GNU Lesser General Public License for more details. You should 
// have received a copy of the GNU Lesser General Public License along with 
// this library; if not, write to 
//    Free Software Foundation, Inc. 
//    51 Franklin Street, Fifth Floor 
//    Boston, MA 02110-1301 USA
//===========================================================================
// System References
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
// Project References

namespace NLogEx
{
   /// <summary>
   /// The log class
   /// </summary>
   /// <remarks>
   /// This class provides support for statically configuring log 
   /// instances and dispatching log records to registered ILogger
   /// implementations. Logging clients instantiate a Log based on 
   /// their class type or object instance. Each logged event 
   /// includes a set of properties sampled from the event 
   /// instance, the log object, or globally-registered contexts.
   /// Log instances may be registered manually or preferably via
   /// App.Config. The log enforces mutual exclusion for both 
   /// configuration and runtime operations, and imposes minimal 
   /// overhead when no log instances have been established. The
   /// log will attempt to flush any asynchronously-published or
   /// buffered log records during process termination, but clients
   /// may ensure this by calling Flush() during shutdown.
   /// </remarks>
   public sealed class Log
   {
      #region Log Configuration
      // the doubly-checked initialization flag
      private static Boolean initialized = false;
      // the log instance/configuration read/write lock
      private static ReaderWriterLockSlim configLock =
         new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
      // the list of registered log instances
      private static List<Instance> instances =
         new List<Instance>();
      // the list of global property contexts
      private static Dictionary<String, Func<Object>> globalContexts =
         new Dictionary<String, Func<Object>>(StringComparer.OrdinalIgnoreCase);
      // the list of active global property contexts,
      // those referenced by at least one log instance
      private static Dictionary<String, Func<Object>> activeContexts =
         new Dictionary<String, Func<Object>>(StringComparer.OrdinalIgnoreCase);
      // the asynchronous log event queue
      private static ConcurrentQueue<EventContext> eventQueue =
         new ConcurrentQueue<EventContext>();
      #endregion

      #region Log Instance Members
      private Type sourceType;
      private Object sourceProps;
      #endregion

      #region Construction/Disposal
      /// <summary>
      /// Initializes a new log
      /// </summary>
      /// <param name="logType">
      /// The runtime type of the logging class
      /// </param>
      /// <param name="logProps">
      /// The logging object's properties, which
      /// can be sampled by log instances
      /// </param>
      public Log (Type logType, Object logProps = null)
      {
         if (logType == null)
            throw new ArgumentNullException("logType");
         this.sourceType = logType;
         this.sourceProps = logProps;
      }
      /// <summary>
      /// Initializes a new log
      /// </summary>
      /// <param name="logProps">
      /// The log object's runtime type and properties
      /// </param>
      public Log (Object logProps) : this(logProps.GetType(), logProps)
      {
         if (logProps == null)
            throw new ArgumentNullException("logProps");
      }
      #endregion

      #region Log Configuration
      /// <summary>
      /// Performs exactly once static log initialization
      /// </summary>
      public static void Initialize ()
      {
         Boolean initializer = false;
         // perform one-time double-checked protected initialization
         if (!initialized)
            using (ConfigWriteLock())
               if (!initialized)
               {
                  // register each configuration-based log instance
                  foreach (Instance instance in Config.Instances)
                     ConfigureInstance(instance);
                  // register the Type type converter
                  TypeDescriptor.AddAttributes(
                     typeof(Type),
                     new TypeConverterAttribute(
                        typeof(System.Configuration.TypeNameConverter)
                     )
                  );
                  initialized = initializer = true;
               }
         // if this call initialized, then perform unprotected initialization
         if (initializer)
         {
            AppDomain.CurrentDomain.DomainUnload += (o, a) => Flush();
            AppDomain.CurrentDomain.ProcessExit += (o, a) => Flush();
         }
      }
      /// <summary>
      /// Establishes a new global property context
      /// </summary>
      /// <param name="context">
      /// The namespace for the context
      /// </param>
      /// <param name="properties">
      /// The list of properties to register
      /// </param>
      public static void RegisterContext (String context, IDictionary<String, Func<Object>> properties)
      {
         Initialize();
         using (ConfigWriteLock())
         {
            // qualify the list of property names
            // fail if any properties are already registered
            List<KeyValuePair<String, Func<Object>>> list = properties
               .Select(p => new KeyValuePair<String, Func<Object>>(String.Format("{0}.{1}", context, p.Key), p.Value))
               .ToList();
            if (list.Any(l => globalContexts.Keys.Contains(l.Key, StringComparer.OrdinalIgnoreCase)))
               throw new ArgumentException("Duplicate property name found");
            // add the new properties, and refresh the
            // active global property list
            foreach (KeyValuePair<String, Func<Object>> prop in list)
               globalContexts.Add(prop.Key, prop.Value);
            ConfigureContexts();
         }
      }
      /// <summary>
      /// Manually registers a new log instance
      /// </summary>
      /// <param name="instance">
      /// The log instance to register
      /// </param>
      /// <returns>
      /// A disposable object that can be used
      /// to unregister the instance
      /// </returns>
      public static IDisposable RegisterInstance (Instance instance)
      {
         Initialize();
         using (ConfigWriteLock())
         {
            // add the new instance, and refresh the
            // active global property list
            ConfigureInstance(instance);
            ConfigureContexts();
            return new Disposable(() => UnregisterInstance(instance));
         }
      }
      /// <summary>
      /// Removes a log instance
      /// </summary>
      /// <param name="instance">
      /// The log instance to remove
      /// </param>
      public static void UnregisterInstance (Instance instance)
      {
         if (initialized)
         {
            Boolean removed = false;
            // remove the instance from the mapping,
            // and update the active global context
            using (ConfigWriteLock())
               if (removed = instances.Remove(instance))
                  ConfigureContexts();
            // flush the instance's buffer
            if (removed && instance.Logger is Loggers.Buffer)
               ((Loggers.Buffer)instance.Logger).Flush();
         }
      }
      /// <summary>
      /// Prepares a log instance and adds it to the instance list
      /// </summary>
      /// <param name="instance">
      /// The log instance to configure
      /// </param>
      private static void ConfigureInstance (Instance instance)
      {
         String[] props = instance.Properties.ToArray();
         // validate the instance configuration, and initialize the logger
         instance.Validate();
         instance.Logger.Initialize(props);
         // if this instance is buffered, attach the log buffer
         if (instance.Buffer > 0)
         {
            Loggers.Buffer buffer = new Loggers.Buffer(instance.Logger, instance.Buffer);
            buffer.Initialize(props);
            instance.Logger = buffer;
         }
         // register the instance
         instances.Add(instance);
      }
      /// <summary>
      /// Updates the active (referenced by at least one instance)
      /// global context list
      /// </summary>
      private static void ConfigureContexts ()
      {
         activeContexts.Clear();
         Func<Object> accessor;
         foreach (Instance instance in instances)
         {
            IEnumerable<String> props = instance.Properties
               .Concat(instance.Filter.IncludeProps.Select(i => i.Name))
               .Concat(instance.Filter.ExcludeProps.Select(i => i.Name));
            foreach (String prop in props)
               if (globalContexts.TryGetValue(prop, out accessor))
                  activeContexts[prop] = accessor;
         }
      }
      #endregion

      #region Log Properties
      public Type SourceType { get { return this.sourceType; } }
      #endregion

      #region Log Operations
      /// <summary>
      /// Samples the high resolution system timer
      /// </summary>
      /// <returns>
      /// The current log timestamp
      /// </returns>
      public static Int64 GetTimestamp ()
      {
         return 
            System.Diagnostics.Stopwatch.GetTimestamp() /
            (System.Diagnostics.Stopwatch.Frequency / 1000000);
      }
      /// <summary>
      /// Dispatches any asynchronous or buffered log
      /// records to the associated logger instances
      /// </summary>
      public static void Flush ()
      {
         if (initialized)
         {
            // flush the event queue
            DispatchEventQueue();
            // flush any log buffers
            foreach (Instance instance in instances)
               if (instance.Buffer > 0)
                  ((Loggers.Buffer)instance.Logger).Flush();
         }
      }
      #endregion

      #region Event Logging
      /// <summary>
      /// Logs a generic event
      /// </summary>
      /// <param name="type">
      /// The event type
      /// </param>
      /// <param name="format">
      /// The event message format string
      /// </param>
      /// <param name="objects">
      /// The event message format parameters
      /// </param>
      public void Event (EventType type, String format, params Object[] objects)
      {
         DispatchEvent(type, null, null, format, objects);
      }
      /// <summary>
      /// Logs a generic event
      /// </summary>
      /// <param name="type">
      /// The event type
      /// </param>
      /// <param name="except">
      /// The event exception
      /// </param>
      /// <param name="format">
      /// The event message format string
      /// </param>
      /// <param name="objects">
      /// The event message format parameters
      /// </param>
      public void Event (EventType type, Exception except, String format = null, params Object[] objects)
      {
         DispatchEvent(type, null, except, format, objects);
      }
      /// <summary>
      /// Logs a generic event
      /// </summary>
      /// <param name="type">
      /// The event type
      /// </param>
      /// <param name="props">
      /// The event context properties
      /// </param>
      /// <param name="format">
      /// The event message format string
      /// </param>
      /// <param name="objects">
      /// The event message format parameters
      /// </param>
      public void EventEx (EventType type, Object props, String format = null, params Object[] objects)
      {
         DispatchEvent(type, props, null, format, objects);
      }
      /// <summary>
      /// Logs a generic event
      /// </summary>
      /// <param name="type">
      /// The event type
      /// </param>
      /// <param name="props">
      /// The event context properties
      /// </param>
      /// <param name="except">
      /// The event exception
      /// </param>
      /// <param name="format">
      /// The event message format string
      /// </param>
      /// <param name="objects">
      /// The event message format parameters
      /// </param>
      public void EventEx (EventType type, Object props, Exception except, String format = null, params Object[] objects)
      {
         DispatchEvent(type, props, except, format, objects);
      }
      #endregion

      #region Trace Logging
      /// <summary>
      /// Logs a trace event
      /// </summary>
      /// <param name="format">
      /// The event message format string
      /// </param>
      /// <param name="objects">
      /// The event message format parameters
      /// </param>
      public void Trace (String format, params Object[] objects)
      {
         DispatchEvent(EventType.Trace, null, null, format, objects);
      }
      /// <summary>
      /// Logs a trace event
      /// </summary>
      /// <param name="props">
      /// The trace event context properties
      /// </param>
      /// <param name="format">
      /// The event message format string
      /// </param>
      /// <param name="objects">
      /// The event message format parameters
      /// </param>
      public void TraceEx (Object props, String format = null, params Object[] objects)
      {
         DispatchEvent(EventType.Trace, props, null, format, objects);
      }
      #endregion

      #region Informational Logging
      /// <summary>
      /// Logs an informational event
      /// </summary>
      /// <param name="format">
      /// The event message format string
      /// </param>
      /// <param name="objects">
      /// The event message format parameters
      /// </param>
      public void Info (String format, params Object[] objects)
      {
         DispatchEvent(EventType.Info, null, null, format, objects);
      }
      /// <summary>
      /// Logs an informational event
      /// </summary>
      /// <param name="props">
      /// The informational event context properties
      /// </param>
      /// <param name="format">
      /// The event message format string
      /// </param>
      /// <param name="objects">
      /// The event message format parameters
      /// </param>
      public void InfoEx (Object props, String format = null, params Object[] objects)
      {
         DispatchEvent(EventType.Info, props, null, format, objects);
      }
      #endregion

      #region Warning Logging
      /// <summary>
      /// Logs a warning event
      /// </summary>
      /// <param name="format">
      /// The event message format string
      /// </param>
      /// <param name="objects">
      /// The event message format parameters
      /// </param>
      public void Warn (String format, params Object[] objects)
      {
         DispatchEvent(EventType.Warning, null, null, format, objects);
      }
      /// <summary>
      /// Logs a warning event
      /// </summary>
      /// <param name="except">
      /// The warning exception
      /// </param>
      /// <param name="format">
      /// The event message format string
      /// </param>
      /// <param name="objects">
      /// The event message format parameters
      /// </param>
      public void Warn (Exception except, String format = null, params Object[] objects)
      {
         DispatchEvent(EventType.Warning, null, except, format, objects);
      }
      /// <summary>
      /// Logs a warning event
      /// </summary>
      /// <param name="props">
      /// The warning event context properties
      /// </param>
      /// <param name="format">
      /// The event message format string
      /// </param>
      /// <param name="objects">
      /// The event message format parameters
      /// </param>
      public void WarnEx (Object props, String format = null, params Object[] objects)
      {
         DispatchEvent(EventType.Warning, props, null, format, objects);
      }
      /// <summary>
      /// Logs a warning event
      /// </summary>
      /// <param name="props">
      /// The warning event context properties
      /// </param>
      /// <param name="except">
      /// The warning exception
      /// </param>
      /// <param name="format">
      /// The event message format string
      /// </param>
      /// <param name="objects">
      /// The event message format parameters
      /// </param>
      public void WarnEx (Object props, Exception except, String format = null, params Object[] objects)
      {
         DispatchEvent(EventType.Warning, props, except, format, objects);
      }
      #endregion

      #region Error Logging
      /// <summary>
      /// Logs an error event
      /// </summary>
      /// <param name="format">
      /// The event message format string
      /// </param>
      /// <param name="objects">
      /// The event message format parameters
      /// </param>
      public void Error (String format, params Object[] objects)
      {
         DispatchEvent(EventType.Error, null, null, format, objects);
      }
      /// <summary>
      /// Logs an error event
      /// </summary>
      /// <param name="except">
      /// The error exception
      /// </param>
      /// <param name="format">
      /// The event message format string
      /// </param>
      /// <param name="objects">
      /// The event message format parameters
      /// </param>
      public void Error (Exception except, String format = null, params Object[] objects)
      {
         DispatchEvent(EventType.Error, null, except, format, objects);
      }
      /// <summary>
      /// Logs an error event
      /// </summary>
      /// <param name="props">
      /// The error event context properties
      /// </param>
      /// <param name="format">
      /// The event message format string
      /// </param>
      /// <param name="objects">
      /// The event message format parameters
      /// </param>
      public void ErrorEx (Object props, String format = null, params Object[] objects)
      {
         DispatchEvent(EventType.Error, props, null, format, objects);
      }
      /// <summary>
      /// Logs an error event
      /// </summary>
      /// <param name="props">
      /// The error event context properties
      /// </param>
      /// <param name="except">
      /// The error exception
      /// </param>
      /// <param name="format">
      /// The event message format string
      /// </param>
      /// <param name="objects">
      /// The event message format parameters
      /// </param>
      public void ErrorEx (Object props, Exception except, String format = null, params Object[] objects)
      {
         DispatchEvent(EventType.Error, props, except, format, objects);
      }
      #endregion

      #region Log Implementation
      /// <summary>
      /// Writes an event to registered log instances
      /// </summary>
      /// <param name="type">
      /// The type of event
      /// </param>
      /// <param name="props">
      /// The event context properties
      /// </param>
      /// <param name="except">
      /// The event exception
      /// </param>
      /// <param name="format">
      /// The event message format string
      /// </param>
      /// <param name="objects">
      /// The event message format parameters
      /// </param>
      private void DispatchEvent (
         EventType type,
         Object props,
         Exception except,
         String format,
         Object[] objects)
      {
         // sample timestamps at method start, so that
         // they are as close to the event time as possible
         Int64 timestamp = GetTimestamp();
         DateTime time = DateTime.UtcNow;
         // ensure initialization/configuration is complete
         Initialize();
         if (instances.Count > 0)
         {
            Boolean hasAsync = false;
            // create the event context, and attach
            // all local event properties
            EventContext ctx = new EventContext()
            {
               Timestamp = timestamp,
               Time = time,
               Type = type,
               Source = this.sourceType,
               Exception = except,
               MessageFormat = format,
               MessageParams = objects
            };
            ctx.SetSourceProps(this.sourceType, this.sourceProps);
            ctx.SetEventProps(props);
            using (ConfigReadLock())
            {
               // sample global state on the current execution 
               // context/thread, so that the remainder of event 
               // processing can occur concurrently
               if (activeContexts.Any())
                  ctx.SetGlobalProps(activeContexts.ToDictionary(
                     m => m.Key,
                     m => { try { return m.Value(); } catch { return null; } }
                  ));
               // dispatch to any synchronous log instances
               foreach (Instance instance in instances)
                  if (instance.Synchronous)
                     DispatchEventInstance(instance, ctx);
                  else
                     hasAsync = true;
            }
            // dispatch to asynchronous log instances
            // . only start a new dispatcher work item if we sample a count 
            //   of 0 in the event queue
            // . there is a potential race here in which multiple threads could 
            //   sample a counter of 0, which would simply result in multiple 
            //   dispatcher work items 
            // . in addition, it is possible for a dispatcher to completely drain 
            //   the queue and exit after the logging thread samples a nonzero count, 
            //   although this would require the low-priority dispatcher to dispatch 
            //   the last event faster than the logger could enqueue the next event, 
            //   the only result being an event remaining in the queue longer
            // . these compromises are preferable to more heavy-handed synchronization 
            //   mechanisms that would cause more contention on the logger and 
            //   reduce application throughput
            if (hasAsync)
            {
               Boolean startDispatch = (eventQueue.Count == 0);
               eventQueue.Enqueue(ctx);
               if (startDispatch)
                  ThreadPool.UnsafeQueueUserWorkItem(AsyncEventDispatcher, null);
            }
         }
      }
      /// <summary>
      /// The asynchronous log event dispatcher
      /// </summary>
      /// <param name="o">
      /// Work item parameter
      /// </param>
      private static void AsyncEventDispatcher (Object o)
      {
         // set the async dispatcher priority to lowest
         Thread thread = Thread.CurrentThread;
         ThreadPriority priority = thread.Priority;
         try
         {
            try { thread.Priority = ThreadPriority.Lowest; }
            catch { }
            // flush the event queue
            DispatchEventQueue();
         }
         finally
         {
            // restore the thread priority for the pooled thread
            if (priority != ThreadPriority.Lowest)
               thread.Priority = priority;
         }
      }
      /// <summary>
      /// Dispatches all events in the asynchronous
      /// log event queue
      /// </summary>
      private static void DispatchEventQueue ()
      {
         // to preserve causal ordering, ensure that at most
         // one dispatcher is executing at a time
         // drain the event queue, dispatching async instances
         EventContext ctx;
         lock (eventQueue)
            while (eventQueue.TryDequeue(out ctx))
               using (ConfigReadLock())
                  foreach (Instance instance in instances)
                     if (!instance.Synchronous)
                        DispatchEventInstance(instance, ctx);
      }
      /// <summary>
      /// Dispatches a log event to a single log instance
      /// </summary>
      /// <param name="instance">
      /// The log instance to receive the event
      /// </param>
      /// <param name="ctx">
      /// The event to dispatch
      /// </param>
      private static void DispatchEventInstance (Instance instance, EventContext ctx)
      {
         if (instance.Filter.Evaluate(ctx))
            try { instance.Logger.Log(new[] { new Event(ctx, instance.Properties) }); }
            catch { }
      }
      #endregion

      #region Log Configuration Locking
      /// <summary>
      /// Acquires the log configuration read lock
      /// </summary>
      /// <returns>
      /// A disposable object used to release the lock
      /// </returns>
      private static Disposable ConfigReadLock ()
      {
         configLock.EnterReadLock();
         return new Disposable(configLock.ExitReadLock);
      }
      /// <summary>
      /// Acquires the log configuration write lock
      /// </summary>
      /// <returns>
      /// A disposable object used to release the lock
      /// </returns>
      private static Disposable ConfigWriteLock ()
      {
         configLock.EnterWriteLock();
         return new Disposable(configLock.ExitWriteLock);
      }
      #endregion
   }
}
