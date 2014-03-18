﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IPTables.Net.Iptables.DataTypes;

namespace IPTables.Net.Iptables.Modules.Multiport
{
    public class MultiportModule : ModuleBase, IIpTablesModuleGod, IEquatable<MultiportModule>
    {
        private const String OptionPorts = "--ports";
        private const String OptionDestinationPorts = "--dports";
        private const String OptionSourcePorts = "--sports";
        private const String OptionDestinationPortsLong = "--destination-ports";
        private const String OptionSourcePortsLong = "--source-ports";

        public HashSet<PortOrRange> DestinationPorts = new HashSet<PortOrRange>();
        public HashSet<PortOrRange> Ports = new HashSet<PortOrRange>();
        public HashSet<PortOrRange> SourcePorts = new HashSet<PortOrRange>();

        public bool Equals(MultiportModule other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Ports.SetEquals(other.Ports) && DestinationPorts.SetEquals(other.DestinationPorts) &&
                   SourcePorts.SetEquals(other.SourcePorts);
        }

        public bool NeedsLoading
        {
            get { return true; }
        }

        int IIpTablesModuleInternal.Feed(RuleParser parser, bool not)
        {
            switch (parser.GetCurrentArg())
            {
                case OptionPorts:
                    Ports = ParseListOfPortOrRanges(parser.GetNextArg());
                    return 1;
                case OptionDestinationPorts:
                case OptionDestinationPortsLong:
                    DestinationPorts = ParseListOfPortOrRanges(parser.GetNextArg());
                    return 1;
                case OptionSourcePorts:
                case OptionSourcePortsLong:
                    SourcePorts = ParseListOfPortOrRanges(parser.GetNextArg());
                    return 1;
            }

            return 0;
        }

        public String GetRuleString()
        {
            var sb = new StringBuilder();

            if (Ports.Count != 0)
            {
                if (sb.Length != 0)
                    sb.Append(" ");
                sb.Append(OptionPorts + " ");
                sb.Append(String.Join(",", Ports.Select(a => a.ToString()).ToArray()));
            }
            if (DestinationPorts.Count != 0)
            {
                if (sb.Length != 0)
                    sb.Append(" ");
                sb.Append(OptionDestinationPorts + " ");
                sb.Append(String.Join(",", DestinationPorts.Select(a => a.ToString()).ToArray()));
            }
            if (SourcePorts.Count != 0)
            {
                if (sb.Length != 0)
                    sb.Append(" ");
                sb.Append(OptionSourcePorts + " ");
                sb.Append(String.Join(",", SourcePorts.Select(a => a.ToString()).ToArray()));
            }

            return sb.ToString();
        }

        private HashSet<PortOrRange> ParseListOfPortOrRanges(String csv)
        {
            var ret = new HashSet<PortOrRange>();
            foreach (string a in csv.Split(new[] {','}))
            {
                ret.Add(PortOrRange.Parse(a, ':'));
            }
            return ret;
        }

        public static IEnumerable<String> GetOptions()
        {
            var options = new List<string>
            {
                OptionDestinationPorts,
                OptionDestinationPortsLong,
                OptionPorts,
                OptionSourcePorts,
                OptionSourcePortsLong
            };
            return options;
        }

        public static ModuleEntry GetModuleEntry()
        {
            return GetModuleEntryInternal("multiport", typeof (MultiportModule), GetOptions);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((MultiportModule) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (Ports != null ? Ports.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (DestinationPorts != null ? DestinationPorts.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (SourcePorts != null ? SourcePorts.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}