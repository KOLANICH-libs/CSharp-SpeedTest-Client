using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MeasuringStreams;
using System.IO;
using System.Threading.Tasks;

namespace MeasuringStreamsTest
{
    [TestClass]
    public class Stream2StreamTest
    {
        const long defCount = 0xFFFF;
        [TestMethod]
        public async Task ConstantToDevNull()
        {
            using (Stream cs = new ConstantStream(defCount, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF }), dns = new DevNullStream())
               await dns.CopyToAsync(cs);
        }
        [TestMethod]
        public async Task RandomToDevNull()
        {
            using (Stream cs = new RandomBytesStream(defCount), dns = new DevNullStream())
              await dns.CopyToAsync(cs);
        }
        [TestMethod]
        public async Task RangedRandomToDevNull()
        {
            using (Stream cs = new RangedRandomBytesStream(defCount, 0xAD, 0xEF), dns = new DevNullStream())
                await dns.CopyToAsync(cs);
        }
    }
}