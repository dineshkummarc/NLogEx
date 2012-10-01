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
* Customizable ambient property contexts that supports the ability to include
  log properties independent of and unknown to the client application
* A powerful log filtering model used to selectively include and/or exclude
  log events based on property values and customizable comparison operators
* Asynchronous log publication by default and minimal overhead when no log 
  instances are configured, to minimze the impact on logging applications
* A suite of logger implementations, such as text file, database, debug
  console, and Windows event log
* Integration with WCF, for automatically logging service calls on the client 
  or server
* Integration with ASP.NET MVC, for automatically logging controller actions
* Integration with System.Diagnostics, for routing existing traces to NLogEx

##Logging Interface##

##Event Model##

##Ambient Properties##

##Event Filtering##

##Asynchronous Logging##

##Built-In Logger Suite##

##WCF Integration##

##ASP.NET MVC Integration##

##System.Diagnostics Integration##
