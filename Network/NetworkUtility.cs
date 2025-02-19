/*
 * Copyright (c) 2025 Christopher Hayes
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
using System;
using System.Net;

namespace CrossfireRPG.ServerInterface.Network
{
    public static partial class NetworkUtility
    {
        /* Examples of supported host strings:
         *   192.168.0.1
         *   192.168.0.1:80
         *   1.1
         *   example.com
         *   example.com:80
         *   ::1
         *   [::1]
         *   [::1]:80
         *   a:b::c:d
         *   [a:b::c:d]
         *   [a:b::c:d]:80
         *   [a:b::c:d/24%16]:80
         *
         * Note: If available, use IPEndPoint.TryParse()
         * https://learn.microsoft.com/en-us/dotnet/api/system.net.ipendpoint.tryparse
         */
        /// <summary>
        /// Parses an IPv4 or IPv6 address, or DNS name with an optional port
        /// (Formats: v4addr / v4addr:port / v6addr / [v6addr]:port / hostname / hostname:port)
        /// </summary>
        /// <param name="hostString">Host string to parse</param>
        /// <param name="hostNameType">Type of hostname</param>
        /// <param name="hostName">Parsed hostname</param>
        /// <param name="port">Parsed port, or -1 if no port</param>
        /// <returns>true if the host string was parsed</returns>
        public static bool TryParseHost(string hostString, out UriHostNameType hostNameType,
            out string hostName, out int port)
        {
            hostNameType = UriHostNameType.Unknown;
            hostName = string.Empty;
            port = -1;

            //Fail if no host string to parse
            if (string.IsNullOrWhiteSpace(hostString))
                return false;

            try
            {
                //Parse the host string as a URI, to get the host and port
                var uri = new Uri("tcp://" + hostString);

                switch (uri.HostNameType)
                {
                    case UriHostNameType.IPv4:
                    case UriHostNameType.IPv6:
                    case UriHostNameType.Dns:

                        //Catch case of (x, x.y, x.y.z) as IPV4
                        if ((uri.HostNameType == UriHostNameType.Dns) &&
                            (Uri.CheckHostName(hostString) == UriHostNameType.IPv4) &&
                            IPAddress.TryParse(hostString, out var address))
                        {
                            hostNameType = UriHostNameType.IPv4;
                            hostName = address.ToString();
                        }
                        else
                        {
                            hostNameType = uri.HostNameType;
                            hostName = uri.DnsSafeHost;
                        }

                        port = uri.Port;

                        return true;
                }
            }
            catch (UriFormatException) //Failed to parse as URI (eg: IPV6 without [])
            {
                //Parse the host string as an IP address, to get the host but no port
                if (IPAddress.TryParse(hostString, out var address))
                {
                    switch (address.AddressFamily)
                    {
                        case System.Net.Sockets.AddressFamily.InterNetwork:
                            hostNameType = UriHostNameType.IPv4;
                            hostName = address.ToString();
                            return true;

                        case System.Net.Sockets.AddressFamily.InterNetworkV6:
                            hostNameType = UriHostNameType.IPv6;
                            hostName = address.ToString();
                            return true;
                    }
                }
            }

            //Unable to parse host string
            return false;
        }

        /// <summary>
        /// Returns the hostname type of a given host string
        /// </summary>
        /// <param name="hostString">Host string to parse</param>
        /// <returns>hostname type</returns>
        public static UriHostNameType CheckHostType(string hostString)
        {
            if (TryParseHost(hostString, out var type, out _, out _))
                return type;

            return UriHostNameType.Unknown;
        }

        /// <summary>
        /// Returns if the hostname type of a given host string is IPv6
        /// </summary>
        /// <param name="hostString">Host string to parse</param>
        /// <returns>true if the host string is an IPv6 formatted address</returns>
        public static bool CheckHostIPv6(string hostString)
        {
            return CheckHostType(hostString) == UriHostNameType.IPv6;
        }

        /// <summary>
        /// Returns if the hostname type of a given host string is IPv4
        /// </summary>
        /// <param name="hostString">Host string to parse</param>
        /// <returns>true if the host string is an IPv4 formatted address</returns>
        public static bool CheckHostIPv4(string hostString)
        {
            return CheckHostType(hostString) == UriHostNameType.IPv4;
        }
    }
}
