﻿/*
This file is part of PacketDotNet

PacketDotNet is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

PacketDotNet is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with PacketDotNet.  If not, see <http://www.gnu.org/licenses/>.
*/
/*
 * Copyright 2010 Chris Morgan <chmorgan@gmail.com>
 */

using System;
using System.Collections.Generic;
using System.Text;
using PacketDotNet.Drda;
using PacketDotNet.Interfaces;
using PacketDotNet.IP;
using PacketDotNet.Utils;
using PacketDotNet.Utils.Conversion;

namespace PacketDotNet.Tcp
{
    /// <summary>
    ///     TcpPacket
    ///     See: http://en.wikipedia.org/wiki/Transmission_Control_Protocol
    /// </summary>
    [Serializable]
    public class TcpPacket : TransportPacket, ISourceDestinationPort
    {
#if DEBUG_PACKETDOTNET
        private static readonly log4net.ILog Log =
 log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
#else
        // NOTE: No need to warn about lack of use, the compiler won't
        //       put any calls to 'log' here but we need 'log' to exist to compile
#pragma warning disable 0169, 0649
        private static readonly ILogInactive Log;
#pragma warning restore 0169, 0649
#endif

        /// <value>
        ///     20 bytes is the smallest tcp header
        /// </value>
        public const Int32 HeaderMinimumLength = 20;

        /// <summary> Fetch the port number on the source host.</summary>
        public virtual UInt16 SourcePort
        {
            get => EndianBitConverter.Big.ToUInt16(this.HeaderByteArraySegment.Bytes,
                this.HeaderByteArraySegment.Offset + TcpFields.SourcePortPosition);

            set
            {
                var theValue = value;
                EndianBitConverter.Big.CopyBytes(theValue, this.HeaderByteArraySegment.Bytes,
                    this.HeaderByteArraySegment.Offset + TcpFields.SourcePortPosition);
            }
        }

        /// <summary> Fetches the port number on the destination host.</summary>
        public virtual UInt16 DestinationPort
        {
            get => EndianBitConverter.Big.ToUInt16(this.HeaderByteArraySegment.Bytes,
                this.HeaderByteArraySegment.Offset + TcpFields.DestinationPortPosition);

            set
            {
                var theValue = value;
                EndianBitConverter.Big.CopyBytes(theValue, this.HeaderByteArraySegment.Bytes,
                    this.HeaderByteArraySegment.Offset + TcpFields.DestinationPortPosition);
            }
        }

        /// <summary> Fetch the packet sequence number.</summary>
        public UInt32 SequenceNumber
        {
            get => EndianBitConverter.Big.ToUInt32(this.HeaderByteArraySegment.Bytes,
                this.HeaderByteArraySegment.Offset + TcpFields.SequenceNumberPosition);

            set => EndianBitConverter.Big.CopyBytes(value, this.HeaderByteArraySegment.Bytes,
                this.HeaderByteArraySegment.Offset + TcpFields.SequenceNumberPosition);
        }

        /// <summary> Fetch the packet acknowledgment number.</summary>
        public UInt32 AcknowledgmentNumber
        {
            get => EndianBitConverter.Big.ToUInt32(this.HeaderByteArraySegment.Bytes,
                this.HeaderByteArraySegment.Offset + TcpFields.AckNumberPosition);

            set => EndianBitConverter.Big.CopyBytes(value, this.HeaderByteArraySegment.Bytes,
                this.HeaderByteArraySegment.Offset + TcpFields.AckNumberPosition);
        }

        private UInt16 DataOffsetAndFlags
        {
            get => EndianBitConverter.Big.ToUInt16(this.HeaderByteArraySegment.Bytes,
                this.HeaderByteArraySegment.Offset + TcpFields.DataOffsetAndFlagsPosition);

            set => EndianBitConverter.Big.CopyBytes(value, this.HeaderByteArraySegment.Bytes,
                this.HeaderByteArraySegment.Offset + TcpFields.DataOffsetAndFlagsPosition);
        }

