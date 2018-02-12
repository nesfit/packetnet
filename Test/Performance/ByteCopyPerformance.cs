using System;
using log4net.Core;
using NUnit.Framework;
using PacketDotNet.Ethernet;

namespace Test.Performance
{
    [TestFixture]
    public class ByteCopyPerformance
    {
        // The number of times the test is run
        private readonly Int32 _testRuns = 40000;

        [Test]
        public void ArrayCopyPerformance()
        {
            // create a realistic packet for testing
            var ethernetPacket = EthernetPacket.RandomPacket();
            // create the array to store the copy result
            Byte[] hwAddress = new Byte[EthernetFields.MacAddressLength];

            // store the logging value
            var oldThreshold = LoggingConfiguration.GlobalLoggingLevel;

            // disable logging to improve performance
            LoggingConfiguration.GlobalLoggingLevel = Level.Off;

            // Store the time before the processing starts
            var startTime = DateTime.Now;

            // run the test
            for (Int32 i = 0; i < this._testRuns; i++)
            {
                Array.Copy(ethernetPacket.Bytes, EthernetFields.SourceMacPosition,
                    hwAddress, 0, EthernetFields.MacAddressLength);
            }

            // store the time after the processing is finished
            var endTime = DateTime.Now;

            // restore logging
            LoggingConfiguration.GlobalLoggingLevel = oldThreshold;

            // calculate the statistics
            var rate = new Rate(startTime, endTime, this._testRuns, "Test runs");

            // output the statistics to the console
            Console.WriteLine(rate.ToString());
        }

        [Test]
        public void BufferCopyPerformance()
        {
            // create a realistic packet for testing
            var ethernetPacket = EthernetPacket.RandomPacket();
            // create the array to store the copy result
            Byte[] hwAddress = new Byte[EthernetFields.MacAddressLength];

            // store the logging value
            var oldThreshold = LoggingConfiguration.GlobalLoggingLevel;

            // disable logging to improve performance
            LoggingConfiguration.GlobalLoggingLevel = Level.Off;

            // Store the time before the processing starts
            var startTime = DateTime.Now;

            // run the test
            for (Int32 i = 0; i < this._testRuns; i++)
            {
                Buffer.BlockCopy(ethernetPacket.Bytes, EthernetFields.SourceMacPosition,
                    hwAddress, 0, EthernetFields.MacAddressLength);
            }

            // store the time after the processing is finished
            var endTime = DateTime.Now;

            // restore logging
            LoggingConfiguration.GlobalLoggingLevel = oldThreshold;

            // calculate the statistics
            var rate = new Rate(startTime, endTime, this._testRuns, "Test runs");

            // output the statistics to the console
            Console.WriteLine(rate.ToString());
        }
    }
}