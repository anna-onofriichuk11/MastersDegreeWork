using System;
using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Cuda;
using Emgu.CV.Util;
using System.Drawing;
using System.Collections.Generic;

interface IFrame
{
    Mat GetFrame();

    Mat Resample(float[,] rPPG);
}

class WebcamCapture : IFrame
{
    private VideoCapture camera;
    private List<float> timestamps;
    private double tprev;
    private int fs;

    public WebcamCapture(int frameRate = 20)
    {
        fs = frameRate;
        camera = new VideoCapture(0);
        timestamps = new List<float>();
        tprev = 0;
    }

    public Mat GetFrame()
    {
        Mat frame = new Mat();
        camera.Read(frame);

        double currentTime = CvInvoke.GetTickCount() / CvInvoke.GetTickFrequency();
        if (tprev != 0)
        {
            timestamps.Add((float)(timestamps[timestamps.Count - 1] + (currentTime - tprev)));
        }
        else
        {
            timestamps.Add(0);
        }

        tprev = currentTime;
        return frame;
    }

    public Mat Resample(float[,] rPPG)
    {
        if (timestamps.Count == 0)
        {
            timestamps.Add(fs);
        }

        float[] t = new float[timestamps.Count];
        for (int i = 0; i < timestamps.Count; i++)
        {
            t[i] = timestamps[i];
        }

        int length = (int)Math.Ceiling((t[t.Length - 1] - t[0]) * fs) + 1;
        float[,] rPPGResampled = new float[3, length];

        for (int col = 0; col < 3; col++)
        {
            for (int i = 0; i < length; i++)
            {
                rPPGResampled[col, i] = Interpolate(t, rPPG[col], t[0] + i / fs);
            }
        }

        return new Mat(rPPGResampled.GetLength(0), rPPGResampled.GetLength(1), DepthType.Cv32F, 1);
    }

    private float Interpolate(float[] x, float[] y, float x0)
    {
        int i = Array.BinarySearch(x, x0);

        if (i < 0)
        {
            i = ~i;

            if (i == 0 || i == x.Length)
            {
                return float.NaN;
            }
        }

        float xL = x[i - 1];
        float xR = x[i];
        float yL = y[i - 1];
        float yR = y[i];
        float dydx = (yR - yL) / (xR - xL);
        return yL + dydx * (x0 - xL);
    }
}
