using System;
using System.Collections.Generic;
using System.Threading;

namespace ThorIOClient.Queue
{
    public class ThreadSafeQueue<T> : Queue<T>
    {
        private readonly object _LockObject = new object();
        public new int Count
        {
            get
            {
                int returnValue;

                lock (_LockObject)
                {
                    returnValue = base.Count;
                }

                return returnValue;
            }
        }
        public new void Clear()
        {
            lock (_LockObject)
            {
                base.Clear();
            }
        }

        public new T Dequeue()
        {
            T returnValue;

            lock (_LockObject)
            {
                returnValue = base.Dequeue();
            }

            return returnValue;
        }
        public new void Enqueue(T item)
        {
            lock (_LockObject)
            {
                base.Enqueue(item);
            }
        }

        public new void TrimExcess()
        {
            lock (_LockObject)
            {
                base.TrimExcess();
            }
        }
    }

}
