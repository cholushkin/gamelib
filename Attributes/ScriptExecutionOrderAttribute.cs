using System;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public class ScriptExecutionOrderAttribute : System.Attribute
{
    public int order;

    public ScriptExecutionOrderAttribute(int order)
    {
        this.order = order;
    }
}

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public class ScriptExecutionOrderDependsOnAttribute : System.Attribute
{
    public Type targetType;
    public int orderIncrease;

    public ScriptExecutionOrderDependsOnAttribute(Type targetType)
    {
        this.targetType = targetType;
        this.orderIncrease = 10;
    }
}