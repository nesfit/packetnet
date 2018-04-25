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
 *  Copyright 2010 Evan Plaice <evanplaice@gmail.com>
 *  Copyright 2010 Chris Morgan <chmorgan@gmail.com>
 */

using System;
using System.Text;
using PacketDotNet.Utils;
using PacketDotNet.Utils.Conversion;

namespace PacketDotNet.LLDP
{
    /// <summary>
    ///     A Time to Live TLV
    ///     [TLV Type Length : 2][Mgmt Addr length : 1][Mgmt Addr Subtype : 1][Mgmt Addr : 1-31]
    ///     [Interface Subtype : 1][Interface number : 4][OID length : 1][OID : 0-128]
    /// </summary>
    [Serializable]
    public class ManagementAddress : TLV
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

        /// <summary>
        ///     Number of bytes in the AddressLength field
        /// </summary>
        private const Int32 MgmtAddressLengthLength = 1;

        /// <summary>
        ///     Number of bytes in the interface number subtype field
        /// </summary>
        private const Int32 InterfaceNumberSubTypeLength = 1;

        /// <summary>
        ///     Number of bytes in the interface number field
        /// </summary>
        private const Int32 InterfaceNumberLength = 4;

        /// <summary>
        ///     Number of bytes in the object identifier length field
        /// </summary>
        private const Int32 ObjectIdentifierLengthLength = 1;

        /// <summary>
        ///     Maximum number of bytes in the object identifier field
        /// </summary>
        private const Int32 MaxObjectIdentifierLength = 128;

        #region Constructors

        /// <summary>
        ///     Creates a Management Address TLV
        /// </summary>
        /// <param name="bytes">
        ///     The LLDP Data unit being modified
        /// </param>
        /// <param name="offset">
        ///     The Management Address TLV's offset from the
        ///     origin of the LLDP
        /// </param>
        public ManagementAddress(Byte[] bytes, Int32 offset) :
            base(bytes, offset)
        {
            Log.Debug("");
        }

        /// <summary>
        ///     Creates a Management Address TLV and sets it value
        /// </summary>
        /// <param name="managementAddress">
        ///     The Management Address
        /// </param>
        /// <param name="interfaceSubType">
        ///     The Interface Numbering Sub Type
        /// </param>
        /// <param name="ifNumber">
        ///     The Interface Number
        /// </param>
        /// <param name="oid">
        ///     The Object Identifier
        /// </param>
        public ManagementAddress(NetworkAddress managementAddress,
            InterfaceNumbering interfaceSubType, UInt32 ifNumber,
            String oid)
        {
            Log.Debug("");

            // NOTE: We presume that the mgmt address length and the
            //       object identifier length are zero
            var length = TLVTypeLength.TypeLengthLength + MgmtAddressLengthLength +
                         InterfaceNumberSubTypeLength + InterfaceNumberLength +
                         ObjectIdentifierLengthLength;
            var bytes = new Byte[length];
            var offset = 0;
            this.TLVData = new ByteArraySegment(bytes, offset, length);

            // The lengths are both zero until the values are set
            this.AddressLength = 0;
            this.ObjIdLength = 0;

            this.Type = TLVTypes.ManagementAddress;

            this.MgmtAddress = managementAddress;
            this.InterfaceSubType = interfaceSubType;
            this.InterfaceNumber = ifNumber;
            this.ObjectIdentifier = oid;
        }

        #endregion

        #region Properties

        /// <value>
        ///     The Management Address Length
        /// </value>
        public Int32 AddressLength
        {
            get => this.TLVData.Bytes[this.ValueOffset];
            internal set => this.TLVData.Bytes[this.ValueOffset] = (Byte) value;
        }

        /// <value>
        ///     The Management Address Subtype
        ///     Forward to the MgmtAddress instance
        /// </value>
        public AddressFamily AddressSubType => this.MgmtAddress.AddressFamily;

