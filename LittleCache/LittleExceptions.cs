using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LittleCache
{
    class LittleCacheExceptions : Exception
    {
        public LittleCacheExceptions() { }

        public LittleCacheExceptions(string message) : base(message) { }
    }
}
