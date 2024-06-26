﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using FlowGraphBase.Node;
using FlowGraphBase.Process;

namespace FlowGraphBase
{
    public class SequenceBase : INotifyPropertyChanged
    {
        static int _newId;

        protected readonly Dictionary<int, SequenceNode> SequenceNodes = new Dictionary<int, SequenceNode>();

        private string _name, _description;

        public string Name
        {
            get => _name;
            set
            {
                if (string.Equals(_name, value) == false)
                {
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                if (string.Equals(_description, value) == false)
                {
                    _description = value;
                    OnPropertyChanged("Description");
                }
            }
        }

        public int Id
        {
            get;
            private set;
        }

        public IEnumerable<SequenceNode> Nodes => SequenceNodes.Values.ToArray();

        public int NodeCount => SequenceNodes.Values.Count;

        protected SequenceBase(string name)
        {
            Name = name;
            Id = _newId++;
        }
        protected SequenceBase(XmlNode node)
        {
            Load(node);
        }

        public SequenceNode GetNodeById(int id)
        {
            return SequenceNodes[id];
        }

        public void AddNode(SequenceNode node)
        {
            SequenceNodes.Add(node.Id, node);
        }

        public void RemoveNode(SequenceNode node)
        {
            SequenceNodes.Remove(node.Id);
        }

        public void RemoveAllNodes()
        {
            SequenceNodes.Clear();
        }

        public void AllocateAllVariables(MemoryStack memoryStack)
        {
            foreach (VariableNode varNode in SequenceNodes.Select(pair => pair.Value).OfType<VariableNode>())
            {
                varNode.Allocate(memoryStack);
            }
        }

        public void ResetNodes()
        {
            foreach (var pair in SequenceNodes)
            {
                pair.Value.Reset();
            }
        }

        public void OnEvent(ProcessingContext context, Type type, int index, object param)
        {
            //_MustStop = false;

            foreach (var eventNode in SequenceNodes.Select(pair => pair.Value as EventNode)
                .Where(node => node != null
                       && node.GetType() == type))
            {
                eventNode.Triggered(context, index, param);
            }
        }

        public virtual void Load(XmlNode node)
        {
            Id = int.Parse(node.Attributes["id"].Value);
            if (_newId <= Id) _newId = Id + 1;
            Name = node.Attributes["name"].Value;
            Description = node.Attributes["description"].Value;

            foreach (XmlNode nodeNode in node.SelectNodes("NodeList/Node"))
            {
                int versionNode = int.Parse(nodeNode.Attributes["version"].Value);

                SequenceNode seqNode = SequenceNode.CreateNodeFromXml(nodeNode);

                if (seqNode != null)
                {
                    AddNode(seqNode);
                }
                else
                {
                    throw new InvalidOperationException("Can't create SequenceNode from xml " +
                                                        $"id={nodeNode.Attributes["id"].Value}");
                }
            }
        }

        internal void ResolveNodesLinks(XmlNode node)
        {
            if (node == null) throw new ArgumentNullException("XmlNode");

            XmlNode connectionListNode = node.SelectSingleNode("ConnectionList");

            foreach (var sequenceNode in SequenceNodes)
            {
                sequenceNode.Value.ResolveLinks(connectionListNode, this);
            }
        }

        public virtual void Save(XmlNode node)
        {
            const int version = 1;

            XmlNode graphNode = node.OwnerDocument.CreateElement("Graph");
            node.AppendChild(graphNode);

            graphNode.AddAttribute("version", version.ToString());
            graphNode.AddAttribute("id", Id.ToString());
            graphNode.AddAttribute("name", Name);
            graphNode.AddAttribute("description", Description);

            //save all nodes
            XmlNode nodeList = node.OwnerDocument.CreateElement("NodeList");
            graphNode.AppendChild(nodeList);
            //save all connections
            XmlNode connectionList = node.OwnerDocument.CreateElement("ConnectionList");
            graphNode.AppendChild(connectionList);

            foreach (var pair in SequenceNodes)
            {
                XmlNode nodeNode = node.OwnerDocument.CreateElement("Node");
                nodeList.AppendChild(nodeNode);
                pair.Value.Save(nodeNode);
                pair.Value.SaveConnections(connectionList);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
