using System;
using System.IO;
using MiscUtil.Conversion;

namespace PacketDotNet.Ieee80211
{
    /// <summary>
    /// Lock quality
    /// </summary>
    public class LockQualityRadioTapField : RadioTapField
    {
        /// <summary>Type of the field</summary>
        public override RadioTapType FieldType { get { return RadioTapType.LockQuality; } }
   
            
        /// <summary>
        /// Gets the length of the field data.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public override ushort Length { get { return 2; } }
            
        /// <summary>
        /// Signal quality
        /// </summary>
        public UInt16 SignalQuality { get; set; }
   
        /// <summary>
        /// Copies the field data to the destination buffer at the specified offset.
        /// </summary>
        public override void CopyTo(byte[] dest, int offset)
        {
            EndianBitConverter.Little.CopyBytes(this.SignalQuality, dest, offset);
        }
            
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="br">
        /// A <see cref="BinaryReader"/>
        /// </param>
        public LockQualityRadioTapField(BinaryReader br)
        {
            this.SignalQuality = br.ReadUInt16();
        }
            
        /// <summary>
        /// Initializes a new instance of the <see cref="PacketDotNet.Ieee80211.LockQualityRadioTapField"/> class.
        /// </summary>
        public LockQualityRadioTapField()
        {
             
        }
            
        /// <summary>
        /// Initializes a new instance of the <see cref="PacketDotNet.Ieee80211.LockQualityRadioTapField"/> class.
        /// </summary>
        /// <param name='SignalQuality'>
        /// Signal quality.
        /// </param>
        public LockQualityRadioTapField(UInt16 SignalQuality)
        {
            this.SignalQuality = SignalQuality;
        }

        /// <summary>
        /// ToString() override
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/>
        /// </returns>
        public override string ToString()
        {
            return string.Format("SignalQuality {0}", this.SignalQuality);
        }
    }
}