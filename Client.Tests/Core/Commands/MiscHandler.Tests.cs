using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace xClient.Tests.Core.Compression
{
    [TestClass]
    public class MiscHandlerTests
    {
        [TestMethod]
        public void UploadValidBatch()
        {
            var bytes = new byte[2];
            bytes[0] = 101;
            bytes[1] = 99;
            var command = new xClient.Core.Packets.ServerPackets.DoUploadAndExecute(1, "bat.bat", bytes, 100, 0, false);

            command.IsValidExecuteFile();

            Assert.IsTrue(command.CorrectFileType, "Uploading a .bat file failed!");
        }

        [TestMethod]
        public void UploadValidExe()
        {
            var bytes = new byte[2];
            bytes[0] = 77;
            bytes[1] = 90;
            var command = new xClient.Core.Packets.ServerPackets.DoUploadAndExecute(1, "bat.bat", bytes, 100, 0, false);

            command.IsValidExecuteFile();

            Assert.IsTrue(command.CorrectFileType, "Uploading a .exe file failed!");
        }

        [TestMethod]
        public void UploadInValidFile()
        {
            var bytes = new byte[2];
            bytes[0] = 22;
            bytes[1] = 93;
            var command = new xClient.Core.Packets.ServerPackets.DoUploadAndExecute(1, "bat.bat", bytes, 100, 0, false);

            command.IsValidExecuteFile();

            Assert.IsFalse(command.CorrectFileType, "Uploading an invalid file worked!");
        }
    }
}