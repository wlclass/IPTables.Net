﻿using System;
using System.Collections.Generic;
using System.Text;
using IPTables.Net.Iptables.Helpers;

namespace IPTables.Net.Iptables.Modules.Nfqueue
{
    public class NfqueueModule : ModuleBase, IEquatable<NfqueueModule>, IIpTablesModule
    {
        private const string OptionQueueNumber = "--queue-num";
        private const string OptionQueueBypass = "--queue-bypass";


        public int Num = 0;
        public bool Bypass = false;


        public NfqueueModule(int version) : base(version)
        {
        }

        public int Feed(CommandParser parser, bool not)
        {
            switch (parser.GetCurrentArg())
            {
                case OptionQueueNumber:
                    Num = int.Parse(parser.GetNextArg());
                    return 1;
                case OptionQueueBypass:
                    Bypass = true;
                    return 0;
            }

            return 0;
        }

        public bool NeedsLoading => false;

        public string GetRuleString()
        {
            var sb = new StringBuilder();

            if (Num != 0)
            {
                sb.Append(OptionQueueNumber + " ");
                sb.Append(Num);
            }

            if (Bypass)
            {
                if (sb.Length != 0) sb.Append(" ");
                sb.Append(OptionQueueBypass);
            }

            return sb.ToString();
        }

        public static HashSet<string> GetOptions()
        {
            var options = new HashSet<string>
            {
                OptionQueueNumber,
                OptionQueueBypass
            };
            return options;
        }

        public static ModuleEntry GetModuleEntry()
        {
            return GetTargetModuleEntryInternal("NFQUEUE", typeof(NfqueueModule), GetOptions);
        }

        public bool Equals(NfqueueModule other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Num == other.Num && Bypass.Equals(other.Bypass);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((NfqueueModule) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Num * 397) ^ Bypass.GetHashCode();
            }
        }
    }
}