using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace wServer.networking.server
{    
    internal sealed class SocketAsyncEventArgsPool
    {
        // Pool of reusable SocketAsyncEventArgs objects.        
        Stack<SocketAsyncEventArgs> pool;
        
        // initializes the object pool to the specified size.
        // "capacity" = Maximum number of SocketAsyncEventArgs objects
        internal SocketAsyncEventArgsPool(Int32 capacity)
        {
            this.pool = new Stack<SocketAsyncEventArgs>(capacity);
        }

        // The number of SocketAsyncEventArgs instances in the pool.         
        internal Int32 Count
        {
            get { return this.pool.Count; }
        }

        // Removes a SocketAsyncEventArgs instance from the pool.
        // returns SocketAsyncEventArgs removed from the pool.
        internal SocketAsyncEventArgs Pop()
        {
            using (TimedLock.Lock(pool))
            {
                return this.pool.Pop();
            }
        }

        // Add a SocketAsyncEventArg instance to the pool. 
        // "item" = SocketAsyncEventArgs instance to add to the pool.
        internal void Push(SocketAsyncEventArgs item)
        {
            if (item == null) 
            { 
                throw new ArgumentNullException("Items added to a SocketAsyncEventArgsPool cannot be null"); 
            }
            using (TimedLock.Lock(pool))
            {
                if (!pool.Contains(item))
                    pool.Push(item);
            }
        }
    }
}
