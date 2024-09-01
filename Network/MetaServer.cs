/*
 * Copyright (c) 2024 Christopher Hayes
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
using CrossfireRPG.ServerInterface.Definitions;
using CrossfireRPG.Utility.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace CrossfireRPG.ServerInterface.Network
{
    public class MetaServer
    {
        public static readonly string[] DefaultMetaServerURLs = new string[]
        {
            @"http://crossfire.real-time.com/metaserver2/meta_client.php",
            @"http://metaserver.eu.cross-fire.org/meta_client.php",
            @"http://metaserver.us.cross-fire.org/meta_client.php",
        };

        public static Logger Logger { get; } = new Logger(nameof(MetaServer));

        public string HostName { get; set; }
        public int Port { get; set; } = Config.CSPORT;
        public string HtmlComment { get; set; }
        public string TextComment { get; set; }
        public string ArchBase { get; set; }
        public string MapBase { get; set; }
        public string CodeBase { get; set; }
        public int NumPlayers { get; set; }
        public int InBytes { get; set; }
        public int OutBytes { get; set; }
        public string Uptime { get; set; }
        public string Version { get; set; }
        public string SCVersion { get; set; }
        public string CSVersion { get; set; }
        public string LastUpdate { get; set; }
        public string Flags { get; set; }

        public static List<MetaServer> LoadMetaServers()
        {
            return LoadMetaServers(DefaultMetaServerURLs);
        }

        public static List<MetaServer> LoadMetaServers(string[] URLs)
        {
            var metaServers = new List<MetaServer>();

            foreach (var URL in URLs)
            {
                MetaServer metaServer = null;
                string webData = null;
                string line;
                int lineNumber = 0;

                Logger.Info($"Metaserver: {URL}");

                try
                {
                    using (var client = new WebClient())
                        webData = client.DownloadString(URL);
                }
                catch (WebException e)
                {
                    Logger.Error("Failed to get {0}: {1}", URL, e.Message);
                    continue;
                }

                using (var reader = new StringReader(webData))
                {
                    while ((line = reader.ReadLine()) != null)
                    {
                        lineNumber++;

                        Logger.Debug("{0}: {1}", lineNumber, line);

                        if (line == "START_SERVER_DATA")
                        {
                            if (metaServer != null)
                                Logger.Warning("missing START_SERVER_DATA, line {0}", lineNumber);

                            metaServer = new MetaServer();
                            continue;
                        }

                        if (metaServer == null)
                            break;

                        if (line == "END_SERVER_DATA")
                        {
                            if (string.IsNullOrWhiteSpace(metaServer.HostName))
                            {
                                metaServer = null;
                                continue;
                            }

                            if (metaServers.Any(x => x.HostName == metaServer.HostName))
                            {
                                metaServer = null;
                                continue;
                            }

                            metaServers.Add(metaServer);
                            metaServer = null;
                            continue;
                        }

                        var spl = line.Split(new char[] { '=' }, 2);
                        if ((spl == null) || (spl.Length != 2))
                            continue;

                        var key = spl[0].Trim();
                        var value = spl[1].Trim();

                        switch (key)
                        {
                            case "hostname": metaServer.HostName = value; break;
                            case "port": metaServer.Port = int.Parse(value); break;
                            case "html_comment": metaServer.HtmlComment = value; break;
                            case "text_comment": metaServer.TextComment = value; break;
                            case "archbase": metaServer.ArchBase = value; break;
                            case "mapbase": metaServer.MapBase = value; break;
                            case "codebase": metaServer.CodeBase = value; break;
                            case "num_players": metaServer.NumPlayers = int.Parse(value); break;
                            case "in_bytes": metaServer.InBytes = int.Parse(value); break;
                            case "out_bytes": metaServer.OutBytes = int.Parse(value); break;
                            case "uptime": metaServer.Uptime = value; break;
                            case "version": metaServer.Version = value; break;
                            case "sc_version": metaServer.SCVersion = value; break;
                            case "cs_version": metaServer.CSVersion = value; break;
                            case "last_update": metaServer.LastUpdate = value; break;
                            case "flags": metaServer.Flags = value; break;

                            default:
                                Logger.Warning("Unknown key {0} value {1}, line {2}", key, value, lineNumber);
                                break;
                        }
                    }

                    if (metaServer != null)
                    {
                        Logger.Warning("missing END_SERVER_DATA, line {0}", lineNumber);
                    }
                }
            }

            return metaServers;
        }
    }
}
