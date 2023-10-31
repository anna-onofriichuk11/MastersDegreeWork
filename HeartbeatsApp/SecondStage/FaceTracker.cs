using System;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Cuda;
using Emgu.CV.Util;
using System.Drawing;
using System.Collections.Generic;

class EyeTracker
{
    private CascadeClassifier eye_cascade;

    public EyeTracker(string eyeCascadePath)
    {
        eye_cascade = new CascadeClassifier(eyeCascadePath);
    }

    public Rectangle[] DetectEyes(Mat frame)
    {
        Mat gray_frame = new Mat();
        CvInvoke.CvtColor(frame, gray_frame, ColorConversion.Bgr2Gray);
        return eye_cascade.DetectMultiScale(gray_frame, 1.35, 10);
    }
}

class FaceTracker
{
    private CascadeClassifier face_cascade;
    private bool foundFace = false;

    public FaceTracker(string faceCascadePath)
    {
        face_cascade = new CascadeClassifier(faceCascadePath);
    }

    public bool FoundFace { get { return foundFace; } }

    public void ResetTracker()
    {
        foundFace = false;
    }

    public Mat CropToFace(Mat frame)
    {
        Mat gray_frame = new Mat();
        CvInvoke.CvtColor(frame, gray_frame, ColorConversion.Bgr2Gray);

        if (!foundFace)
        {
            Rectangle[] faces = face_cascade.DetectMultiScale(gray_frame, 1.3, 5);

            if (faces.Length > 0)
            {
                Rectangle face = faces[0];
                face = new Rectangle(face.Left, face.Top, face.Width, face.Height);
                foundFace = true;
            }
        }

        // Your cropping and processing logic here

        return frame;
    }
}
