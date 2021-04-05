﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SystemInteract;
using IPTables.Net.Exceptions;
using IPTables.Net.Iptables.Adapter.Client.Helper;
using IPTables.Net.Iptables.NativeLibrary;
using IPTables.Net.Netfilter;

namespace IPTables.Net.Iptables.Adapter.Client
{
    internal class IPTablesLibAdapterClient : IpTablesAdapterClientBase, IIPTablesAdapterClient
    {
        private readonly NetfilterSystem _system;
        private bool _inTransaction = false;
        protected Dictionary<String, IptcInterface> _interfaces = new Dictionary<String, IptcInterface>();
        private string _iptablesBinary;
        private int _ipVersion;

        public IPTablesLibAdapterClient(int ipVersion, NetfilterSystem system, String iptablesBinary)
        {
            _system = system;
            _iptablesBinary = iptablesBinary;
            _ipVersion = ipVersion;
        }

        private IptcInterface GetInterface(String table)
        {
            IptcInterface i;
            if (_interfaces.ContainsKey(table))
            {
                i = _interfaces[table];
            }
            else
            {
                i = new IptcInterface(table, _ipVersion, Log);
                _interfaces.Add(table, i);
            }
            Debug.Assert(i.IpVersion == _ipVersion);
            return i;
        }


        public override void DeleteRule(String table, String chainName, int position)
        {
            if (!_inTransaction)
            {
                //Revert to using IPTables Binary if non transactional
                IPTablesBinaryAdapterClient binaryClient = new IPTablesBinaryAdapterClient(_ipVersion, _system, _iptablesBinary);
                binaryClient.DeleteRule(table, chainName, position);
                return;
            }

            String command = "-D " + chainName + " " + position;
            if (!String.IsNullOrEmpty(table) && table != "filter")
            {
                command += " -t " + table;
            }

            var ipInterface = GetInterface(table);
            if (ipInterface.ExecuteCommand(_iptablesBinary + " " + command) != 1)
            {
                throw new IpTablesNetException(String.Format("Failed to delete rule \"{0}\" due to error: \"{1}\"", command, ipInterface.GetErrorString()));
            }
        }

        INetfilterChainSet INetfilterAdapterClient.ListRules(string table)
        {
            return ListRules(table);
        }

        public override void DeleteRule(IpTablesRule rule)
        {
            if (!_inTransaction)
            {
                //Revert to using IPTables Binary if non transactional
                IPTablesBinaryAdapterClient binaryClient = new IPTablesBinaryAdapterClient(_ipVersion, _system, _iptablesBinary);
                binaryClient.DeleteRule(rule);
                return;
            }

            String command = rule.GetActionCommand("-D");
            if (GetInterface(rule.Chain.Table).ExecuteCommand(_iptablesBinary + " " + command) != 1)
            {
                throw new IpTablesNetException(String.Format("Failed to delete rule \"{0}\" due to error: \"{1}\"", command, GetInterface(rule.Chain.Table).GetErrorString()));
            }
        }

        public override void InsertRule(IpTablesRule rule)
        {
            if (!_inTransaction)
            {
                //Revert to using IPTables Binary if non transactional
                IPTablesBinaryAdapterClient binaryClient = new IPTablesBinaryAdapterClient(_ipVersion, _system, _iptablesBinary);
                binaryClient.InsertRule(rule);
                return;
            }

            String command = rule.GetActionCommand("-I");
            if (GetInterface(rule.Chain.Table).ExecuteCommand(_iptablesBinary + " " + command) != 1)
            {
                throw new IpTablesNetException(String.Format("Failed to insert rule \"{0}\" due to error: \"{1}\"", command, GetInterface(rule.Chain.Table).GetErrorString()));
            }
        }

        public override void ReplaceRule(IpTablesRule rule)
        {
            if (!_inTransaction)
            {
                //Revert to using IPTables Binary if non transactional
                IPTablesBinaryAdapterClient binaryClient = new IPTablesBinaryAdapterClient(_ipVersion, _system, _iptablesBinary);
                binaryClient.ReplaceRule(rule);
                return;
            }

            String command = rule.GetActionCommand("-R");
            if (GetInterface(rule.Chain.Table).ExecuteCommand(_iptablesBinary + " " + command) != 1)
            {
                throw new IpTablesNetException(String.Format("Failed to replace rule \"{0}\" due to error: \"{1}\"", command, GetInterface(rule.Chain.Table).GetErrorString()));
            }
        }

        public override void AddRule(IpTablesRule rule)
        {
            if (!_inTransaction)
            {
                //Revert to using IPTables Binary if non transactional
                IPTablesBinaryAdapterClient binaryClient = new IPTablesBinaryAdapterClient(_ipVersion, _system, _iptablesBinary);
                binaryClient.AddRule(rule);
                return;
            }

            String command = rule.GetActionCommand("-A");
            if (GetInterface(rule.Chain.Table).ExecuteCommand(_iptablesBinary + " " + command) != 1)
            {
                throw new IpTablesNetException(String.Format("Failed to add rule \"{0}\" due to error: \"{1}\"", command, GetInterface(rule.Chain.Table).GetErrorString()));
            }
        }



