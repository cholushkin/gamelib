using System;
using System.Threading.Tasks;
using UnityEngine;
using VitalRouter;

public static class VitalRouterExtensions
{
    public static Subscription SafeSubscribe<T>(
        this ICommandSubscribable subscribable,
        UnityEngine.Object owner,
        Action<T, PublishContext> callback)
        where T : ICommand
    {
        Subscription subscription = default;
        return subscription = subscribable.Subscribe<T>((e, ctx) =>
        {
            if (owner == null) subscription.Dispose();
            else callback(e, ctx);
        });
    }


    public static Subscription SafeSubscribeAwait<T>(
        this ICommandSubscribable subscribable,
        UnityEngine.Object owner,
        Func<T, PublishContext, ValueTask> callback,
        CommandOrdering? ordering = null) where T : ICommand
        => subscribable.SubscribeAwait<T>(async (e, ctx) =>
        {
            if (owner == null) return; // skip if destroyed
            await callback(e, ctx);
        }, ordering);
}