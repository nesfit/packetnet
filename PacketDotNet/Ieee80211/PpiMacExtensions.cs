using System.IO;

namespace PacketDotNet.Ieee80211
{
    /// <summary>
    /// The 802.11n MAC Extension field contains radio information specific to 802.11n.
    /// </summary>
    public class PpiMacExtensions : PpiField
    {
            
            
        #region Properties

        /// <summary>Type of the field</summary>
        public override PpiFieldType FieldType
        {
            get { return PpiFieldType.PpiMacExtensions;}
        }
   
        /// <summary>
        /// Gets the length of the field data.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public override int Length { get { return 12; } }
            
        /// <summary>
        /// Gets or sets the 802.11n MAC extension flags.
        /// </summary>
        /// <value>
        /// The flags.
        /// </value>
        public PpiMacExtensionFlags Flags { get; set; }
        /// <summary>
        /// Gets or sets the A-MPDU identifier.
        /// </summary>
        /// <value>
        /// the A-MPDU id.
        /// </value>
        public uint AMpduId { get; set; }
        /// <summary>
        /// Gets or sets the number of zero-length pad delimiters
        /// </summary>
        /// <value>
        /// The delimiter count.
        /// </value>
        public byte DelimiterCount { get; set; }
            
        /// <summary>
        /// Gets the field bytes. This doesn't include the PPI field header.
        /// </summary>
        /// <value>
        /// The bytes.
        /// </value>
        public override byte[] Bytes
        {
            get
            {
                MemoryStream ms = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(ms);
                    
                writer.Write((uint) this.Flags);
                writer.Write(this.AMpduId);
                writer.Write(this.DelimiterCount);
                writer.Write(new byte[3]);
                    
                return ms.ToArray();
            }
        }
            
        #endregion Properties

        #region Constructors
   
        /// <summary>
        /// Initializes a new instance of the <see cref="PacketDotNet.Ieee80211.PpiMacExtensions"/> class from the 
        /// provided stream.
        /// </summary>
        /// <remarks>
        /// The position of the BinaryReader's underlying stream will be advanced to the end
        /// of the PPI field.
        /// </remarks>
        /// <param name='br'>
        /// The stream the field will be read from
        /// </param>
        public PpiMacExtensions (BinaryReader br)
        {
            this.Flags = (PpiMacExtensionFlags) br.ReadUInt32();
            this.AMpduId = br.ReadUInt32();
            this.DelimiterCount = br.ReadByte();
        }
            
        /// <summary>
        /// Initializes a new instance of the <see cref="PacketDotNet.Ieee80211.PpiMacExtensions"/> class.
        /// </summary>
        public PpiMacExtensions ()
        {
             
        }

        #endregion Constructors
    }
}