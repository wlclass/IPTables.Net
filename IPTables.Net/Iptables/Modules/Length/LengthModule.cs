﻿using System;
using System.Collections.Generic;
using System.Text;
using IPTables.Net.Iptables.DataTypes;
using IPTables.Net.Iptables.Helpers;
using IPTables.Net.Iptables.Modules.Comment;

namespace IPTables.Net.Iptables.Modules.Length
{
    public class LengthModule : ModuleBase, IEquatable<LengthModule>, IIpTablesModule
    {
        private const string OptionLengthLong = "--length";

        public ValueOrNot<PortOrRange> Length;

        public LengthModule(int version) : base(version)
        {
        }

        public bool Equals(LengthModule other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Length.Equals(other.Length);
        }

        public int Feed(CommandParser parser, bool not)
        {
            switch (parser.GetCurrentArg())
            {
                case OptionLengthLong:
                    Length = new ValueOrNot<PortOrRange>(PortOrRange.Parse(parser.GetNextArg(), ':'), not);
                    return 1;
            }

            return 0;
        }

        public bool NeedsLoading => true;

        public string GetRuleString()
        {
            return Length.ToOption(OptionLengthLong);
        }

        public static HashSet<string> GetOptions()
        {
            var options = new HashSet<string>
            {
                OptionLengthLong
            };
            return options;
        }

        public static ModuleEntry GetModuleEntry()
        {
            return GetModuleEntryInternal("length", typeof(LengthModule), GetOptions);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((LengthModule) obj);
        }

        public override int GetHashCode()
        {
            return Length.GetHashCode();
        }
    }
}