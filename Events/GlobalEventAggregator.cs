using Events;

public static class GlobalEventAggregator
{
    public static EventAggregator EventAggregator = new EventAggregator();
    public static void Publish(object message) => EventAggregator.Publish(message);
}