        public void AddRule(String command)
        {
            var table = ExtractTable(command);
            var iface = GetInterface(table);
            if (iface.ExecuteCommand(_iptablesBinary + " " + command) != 1)
            {
                throw new IpTablesNetException(String.Format("Failed to add rule \"{0}\" due to error: \"{1}\"", command, iface.GetErrorString()));
            }
        }

        public Version GetIptablesVersion()
        {
            IPTablesBinaryAdapterClient binaryClient = new IPTablesBinaryAdapterClient(_ipVersion, _system, _iptablesBinary);
            return binaryClient.GetIptablesVersion();
        }

        public override bool HasChain(string table, string chainName)
        {
            if (_inTransaction)
            {
                if (GetInterface(table).HasChain(chainName))
                {
                    return true;
                }
                return false;
            }

            IPTablesBinaryAdapterClient binaryClient = new IPTablesBinaryAdapterClient(_ipVersion, _system, _iptablesBinary);
            return binaryClient.HasChain(table, chainName);
        }

        public override void AddChain(string table, string chainName)
        {
            Debug.Assert(chainName != null);
            if (!IpTablesChain.ValidateChainName(chainName))
            {
                throw new IpTablesNetException(String.Format("Failed to add chain \"{0}\" to table \"{1}\" due to validation error", chainName, table));
            }
            if (!_inTransaction)
            {
                //Revert to using IPTables Binary if non transactional
                IPTablesBinaryAdapterClient binaryClient = new IPTablesBinaryAdapterClient(_ipVersion, _system, _iptablesBinary);
                binaryClient.AddChain(table, chainName);
                return;
            }

            if (!GetInterface(table).AddChain(chainName))
            {
                throw new IpTablesNetException(String.Format("Failed to add chain \"{0}\" to table \"{1}\" due to error: \"{2}\"", chainName, table, GetInterface(table).GetErrorString()));
            }
        }

        public override void DeleteChain(string table, string chainName, bool flush = false)
        {
            if (_inTransaction)
            {
                if (flush)
                {
                    GetInterface(table).FlushChain(chainName);
                }
                GetInterface(table).DeleteChain(chainName);
                return;
            }

            IPTablesBinaryAdapterClient binaryClient = new IPTablesBinaryAdapterClient(_ipVersion, _system, _iptablesBinary);
            binaryClient.DeleteChain(table, chainName, flush);
        }

        public override IpTablesChainSet ListRules(String table)
        {
            IpTablesChainSet chains = new IpTablesChainSet(_ipVersion);
            
            var ipc = GetInterface(table);
            Debug.Assert(ipc != null, "Unable to get interface for "+table);

            foreach (String chain in ipc.GetChains())
            {
                var newChain = chains.AddChain(chain, table, _system);
                Debug.Assert(newChain.IpVersion == _ipVersion);
            }

            Debug.Assert(_ipVersion == chains.IpVersion);
            foreach (var chain in chains)
            {
                foreach (var ipcRule in ipc.GetRules(chain.Name))
                {
                    String rule = ipc.GetRuleString(chain.Name, ipcRule);
                    if (rule == null)
                    {
                        throw new IpTablesNetException("Unable to get string version of rule");
                    }
                    chains.AddRule(IpTablesRule.Parse(rule, _system, chains, _ipVersion, table));
                }
            }   

            return chains;
        }

        public override List<string> GetChains(String table)
        {
            var ipc = GetInterface(table);
            List<String> ret = new List<string>();
            foreach (String chain in ipc.GetChains())
            {
                ret.Add(chain);
            }
            return ret;
        }

        public override void StartTransaction()
        {
            if (_inTransaction)
            {
                throw new IpTablesNetException("IPTables transaction already started");
            }
            _inTransaction = true;
        }

        public void EndTransactionCommit(IEnumerable<string> tableCommitOrder)
        {
            if (!_inTransaction)
            {
                return;
            }

            if (tableCommitOrder == null) tableCommitOrder = _interfaces.Keys;

            IpTablesNetExceptionErrno ex = null;
            foreach (var table in tableCommitOrder)
            {
                if (!_interfaces.TryGetValue(table, out var tableInterface)) continue;
                if (!tableInterface.Commit())
                {
                    var errno = tableInterface.GetLastError();
                    //Attempt to complete all commits
                    ex = new IpTablesNetExceptionErrno(
                        String.Format("Failed commit to table \"{0}\" due to error: \"{1}\"", table,
                            tableInterface.GetErrorString()), errno);
                }
                tableInterface.Dispose();
            }
            _interfaces.Clear();
            _inTransaction = false;

            if (ex != null)
            {
                throw ex;
            }
        }


        public override void EndTransactionCommit()
        {
            EndTransactionCommit(null);
        }

        public override void EndTransactionRollback()
        {
            foreach (var i in _interfaces)
            {
                i.Value.Dispose();
            }
            _interfaces.Clear();
            _inTransaction = false;
        }

        ~IPTablesLibAdapterClient()
        {
            Dispose();
        }

        public override void Dispose()
        {
            foreach (var i in _interfaces)
            {
                i.Value.Dispose();
            }
            _interfaces.Clear();

            if (_inTransaction)
            {
                throw new IpTablesNetException("Transaction active, must be commited or rolled back.");
            }
        }
    }
}
