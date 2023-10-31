public class BaseCamera
{
    public static Thread thread = null;
    public static object frame = null;
    public static DateTime last_access = DateTime.Now;
    public static CameraEvent @event = new CameraEvent();

    public BaseCamera()
    {
        if (thread == null)
        {
            last_access = DateTime.Now;
            thread = new Thread(new ThreadStart(_thread));
            thread.Start();
            while (GetFrame() == null)
            {
                Thread.Sleep(0);
            }
        }
    }

    public object GetFrame()
    {
        last_access = DateTime.Now;
        @event.Wait();
        @event.Clear();
        return frame;
    }

    public virtual IEnumerable<byte[]> Frames()
    {
        throw new NotImplementedException("Must be implemented by subclasses.");
    }

    private void _thread()
    {
        var frames_iterator = Frames();
        foreach (var frame in frames_iterator)
        {
            BaseCamera.frame = frame;
            @event.Set();
            Thread.Sleep(0);
            if (DateTime.Now.Subtract(BaseCamera.last_access).TotalSeconds > 10)
            {
                break;
            }
        }
        thread = null;
    }
}