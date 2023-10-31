using System;
using System.Collections.Generic;
using System.Threading;

public class CameraEvent
{
    private Dictionary<int, Tuple<AutoResetEvent, DateTime>> events = new Dictionary<int, Tuple<AutoResetEvent, DateTime>>();

    public void Wait()
    {
        int ident = Thread.CurrentThread.ManagedThreadId;
        if (!events.ContainsKey(ident))
        {
            events[ident] = new Tuple<AutoResetEvent, DateTime>(new AutoResetEvent(false), DateTime.Now);
        }
        events[ident].Item1.WaitOne();
    }

    public void Set()
    {
        DateTime now = DateTime.Now;
        int? remove = null;
        foreach (var pair in events)
        {
            if (!pair.Value.Item1.WaitOne(0))
            {
                pair.Value.Item1.Set();
                pair.Value.Item2 = now;
            }
            else
            {
                if (now.Subtract(pair.Value.Item2).TotalSeconds > 5)
                {
                    remove = pair.Key;
                }
            }
        }
        if (remove.HasValue)
        {
            events.Remove(remove.Value);
        }
    }

    public void Clear()
    {
        events[Thread.CurrentThread.ManagedThreadId].Item1.Reset();
    }
}