        /// <summary> The size of the tcp header in 32bit words </summary>
        public virtual Int32 DataOffset
        {
            get
            {
                var dataOffset = (Byte) ((this.DataOffsetAndFlags >> 12) & 0xF);
                return dataOffset;
            }

            set
            {
                var dataOffset = this.DataOffsetAndFlags;

                dataOffset = (UInt16) ((dataOffset & 0x0FFF) | ((value << 12) & 0xF000));

                // write the value back
                this.DataOffsetAndFlags = dataOffset;
            }
        }

        /// <summary>
        ///     The size of the receive window, which specifies the number of
        ///     bytes (beyond the sequence number in the acknowledgment field) that
        ///     the receiver is currently willing to receive.
        /// </summary>
        public virtual UInt16 WindowSize
        {
            get => EndianBitConverter.Big.ToUInt16(this.HeaderByteArraySegment.Bytes,
                this.HeaderByteArraySegment.Offset + TcpFields.WindowSizePosition);

            set => EndianBitConverter.Big.CopyBytes(value, this.HeaderByteArraySegment.Bytes,
                this.HeaderByteArraySegment.Offset + TcpFields.WindowSizePosition);
        }

        /// <value>
        ///     Tcp checksum field value of type UInt16
        /// </value>
        public override UInt16 Checksum
        {
            get => EndianBitConverter.Big.ToUInt16(this.HeaderByteArraySegment.Bytes,
                this.HeaderByteArraySegment.Offset + TcpFields.ChecksumPosition);

            set
            {
                var theValue = value;
                EndianBitConverter.Big.CopyBytes(theValue, this.HeaderByteArraySegment.Bytes,
                    this.HeaderByteArraySegment.Offset + TcpFields.ChecksumPosition);
            }
        }

        /// <summary> Check if the TCP packet is valid, checksum-wise.</summary>
        public Boolean ValidChecksum
        {
            get
            {
                // IPv6 has no checksum so only the TCP checksum needs evaluation
                if (this.ParentPacket.GetType() == typeof(IPv6Packet))
                    return this.ValidTCPChecksum;
                // For IPv4 both the IP layer and the TCP layer contain checksums
                return ((IPv4Packet) this.ParentPacket).ValidIPChecksum && this.ValidTCPChecksum;
            }
        }

        /// <value>
        ///     True if the tcp checksum is valid
        /// </value>
        public virtual Boolean ValidTCPChecksum
        {
            get
            {
                Log.Debug("ValidTCPChecksum");
                var retval = this.IsValidChecksum(TransportChecksumOption.AttachPseudoIPHeader);
                Log.DebugFormat("ValidTCPChecksum {0}", retval);
                return retval;
            }
        }

        /// <summary>
        ///     Flags, 9 bits
        /// </summary>
        public UInt16 AllFlags
        {
            get
            {
                var flags = (this.DataOffsetAndFlags & 0x1FF);
                return (UInt16) flags;
            }

            set
            {
                var flags = this.DataOffsetAndFlags;

                flags = (UInt16) ((flags & 0xFE00) | (value & 0x1FF));
                this.DataOffsetAndFlags = flags;
            }
        }

        /// <summary> Check the URG flag, flag indicates if the urgent pointer is valid.</summary>
        public virtual Boolean Urg
        {
            get => (this.AllFlags & TcpFields.TCP_URG_MASK) != 0;
            set => this.SetFlag(value, TcpFields.TCP_URG_MASK);
        }

        /// <summary> Check the ACK flag, flag indicates if the ack number is valid.</summary>
        public virtual Boolean Ack
        {
            get => (this.AllFlags & TcpFields.TCP_ACK_MASK) != 0;
            set => this.SetFlag(value, TcpFields.TCP_ACK_MASK);
        }

        /// <summary>
        ///     Check the PSH flag, flag indicates the receiver should pass the
        ///     data to the application as soon as possible.
        /// </summary>
        public virtual Boolean Psh
        {
            get => (this.AllFlags & TcpFields.TCP_PSH_MASK) != 0;
            set => this.SetFlag(value, TcpFields.TCP_PSH_MASK);
        }

        /// <summary>
        ///     Check the RST flag, flag indicates the session should be reset between
        ///     the sender and the receiver.
        /// </summary>
        public virtual Boolean Rst
        {
            get => (this.AllFlags & TcpFields.TCP_RST_MASK) != 0;
            set => this.SetFlag(value, TcpFields.TCP_RST_MASK);
        }

