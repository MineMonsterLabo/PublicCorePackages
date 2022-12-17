namespace InterfaceHost
{
    public class UserInterfacePageBase<TArgument> : UserInterfacePageBase, IUserInterfacePage<TArgument>
    {
        public TArgument Arg { get; private set; }

        public virtual void Initialize(TArgument argument)
        {
            Arg = argument;
        }
    }
}