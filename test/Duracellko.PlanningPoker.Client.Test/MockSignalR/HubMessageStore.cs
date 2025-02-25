﻿using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.AspNetCore.SignalR.Protocol;

namespace Duracellko.PlanningPoker.Client.Test.MockSignalR
{
    internal sealed class HubMessageStore
    {
        private readonly ConcurrentDictionary<long, HubMessage> _messages = new ConcurrentDictionary<long, HubMessage>();
        private long _nextId;

        public HubMessage this[long id]
        {
            get => _messages[id];
        }

        public long Add(HubMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            long nextId = Interlocked.Increment(ref _nextId);
            if (!_messages.TryAdd(nextId, message))
            {
                throw new InvalidOperationException("Message ID should be unique.");
            }

            return nextId;
        }

        public bool TryGetMessage(long id, [MaybeNullWhen(false)] out HubMessage message)
        {
            return _messages.TryGetValue(id, out message);
        }

        public bool TryRemove(long id)
        {
            return _messages.TryRemove(id, out _);
        }
    }
}