        /// <summary>
        ///     Check the SYN flag, flag indicates the sequence numbers should
        ///     be synchronized between the sender and receiver to initiate
        ///     a connection.
        /// </summary>
        public virtual Boolean Syn
        {
            get => (this.AllFlags & TcpFields.TCP_SYN_MASK) != 0;
            set => this.SetFlag(value, TcpFields.TCP_SYN_MASK);
        }

        /// <summary> Check the FIN flag, flag indicates the sender is finished sending.</summary>
        public virtual Boolean Fin
        {
            get => (this.AllFlags & TcpFields.TCP_FIN_MASK) != 0;
            set => this.SetFlag(value, TcpFields.TCP_FIN_MASK);
        }

        /// <value>
        ///     ECN flag
        /// </value>
        public virtual Boolean ECN
        {
            get => (this.AllFlags & TcpFields.TCP_ECN_MASK) != 0;
            set => this.SetFlag(value, TcpFields.TCP_ECN_MASK);
        }

        /// <value>
        ///     CWR flag
        /// </value>
        public virtual Boolean CWR
        {
            get => (this.AllFlags & TcpFields.TCP_CWR_MASK) != 0;
            set => this.SetFlag(value, TcpFields.TCP_CWR_MASK);
        }

        /// <value>
        ///     NS flag
        /// </value>
        public virtual Boolean NS
        {
            get => (this.AllFlags & TcpFields.TCP_NS_MASK) != 0;
            set => this.SetFlag(value, TcpFields.TCP_NS_MASK);
        }

        private void SetFlag(Boolean on, Int32 mask)
        {
            if (on)
                this.AllFlags = (UInt16) (this.AllFlags | mask);
            else
                this.AllFlags = (UInt16) (this.AllFlags & ~mask);
        }

        /// <summary> Fetch ascii escape sequence of the color associated with this packet type.</summary>
        public override String Color => AnsiEscapeSequences.Yellow;

