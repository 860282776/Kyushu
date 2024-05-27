using System;
using System.Threading.Tasks;
public interface IMessageHandler
{
    Type GetHandlerType();
}
[MessageHandler]
public abstract class MessageHandler<T> : IMessageHandler where T : struct
{
    public Type GetHandlerType()
    {
        return typeof(T);
    }
    public abstract Task HandleMessage(T arg);
}
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
sealed class MessageHandlerAttribute : Attribute
{

}