        /// <value>
        ///     The Management Address
        /// </value>
        public NetworkAddress MgmtAddress
        {
            get
            {
                Int32 offset = this.ValueOffset + MgmtAddressLengthLength;

                return new NetworkAddress(this.TLVData.Bytes, offset, this.AddressLength);
            }

            set
            {
                var valueLength = value.Length;
                var valueBytes = value.Bytes;

                // is the new address the same size as the old address?
                if (this.AddressLength != valueLength)
                {
                    // need to resize the tlv and shift data fields down
                    var newLength = TLVTypeLength.TypeLengthLength + MgmtAddressLengthLength +
                                    valueLength +
                                    InterfaceNumberSubTypeLength +
                                    InterfaceNumberLength +
                                    ObjectIdentifierLengthLength + this.ObjIdLength;

                    var newBytes = new Byte[newLength];

                    Int32 headerLength = TLVTypeLength.TypeLengthLength + MgmtAddressLengthLength;
                    Int32 oldStartOfAfterData = this.ValueOffset + MgmtAddressLengthLength + this.AddressLength;
                    Int32 newStartOfAfterData = TLVTypeLength.TypeLengthLength + MgmtAddressLengthLength + value.Length;
                    Int32 afterDataLength = InterfaceNumberSubTypeLength + InterfaceNumberLength +
                                            ObjectIdentifierLengthLength + this.ObjIdLength;

                    // copy the data before the mgmt address
                    Array.Copy(this.TLVData.Bytes, this.TLVData.Offset,
                        newBytes, 0,
                        headerLength);

                    // copy the data over after the mgmt address over
                    Array.Copy(this.TLVData.Bytes, oldStartOfAfterData,
                        newBytes, newStartOfAfterData,
                        afterDataLength);

                    var offset = 0;
                    this.TLVData = new ByteArraySegment(newBytes, offset, newLength);

                    // update the address length field
                    this.AddressLength = valueLength;
                }

                // copy the new address into the appropriate position in the byte[]
                Array.Copy(valueBytes, 0, this.TLVData.Bytes, this.ValueOffset + MgmtAddressLengthLength,
                    valueLength);
            }
        }

        /// <value>
        ///     Interface Number Sub Type
        /// </value>
        public InterfaceNumbering InterfaceSubType
        {
            get => (InterfaceNumbering) this.TLVData.Bytes[
                this.ValueOffset + MgmtAddressLengthLength + this.MgmtAddress.Length];

            set => this.TLVData.Bytes[this.ValueOffset + MgmtAddressLengthLength + this.MgmtAddress.Length] =
                (Byte) value;
        }

        private Int32 InterfaceNumberOffset => this.ValueOffset + MgmtAddressLengthLength + this.AddressLength +
                                               InterfaceNumberSubTypeLength;

        /// <value>
        ///     Interface Number
        /// </value>
        public UInt32 InterfaceNumber
        {
            get => EndianBitConverter.Big.ToUInt32(this.TLVData.Bytes, this.InterfaceNumberOffset);

            set => EndianBitConverter.Big.CopyBytes(value, this.TLVData.Bytes, this.InterfaceNumberOffset);
        }

        private Int32 ObjIdLengthOffset => this.InterfaceNumberOffset + InterfaceNumberLength;

        /// <value>
        ///     Object ID Length
        /// </value>
        public Byte ObjIdLength
        {
            get => this.TLVData.Bytes[this.ObjIdLengthOffset];

            internal set => this.TLVData.Bytes[this.ObjIdLengthOffset] = value;
        }

        private Int32 ObjectIdentifierOffset => this.ObjIdLengthOffset + ObjectIdentifierLengthLength;

        /// <value>
        ///     Object ID
        /// </value>
        public String ObjectIdentifier
        {
            get => Encoding.UTF8.GetString(this.TLVData.Bytes, this.ObjectIdentifierOffset, this.ObjIdLength);

            set
            {
                Byte[] oid = Encoding.UTF8.GetBytes(value);

                // check for out-of-range sizes
                if (oid.Length > MaxObjectIdentifierLength)
                {
                    throw new ArgumentOutOfRangeException("ObjectIdentifier",
                        "length > maxObjectIdentifierLength of " + MaxObjectIdentifierLength);
                }

                // does the object identifier length match the existing one?
                if (this.ObjIdLength != oid.Length)
                {
                    var oldLength = TLVTypeLength.TypeLengthLength + MgmtAddressLengthLength + this.AddressLength +
                                    InterfaceNumberSubTypeLength + InterfaceNumberLength +
                                    ObjectIdentifierLengthLength;
                    var newLength = oldLength + oid.Length;

                    var newBytes = new Byte[newLength];

                    // copy the original bytes over
                    Array.Copy(this.TLVData.Bytes, this.TLVData.Offset,
                        newBytes, 0,
                        oldLength);

                    var offset = 0;
                    this.TLVData = new ByteArraySegment(newBytes, offset, newLength);

                    // update the length
                    this.ObjIdLength = (Byte) value.Length;
                }

                Array.Copy(oid, 0, this.TLVData.Bytes, this.ObjectIdentifierOffset,
                    oid.Length);
            }
        }

        /// <summary>
        ///     Convert this Management Address TLV to a string.
        /// </summary>
        /// <returns>
        ///     A human readable string
        /// </returns>
        public override String ToString()
        {
            return String.Format(
                "[ManagementAddress: AddressLength={0}, AddressSubType={1}, MgmtAddress={2}, InterfaceSubType={3}, InterfaceNumber={4}, ObjIdLength={5}, ObjectIdentifier={6}]",
                this.AddressLength, this.AddressSubType, this.MgmtAddress, this.InterfaceSubType, this.InterfaceNumber,
                this.ObjIdLength, this.ObjectIdentifier);
        }

        #endregion
    }
}