        /// <summary>
        ///     Create a new TCP packet from values
        /// </summary>
        public TcpPacket(UInt16 sourcePort,
            UInt16 destinationPort)
        {
            Log.Debug("");

            // allocate memory for this packet
            Int32 offset = 0;
            Int32 length = TcpFields.HeaderLength;
            var headerBytes = new Byte[length];
            this.HeaderByteArraySegment = new ByteArraySegment(headerBytes, offset, length);

            // make this packet valid
            this.DataOffset = length / 4;

            // set instance values
            this.SourcePort = sourcePort;
            this.DestinationPort = destinationPort;
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="bas">
        ///     A <see cref="ByteArraySegment" />
        /// </param>
        public TcpPacket(ByteArraySegment bas)
        {
            Log.Debug("");

            // set the header field, header field values are retrieved from this byte array
            // ReSharper disable once UseObjectOrCollectionInitializer
            this.HeaderByteArraySegment = new ByteArraySegment(bas);

            // NOTE: we update the Length field AFTER the header field because
            // we need the header to be valid to retrieve the value of DataOffset
            this.HeaderByteArraySegment.Length = this.DataOffset * 4;

            // store the payload bytes
            this.PayloadPacketOrData = new PacketOrByteArraySegment
            {
                TheByteArraySegment = this.HeaderByteArraySegment.EncapsulatedBytes()
            };
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="bas">
        ///     A <see cref="ByteArraySegment" />
        /// </param>
        /// <param name="parentPacket">
        ///     A <see cref="Packet" />
        /// </param>
        public TcpPacket(ByteArraySegment bas, Packet parentPacket) : this(bas)
        {
            Log.DebugFormat($"ParentPacket.GetType() {parentPacket.GetType()}");

            this.ParentPacket = parentPacket;

            // if the parent packet is an IPv4Packet we need to adjust
            // the payload length because it is possible for us to have
            // X bytes of data but only (X - Y) bytes are actually valid
            if (!(this.ParentPacket is IPv4Packet ipv4Parent)) return;

            // actual total length (tcp header + tcp payload)
            var ipPayloadTotalLength = ipv4Parent.TotalLength - (ipv4Parent.HeaderLength * 4);

            Log.DebugFormat(
                $"ipv4Parent.TotalLength {ipv4Parent.TotalLength}, ipv4Parent.HeaderLength {ipv4Parent.HeaderLength * 4}");

            var newTcpPayloadLength = ipPayloadTotalLength - this.Header.Length;

            Log.DebugFormat(
                $"Header.Length {this.HeaderByteArraySegment.Length}, Current payload length: {this.PayloadPacketOrData.TheByteArraySegment.Length}, new payload length {newTcpPayloadLength}");

            // the length of the payload is the total payload length
            // above, minus the length of the tcp header
            this.PayloadPacketOrData.TheByteArraySegment.Length = newTcpPayloadLength;
            this.DecodePayload();
        }

        /// <summary>
        ///     Decode Payload to Support Drda procotol
        /// </summary>
        /// <returns></returns>
        public TcpPacket DecodePayload()
        {
            if (this.PayloadData == null)
            {
                return this;
            }

            //PayloadData[2] is Magic field and Magic field==0xd0 means this may be a Drda Packet
            if (this.PayloadData.Length < DrdaDDMFields.DDMHeadTotalLength || this.PayloadData[2] != 0xd0) return this;

            var drdaPacket = new DrdaPacket(this.PayloadPacketOrData.TheByteArraySegment, this);
            this.PayloadPacketOrData.ThePacket = drdaPacket;
            return this;
        }

        /// <summary>
        ///     Computes the TCP checksum. Does not update the current checksum value
        /// </summary>
        /// <returns> The calculated TCP checksum.</returns>
        public Int32 CalculateTCPChecksum()
        {
            var newChecksum = this.CalculateChecksum(TransportChecksumOption.AttachPseudoIPHeader);
            return newChecksum;
        }

        /// <summary>
        ///     Update the checksum value.
        /// </summary>
        public void UpdateTCPChecksum()
        {
            Log.Debug("");
            this.Checksum = (UInt16) this.CalculateTCPChecksum();
        }

        /// <summary> Fetch the urgent pointer.</summary>
        public Int32 UrgentPointer
        {
            get => EndianBitConverter.Big.ToInt16(this.HeaderByteArraySegment.Bytes,
                this.HeaderByteArraySegment.Offset + TcpFields.UrgentPointerPosition);

            set
            {
                var theValue = (Int16) value;
                EndianBitConverter.Big.CopyBytes(theValue, this.HeaderByteArraySegment.Bytes,
                    this.HeaderByteArraySegment.Offset + TcpFields.UrgentPointerPosition);
            }
        }

        /// <summary>
        ///     Bytes that represent the tcp options
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" />
        /// </returns>
        public Byte[] Options
        {
            get
            {
                if (this.Urg)
                {
                    throw new NotImplementedException("Urg == true not implemented yet");
                }

                Int32 optionsOffset = TcpFields.UrgentPointerPosition + TcpFields.UrgentPointerLength;
                Int32 optionsLength = (this.DataOffset * 4) - optionsOffset;

                Byte[] optionBytes = new Byte[optionsLength];
                Array.Copy(this.HeaderByteArraySegment.Bytes, this.HeaderByteArraySegment.Offset + optionsOffset,
                    optionBytes, 0,
                    optionsLength);

                return optionBytes;
            }
        }

        /// <summary>
        ///     Parses options, pointed to by optionBytes into an array of Options
        /// </summary>
        /// <param name="optionBytes">
        ///     A <see cref="T:System.Byte[]" />
        /// </param>
        /// <returns>
        ///     A <see cref="List&lt;Option&gt;" />
        /// </returns>
        private List<Option> ParseOptions(Byte[] optionBytes)
        {
            Int32 offset = 0;
            OptionTypes type;
            Byte length;

            if (optionBytes.Length == 0)
                return null;

            // reset the OptionsCollection list to prepare
            //  to be re-populated with new data
            var retval = new List<Option>();

            while (offset < optionBytes.Length)
            {
                type = (OptionTypes) optionBytes[offset + Option.KindFieldOffset];

                // some options have no length field, we cannot read
                // the length field if it isn't present or we risk
                // out-of-bounds issues
                if ((type == OptionTypes.EndOfOptionList) ||
                    (type == OptionTypes.NoOperation))
                {
                    length = 1;
                }
                else
                {
                    length = optionBytes[offset + Option.LengthFieldOffset];
                }

                switch (type)
                {
                    case OptionTypes.EndOfOptionList:
                        retval.Add(new EndOfOptions(optionBytes, offset, length));
                        offset += EndOfOptions.OptionLength;
                        break;
                    case OptionTypes.NoOperation:
                        retval.Add(new NoOperation(optionBytes, offset, length));
                        offset += NoOperation.OptionLength;
                        break;
                    case OptionTypes.MaximumSegmentSize:
                        retval.Add(new MaximumSegmentSize(optionBytes, offset, length));
                        offset += length;
                        break;
                    case OptionTypes.WindowScaleFactor:
                        retval.Add(new WindowScaleFactor(optionBytes, offset, length));
                        offset += length;
                        break;
                    case OptionTypes.SACKPermitted:
                        retval.Add(new SACKPermitted(optionBytes, offset, length));
                        offset += length;
                        break;
                    case OptionTypes.SACK:
                        retval.Add(new SACK(optionBytes, offset, length));
                        offset += length;
                        break;
                    case OptionTypes.Echo:
                        retval.Add(new Echo(optionBytes, offset, length));
                        offset += length;
                        break;
                    case OptionTypes.EchoReply:
                        retval.Add(new EchoReply(optionBytes, offset, length));
                        offset += length;
                        break;
                    case OptionTypes.Timestamp:
                        retval.Add(new TimeStamp(optionBytes, offset, length));
                        offset += length;
                        break;
                    case OptionTypes.AlternateChecksumRequest:
                        retval.Add(new AlternateChecksumRequest(optionBytes, offset, length));
                        offset += length;
                        break;
                    case OptionTypes.AlternateChecksumData:
                        retval.Add(new AlternateChecksumData(optionBytes, offset, length));
                        offset += length;
                        break;
                    case OptionTypes.MD5Signature:
                        retval.Add(new MD5Signature(optionBytes, offset, length));
                        offset += length;
                        break;
                    case OptionTypes.UserTimeout:
                        retval.Add(new UserTimeout(optionBytes, offset, length));
                        offset += length;
                        break;
                    // these fields aren't supported because they're still considered
                    //  experimental in their respecive RFC specifications
                    case OptionTypes.POConnectionPermitted:
                    case OptionTypes.POServiceProfile:
                    case OptionTypes.ConnectionCount:
                    case OptionTypes.ConnectionCountNew:
                    case OptionTypes.ConnectionCountEcho:
                    case OptionTypes.QuickStartResponse:
                        throw new NotSupportedException("Option: " + type +
                                                        " is not supported because its RFC specification is still experimental");
                    // add more options types here
                    default:
                        throw new NotImplementedException("Option: " + type + " not supported in Packet.Net yet");
                }
            }

            return retval;
        }

        /// <summary cref="Packet.ToString(StringOutputType)" />
        public override String ToString(StringOutputType outputFormat)
        {
            var buffer = new StringBuilder();
            String color = "";
            String colorEscape = "";

            if (outputFormat == StringOutputType.Colored || outputFormat == StringOutputType.VerboseColored)
            {
                color = this.Color;
                colorEscape = AnsiEscapeSequences.Reset;
            }

            switch (outputFormat)
            {
                case StringOutputType.Normal:
                case StringOutputType.Colored:
                {
                    // build flagstring
                    String flags = "{";
                    if (this.Urg)
                        flags += "urg[0x" + Convert.ToString(this.UrgentPointer, 16) + "]|";
                    if (this.Ack)
                        flags += "ack[" + this.AcknowledgmentNumber + " (0x" +
                                 Convert.ToString(this.AcknowledgmentNumber, 16) + ")]|";
                    if (this.Psh)
                        flags += "psh|";
                    if (this.Rst)
                        flags += "rst|";
                    if (this.Syn)
                        flags += "syn[0x" + Convert.ToString(this.SequenceNumber, 16) + "," + this.SequenceNumber +
                                 "]|";
                    flags = flags.TrimEnd('|');
                    flags += "}";

                    // build the output string
                    buffer.AppendFormat("{0}[TCPPacket: SourcePort={2}, DestinationPort={3}, Flags={4}]{1}",
                        color,
                        colorEscape, this.SourcePort, this.DestinationPort,
                        flags);
                    break;
                }
                case StringOutputType.Verbose:
                case StringOutputType.VerboseColored:
                {
                    // collect the properties and their value
                    Dictionary<String, String> properties = new Dictionary<String, String>
                    {
                        {"source port", this.SourcePort.ToString()},
                        {"destination port", this.DestinationPort.ToString()},
                        {"sequence number", this.SequenceNumber + " (0x" + this.SequenceNumber.ToString("x") + ")"},
                        {
                            "acknowledgement number",
                            this.AcknowledgmentNumber + " (0x" + this.AcknowledgmentNumber.ToString("x") + ")"
                        },
                        // TODO: Implement a HeaderLength property for TCPPacket
                        //properties.Add("header length", HeaderLength.ToString());
                        {"flags", "(0x" + this.AllFlags.ToString("x") + ")"}
                    };
                    String flags = Convert.ToString(this.AllFlags, 2).PadLeft(8, '0');
                    properties.Add("", flags[0] + "... .... = [" + flags[0] + "] congestion window reduced");
                    properties.Add(" ", "." + flags[1] + ".. .... = [" + flags[1] + "] ECN - echo");
                    properties.Add("  ", ".." + flags[2] + ". .... = [" + flags[2] + "] urgent");
                    properties.Add("   ", "..." + flags[3] + " .... = [" + flags[3] + "] acknowledgement");
                    properties.Add("    ", ".... " + flags[4] + "... = [" + flags[4] + "] push");
                    properties.Add("     ", ".... ." + flags[5] + ".. = [" + flags[5] + "] reset");
                    properties.Add("      ", ".... .." + flags[6] + ". = [" + flags[6] + "] syn");
                    properties.Add("       ", ".... ..." + flags[7] + " = [" + flags[7] + "] fin");
                    properties.Add("window size", this.WindowSize.ToString());
                    properties.Add("checksum",
                        "0x" + this.Checksum + " [" + (this.ValidChecksum ? "valid" : "invalid") + "]");
                    properties.Add("options",
                        "0x" + BitConverter.ToString(this.Options).Replace("-", "").PadLeft(12, '0'));
                    var parsedOptions = this.OptionsCollection;
                    if (parsedOptions != null)
                    {
                        for (Int32 i = 0; i < parsedOptions.Count; i++)
                        {
                            properties.Add("option" + (i + 1), parsedOptions[i].ToString());
                        }
                    }

                    // calculate the padding needed to right-justify the property names
                    Int32 padLength = RandomUtils.LongestStringLength(new List<String>(properties.Keys));

                    // build the output string
                    buffer.AppendLine("TCP:  ******* TCP - \"Transmission Control Protocol\" - offset=? length=" +
                                      this.TotalPacketLength);
                    buffer.AppendLine("TCP:");
                    foreach (var property in properties)
                    {
                        if (property.Key.Trim() != "")
                        {
                            buffer.AppendLine("TCP: " + property.Key.PadLeft(padLength) + " = " + property.Value);
                        }
                        else
                        {
                            buffer.AppendLine("TCP: " + property.Key.PadLeft(padLength) + "   " + property.Value);
                        }
                    }

                    buffer.AppendLine("TCP:");
                    break;
                }
            }

            // append the base class output
            buffer.Append(base.ToString(outputFormat));

            return buffer.ToString();
        }

        /// <summary>
        ///     Create a randomized tcp packet with the given ip version
        /// </summary>
        /// <returns>
        ///     A <see cref="Packet" />
        /// </returns>
        public static TcpPacket RandomPacket()
        {
            var rnd = new Random();

            // create a randomized TcpPacket
            var srcPort = (UInt16) rnd.Next(UInt16.MinValue, UInt16.MaxValue);
            var dstPort = (UInt16) rnd.Next(UInt16.MinValue, UInt16.MaxValue);
            var tcpPacket = new TcpPacket(srcPort, dstPort);

            return tcpPacket;
        }

        /// <summary>
        ///     Contains the Options list attached to the TCP header
        /// </summary>
        public List<Option> OptionsCollection => this.ParseOptions(this.Options);
    }
}