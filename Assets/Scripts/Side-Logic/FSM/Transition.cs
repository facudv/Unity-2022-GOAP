using System;
using IA2;

namespace Side_Logic.FSM 
{
	public class Transition<T> 
	{
		public event Action<T> OnTransition = delegate { };
		public T Input => _input;
		public State<T> TargetState => _targetState;

		private T _input;
		private readonly State<T> _targetState;

		public void OnTransitionExecute(T input) => OnTransition(input);

		public Transition(T input, State<T> targetState) 
		{
			_input = input;
			_targetState = targetState;
		}
	}
}