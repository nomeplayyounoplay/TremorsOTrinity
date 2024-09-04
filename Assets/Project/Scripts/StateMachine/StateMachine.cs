using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RetroHorror
{            
    public class StateMachine
    {
        //state the machine is currently in
        //will also contain possible transitions to other states from it
        StateNode currentState;
        
        Dictionary<Type, StateNode> nodes = new();
        HashSet<ITransition> anyTransition = new();

        public void Update()
        {
            //check every frame to see if condition to change to certain state has been met
            //loop through each every frame - sounds no bueno
            //Throw a flag when ConditionIsMet instead? or overkill or worse?
            var transition = GetTransition();
            if(transition != null) ChangeState(transition.TransitionTo);

            currentState.State?.Update();
        }

        public void FixedUpdate()
        {
            currentState.State?.FixedUpdate();
        }

        public void SetState(IState state)
        {
            currentState = nodes[state.GetType()];
            currentState.State?.OnEnter();
        }
        
        //Must set to new state first or just returns
        void ChangeState(IState state)
        {
            if(currentState.State == state) return;

            var previousState = currentState.State;
            var nextState = nodes[state.GetType()].State;

            previousState?.OnExit();
            nextState?.OnEnter();
            currentState = nodes[state.GetType()];
        }

        ITransition GetTransition()
        {
            //I dont really like the idea of looping through each everyframe
            //would just throwing a flag to signal to change states be better?
            foreach (var transition in anyTransition)
                if(transition.Condition.IsConditionMet())
                    return transition;
            foreach (var transition in currentState.Transitions)
                if(transition.Condition.IsConditionMet())
                    return transition;
            return null;
        }

        public void AddTransition(IState from, IState to, IPredicate condition)
        {
            GetOrAddNode(from).AddTransition(GetOrAddNode(to).State, condition);
        }

        public void AddAnyTransition(IState to, IPredicate condition)
        {
            anyTransition.Add(new Transition(GetOrAddNode(to).State, condition));
        }

        //Looks for and if finds it in nodes - Get
        //If not in Nodes - Add
        StateNode GetOrAddNode(IState state)
        {
            var node = nodes.GetValueOrDefault(state.GetType());
            if(node == null)
            {
                node = new StateNode(state);
                nodes.Add(state.GetType(), node);
            }
            return node;
        }   
        
        //State and all its Transitions
        class StateNode
        {
            public IState State {get;}
            public HashSet<ITransition> Transitions {get;}

            public StateNode(IState state)
            {
                State = state;
                Transitions = new HashSet<ITransition>();
            }

            public void AddTransition(IState transitionTo, IPredicate condition)
            {
                Transitions.Add(new Transition(transitionTo, condition));
            }
        }
    }
}
