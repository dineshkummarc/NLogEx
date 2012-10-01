//===========================================================================
// MODULE:  LogBehavior.cs
// PURPOSE: WCF logging endpoint behavior extension
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
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
// Project References

namespace NLogEx.Wcf
{
   /// <summary>
   /// WCF logging behavior
   /// </summary>
   /// <remarks>
   /// This class is attached to a client or service endpoint and is
   /// used to log the service/callback contract calls on that endpoint, 
   /// using the WCF parameter inspector mechanism.
   /// </remarks>
   public sealed class LogBehavior : IEndpointBehavior
   {
      #region IEndpointBehavior Implementation
      /// <summary>
      /// Registers binding parameters with a contract endpoint
      /// </summary>
      /// <param name="endpoint">
      /// The endpoint
      /// </param>
      /// <param name="bindings">
      /// The list of binding parameters to modify
      /// </param>
      public void AddBindingParameters (
         ServiceEndpoint endpoint,
         BindingParameterCollection bindings)
      {
      }
      /// <summary>
      /// Adds the logger parameter inspector to
      /// the client side of an endpoint
      /// </summary>
      /// <param name="endpoint">
      /// The client endpoint
      /// </param>
      /// <param name="client">
      /// The client runtime to log
      /// </param>
      public void ApplyClientBehavior (
         ServiceEndpoint endpoint, 
         ClientRuntime client)
      {
         // register a logging parameter inspector for all outbound operations
         client.MessageInspectors.Add(
            new LoggingInspector(client.ContractClientType)
         );
         // if the contract supports callbacks, register a logging parameter
         // inspector for all inbound operations
         if (endpoint.Contract.CallbackContractType != null)
            client.CallbackDispatchRuntime.MessageInspectors.Add(
               new LoggingInspector(endpoint.Contract.CallbackContractType)
            );
      }
      /// <summary>
      /// Adds the logger parameter inspector to
      /// the server side of an endpoint
      /// </summary>
      /// <param name="endpoint">
      /// The server endpoint
      /// </param>
      /// <param name="server">
      /// The server runtime to log
      /// </param>
      public void ApplyDispatchBehavior (
         ServiceEndpoint endpoint, 
         EndpointDispatcher server)
      {
         // register a logging parameter inspector for all inbound operations
         server.DispatchRuntime.MessageInspectors.Add(
            new LoggingInspector(server.DispatchRuntime.Type)
         );
         // if the contract supports callbacks, register a logging parameter
         // inspector for all outbound operations
         if (endpoint.Contract.CallbackContractType != null)
            server.DispatchRuntime.CallbackClientRuntime.MessageInspectors.Add(
               new LoggingInspector(endpoint.Contract.CallbackContractType)
            );
      }
      /// <summary>
      /// Validates the endpoint behavior
      /// </summary>
      /// <param name="endpoint">
      /// The client/service endpoint
      /// </param>
      public void Validate (ServiceEndpoint endpoint)
      {
      }
      #endregion

