using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

public static class AwaitExtensions
{
    public static TaskAwaiter<int> GetAwaiter(this Process process)
    {
        var tcs = new TaskCompletionSource<int>();
        process.EnableRaisingEvents = true;

        process.Exited += (s, e) => tcs.TrySetResult(process.ExitCode);

        if (process.HasExited)
        {
            tcs.TrySetResult(process.ExitCode);
        }

        return tcs.Task.GetAwaiter();
    }

    // Any time you call an async method from sync code, you can either use this wrapper
    // method or you can define your own `async void` method that performs the await
    // on the given Task

    //每当你从同步代码中调用异步方法时,你可以使用这个包装方法
    //也可以定义自己的async void方法，在给定的任务上执行等待。
    public static async void Coroutine(this Task task)
    {
        await task;
    }
}
