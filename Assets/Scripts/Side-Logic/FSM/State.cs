using System;
using System.Collections.Generic;
using Side_Logic.FSM;

namespace IA2 
{
	public class State<T> 
	{
		public string Name => _name;

		public event Action<T> OnEnter = delegate {};
		public event Action OnUpdate = delegate {};
		public event Action<T> OnExit = delegate {};

		private readonly string _name;
		private Dictionary<T, Transition<T>> _transitions = new Dictionary<T, Transition<T>>();
		

		public State(string name) => _name = name;

		public State<T> Configure(Dictionary<T, Transition<T>> transitions) 
		{
			_transitions = transitions;
			return this;
		}

		public Transition<T> GetTransition(T input) => _transitions[input];

		public bool Feed(T input, out State<T> next) 
		{
			if(_transitions.ContainsKey(input)) 
			{
				var transition = _transitions[input];
				transition.OnTransitionExecute(input);
				next = transition.TargetState;
				return true;
			}

			next = this;
			return false;
		}

		public void Enter(T input) => OnEnter(input);

		public void Update() => OnUpdate();
		
		public void Exit(T input) => OnExit(input);

	}
}