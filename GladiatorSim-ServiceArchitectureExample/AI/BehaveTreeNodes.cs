using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.CodeBase.Gameplay.AI.BehaveTreeBase
{
    public class BehaveTreeNodes
    {
        public abstract class Node
        {
            protected string nodeName = "";
            public abstract bool Execute(int depth, bool needLog = false);

            protected void LogNode(int depth)
            {
                Debug.Log(new string('-', depth) + nodeName);
            }
        }

        public class ActionNode : Node
        {
            private readonly Action _action;

            public ActionNode(string nodeName, Action action)
            {
                this.nodeName = nodeName;
                _action = action;
            }

            public override bool Execute(int depth, bool needLog = false)
            {
                if (needLog) LogNode(depth);
                _action();
#if UNITY_EDITOR
                //Debug.Log("ActionNode: " + nodeName + " executed");
#endif
                return true;
            }
        }

        public class ConditionNode : Node
        {
            private readonly Func<bool> _condition;
            private readonly bool _invert;

            public ConditionNode(string nodeName, Func<bool> condition, bool invert = false)
            {
                this.nodeName = nodeName;
                _condition = condition;
                _invert = invert;
            }

            public ConditionNode Invert()
            {
                return new ConditionNode(nodeName, _condition, !_invert);
            }

            public override bool Execute(int depth, bool needLog = false)
            {
                if (needLog) LogNode(depth);
                var result = _condition();
                return _invert ? !result : result;
            }
        }

        public class SelectorNode : Node
        {
            private readonly List<Node> _nodes;

            public SelectorNode(string nodeName, List<Node> nodes)
            {
                this.nodeName = nodeName;
                _nodes = nodes;
            }

            public override bool Execute(int depth, bool needLog = false)
            {
                if (needLog) LogNode(depth);
                foreach (var node in _nodes)
                {
                    if (node.Execute(depth + 1))
                    {
                        //print("SelectorNode: " + node.Name + " executed");
                        return true;
                    }
                }

                return false;
            }
        }

        public class SequenceNode : Node
        {
            private readonly List<Node> _nodes;

            public SequenceNode(string nodeName, List<Node> nodes)
            {
                this.nodeName = nodeName;
                _nodes = nodes;
            }

            public override bool Execute(int depth, bool needLog = false)
            {
                if (needLog) LogNode(depth);
                foreach (var node in _nodes)
                {
                    if (!node.Execute(depth + 1))
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}