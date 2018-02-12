/*
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
 *  Copyright 2012 Chris Morgan <chmorgan@gmail.com>
 */

using System;
using System.Linq;
using PacketDotNet.Utils;

namespace PacketDotNet.Ieee80211
{
    /// <summary>
    ///     Information element, a variable-length component of management frames
    /// </summary>
    /// <exception cref='ArgumentException'>
    ///     Is thrown when an argument passed to a method is invalid.
    /// </exception>
    public class InformationElement
    {
        /// <summary>
        ///     Types of information elements
        /// </summary>
        public enum ElementId
        {
            /// <summary>
            ///     Assign an identifier to the service set
            /// </summary>
            ServiceSetIdentity = 0x00,

            /// <summary>
            ///     Specifies the data rates supported by the network
            /// </summary>
            SupportedRates = 0x01,

            /// <summary>
            ///     Provides the parameters necessary to join a frequency-hopping 802.11 network
            /// </summary>
            FhParamterSet = 0x02,

            /// <summary>
            ///     Direct-sequence 802.11 networks have one parameter, the channel number of the network
            /// </summary>
            DsParameterSet = 0x03,

            /// <summary>
            ///     Contention-free parameter. Transmitted in Becons by access points that support
            ///     contention-free operation.
            /// </summary>
            CfParameterSet = 0x04,

            /// <summary>
            ///     Indicates which stations have buffered traffic waiting to be picked up
            /// </summary>
            TrafficIndicationMap = 0x05,

            /// <summary>
            ///     Indicates the number of time units (TUs) between ATIM frames in an IBSS.
            /// </summary>
            IbssParameterSet = 0x06,

            /// <summary>
            ///     Specifies regulatory constraints stations must adhere to based on the country the network is operating in.
            /// </summary>
            Country = 0x07,

            /// <summary>
            ///     Specifies the hopping pattern of timeslots used in frequency hopping physical layers.
            /// </summary>
            HoppingParametersPattern = 0x08,

            /// <summary>
            ///     Specifies the hopping pattern table used in frequency hopping physical layers.
            /// </summary>
            HoppingPatternTable = 0x09,

            /// <summary>
            ///     Specifies the Ids of the information elements being requested in a
            ///     <see cref="PacketDotNet.Ieee80211.ProbeRequestFrame" />.
            /// </summary>
            Request = 0x0A,

            /// <summary>
            ///     Specifies the encrypted challenge text that stations must decrypt as part of the authentication process.
            /// </summary>
            ChallengeText = 0x10,

            /// <summary>
            ///     Specifies the difference between the regulatory maximum transmit power and any local constraint.
            /// </summary>
            PowerContstraint = 0x20,

            /// <summary>
            ///     Specifies the minimum and maximum transmit power a station is capable of.
            /// </summary>
            PowerCapability = 0x21,

            /// <summary>
            ///     Used to request radio link management information. This type of information element never has an associated value.
            /// </summary>
            TransmitPowerControlRequest = 0x22,

            /// <summary>
            ///     Radio link managment report used by stations to tune their transmission power.
            /// </summary>
            TransmitPowerControlReport = 0x23,

            /// <summary>
            ///     Specifies local constraints on the channels in use.
            /// </summary>
            SupportedChannels = 0x24,

            /// <summary>
            ///     Announces an impending change of channel for the network.
            /// </summary>
            ChannelSwitchAnnouncement = 0x25,

            /// <summary>
            ///     Requests a report on the state of the radio channel.
            /// </summary>
            MeasurementRequest = 0x26,

            /// <summary>
            ///     A report of on the status of the radio channel.
            /// </summary>
            MeasurementReport = 0x27,

            /// <summary>
            ///     Specifies the scheduling of temporary quiet periods on the channel.
            /// </summary>
            Quiet = 0x28,

            /// <summary>
            ///     Specifies the details the Dynamic Frequency Selection (DFS) algorithm in use in the IBSS.
            /// </summary>
            IbssDfs = 0x29,

            /// <summary>
            ///     Indicates whether or not the Extended Rate PHY is in use on the network at that time.
            /// </summary>
            ErpInformation = 0x2A,

            /// <summary>
            ///     Specifies a stations high throughput capabilities.
            /// </summary>
            HighThroughputCapabilities = 0x2d,

            /// <summary>
            ///     The erp information2.
            /// </summary>
            ErpInformation2 = 0x2F,

            /// <summary>
            ///     Specifies details of the Robust Security Network encryption in use on the network.
            /// </summary>
            RobustSecurityNetwork = 0x30,

            /// <summary>
            ///     Specifies more data rates supported by the network. This is identical to the Supported Rates element but it allows
            ///     for a longer value.
            /// </summary>
            ExtendedSupportedRates = 0x32,

            /// <summary>
            ///     Specified how high throughput capable stations will be operated in the network.
            /// </summary>
            HighThroughputInformation = 0x3d,

            /// <summary>
            ///     Specifies details of the WiFi Protected Access encryption in use on the network.
            /// </summary>
            WifiProtectedAccess = 0xD3,

            /// <summary>
            ///     Non standard information element implemented by the hardware vendor.
            /// </summary>
            VendorSpecific = 0xDD
        }

        /// <summary>
        ///     The length in bytes of the Information Element id field.
        /// </summary>
        public static readonly Int32 ElementIdLength = 1;

        /// <summary>
        ///     The index of the id field in an Information Element.
        /// </summary>
        public static readonly Int32 ElementIdPosition = 0;

