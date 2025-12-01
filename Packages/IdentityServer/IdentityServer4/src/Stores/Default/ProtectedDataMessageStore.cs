// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using IdentityModel;
using System.Text;
using System;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Stores
{
    /// <summary>
    /// IMessageStore implementation that uses data protection to protect message.
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public class ProtectedDataMessageStore<TModel> : IMessageStore<TModel>
    {
        private const string Purpose = "IdentityServer4.Stores.ProtectedDataMessageStore";

        /// <summary>
        /// The data protector.
        /// </summary>
        protected readonly IDataProtector Protector;

        /// <summary>
        /// The logger.
        /// </summary>
        protected readonly ILogger Logger;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="logger"></param>
        public ProtectedDataMessageStore(IDataProtectionProvider provider, ILogger<ProtectedDataMessageStore<TModel>> logger)
        {
            Protector = provider.CreateProtector(Purpose);
            Logger = logger;
        }

        /// <inheritdoc />
        public virtual Task<Message<TModel>> ReadAsync(string value)
        {
            Message<TModel> result = null;

            if (!String.IsNullOrWhiteSpace(value))
            {
                try
                {
                    Logger.LogCritical("ProtectedDataMessageStore_Log_ReadAsync: value: {0}", value);
                    var bytes = Base64Url.Decode(value);
                    bytes = Protector.Unprotect(bytes);
                    var json = Encoding.UTF8.GetString(bytes);
                    result = ObjectSerializer.FromString<Message<TModel>>(json);
                    Logger.LogCritical("ProtectedDataMessageStore_Log_ReadAsync: json: {0}", json);
                }
                catch(Exception ex)
                {
                    Logger.LogError(ex, "Exception reading protected message");
                }
            }

            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public virtual Task<string> WriteAsync(Message<TModel> message)
        {
            string value = null;

            try
            {
                var json = ObjectSerializer.ToString(message);
                Logger.LogCritical("ProtectedDataMessageStore_Log_WriteAsync: json: {0}", json);
                var bytes = Encoding.UTF8.GetBytes(json);
                bytes = Protector.Protect(bytes);
                value = Base64Url.Encode(bytes);
                Logger.LogCritical("ProtectedDataMessageStore_Log_WriteAsync: value: {0}", value);
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, "Exception writing protected message");
            }

            return Task.FromResult(value);
        }
    }
}
