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
 *  Copyright 2009 Chris Morgan <chmorgan@gmail.com>
 */

using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using PacketDotNet.ARP;
using PacketDotNet.Ieee8021Q;
using PacketDotNet.IP;
using PacketDotNet.LLDP;
using PacketDotNet.PPP;
using PacketDotNet.Utils;
using PacketDotNet.Utils.Conversion;

namespace PacketDotNet.Ethernet
{
    /// <summary>
    ///     See http://en.wikipedia.org/wiki/Ethernet#Ethernet_frame_types_and_the_EtherType_field
    /// </summary>
    [Serializable]
    public class EthernetPacket : InternetLinkLayerPacket
    {
#if DEBUG_PACKETDOTNET
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
#else
        // NOTE: No need to warn about lack of use, the compiler won't
        //       put any calls to 'log' here but we need 'log' to exist to compile
        private static readonly ILogInactive Log;
#endif
        /// <value>
        ///     Payload packet, overridden to set the 'Type' field based on
        ///     the type of packet being used here if the PayloadPacket is being set
        /// </value>
        public override Packet PayloadPacket
        {
            get => base.PayloadPacket;

            set
            {
                base.PayloadPacket = value;

                // set Type based on the type of the payload
                switch (value)
                {
                    case IPv4Packet _:
                        this.Type = EthernetPacketType.IpV4;
                        break;
                    case IPv6Packet _:
                        this.Type = EthernetPacketType.IpV6;
                        break;
                    case ARPPacket _:
                        this.Type = EthernetPacketType.Arp;
                        break;
                    case LLDPPacket _:
                        this.Type = EthernetPacketType.LLDP;
                        break;
                    case PPPoEPacket _:
                        this.Type = EthernetPacketType.PointToPointProtocolOverEthernetSessionStage;
                        break;
                    default:
                        this.Type = EthernetPacketType.None;
                        break;
                }
            }
        }

        /// <summary> MAC address of the host where the packet originated from.</summary>
        public virtual PhysicalAddress SourceHwAddress
        {
            get
            {
                Byte[] hwAddress = new Byte[EthernetFields.MacAddressLength];
                Array.Copy(this.HeaderByteArraySegment.Bytes,
                    this.HeaderByteArraySegment.Offset + EthernetFields.SourceMacPosition,
                    hwAddress, 0, hwAddress.Length);
                return new PhysicalAddress(hwAddress);
            }

            set
            {
                Byte[] hwAddress = value.GetAddressBytes();
                if (hwAddress.Length != EthernetFields.MacAddressLength)
                {
                    throw new InvalidOperationException("address length " + hwAddress.Length
                                                                          + " not equal to the expected length of "
                                                                          + EthernetFields.MacAddressLength);
                }

                Array.Copy(hwAddress, 0, this.HeaderByteArraySegment.Bytes,
                    this.HeaderByteArraySegment.Offset + EthernetFields.SourceMacPosition,
                    hwAddress.Length);
            }
        }

        /// <summary> MAC address of the host where the packet originated from.</summary>
        public virtual PhysicalAddress DestinationHwAddress
        {
            get
            {
                Byte[] hwAddress = new Byte[EthernetFields.MacAddressLength];
                Array.Copy(this.HeaderByteArraySegment.Bytes,
                    this.HeaderByteArraySegment.Offset + EthernetFields.DestinationMacPosition,
                    hwAddress, 0, hwAddress.Length);
                return new PhysicalAddress(hwAddress);
            }

            set
            {
                Byte[] hwAddress = value.GetAddressBytes();
                if (hwAddress.Length != EthernetFields.MacAddressLength)
                {
                    throw new InvalidOperationException("address length " + hwAddress.Length
                                                                          + " not equal to the expected length of "
                                                                          + EthernetFields.MacAddressLength);
                }

                Array.Copy(hwAddress, 0, this.HeaderByteArraySegment.Bytes,
                    this.HeaderByteArraySegment.Offset + EthernetFields.DestinationMacPosition,
                    hwAddress.Length);
            }
        }

        /// <value>
        ///     Type of packet that this ethernet packet encapsulates
        /// </value>
        public virtual EthernetPacketType Type
        {
            get => (EthernetPacketType) EndianBitConverter.Big.ToInt16(this.HeaderByteArraySegment.Bytes,
                this.HeaderByteArraySegment.Offset + EthernetFields.TypePosition);

            set
            {
                Int16 val = (Int16) value;
                EndianBitConverter.Big.CopyBytes(val, this.HeaderByteArraySegment.Bytes,
                    this.HeaderByteArraySegment.Offset + EthernetFields.TypePosition);
            }
        }

