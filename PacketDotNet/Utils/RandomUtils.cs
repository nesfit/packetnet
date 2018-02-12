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

using System;
using System.Collections.Generic;
using PacketDotNet.IP;

namespace PacketDotNet.Utils
{
    /// <summary>
    /// Random utility methods
    /// </summary>
    public class RandomUtils
    {
        /// <summary>
        /// Generate a random ip address
        /// </summary>
        /// <param name="version">
        /// A <see cref="IpVersion"/>
        /// </param>
        /// <returns>
        /// A <see cref="System.Net.IPAddress"/>
        /// </returns>
        public static System.Net.IPAddress GetIPAddress(IpVersion version)
        {
            var rnd = new Random();
            Byte[] randomAddressBytes;

            switch (version)
            {
                case IpVersion.IPv4:
                    randomAddressBytes = new Byte[IPv4Fields.AddressLength];
                    rnd.NextBytes(randomAddressBytes);
                    break;
                case IpVersion.IPv6:
                    randomAddressBytes = new Byte[IPv6Fields.AddressLength];
                    rnd.NextBytes(randomAddressBytes);
                    break;
                default:
                    throw new InvalidOperationException("Unknown version of " + version);
            }

            return new System.Net.IPAddress(randomAddressBytes);
        }

        /// <summary>
        /// Get the length of the longest string in a list of strings
        /// </summary>
        /// <param name="stringsList">
        /// A <see cref="T:List{System.String}"/>
        /// </param>
        /// <returns>
        /// A <see cref="System.Int32"/>
        /// </returns>
        public static Int32 LongestStringLength(List<String> stringsList)
        {
            String longest="";

            foreach(String L in stringsList)
            {
                if (L.Length > longest.Length)
                {
                    longest = L;
                }
            }
            return longest.Length;
        }
    }
}
