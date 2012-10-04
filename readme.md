![NLogEx](https://raw.github.com/bspell1/NLogEx/master/NLogEx.png) NLogEx
=========================================================================
A lightweight logging framework for .NET 4.0+

###`PM> Install-Package NLogEx`###

##Features:##
* A simple application logging interface that is decoupled from log
  implementation, allowing logging policy to change through configuration
  without modifying application source code
* A generic property-based log event model, providing flexibility in the
  type/amount of data to send to logger implementations
* Customizable ambient property contexts that support the ability to include
  event properties that are independent of the client application
* A powerful log filtering model used to selectively include and/or exclude
  log events based on property values and customizable comparison operators
* Asynchronous log publication by default and minimal overhead when no log 
  instances are configured, to minimize the performance impact of logging on 
  applications
* A suite of logger implementations for text files, database tables, the 
  debug console, and the Windows event log
* Integration with Windows Communication Foundation (WCF), for automatically 
  logging service calls on the client or server
* Integration with ASP.NET MVC, for automatically logging controller actions
* Integration with System.Diagnostics, for routing traces in existing
  applications to NLogEx

###Logging Interface###
The client interface is simple and should be familiar to users of log4net.
To add logging support to a class, instantiate a new **Log** instance and
pass in either the current object or a **System.Type** instance representing 
the log source.

```c#
   // shared log instance (no source context)
   class MyClass
   {
      private static Log Log = new Log(typeof(MyClass));
      ...
   }

   // per-object log instance, with source context
   class MyClass
   {
      private Log;
      public MyClass ()
      {
         this.Log = new Log(this);
      }
      ...
   }
```

In order to raise an event, simply call one of the event-type overrides on
the log instance, specifying the formatted event message, exception, and/or 
custom event properties.

```c#
   // formatted informational event
   Log.Info("Frobbed the {0}th frobber.", frobIdx);

   // event with exception
   try { stream.Write(buffer, 0, buffer.Length); }
   catch (IOException e) { Log.Error(e); }

   // event with context properties
   Log.WarnEx(new { Level = 11 }, "You've been warned.");

   // event with dynamic type/severity
   EventType mappedType = MapType(otherEventType);
   Log.Event(mappedType, "Some kind of event.");
```

###Event Model###
The model for each **NLogEx** event consists of a set of string/object pairs
representing the named properties associated with the event. These properties
are sampled from the event context when an event is logged. Event properties 
may be taken from the following contexts:

1. The intrinsic context, including well-known event properties (type, 
   source, time, etc.)
2. The event context, passed into one of the **EventEx** (**InfoEx**, etc.) 
   methods
3. The source context, passed into the **Log** constructor
4. The ambient context, consisting of properties registered using 
   **Log.RegisterContext**

Property names are specified with the syntax `<scope>.<name>`. All property
names are qualified with a scope name. The following samples demonstrate
accessing event properties through valid property names.

```c#
   evt["Event.Source"]              // intrinsic event source property
   evt["Context.Level"]             // custom event context property
   evt["MyClass.MyProperty"]        // event source context property
   evt["Environment.MachineName"]   // ambient context property
```

An individual event property is sampled only if there is at least one log 
instance subscribing it. This means that potentially expensive property 
accessors, such as Thread.StackTrace can be supported without imposing a 
performance penalty when not needed.

###Ambient Properties###
**NLogEx** supports binding properties to events that are not directly 
associated with the event (such as thread IDs, environment variables, etc.). 
These properties are registered as ambient contexts using 
**Log.RegisterContext** and are an easy way to separate concerns when logging 
events. Contexts are registered with a scope name and a map of property names 
to accessor delegates.

```c#
   NLogEx.Log.RegisterContext(
      "MyContext",
      new Dictionary<String, Func<Object>>()
      {
         { "TickCount", () => Environment.TickCount },
         { "MyProperty", () => MyProperty },
         { "MyReturnValue", MyMethod }
      }
   );
```

**NLogEx** ships with the following built-in ambient contexts.

* **NLogEx.Context.Environment**, for environment variables
* **NLogEx.Context.Process**, for process-wide properties
* **NLogEx.Context.Thread**, for current thread properties
* **NLogEx.Context.Transaction**, for current transaction properties
* **NLogEx.Wcf.LogContext**, for WCF channel properties
* **NLogEx.Mvc.LogContext**, for HTTP/MVC properties

###Event Filtering###
Events may be filtered based on any event property, even those not sampled
by the log instance. Filters are specified using inclusion and exclusion for
whitelisting/blacklisting semantics. **NLogEx** supports a flexible and 
extensible property comparison mechanism that goes beyond typed equality.

```xml
   <!-- Include only warning or error events -->
   <filter>
      <include name="Event.Type" value="Error"/>
      <include name="Event.Type" value="Warning"/>
   </filter>

   <!-- Exclude all trace and informational events -->
   <filter>
      <exclude name="Event.Type" value="Trace"/>
      <exclude name="Event.Type" value="Info"/>
   </filter>

   <!-- Include any events with messages including the error/warning string,
        but ignore all events when processor usage is > 50%
     -->
   <filter>
      <include 
         name="Event.Message" 
         comparer="NLogEx.Comparers.RegEx,NLogEx" 
         value=".*error|warning.*"/>
      <exclude
         name="Wmi.Win32_Processor.LoadPercentage" 
         comparer="NLogEx.Comparers.IsGreaterThan,NLogEx" 
         value="50"/>
   </filter>

   <!-- Include only events logged by a class derived from MyBase -->
   <filter>
      <include 
         name="Event.Source" 
         comparer="NLogEx.Comparers.IsSubclassOf,NLogEx" 
         value="MyNamespace.MyBase,MyAssembly"/>
   </filter>
```

###Asynchronous Logging and Buffering###
By default, all events are logged asynchronously, via a single CLR thread 
pool work item. The work item executes only when there is at least one event
queued up to be logged. However, it is possible to force a log instance to
log events synchronously as they are published, via the **synchronous**
configuration attribute.

When events are logged asynchronously, the log performs the minimum amount
of work on the publishing thread required to queue up the event for dispatch.
This includes sampling any referencced or filtered ambient properties. All
filtering and logging operations are performed asynchronously, imposing
minimal overhead on the client application. In addition, when there are no
log instances registered, the log performs almost no processing during
event publication.

In addition, **NLogEx** supports buffering events for batch logging, using
the **buffer** configuration attribute. This allows events to be delivered
effiently to some logs, such as files/databases without having to maintain an 
open file handle/database connection when the log is idle. By default, log 
instances are not buffered.

###Built-In Logger Suite###
**NLogEx** ships with a set of loggers for common logging destinations, 
including the following.

* **NLogEx.Loggers.Null**, which discards all log records
* **NLogEx.Loggers.Queue**, for logging events to an in-memory event queue
* **NLogEx.Loggers.Debug**, for writing to the Visual Studio debug console or DebugView
* **NLogEx.Loggers.Console**, to write events to System.Console.Out (in color, of course)
* **NLogEx.Loggers.TextFile**, for logging events to a rotating text file
* **NLogEx.Loggers.Windows**, to log events to a Windows event log
* **NLogEx.Loggers.DBTable**, for inserting events into a table via an ADO.NET provider
* **NLogEx.Loggers.Smtp**, to dispatch (batched) events to an email address

Custom loggers need only implement the **ILogger** interface to receive
events from **NLogEx** for application-specific processing.

###WCF Integration###
For WCF clients/services, **NLogEx** provides the ability to log messages
transferred over the contract interface via configuration, without modifying
client/service code. The **LogBehavior** WCF endpoint behavior logs messages
on either the client or service side of an interface, including calls over
duplex/callback contracts.

```xml
   <system.serviceModel>
      <extensions>
         <behaviorExtensions>
            <add name="log" type="NLogEx.Wcf.LogBehavior+Element,NLogEx.Wcf"/>
         </behaviorExtensions>
      </extensions>
      <behaviors>
         <endpointBehaviors>
            <!-- log all endpoint messages -->
            <behavior>
               <log/>
            </behavior>
         </endpointBehaviors>
      </behaviors>
   </system.serviceModel>
```

###ASP.NET MVC Integration###
For ASP.NET MVC applications, **NLogEx** includes a configurable logging
action filter. This filter supports logging all actions or filtering the
list of actions by controller or action name.

```xml
   <configuration>
      <configSections>
         <section name="NLogEx.Mvc" type="NLogEx.Mvc.Config,NLogEx.Mvc"/>
      </configSections>
      <NLogEx.Mvc>
         <filters>
            <!-- filter for specific controller -->
            <add type="NLogEx.Mvc.LogAttribute,NLogEx.Mvc"
               controller="MyController"/>
            <!-- filter for specific controller/action -->
            <add type="NLogEx.Mvc.LogAttribute,NLogEx.Mvc"
               controller="MyOtherController"
               action="Index"/>
         </filters>
      </NLogEx.Mvc>
   </configuration>
```

###System.Diagnostics Integration###
For existing components/applications built on the **System.Diagnostics** 
tracing mechanism, **NLogEx** provides the ability to log their traces to 
a configured log. Simply register **NLogEx.TraceListener** via trace
configuration.

```xml
   <system.diagnostics>
      <trace>
         <listeners>
            <add name="NLogEx" type="NLogEx.TraceListener,NLogEx"/>
         </listeners>
      </trace>
   </system.diagnostics>
```