        /// <summary>
        ///     Construct a new ethernet packet from source and destination mac addresses
        /// </summary>
        public EthernetPacket(PhysicalAddress sourceHwAddress,
            PhysicalAddress destinationHwAddress,
            EthernetPacketType ethernetPacketType)
        {
            Log.Debug("");

            // allocate memory for this packet
            Int32 offset = 0;
            Int32 length = EthernetFields.HeaderLength;
            var headerBytes = new Byte[length];
            this.HeaderByteArraySegment = new ByteArraySegment(headerBytes, offset, length);

            // set the instance values
            this.SourceHwAddress = sourceHwAddress;
            this.DestinationHwAddress = destinationHwAddress;
            this.Type = ethernetPacketType;
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="bas">
        ///     A <see cref="ByteArraySegment" />
        /// </param>
        public EthernetPacket(ByteArraySegment bas)
        {
            Log.Debug("");

            // slice off the header portion
            this.HeaderByteArraySegment = new ByteArraySegment(bas)
            {
                Length = EthernetFields.HeaderLength
            };

            // parse the encapsulated bytes
            this.PayloadPacketOrData = ParseEncapsulatedBytes(this.HeaderByteArraySegment, this.Type);
        }

        /// <summary>
        ///     Used by the EthernetPacket constructor. Located here because the LinuxSLL constructor
        ///     also needs to perform the same operations as it contains an ethernet type
        /// </summary>
        /// <param name="header">
        ///     A <see cref="ByteArraySegment" />
        /// </param>
        /// <param name="type">
        ///     A <see cref="EthernetPacketType" />
        /// </param>
        /// <returns>
        ///     A <see cref="PacketOrByteArraySegment" />
        /// </returns>
        internal static PacketOrByteArraySegment ParseEncapsulatedBytes(ByteArraySegment header,
            EthernetPacketType type)
        {
            // slice off the payload
            var payload = header.EncapsulatedBytes();
            Log.DebugFormat("payload {0}", payload.ToString());

            var payloadPacketOrData = new PacketOrByteArraySegment();

            // parse the encapsulated bytes
            switch (type)
            {
                case EthernetPacketType.IpV4:
                    payloadPacketOrData.ThePacket = new IPv4Packet(payload);
                    break;
                case EthernetPacketType.IpV6:
                    payloadPacketOrData.ThePacket = new IPv6Packet(payload);
                    break;
                case EthernetPacketType.Arp:
                    payloadPacketOrData.ThePacket = new ARPPacket(payload);
                    break;
                case EthernetPacketType.LLDP:
                    payloadPacketOrData.ThePacket = new LLDPPacket(payload);
                    break;
                case EthernetPacketType.PointToPointProtocolOverEthernetSessionStage:
                    payloadPacketOrData.ThePacket = new PPPoEPacket(payload);
                    break;
                case EthernetPacketType.WakeOnLan:
                    payloadPacketOrData.ThePacket = new WakeOnLanPacket(payload);
                    break;
                case EthernetPacketType.VLanTaggedFrame:
                    payloadPacketOrData.ThePacket = new Ieee8021QPacket(payload);
                    break;
                default: // consider the sub-packet to be a byte array
                    payloadPacketOrData.TheByteArraySegment = payload;
                    break;
            }

            return payloadPacketOrData;
        }

        /// <summary> Fetch ascii escape sequence of the color associated with this packet type.</summary>
        public override String Color => AnsiEscapeSequences.DarkGray;

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
                    // build the output string
                    buffer.AppendFormat(
                        "{0}[EthernetPacket: SourceHwAddress={2}, DestinationHwAddress={3}, Type={4}]{1}",
                        color,
                        colorEscape,
                        HexPrinter.PrintMACAddress(this.SourceHwAddress),
                        HexPrinter.PrintMACAddress(this.DestinationHwAddress), this.Type.ToString());
                    break;
                case StringOutputType.Verbose:
                case StringOutputType.VerboseColored:
                    // collect the properties and their value
                    Dictionary<String, String> properties = new Dictionary<String, String>
                    {
                        {"destination", HexPrinter.PrintMACAddress(this.DestinationHwAddress)},
                        {"source", HexPrinter.PrintMACAddress(this.SourceHwAddress)},
                        {"type", this.Type + " (0x" + this.Type.ToString("x") + ")"}
                    };

                    // calculate the padding needed to right-justify the property names
                    Int32 padLength = RandomUtils.LongestStringLength(new List<String>(properties.Keys));

                    // build the output string
                    buffer.AppendLine("Eth:  ******* Ethernet - \"Ethernet\" - offset=? length=" +
                                      this.TotalPacketLength);
                    buffer.AppendLine("Eth:");
                    foreach (var property in properties)
                    {
                        buffer.AppendLine("Eth: " + property.Key.PadLeft(padLength) + " = " + property.Value);
                    }

                    buffer.AppendLine("Eth:");
                    break;
            }

            // append the base output
            buffer.Append(base.ToString(outputFormat));

            return buffer.ToString();
        }

        /// <summary>
        ///     Generate a random EthernetPacket
        ///     TODO: could improve this routine to set a random payload as well
        /// </summary>
        /// <returns>
        ///     A <see cref="EthernetPacket" />
        /// </returns>
        public static EthernetPacket RandomPacket()
        {
            var rnd = new Random();

            Byte[] srcPhysicalAddress = new Byte[EthernetFields.MacAddressLength];
            Byte[] dstPhysicalAddress = new Byte[EthernetFields.MacAddressLength];

            rnd.NextBytes(srcPhysicalAddress);
            rnd.NextBytes(dstPhysicalAddress);

            return new EthernetPacket(new PhysicalAddress(srcPhysicalAddress),
                new PhysicalAddress(dstPhysicalAddress),
                EthernetPacketType.None);
        }
    }
}