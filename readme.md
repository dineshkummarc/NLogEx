![NLogEx](https://raw.github.com/bspell1/NLogEx/master/NLogEx.png) NLogEx
=========================================================================
A lightweight logging framework for .NET 4.0+

###`PM> Install-Package NLogEx`###

##Features:##
* A simple application logging interface that is decoupled from logger 
  implementations, allowing logging policy to change through configuration
  without modifying application source code
* A generic property-based log event model, providing flexibility in the
  type of data to send to logger implementations
* Customizable ambient property contexts that support the ability to include
  event properties independent of the client application
* A powerful log filtering model used to selectively include and/or exclude
  log events based on property values and customizable comparison operators
* Asynchronous log publication by default and minimal overhead when no log 
  instances are configured, to minimze the impact on logging applications
* A suite of logger implementations, such as text file, database, debug
  console, and Windows event log
* Integration with WCF, for automatically logging service calls on the client 
  or server
* Integration with ASP.NET MVC, for automatically logging controller actions
* Integration with System.Diagnostics, for routing existing traces to 
  **NLogEx**

###Logging Interface###
The client interface is simple and should be familiar to users of log4net.
To raise events, instantiate a new **Log** instance, passing in either the 
current object or a `System.Type` instance representing the log source.

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
   // simple informational event
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
are sampled from the event context when an event is raised (and there is at
least one configured log instance). Event properties may be taken from the 
following contexts:

1. The intrinsic context, including well-known event properties (i.e. type, 
   source, time, etc.)
2. The event context, passed into one of the EventEx (InfoEx, etc.) methods
3. The source context, passed into the **Log** constructor
4. The global context, consisting of contexts registered using 
   **Log.RegisterContext**

Property names are specified with the syntax `<scope>.<name>`. All property
names are qualified with a scope name. The following samples demonstrate
accessing event properties through valid property names.

```c#
   evt["Event.Source"]              // intrinsic event source property
   evt["Context.Level"]             // custom event context property
   evt["MyClass.MyProperty"]        // event source context property
   evt["Environment.MachineName"]   // global context property
```

Event properties are sampled and bound to the event at the point of 
publication, so that context-specific events, such as time, are recorded 
accurately. Note, though, that an individual event property is only sampled 
if there is at least one log instance subscribing it. This means that 
potentially expensive property accessors, such as Thread.StackTrace can be 
supported without imposing a performance penalty when not needed.

###Ambient Properties###

###Event Filtering###

###Asynchronous Logging###

###Built-In Logger Suite###

###WCF Integration###

###ASP.NET MVC Integration###

###System.Diagnostics Integration###

TODO: spellcheck
