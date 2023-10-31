using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Cuda;
using System;
using System.Drawing;

class SkinClassifier
{
    private int minHue = 0;
    private int minSaturation = 40;
    private int minValue = 80;
    private int maxHue = 20;
    private int maxSaturation = 255;
    private int maxValue = 255;
    private int ellipseSize = 12;
    private int blurSize = 5;
    private int numSkinPixels = 0;
    private int myy = 100;
    private bool enabled = true;

    public SkinClassifier()
    {
        
    }

    public (Mat, int) ApplySkinClassifier(Mat frame)
    {
        if (enabled)
        {
            try
            {
                var lowerBound = new MCvScalar(minHue, minSaturation, minValue);
                var upperBound = new MCvScalar(maxHue, maxSaturation, maxValue);

                int ellipseSize = this.ellipseSize;
                int blurSize = this.blurSize;

                Mat convertedFrame = new Mat();
                CvInvoke.CvtColor(frame, convertedFrame, ColorConversion.Bgr2Hsv);
                Mat skinMask = new Mat();

                CvInvoke.InRange(convertedFrame, lowerBound, upperBound, skinMask);

                Mat kernel = CvInvoke.GetStructuringElement(ElementShape.Ellipse, new Size(ellipseSize, ellipseSize));
                CvInvoke.Erode(skinMask, skinMask, kernel, new Point(-1, -1), 2, BorderType.Default, new MCvScalar(0));
                CvInvoke.Dilate(skinMask, skinMask, kernel, new Point(-1, -1), 2, BorderType.Default, new MCvScalar(0));
                CvInvoke.GaussianBlur(skinMask, skinMask, new Size(blurSize, blurSize), 0);

                int numSkinPixels = (int)CvInvoke.Sum(skinMask).V0;
                Mat skin = new Mat();
                CvInvoke.BitwiseAnd(frame, frame, skin, mask: skinMask);

                return (skin, numSkinPixels);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in ApplySkinClassifier: {e.Message}");
                return (frame, frame.Size.Height * frame.Size.Width);
            }
        }
        else
        {
            return (frame, frame.Size.Height * frame.Size.Width);
        }
    }
}
