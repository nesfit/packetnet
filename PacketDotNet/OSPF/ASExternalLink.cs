using System;
using System.Net;
using PacketDotNet.Utils;
using PacketDotNet.Utils.Conversion;

namespace PacketDotNet.OSPF
{
    /// <summary>
    /// Describes a particular external destination
    /// </summary>
    public class ASExternalLink
    {
        /// <summary>
        /// The length.
        /// </summary>
        public static readonly Int32 Length = 12;
        internal ByteArraySegment Header;

        /// <summary>
        /// Default constructor
        /// </summary>
        public ASExternalLink()
        {
            Byte[] b = new Byte[Length];
            this.Header = new ByteArraySegment(b);
        }

        /// <summary>
        /// Constructs a packet from bytes and offset and length
        /// </summary>
        /// <param name="packet">
        /// A <see cref="System.Byte"/>
        /// </param>
        /// <param name="offset">
        /// A <see cref="System.Int32"/>
        /// </param>
        /// <param name="length">
        /// A <see cref="System.Int32"/>
        /// </param>
        public ASExternalLink(Byte[] packet, Int32 offset, Int32 length)
        {
            this.Header = new ByteArraySegment(packet, offset, length);
        }

        /// <summary>
        /// The type of external metric.  If bit E is set, the metric
        /// specified is a Type 2 external metric.
        /// </summary>
        public Byte EBit
        {
            get
            {
                var val = EndianBitConverter.Big.ToUInt32(this.Header.Bytes, this.Header.Offset + ASExternalLinkFields.TOSPosition);
                return (Byte)((val >> 31) & 0xFF);
            }
            set
            {
                UInt32 original = EndianBitConverter.Big.ToUInt32(this.Header.Bytes, this.Header.Offset + ASExternalLinkFields.TOSPosition);
                UInt32 val = (UInt32)((value & 1) << 31) | original;
                EndianBitConverter.Big.CopyBytes(val, this.Header.Bytes, this.Header.Offset + ASExternalLinkFields.TOSPosition);
            }
        }

        /// <summary>
        /// The Type of Service that the following fields concern.
        /// </summary>
        public Byte TOS
        {
            get
            {
                var val = EndianBitConverter.Big.ToUInt32(this.Header.Bytes, this.Header.Offset + ASExternalLinkFields.TOSPosition);
                return (Byte)((val >> 24) & 0x7F);
            }
            set
            {
                UInt32 original = EndianBitConverter.Big.ToUInt32(this.Header.Bytes, this.Header.Offset + ASExternalLinkFields.TOSPosition);
                var val = (Byte)((value & 0x7F) << 24) | original;
                EndianBitConverter.Big.CopyBytes(val, this.Header.Bytes, this.Header.Offset + ASExternalLinkFields.TOSPosition);
            }
        }

        /// <summary>
        /// The cost of this route.  Interpretation depends on the external
        /// type indication (bit E above).
        /// </summary>
        public UInt32 Metric
        {
            get
            {
                var val = EndianBitConverter.Big.ToUInt32(this.Header.Bytes, this.Header.Offset + ASExternalLinkFields.TOSPosition);
                return val & 0x00FFFFFF;
            }
            set
            {
                UInt32 original = EndianBitConverter.Big.ToUInt32(this.Header.Bytes, this.Header.Offset + ASExternalLinkFields.TOSPosition);
                var val = value & 0x00FFFFFF | original;
                EndianBitConverter.Big.CopyBytes(val, this.Header.Bytes, this.Header.Offset + ASExternalLinkFields.TOSPosition);
            }
        }

        /// <summary>
        /// Data traffic for the advertised destination will be forwarded to this address.
        /// </summary>
        public IPAddress ForwardingAddress
        {
            get
            {
                var val = EndianBitConverter.Little.ToUInt32(this.Header.Bytes, this.Header.Offset + ASExternalLinkFields.ForwardingAddressPosition);
                return new IPAddress(val);
            }
            set
            {
                Byte[] address = value.GetAddressBytes();
                Array.Copy(address, 0,
                    this.Header.Bytes, this.Header.Offset + ASExternalLinkFields.ForwardingAddressPosition,
                    address.Length);
            }
        }

        /// <summary>
        ///  A 32-bit field attached to each external route.  This is not used by the OSPF protocol itself.
        /// </summary>
        public UInt32 ExternalRouteTag
        {
            get => EndianBitConverter.Big.ToUInt32(this.Header.Bytes, this.Header.Offset + ASExternalLinkFields.ExternalRouteTagPosition);
            set => EndianBitConverter.Big.CopyBytes(value, this.Header.Bytes, this.Header.Offset + ASExternalLinkFields.ExternalRouteTagPosition);
        }

        /// <summary>
        /// Bytes representation
        /// </summary>
        public Byte[] Bytes => this.Header.Bytes;
    }
}