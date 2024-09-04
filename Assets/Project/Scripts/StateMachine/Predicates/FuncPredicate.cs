using System;

namespace RetroHorror
{
    public class FuncPredicate : IPredicate
    {
        readonly Func<bool> func;

        public FuncPredicate(Func<bool> func)
        {
            this.func = func;
        }
        public bool IsConditionMet() => func.Invoke();
    }

}
