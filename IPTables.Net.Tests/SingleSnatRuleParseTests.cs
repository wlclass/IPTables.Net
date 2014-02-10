﻿using System;
using IPTables.Net.Iptables;
using NUnit.Framework;

namespace IPTables.Net.Tests
{
    [TestFixture]
    internal class SingleSnatRuleParseTests
    {
        [Test]
        public void TestSnatSingleSource()
        {
            String rule = "-A POSTROUTING -t nat -s 1.1.1.1/24 -j SNAT --to-source 2.2.2.2";
            String chain;

            IpTablesRule irule = IpTablesRule.Parse(rule, null, null);

            Assert.AreEqual(rule, irule.GetFullCommand());
        }

        [Test]
        public void TestSnatRangeSourceAndEquality()
        {
            String rule = "-A POSTROUTING -t nat -s 1.1.1.1/24 -j SNAT --to-source 2.2.2.1-2.2.2.250";
            String chain;

            IpTablesRule irule1 = IpTablesRule.Parse(rule, null, null);
            IpTablesRule irule2 = IpTablesRule.Parse(rule, null, null);

            Assert.AreEqual(irule1, irule2);
            Assert.AreEqual(rule, irule1.GetFullCommand());
            Assert.AreEqual(rule, irule2.GetFullCommand());
        }
    }
}