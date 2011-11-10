using System;
using System.Threading;

namespace Xlnt.Threading
{
    public static class ReaderWriterLockExtensions
    {
        public static void Write(this ReaderWriterLock @lock, Action block)
        {
            try {
                @lock.AcquireWriterLock(Timeout.Infinite);
                block();
            } finally {
                @lock.ReleaseWriterLock();
            }
        }

        public static T Read<T>(this ReaderWriterLock @lock, Func<T> block)
        {
            try {
                @lock.AcquireWriterLock(Timeout.Infinite);
                return block();
            } finally {
                @lock.ReleaseWriterLock();
            }
        }
    }
}
