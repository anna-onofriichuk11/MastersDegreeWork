using System;
using System.Collections.Generic;
using System.Threading;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Cuda;

public class Camera : BaseCamera
{
    private static readonly Camera _instance = new Camera();

    private Camera()
    {
    }

    public static Camera Instance
    {
        get { return _instance; }
    }

    public override IEnumerable<byte[]> Frames()
    {
        while (true)
        {
            var result = RunMainLoop();
            yield return CvInvoke.Imencode(".jpg", result).ToArray();
        }
    }

    public static Mat RunMainLoop()
    {
        Mat result = Main();
        return result;
    }

    public static Mat Main()
    {
        // todo Implement the main logic for capturing frames here
        return new Mat();
    }
}
