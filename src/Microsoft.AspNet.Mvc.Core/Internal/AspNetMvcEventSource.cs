// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.AspNet.Mvc.Core.Internal
{
    /// <summary>
    /// Logger for hardcoded events that must go to ETW regardless of ILogger
    /// implementation or config.
    /// </summary>
    [EventSource(Name = "Microsoft-AspNet-Mvc")]
    internal class AspNetMvcEventSource : EventSource
    {
        /// <summary>
        /// Identifiers for event types from this EventSource.
        /// </summary>
        private const int RequestProcessedId = 1;

        private static Lazy<AspNetMvcEventSource> _lazyInstance = new Lazy<AspNetMvcEventSource>(() => new AspNetMvcEventSource());
        public static AspNetMvcEventSource Log
        {
            get
            {
                return _lazyInstance.Value;
            }
        }

        private AspNetMvcEventSource() : base(false)
        {
        }

        protected override void OnEventCommand(EventCommandEventArgs command)
        {
            base.OnEventCommand(command);

            // Cache keyword state here
        }

        [Event(RequestProcessedId)]
        private void RequestProcessed(string httpMethod, string path, string requestId, string parameters, string moduleName, string methodId, string pathBase)
        {
            WriteEvent(RequestProcessedId, httpMethod, path, requestId, parameters, moduleName, methodId, pathBase);
        }

        [NonEvent]
        public void RequestProcessed(ActionExecutingContext context, MethodInfo methodInfo)
        {
            if (IsEnabled())// Check for keyword enabled-ness here
            {
                if (context != null & methodInfo != null)
                {
                    string httpMethod = context.HttpContext.Request.Method;
                    string path = context.HttpContext.Request.Path;
                    string requestId = GetRequestIdFromContext(context);
                    string parameters = GetJsonArgumentsFromDictionary(context.ActionArguments);
                    string moduleName = methodInfo.Module.Assembly.GetName().Name;
                    string methodId = GetMethodId(methodInfo);
                    string pathBase = context.HttpContext.Request.PathBase;

                    RequestProcessed(httpMethod, path, requestId, parameters, moduleName, methodId, pathBase);
                }
            }
        }

        [NonEvent]
        private static string GetRequestIdFromContext(ActionExecutingContext context)
        {
            var requestIdFeature = context.HttpContext.GetFeature<Http.Features.IHttpRequestIdentifierFeature>();
            return requestIdFeature != null ? requestIdFeature.TraceIdentifier : "";
        }

        /// <summary>
        /// Generates a fully qualified signature for a method of the form
        /// FullyQualifiedContainingTypeName.MethodName([[FullyQualifiedParameterTypeName]...]):FullyQualifiedReturnTypeName
        /// </summary>
        /// <param name="methodInfo">MethodInfo of method for which to generate signature.</param>
        /// <returns></returns>
        [NonEvent]
        private static string GetMethodId(MethodInfo methodInfo)
        {
            var stringBuilder = new StringBuilder();
            var callingConvention = methodInfo.CallingConvention;

            // FullyQualifiedContainingTypeName
            stringBuilder.Append(methodInfo.DeclaringType.FullName);
            // FullyQualifiedContainingTypeName.
            stringBuilder.Append('.');
            // FullyQualifiedContainingTypeName.MethodName
            stringBuilder.Append(methodInfo.Name);
            // FullyQualifiedContainingTypeName.MethodName(
            stringBuilder.Append('(');
            bool noParameters = true;
            foreach (var parameter in methodInfo.GetParameters())
            {
                if (!noParameters)
                {
                    // FullyQualifiedContainingTypeName.MethodName([...],
                    stringBuilder.Append(", ");
                }

                // FullyQualifiedContainingTypeName.MethodName([...,]FullyQualifiedParameterTypeName
                stringBuilder.Append(parameter.ParameterType.Name);

                noParameters = false;
            }

            if (callingConvention.HasFlag(CallingConventions.VarArgs))
            {
                if (!noParameters)
                {
                    // FullyQualifiedContainingTypeName.MethodName([...],
                    stringBuilder.Append(", ");
                }

                // FullyQualifiedContainingTypeName.MethodName([...,]...
                stringBuilder.Append("...");
            }

            // FullyQualifiedContainingTypeName.MethodName([...])
            stringBuilder.Append(')');
            // FullyQualifiedContainingTypeName.MethodName([...]):
            stringBuilder.Append(':');
            // FullyQualifiedContainingTypeName.MethodName([...]):FullyQualifiedReturnTypeName
            stringBuilder.Append(methodInfo.ReturnType.Name);

            return stringBuilder.ToString();
        }

        [NonEvent]
        internal static string GetJsonArgumentsFromDictionary<T, U>(IDictionary<T, U> dictionary)
        {
            if (dictionary != null)
            {
                var jObject = new JObject();
                foreach (var kvp in dictionary)
                {
                    jObject.Add(kvp.Key.ToString(), kvp.Value.ToString());
                }

                return jObject.ToString(Newtonsoft.Json.Formatting.None);
            }
            else
            {
                return "";
            }
        }

        [NonEvent]
        private unsafe void WriteEvent(int eventId, params string[] args)
        {
            EventData* dataDesc = stackalloc EventData[args.Length];
            GCHandle* handles = stackalloc GCHandle[args.Length];
            try
            {
                for (int i = 0; i < args.Length; i++)
                {
                    handles[i] = GCHandle.Alloc(args[i], GCHandleType.Pinned);
                    dataDesc[i].DataPointer = handles[i].AddrOfPinnedObject();
                    dataDesc[i].Size = (args[i].Length + 1) * sizeof(char);
                }
                WriteEventCore(eventId, args.Length, dataDesc);
            }
            catch
            {
                // don't throw an exception for failure to generate ETW event
                Debug.Fail("Exception hit while generating ETW event");
            }
            finally
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (handles[i].IsAllocated)
                    {
                        handles[i].Free();
                    }
                }
            }
        }
    }
}
