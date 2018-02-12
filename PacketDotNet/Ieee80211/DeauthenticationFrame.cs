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
 * Copyright 2012 Alan Rushforth <alan.rushforth@gmail.com>
 */

using System;
using System.Net.NetworkInformation;
using PacketDotNet.Utils;
using PacketDotNet.Utils.Conversion;

namespace PacketDotNet.Ieee80211
    {
        /// <summary>
        /// Deauthentication frame.
        /// </summary>
        public class DeauthenticationFrame : ManagementFrame
        {
            private class DeauthenticationFields
            {
                public static readonly Int32 ReasonCodeLength = 2;

                public static readonly Int32 ReasonCodePosition;

                static DeauthenticationFields()
                {
                    ReasonCodePosition = MacFields.SequenceControlPosition + MacFields.SequenceControlLength;
                }
            }
   
            /// <summary>
            /// Gets the reason for deauthentication.
            /// </summary>
            /// <value>
            /// The reason.
            /// </value>
            public ReasonCode Reason { get; set;}
            
            private ReasonCode ReasonBytes
            {
                get
                {
					if(this.HeaderByteArraySegment.Length >= (DeauthenticationFields.ReasonCodePosition + DeauthenticationFields.ReasonCodeLength))
					{
						return (ReasonCode)EndianBitConverter.Little.ToUInt16 (this.HeaderByteArraySegment.Bytes, this.HeaderByteArraySegment.Offset + DeauthenticationFields.ReasonCodePosition);
					}
					else
					{
						return ReasonCode.Unspecified;
					}
                }
                
                set => EndianBitConverter.Little.CopyBytes ((UInt16)value, this.HeaderByteArraySegment.Bytes, this.HeaderByteArraySegment.Offset + DeauthenticationFields.ReasonCodePosition);
            }

            /// <summary>
            /// Gets the size of the frame.
            /// </summary>
            /// <value>
            /// The size of the frame.
            /// </value>
            public override Int32 FrameSize => (MacFields.FrameControlLength +
                                              MacFields.DurationIDLength +
                                              (MacFields.AddressLength * 3) +
                                              MacFields.SequenceControlLength +
                                              DeauthenticationFields.ReasonCodeLength);

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="bas">
            /// A <see cref="ByteArraySegment"/>
            /// </param>
            public DeauthenticationFrame (ByteArraySegment bas)
            {
                this.HeaderByteArraySegment = new ByteArraySegment (bas);

                this.FrameControl = new FrameControlField (this.FrameControlBytes);
                this.Duration = new DurationField (this.DurationBytes);
                this.DestinationAddress = this.GetAddress (0);
                this.SourceAddress = this.GetAddress (1);
                this.BssId = this.GetAddress (2);
                this.SequenceControl = new SequenceControlField (this.SequenceControlBytes);
                this.Reason = this.ReasonBytes;

                this.HeaderByteArraySegment.Length = this.FrameSize;
            }
            
            /// <summary>
            /// Initializes a new instance of the <see cref="PacketDotNet.Ieee80211.DeauthenticationFrame"/> class.
            /// </summary>
            /// <param name='SourceAddress'>
            /// Source address.
            /// </param>
            /// <param name='DestinationAddress'>
            /// Destination address.
            /// </param>
            /// <param name='BssId'>
            /// Bss identifier (MAC Address of the Access Point).
            /// </param>
            public DeauthenticationFrame (PhysicalAddress SourceAddress,
                                          PhysicalAddress DestinationAddress,
                                          PhysicalAddress BssId)
            {
                this.FrameControl = new FrameControlField ();
                this.Duration = new DurationField ();
                this.DestinationAddress = DestinationAddress;
                this.SourceAddress = SourceAddress;
                this.BssId = BssId;
                this.SequenceControl = new SequenceControlField ();
                
                this.FrameControl.SubType = FrameControlField.FrameSubTypes.ManagementDeauthentication;
            }
            
            /// <summary>
            /// Writes the current packet properties to the backing ByteArraySegment.
            /// </summary>
            public override void UpdateCalculatedValues ()
            {
                if ((this.HeaderByteArraySegment == null) || (this.HeaderByteArraySegment.Length > (this.HeaderByteArraySegment.BytesLength - this.HeaderByteArraySegment.Offset)) || (this.HeaderByteArraySegment.Length < this.FrameSize))
                {
                    this.HeaderByteArraySegment = new ByteArraySegment (new Byte[this.FrameSize]);
                }
                
                this.FrameControlBytes = this.FrameControl.Field;
                this.DurationBytes = this.Duration.Field;
                this.SetAddress (0, this.DestinationAddress);
                this.SetAddress (1, this.SourceAddress);
                this.SetAddress (2, this.BssId);
                this.SequenceControlBytes = this.SequenceControl.Field;
                this.ReasonBytes = this.Reason;

                this.HeaderByteArraySegment.Length = this.FrameSize;
            }

        } 
    }
