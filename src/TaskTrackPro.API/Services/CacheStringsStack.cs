using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaskTrackPro.API.Services
{
    public class CacheStringsStack
    {
        private readonly Stack<string> _stack = new Stack<string>();
        private readonly object _lock = new object();

        public void Push(string value)
        {
            lock (_lock)
            {
                _stack.Push(value);
            }
        }

        public string Pop()
        {
            lock (_lock)
            {
                return _stack.Count > 0 ? _stack.Pop() : null;
            }
        }

        public string Peek()
        {
            lock (_lock)
            {
                return _stack.Count > 0 ? _stack.Peek() : null;
            }
        }

        public bool IsEmpty()
        {
            lock (_lock)
            {
                return _stack.Count == 0;
            }
        }

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _stack.Count;
                }
            }
        }

        public async Task PushAsync(string value)
        {
            await Task.Run(() => Push(value));
        }

        public async Task<string> PopAsync()
        {
            return await Task.Run(() => Pop());
        }
    }
}
