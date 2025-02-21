﻿using System;
using System.Collections.Generic;
using System.Text;
using IPTables.Net.Iptables.DataTypes;
using IPTables.Net.Iptables.Helpers;
using IPTables.Net.Iptables.Modules.Log;

namespace IPTables.Net.Iptables.Modules.Statistic
{
    public class StatisticModule : ModuleBase, IEquatable<StatisticModule>, IIpTablesModule
    {
        private const string OptionModeLong = "--mode";
        private const string OptionProbabilityLong = "--probability";
        private const string OptionEveryLong = "--every";
        private const string OptionPacketLong = "--packet";

        public enum Modes
        {
            Random,
            Nth
        }

        public Modes Mode;
        public ValueOrNot<uint> Every;
        public uint Packet;

        public ValueOrNot<float> Probability
        {
            get => new ValueOrNot<float>(Every.Value / 2147483648.0f, Every.Not);
            set => Every = new ValueOrNot<uint>((uint) Math.Ceiling(value.Value * 2147483648), value.Not);
        }

        public StatisticModule(int version)
            : base(version)
        {
        }

        public int Feed(CommandParser parser, bool not)
        {
            switch (parser.GetCurrentArg())
            {
                case OptionModeLong:
                    Mode = ParseMode(parser.GetNextArg());
                    return 1;
                case OptionProbabilityLong:
                    Probability = new ValueOrNot<float>(float.Parse(parser.GetNextArg()), not);
                    return 1;
                case OptionPacketLong:
                    Packet = uint.Parse(parser.GetNextArg());
                    return 1;
                case OptionEveryLong:
                    Every = new ValueOrNot<uint>(uint.Parse(parser.GetNextArg()), not);
                    return 1;
            }

            return 0;
        }

        public static Modes ParseMode(string mode)
        {
            switch (mode)
            {
                case "random":
                    return Modes.Random;
                case "nth":
                    return Modes.Nth;
                default:
                    throw new ArgumentException("Invalid argument: " + mode);
            }
        }

        public static string OutputMode(Modes mode)
        {
            switch (mode)
            {
                case Modes.Random:
                    return "random";
                case Modes.Nth:
                    return "nth";
                default:
                    throw new ArgumentException("Invalid argument: " + mode);
            }
        }

        public bool NeedsLoading => true;

        public string GetRuleString()
        {
            var sb = new StringBuilder();

            sb.Append(OptionModeLong + " " + OutputMode(Mode) + " ");

            switch (Mode)
            {
                case Modes.Nth:
                    sb.Append(Every.ToOption(OptionEveryLong) + " " + OptionPacketLong + " " + Packet);
                    break;
                case Modes.Random:
                    sb.Append(Probability.ToOption(OptionProbabilityLong));
                    break;
            }

            return sb.ToString();
        }

        public static HashSet<string> GetOptions()
        {
            var options = new HashSet<string>
            {
                OptionModeLong,
                OptionEveryLong,
                OptionPacketLong,
                OptionProbabilityLong
            };
            return options;
        }

        public static ModuleEntry GetModuleEntry()
        {
            return GetModuleEntryInternal("statistic", typeof(StatisticModule), GetOptions);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((StatisticModule) obj);
        }

        public bool Equals(StatisticModule other)
        {
            if (Mode != other.Mode) return false;
            if (Mode == Modes.Nth) return Every.Equals(other.Every);
            return Packet.Equals(other.Packet);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) Mode;
                hashCode = (hashCode * 397) ^ Every.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) Packet;
                return hashCode;
            }
        }
    }
}