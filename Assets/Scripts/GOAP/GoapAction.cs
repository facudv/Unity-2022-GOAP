using System;
using System.Collections.Generic;
using Side_Logic;
using UnityEngine;

namespace GOAP
{
    public class GoapAction
    {
        public Dictionary<string, bool> preconditions { get; }

        public Func<GoapState, bool> Preconditions = delegate { return true; };
        
        private readonly Dictionary<string, bool> _effects;

        public Func<GoapState, GoapState> Effects;

        public float Cost { get; private set; }

        public ItemType item;
        public string Name { get; }

        public GoapAction(string name)
        {

            Name = name;
            Cost = 1f;
            preconditions = new Dictionary<string, bool>();
            _effects = new Dictionary<string, bool>();
            
            Effects = (s) =>
            {
                foreach (var dictStrB in _effects)
                {
                    s.worldState.values[dictStrB.Key] = dictStrB.Value;
                }
                return s;
            };
        }

        public GoapAction SetCost(float cost)
        {
            if (cost < 1f)
            {
                Debug.Log($"Warning: Using cost < 1f for '{Name}' could yield sub-optimal results");
            }
            Cost = cost;
            return this;
        }
        public GoapAction Pre(string s, bool value)
        {
            preconditions[s] = value;
            return this;
        }

        public GoapAction Pre(Func<GoapState, bool> p)
        {
            Preconditions = p;
            return this;
        }
        public GoapAction Effect(string s, bool value)
        {
            _effects[s] = value;
            return this;
        }

        public GoapAction Effect(Func<GoapState, GoapState> e)
        {
            Effects =e;
            return this;
        }

        public GoapAction SetItem(ItemType type)
        {
            item = type;
            return this;
        }
    }
}