        /// <summary>
        ///     The length in bytes of the Information Element length field.
        /// </summary>
        public static readonly Int32 ElementLengthLength = 1;

        /// <summary>
        ///     The index of the length field in an Information Element.
        /// </summary>
        public static readonly Int32 ElementLengthPosition;

        /// <summary>
        ///     The index of the first byte of the value field in an Information Element.
        /// </summary>
        public static readonly Int32 ElementValuePosition;

        private ByteArraySegment _bytes;

        static InformationElement()
        {
            ElementLengthPosition = ElementIdPosition + ElementIdLength;
            ElementValuePosition = ElementLengthPosition + ElementLengthLength;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PacketDotNet.Ieee80211.InformationElement" /> class.
        /// </summary>
        /// <param name='bas'>
        ///     The bytes of the information element. The Offset property should point to the first byte of the element, the Id
        ///     byte
        /// </param>
        public InformationElement(ByteArraySegment bas)
        {
            this._bytes = bas;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PacketDotNet.Ieee80211.InformationElement" /> class.
        /// </summary>
        /// <param name='id'>
        ///     Identifier.
        /// </param>
        /// <param name='value'>
        ///     Value.
        /// </param>
        /// <exception cref='ArgumentException'>
        ///     Is thrown when an argument passed to a method is invalid.
        /// </exception>
        public InformationElement(ElementId id, Byte[] value)
        {
            var ie = new Byte[ElementIdLength + ElementLengthLength + value.Length];
            this._bytes = new ByteArraySegment(ie);
            this.Id = id;
            this.Value = value;
        }

        /// <summary>
        ///     Gets the bytes.
        /// </summary>
        /// <value>
        ///     The bytes.
        /// </value>
        public Byte[] Bytes => this._bytes.ActualBytes();

        /// <summary>
        ///     Gets the length of the element including the Id and Length field
        /// </summary>
        /// <value>
        ///     The length of the element.
        /// </value>
        public Byte ElementLength => (Byte) (ElementIdLength + ElementLengthLength + this.ValueLength);

        /// <summary>
        ///     Gets or sets the identifier.
        /// </summary>
        /// <value>
        ///     The identifier.
        /// </value>
        public ElementId Id
        {
            get => (ElementId) this._bytes.Bytes[this._bytes.Offset + ElementIdPosition];
            set => this._bytes.Bytes[this._bytes.Offset + ElementIdPosition] = (Byte) value;
        }

        /// <summary>
        ///     Gets or sets the value of the element
        /// </summary>
        /// <value>
        ///     The value.
        /// </value>
        /// <exception cref='ArgumentException'>
        ///     Is thrown when the value is too large. Values are limited to a maximum size 255 bytes due the single
        ///     byte length field.
        /// </exception>
        public Byte[] Value
        {
            get
            {
                var valueArray = new Byte[this.ValueLength];
                Array.Copy(this._bytes.Bytes, this._bytes.Offset + ElementValuePosition,
                    valueArray, 0, this.ValueLength);
                return valueArray;
            }

            set
            {
                if (value.Length > Byte.MaxValue)
                {
                    throw new ArgumentException("The provided value is too long. Maximum allowed length is 255 bytes.");
                }

                //Decide if the current ByteArraySegement is big enough to hold the new info element
                Int32 newIeLength = ElementIdLength + ElementLengthLength + value.Length;
                if (this._bytes.Length < newIeLength)
                {
                    var newIe = new Byte[newIeLength];
                    newIe[ElementIdPosition] = this._bytes.Bytes[this._bytes.Offset + ElementIdPosition];
                    this._bytes = new ByteArraySegment(newIe);
                }

                Array.Copy(value, 0, this._bytes.Bytes, this._bytes.Offset + ElementValuePosition, value.Length);
                this._bytes.Length = newIeLength;
                this._bytes.Bytes[this._bytes.Offset + ElementLengthPosition] = (Byte) value.Length;
            }
        }

        /// <summary>
        ///     Gets the length.
        /// </summary>
        /// <value>
        ///     The length.
        /// </value>
        public Int32 ValueLength => Math.Min((this._bytes.Length - ElementValuePosition),
            this._bytes.Bytes[this._bytes.Offset + ElementLengthPosition]);

        /// <summary>
        ///     Determines whether the specified <see cref="System.Object" /> is equal to the current
        ///     <see cref="PacketDotNet.Ieee80211.InformationElement" />.
        /// </summary>
        /// <param name='obj'>
        ///     The <see cref="System.Object" /> to compare with the current
        ///     <see cref="PacketDotNet.Ieee80211.InformationElement" />.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the specified <see cref="System.Object" /> is equal to the current
        ///     <see cref="PacketDotNet.Ieee80211.InformationElement" />; otherwise, <c>false</c>.
        /// </returns>
        public override Boolean Equals(Object obj)
        {
            if (obj == null || this.GetType() != obj.GetType())
            {
                return false;
            }

            InformationElement ie = obj as InformationElement;
            return ((this.Id == ie.Id) && (this.Value.SequenceEqual(ie.Value)));
        }

        /// <summary>
        ///     Serves as a hash function for a <see cref="PacketDotNet.Ieee80211.InformationElement" /> object.
        /// </summary>
        /// <returns>
        ///     A hash code for this instance that is suitable for use in hashing algorithms and data structures such as
        ///     a hash table.
        /// </returns>
        public override Int32 GetHashCode()
        {
            return this.Id.GetHashCode() ^ this.Value.GetHashCode();
        }
    }
}