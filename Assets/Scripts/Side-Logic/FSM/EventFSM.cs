using System;
using IA2;
using UnityEngine;

namespace Side_Logic.FSM
{
    public class EventFSM<T>
    {
        private State<T> _current;
        private readonly State<T> _any;

        public EventFSM(State<T> initial, State<T> any = null)
        {
            _current = initial;
            _current.Enter(default(T));
            _any = any ?? new State<T>("<any>");
            _any.OnEnter += a => throw new Exception("Can't make transition to fsm's <any> state");
        }

        public bool Feed(T input)
        {
            //Added any. Notice the or will not execute the second part if it satisfies the first condition.
            if (!_current.Feed(input, out var newState) && !_any.Feed(input, out newState)) return false;
            
            _current.Exit(input);
            Debug.Log("FSM state: " + _current.Name + "---" + input + "---> " + newState.Name);
            _current = newState;
            _current.Enter(input);
            return true; //Added return boolean

        }

        public State<T> Current => _current;

        public void Update() => _current.Update();
    }
}