      /// <summary>
      /// The logging message inspector
      /// </summary>
      /// <remarks>
      /// This class attaches a log instance and traces all calls made and
      /// logs all faults that occurred over WCF for the current contract.
      /// </remarks>
      private sealed class LoggingInspector : 
         IClientMessageInspector, 
         IDispatchMessageInspector
      {
         private Log Log;

         /// <summary>
         /// Initializes a new parameter inspector instance
         /// </summary>
         /// <param name="logType">
         /// The log source type
         /// </param>
         public LoggingInspector (Type logType)
         {
            this.Log = new Log(logType);
         }

         #region IClientMessageInspector Implementation
         /// <summary>
         /// Pre-request submission callback
         /// </summary>
         /// <param name="request">
         /// Request message being submitted
         /// </param>
         /// <param name="channel">
         /// Client channel interface
         /// </param>
         /// <returns>
         /// A correlation object for the operation
         /// </returns>
         public Object BeforeSendRequest (
            ref Message request, 
            IClientChannel channel)
         {
            return OnRequestBegin(request);
         }
         /// <summary>
         /// Post-request completion callback
         /// </summary>
         /// <param name="reply">
         /// Reply message received from server (if any)
         /// </param>
         /// <param name="correlation">
         /// Correlation object returned from BeforeSendRequest
         /// </param>
         public void AfterReceiveReply (
            ref Message reply, 
            Object correlation)
         {
            OnRequestEnd((Operation)correlation, ref reply);
         }
         #endregion

         #region IDispatchMessageInspector Implementation
         /// <summary>
         /// Pre-request dispatch callback
         /// </summary>
         /// <param name="request">
         /// Request message being processed
         /// </param>
         /// <param name="channel">
         /// Client channel interface
         /// </param>
         /// <param name="instanceContext">
         /// Service instance context
         /// </param>
         /// <returns>
         /// A correlation object for the operation
         /// </returns>
         public Object AfterReceiveRequest (
            ref Message request, 
            IClientChannel channel, 
            InstanceContext instanceContext)
         {
            return OnRequestBegin(request);
         }
         /// <summary>
         /// Post-request dispatch callback
         /// </summary>
         /// <param name="reply">
         /// Reply message to send to client (if any)
         /// </param>
         /// <param name="correlation">
         /// Correlation object returned from AfterReceiveRequest
         /// </param>
         public void BeforeSendReply (
            ref Message reply, 
            Object correlation)
         {
            OnRequestEnd((Operation)correlation, ref reply);
         }
         #endregion

         #region Operations
         /// <summary>
         /// Pre-request handler
         /// </summary>
         /// <param name="request">
         /// The request message being submitted or dispatched
         /// </param>
         /// <returns>
         /// A correlation object for the specified request
         /// </returns>
         private Operation OnRequestBegin (Message request)
         {
            return new Operation(request);
         }
         /// <summary>
         /// Post-request handler
         /// </summary>
         /// <param name="operation">
         /// Correlation object returned from OnRequestBegin
         /// </param>
         /// <param name="reply">
         /// The optional reply message for the request
         /// </param>
         private void OnRequestEnd (Operation operation, ref Message reply)
         {
            operation.Stop();
            // trace the completion of the WCF operation
            this.Log.TraceEx(
               operation,
               "{0}.{1}",
               this.Log.SourceType.FullName,
               operation.Name
            );
            // if a fault occurred, log the fault error
            if (reply != null && reply.IsFault)
            {
               using (MessageBuffer buffer = reply.CreateBufferedCopy(Int32.MaxValue))
               {
                  using (Message fault = buffer.CreateMessage())
                     this.Log.ErrorEx(
                        operation,
                        new FaultException(
                           MessageFault.CreateFault(fault, Int32.MaxValue)
                        )
                     );
                  reply = buffer.CreateMessage();
               }
            }
         }
         #endregion

         /// <summary>
         /// Operation correlation/log state
         /// </summary>
         private sealed class Operation
         {
            private String id;
            private String version;
            private String action;
            private String from;
            private String to;
            private Int64 started;
            private Int64 stopped;

            /// <summary>
            /// Initializes a new operation instance
            /// </summary>
            /// <param name="request">
            /// The original request message
            /// </param>
            public Operation (Message request)
            {
               this.started = Log.GetTimestamp();
               this.id = Convert.ToString(request.Headers.MessageId);
               this.version = Convert.ToString(request.Headers.MessageVersion);
               this.action = request.Headers.Action;
               this.from = (request.Headers.From != null) ?
                  Convert.ToString(request.Headers.From.Uri) :
                  null;
               this.to = Convert.ToString(request.Headers.To);
            }
            /// <summary>
            /// The request message ID
            /// </summary>
            public String ID
            {
               get { return this.id; }
            }
            /// <summary>
            /// The message SOAP version
            /// </summary>
            public String Version
            {
               get { return this.version; }
            }
            /// <summary>
            /// The request SOAP action
            /// </summary>
            public String Action
            {
               get { return this.action; }
            }
            /// <summary>
            /// The operation name
            /// </summary>
            public String Name
            {
               get { return this.Action.Substring(this.Action.LastIndexOf('/') + 1); }
            }
            /// <summary>
            /// The request from address (if any)
            /// </summary>
            public String From
            {
               get { return this.from; }
            }
            /// <summary>
            /// The request to address
            /// </summary>
            public String To
            {
               get { return this.to; }
            }
            /// <summary>
            /// The duration, in microseconds, of the request
            /// </summary>
            public Int64 Duration
            {
               get { return this.stopped - this.started; }
            }
            /// <summary>
            /// Stops the request timer
            /// </summary>
            public void Stop ()
            {
               this.stopped = Log.GetTimestamp();
            }
         }
      }

      /// <summary>
      /// Logging WCF behavior extension element
      /// </summary>
      /// <remarks>
      /// This class represents the logging WCF behavior extension
      /// element within a service configuration file. It serves as
      /// a means for enabling operation logging through configuration.
      /// </remarks>
      /// <example>
      ///   <configuration>
      ///      <system.serviceModel>
      ///         <extensions>
      ///            <behaviorExtensions>
      ///               <add name="log" type="NLogEx.Wcf.LogBehavior+Element,NLogEx.Wcf"/>
      ///            </behaviorExtensions>
      ///         </extensions>
      ///         <services>
      ///            <service name="MyNamespace.MyService">
      ///               <endpoint contract="MyNamespace.IMyService" 
      ///                  behaviorConfiguration="enableLogging"
      ///                  .../>
      ///            </service>
      ///         </services>
      ///         <behaviors>
      ///            <endpointBehaviors>
      ///               <behavior name="enableLogging">
      ///                  <log/>
      ///               </behavior>
      ///            </endpointBehaviors>
      ///         </behaviors>
      ///      </system.serviceModel
      ///   </configuration>
      /// </example>
      public sealed class Element : BehaviorExtensionElement
      {
         #region BehaviorExtensionElement Overrides
         /// <summary>
         /// The behavior object type
         /// </summary>
         /// <remarks>
         /// This type must implement one of the I*Behavior
         /// interfaces. Since this must already be done by
         /// LogAttribute, use its implementation for
         /// configuration-based logging, as well.
         /// </remarks>
         public override Type BehaviorType
         {
            get { return typeof(LogBehavior); }
         }
         /// <summary>
         /// Creates and initializes a new behavior object
         /// </summary>
         /// <returns>
         /// The new behavior object
         /// </returns>
         protected override Object CreateBehavior ()
         {
            return new LogBehavior();
         }
         #endregion
      }
   }
}
