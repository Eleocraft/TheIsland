using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ThreadingHandler : MonoSingleton<ThreadingHandler>
{
    const int checkEveryMilliseconds = 1000;
    // for performance reasons the mesh and map generation works on seperate threads
    // this class has all functions necessary to handle this
    Queue<ThreadInfo> dataQueue = new();
    Queue<ThreadStart> startQueue = new();

    void Start()
    {
        new Thread(ThreadStartManager).Start();
    }
    void ThreadStartManager()
    {
        while (true)
        {
            //ThreadPool.GetAvailableThreads(out int avbWorkerThreads, out int avbCompletionPortThreads);
            lock (startQueue)
            {
                if (startQueue.Count > 0 )
                    for (int i = 0; i < startQueue.Count; i++)
                        new Thread(startQueue.Dequeue()).Start();
            }
            Thread.Sleep(checkEveryMilliseconds);
        }
    }
    public static void RequestData(Func<object> generateData, Action<object> callback)
    {
        ThreadStart threadStart = delegate {
            Instance.DataThread(generateData, callback);
        };
        Instance.startQueue.Enqueue(threadStart);
    }
    void DataThread(Func<object> generateData, Action<object> callback)
    {
        object data = generateData();
        lock (dataQueue)
        {
            dataQueue.Enqueue(new ThreadInfo(callback, data));
        }
    }
    void Update()
    {
        if (dataQueue.Count > 0)
        {
            for (int i = 0; i < dataQueue.Count; i++)
            {
                ThreadInfo thread = dataQueue.Dequeue();
                thread.callback(thread.parameter);
            }
        }
    }
    struct ThreadInfo
    {
        public readonly Action<object> callback;
        public readonly object parameter;
        public ThreadInfo(Action<object> callback, object parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}