using System;

namespace Xlnt.IO
{
    public interface ILineReader : IDisposable
    {
        string ReadLine();
    }
}
