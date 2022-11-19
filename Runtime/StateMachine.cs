using System;
using System.Collections.Generic;

namespace VoydVoyd.FSM
{
    public class StateMachine
    {
        private IState currentState;
        private Dictionary<Type, List<Transition>> transitions = new Dictionary<Type, List<Transition>>();
        private List<Transition> currentTransitions = new List<Transition>();
        private List<Transition> anyTransitions = new List<Transition>();
        private static List<Transition> EmptyTransitions = new List<Transition>(capacity: 0);

        private bool handleFixedTick = false;
        
        public IState CurrentState => currentState;

        public void Tick(object param = null)
        {
            var transition = GetTransition();
            if (transition != null)
            {
                SetState(transition.To);
            }

            currentState?.Tick(param);
        }

        public void FixedTick()
        {
            if (handleFixedTick)
            {
                var fixedTickState = currentState as IFixedTick;
                fixedTickState?.FixedTick();
            }
            
        }

        public void SetState(IState state)
        {
            if (state == currentState) return;

            currentState?.OnExit();
            currentState = state;

            if (currentState is IFixedTick)
                handleFixedTick = true;
            else
                handleFixedTick = false;

            transitions.TryGetValue(currentState.GetType(), out currentTransitions);
            if (currentTransitions == null)
            {
                currentTransitions = EmptyTransitions;
            }


            currentState?.OnEnter();
        }

        public void AddTransition(IState from, IState to, Func<bool> predicate)
        {
            if (transitions.TryGetValue(from.GetType(), out var listTransitions) == false)
            {
                listTransitions = new List<Transition>();
                transitions[from.GetType()] = listTransitions;
            }

            listTransitions.Add(new Transition(to, predicate));
        }

        public void AddAnyTransition(IState state, Func<bool> predicate)
        {
            anyTransitions.Add(new Transition(state, predicate));
        }

        private class Transition
        {
            public Func<bool> Condition { get; }

            public IState To { get; }

            public Transition(IState to, Func<bool> condition)
            {
                To = to;
                Condition = condition;
            }
        }

        private Transition GetTransition()
        {
            foreach (var transition in anyTransitions)
                if (transition.Condition())
                    return transition;

            foreach (var transition in currentTransitions)
                if (transition.Condition())
                    return transition;

            return null;

        }

        
    }

}