
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LittleCache.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task CacheFolderTest()
        {
            List<string> keys = new List<string>()
            {
                "test1",
                "test2",
                "test3",
            };
            await LittleCache.CreateCacheFolderAsync();
            await LittleCache.CreateCacheFolderAsync(keys);
            await LittleCache.DeleteCacheFolderAsync();
            //await LittleCache.DeleteAllCacheFolderAsync();
        }
    }
}
