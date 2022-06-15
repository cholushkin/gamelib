namespace Events
{
    public interface IHandle
    {
    }

    public interface IHandle<TMessage> : IHandle
    {
        void Handle(TMessage message);
    }
}
