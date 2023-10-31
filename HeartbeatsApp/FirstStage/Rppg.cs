namespace HeartbeatsApp;

using System;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Cuda;
using Emgu.CV.Util;
using System.Drawing;
using System.Collections.Generic;

class LandmarkRoiFinder
{
    public Mat GetRoi(Mat frame, LandmarkTracker landmarkTracker)
    {
        PointF peyer = landmarkTracker.Peyer;
        PointF peyel = landmarkTracker.Peyel;
        PointF pmouth = landmarkTracker.Pmouth;
        float x0 = peyel.X;
        float x1 = peyer.X;
        float y0 = pmouth.Y;
        float y1 = Math.Max(peyer.Y, peyel.Y);
        float h = y1 - y0;
        float w = x1 - x0;

        Rectangle rect = new Rectangle(
            (int)(x0 - w * 1.3),
            (int)(y0 + 2.2 * h),
            (int)(w * 3.5),
            (int)(-2.5 * h)
        );

        CvInvoke.Rectangle(frame, rect, new Bgr(0, 255, 0).MCvScalar, 2);
        return CropFrame(frame, rect);
    }

    private Mat CropFrame(Mat frame, Rectangle rect)
    {
        //todo Implement the logic to crop the frame here
        return frame;
    }
}

class PPGSensor
{
    protected List<float[]> rppgl = new List<float[]>();
    protected Mat rppg;
    protected FrameCapture cap;

    public PPGSensor(FrameCapture frameCapture)
    {
        rppg = new Mat();
        cap = frameCapture;
    }

    public virtual void SensePPG(Mat frame, int numPixels)
    {
        // Implement PPG sensing logic here
    }

    public void Reset(FrameCapture frameCapture)
    {
        rppgl.Clear();
        rppg = new Mat();
        cap = frameCapture;
    }
}

class SimplePPGSensor : PPGSensor
{
    public override void SensePPG(Mat frame, int numPixels)
    {
        float rAvg = frame.GetAverage(new MCvScalar(2, 0, 0)).V0 / numPixels;
        float gAvg = frame.GetAverage(new MCvScalar(0, 2, 0)).V1 / numPixels;
        float bAvg = frame.GetAverage(new MCvScalar(0, 0, 2)).V2 / numPixels;
        float[] ppg = { rAvg, gAvg, bAvg };

        for (int i = 0; i < ppg.Length; i++)
        {
            if (float.IsNaN(ppg[i]))
            {
                ppg[i] = 0;
            }
        }

        rppgl.Add(ppg);
        rppg = cap.Resample(rppgl.ToArray());
    }

    public SimplePPGSensor(FrameCapture frameCapture) : base(frameCapture)
    {
    }
}

class SimpleForeheadSensor : PPGSensor
{
    public override void SensePPG(Mat frame, float bp)
    {
        Rectangle subRoiRect = GetSubRoiRect(frame, new double[] { 0.35, 0.70, 0.08, 0.23 });
        CvInvoke.Rectangle(frame, subRoiRect, new Bgr(0, 255, 0).MCvScalar, 2);
        Mat forehead = CropFrame(frame, subRoiRect);
        int numPixels = forehead.Width * forehead.Height;

        float rAvg = forehead.GetAverage(new MCvScalar(2, 0, 0)).V0 / numPixels;
        float gAvg = forehead.GetAverage(new MCvScalar(0, 2, 0)).V1 / numPixels;
        float bAvg = forehead.GetAverage(new MCvScalar(0, 0, 2)).V2 / numPixels;
        float[] ppg = { rAvg, gAvg, bAvg };

        for (int i = 0; i < ppg.Length; i++)
        {
            if (float.IsNaN(ppg[i]))
            {
                ppg[i] = 0;
            }
        }

        rppgl.Add(ppg);

        float[][] rppgArray = rppgl.GetRange(Math.Max(rppgl.Count - 300, 0), rppgl.Count).ToArray();
        rppg = cap.Resample(rppgArray);
    }

    private Rectangle GetSubRoiRect(Mat frame, double[] region)
    {
        //todo Implement the logic to calculate the sub-ROI rectangle here
        return new Rectangle();
    }

    private Mat CropFrame(Mat frame, Rectangle rect)
    {
        //todo Implement the logic to crop the frame here
        return frame;
    }

    public SimpleForeheadSensor(FrameCapture frameCapture) : base(frameCapture)
    {
    }
}

class RegionSensor : PPGSensor
{
    public override void SensePPG(Mat frame, float bp)
    {
        double[][] regions = new double[][] {
            new double[] { 0.15, 0.40, 0.45, 0.75 },
            new double[] { 0.6, 0.85, 0.45, 0.75 },
            new double[] { 0.35, 0.70, 0.08, 0.23 }
        };

        int numPixels = 0;
        float[] r = new float[regions.Length];
        float[] g = new float[regions.Length];
        float[] b = new float[regions.Length];

        for (int i = 0; i < regions.Length; i++)
        {
            Rectangle regionRect = GetSubRoiRect(frame, regions[i]);
            CvInvoke.Rectangle(frame, regionRect, new Bgr(0, 255, 0).MCvScalar, 2);
            Mat regionCropped = CropFrame(frame, regionRect);

            r[i] = regionCropped.GetAverage(new MCvScalar(2, 0, 0)).V0;
            g[i] = regionCropped.GetAverage(new MCvScalar(0, 2, 0)).V1;
            b[i] = regionCropped.GetAverage(new MCvScalar(0, 0, 2)).V2;
            numPixels += regionCropped.Width * regionCropped.Height;
        }

        float rAvg = r.Sum() / numPixels;
        float gAvg = g.Sum() / numPixels;
        float bAvg = b.Sum() / numPixels;
        float[] ppg = { rAvg, gAvg, bAvg };

        for (int i = 0; i < ppg.Length; i++)
        {
            if (float.IsNaN(ppg[i]))
            {
                ppg[i] = 0;
            }
        }

        rppgl.Add(ppg);

        float[][] rppgArray = rppgl.GetRange(Math.Max(rppgl.Count - 300, 0), rppgl.Count).ToArray();
        rppg = cap.Resample(rppgArray);
    }

    private Rectangle GetSubRoiRect(Mat frame, double[] region)
    {
        //todo Implement the logic to calculate the sub-ROI rectangle here
        return new Rectangle();
    }

    private Mat CropFrame(Mat frame, Rectangle rect)
    {
        // todo Implement the logic to crop the frame here
        return frame;
    }

    public RegionSensor(FrameCapture frameCapture) : base(frameCapture)
    {
    }
}
