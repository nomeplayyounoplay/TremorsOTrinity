namespace RetroHorror
{
    public class Transition : ITransition
    {
        public IState TransitionTo{get;}

        public IPredicate Condition {get;}

        public Transition(IState targetState, IPredicate condition)
        {
            TransitionTo = targetState;
            Condition = condition;
        }
    }
}
