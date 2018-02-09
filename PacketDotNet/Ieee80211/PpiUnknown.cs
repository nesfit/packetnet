using System.IO;

namespace PacketDotNet.Ieee80211
{
    /// <summary>
    /// The PpiUnknown field class can be used to represent any field types not
    /// currently supported by PacketDotNet. Any unsupported field types encountered during 
    /// parsing will be stored as PpiUnknown fields.
    /// </summary>
    public class PpiUnknown : PpiField
    {
        private PpiFieldType fieldType;
        #region Properties

        /// <summary>Type of the field</summary>
        public override PpiFieldType FieldType{ get { return this.fieldType; } } 
            
        /// <summary>
        /// Gets the length of the field data.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public override int Length { get { return this.Bytes.Length; } }
   
        /// <summary>
        /// Gets the field bytes. This doesn't include the PPI field header.
        /// </summary>
        /// <value>
        /// The bytes.
        /// </value>
        public override byte[] Bytes { get { return this.UnknownBytes; } }
        /// <summary>
        /// Gets or sets the field data.
        /// </summary>
        /// <value>
        /// The fields values bytes.
        /// </value>
        public byte[] UnknownBytes { get; set; }
            
        #endregion Properties

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="PacketDotNet.Ieee80211.PpiUnknown"/> class from the 
        /// provided stream.
        /// </summary>
        /// <remarks>
        /// The position of the BinaryReader's underlying stream will be advanced to the end
        /// of the PPI field.
        /// </remarks>
        /// <param name='typeNumber'>
        /// The PPI field type number
        /// </param>
        /// <param name='br'>
        /// The stream the field will be read from
        /// </param>
        /// <param name='length'>
        /// The number of bytes the unknown field contains.
        /// </param>
        public PpiUnknown (int typeNumber, BinaryReader br, int length)
        {
            this.fieldType = (PpiFieldType) typeNumber;
            this.UnknownBytes = br.ReadBytes(length);
        }
   
        /// <summary>
        /// Initializes a new instance of the <see cref="PacketDotNet.Ieee80211.PpiUnknown"/> class.
        /// </summary>
        /// <param name='typeNumber'>
        /// The PPI field type number.
        /// </param>
        public PpiUnknown (int typeNumber)
        {
            this.fieldType = (PpiFieldType)typeNumber;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="PacketDotNet.Ieee80211.PpiUnknown"/> class.
        /// </summary>
        /// <param name='typeNumber'>
        /// The PPI field type number.
        /// </param>
        /// <param name='UnknownBytes'>
        /// The field data.
        /// </param>
        public PpiUnknown (int typeNumber, byte[] UnknownBytes)
        {
            this.fieldType = (PpiFieldType)typeNumber;
            this.UnknownBytes = UnknownBytes;
        }
            
        #endregion Constructors
    }
}