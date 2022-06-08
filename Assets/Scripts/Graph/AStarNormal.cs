using System;
using System.Collections.Generic;
using System.Linq;
using GOAP;
using Side_Logic;
using U = Side_Logic.Utility;

namespace Graph
{
    public class AStarNormal<Node> where Node : class
    {
        public class Arc
        {
            public readonly Node endpoint;
            public readonly float cost;
            public Arc(Node ep, float c)
            {
                endpoint = ep;
                cost = c;
            }
        }

        //expand can return null as "no neighbours"
        public static IEnumerable<Node> Run
        (
            Node from,
            Node to,
            Func<Node, Node, float> h,				//Current, Goal -> Heuristic cost
            Func<Node, bool> satisfies,				//Current -> Satisfies
            Func<Node, IEnumerable< Arc >> expand	//Current -> (Endpoint, Cost)[]
        )
        {
            var initialState = new AStarState<Node>();
            initialState.open.Add(from);
            initialState.gs[from] = 0;
            initialState.fs[from] = h(from,to);
            initialState.previous[from] = null;
            initialState.current = from;

            var state = initialState;
            while (state.open.Count > 0 && !state.finished)
            {
                //Debugger gets buggy af with this, can't watch variable:
                state = state.Clone();

                var candidate = state.open.OrderBy(x => state.fs[x]).First();
                state.current = candidate;

                //Debug.Log(candidate);
                DebugGoap(state);
                if (satisfies(candidate))
                {
                    U.Log("SATISFIED");
                    state.finished = true;
                }
                else
                {
                    state.open.Remove(candidate);
                    state.closed.Add(candidate);
                    var neighbours = expand(candidate);
                    if(neighbours == null || !neighbours.Any())
                        continue;

                    var gCandidate = state.gs[candidate];

                    foreach (var ne in neighbours)
                    {
                        if (ne.endpoint.In(state.closed))
                            continue;

                        var gNeighbour = gCandidate + ne.cost;
                        state.open.Add(ne.endpoint);

                        if (gNeighbour > state.gs.DefaultGet(ne.endpoint, ()=>gNeighbour))
                            continue;

                        state.previous[ne.endpoint] = candidate;
                        state.gs[ne.endpoint] = gNeighbour;
                        state.fs[ne.endpoint] = gNeighbour + h(ne.endpoint, to);
                    }
                }
            }

            if(!state.finished)
                return null;

            //Climb reversed tree.
            var seq =
                U.Generate(state.current, n => state.previous[n])
                    .TakeWhile(n => n != null)
                    .Reverse();

            return seq;
        }

        static void DebugGoap(AStarState<Node> state)
        {
            var candidate = state.current;
            U.Log("OPEN SET " + state.open.Aggregate("", (a, x) => a + x + "\n\n"));
            U.Log("CLOSED SET " + state.closed.Aggregate("", (a, x) => a + x + "\n\n"));
            U.Log("CHOSEN CANDIDATE COST " + state.fs[candidate] + ":" + candidate);
            if (!(state is AStarState<GoapState>)) return;
            {
                U.Log("SEQUENCE FOR CANDIDATE" +
                      U.Generate(state.current, node => state.previous[node])
                          .TakeWhile(node => node != null)
                          .Reverse()
                          .OfType<GoapState>()
                          .Where(goapState => goapState.generatingAction != null)
                          .Aggregate("", (str, goapState) => str + "-->" + goapState.generatingAction.Name)
                );

                var prevs = state.previous as Dictionary<GoapState, GoapState>;
                U.Log("Other candidate chains:\n"
                      + prevs
                          .Select(kv => kv.Key)
                          .Where(wGoapState => !prevs.ContainsValue(wGoapState))
                          .Aggregate("", (str, aGoapState) => str +
                                                   U.Generate(aGoapState, gGoapState => prevs[gGoapState])
                                                       .TakeWhile(twGoapState => twGoapState != null)
                                                       .Reverse()
                                                       .OfType<GoapState>()
                                                       .Where(wGoapState => wGoapState.generatingAction != null)
                                                       .Aggregate("", (str2, a2GoapState) => str2 + "-->" + a2GoapState.generatingAction.Name + "(" + a2GoapState.step + ")")
                                                   + " (COST: g" + (state.gs)[aGoapState as Node] + "   f" + state.fs[aGoapState as Node] + ")"
                                                   + "\n"
                          )
                );
            }
        }
    }
}
