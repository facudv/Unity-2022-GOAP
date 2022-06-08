using System.Collections.Generic;

namespace Graph
{
    public class AStarState<Node>
    {
        public readonly HashSet<Node> open;
        public readonly HashSet<Node> closed;
        public readonly Dictionary<Node, float> gs;
        public readonly Dictionary<Node, float> fs;
        public readonly Dictionary<Node, Node> previous;
        public Node current;
        public bool finished;

        public AStarState()
        {
            open = new HashSet<Node>();
            closed = new HashSet<Node>();
            gs = new Dictionary<Node, float>();
            fs = new Dictionary<Node, float>();
            previous = new Dictionary<Node, Node>();
            current = default(Node);
            finished = false;
        }

        private AStarState(AStarState<Node> copy)
        {
            open = new HashSet<Node>(copy.open);
            closed = new HashSet<Node>(copy.closed);
            gs = new Dictionary<Node, float>(copy.gs);
            fs = new Dictionary<Node, float>(copy.fs);
            previous = new Dictionary<Node, Node>(copy.previous);
            current = copy.current;
            finished = copy.finished;
        }

        public AStarState<Node> Clone()
        {
            return new AStarState<Node>(this);
        }
    }
}

