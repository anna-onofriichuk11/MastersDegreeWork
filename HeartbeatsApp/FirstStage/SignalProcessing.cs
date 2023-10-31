using System;
using System.Numerics;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Cuda;
using Emgu.CV.Util;
using Emgu.CV;
using System.Linq;

class PulseExtraction
{
    private static readonly int R = 0;
    private static readonly int G = 1;
    private static readonly int B = 2;

    public static double[] ExtractPulseChrominance(int fs, Matrix<float> rPPG, int fftLength = 300)
    {
        if (rPPG.ColumnCount <= fftLength)
        {
            return new double[] { };
        }
        else
        {
            var fftRoi = Enumerable.Range(0, fftLength / 2 + 1).ToArray();
            double bpfDiv = 60.0 * fs / 2.0;
            var (b_BPF40220, a_BPF40220) = Signal.ButterworthBandpassFilter(10, new double[] { 40.0 / bpfDiv, 220.0 / bpfDiv });

            var colC = Matrix<double>.Build.Dense(3, fftLength);
            var skinVec = new double[] { 1.0, 0.66667, 0.5 };

            for (int col = 0; col < 3; col++)
            {
                var colStride = rPPG.Column(col).Subtract(rPPG.Column(col).Mean()).Divide(rPPG.Column(col).Mean());
                var yACDC = Signal.Detrend(colStride);
                colC.SetRow(col, yACDC.Multiply(skinVec[col]));
            }

            var XChrom = colC.Row(R).Subtract(colC.Row(G));
            var YChrom = colC.Row(R).Add(colC.Row(G)).Subtract(colC.Row(B).Multiply(2.0));
            var Xf = Signal.FiltFilt(b_BPF40220, a_BPF40220, XChrom);
            var Yf = Signal.FiltFilt(b_BPF40220, a_BPF40220, YChrom);
            var Nx = Xf.StandardDeviation();
            var Ny = Yf.StandardDeviation();
            var alphaChrom = Nx / Ny;
            var xStrideMethod = Xf.Subtract(Yf.Multiply(alphaChrom));
            var STFT = Fourier.Forward(xStrideMethod, fftLength)[fftRoi];
            var normalizedAmplitude = STFT.PointwiseDivide(STFT.AbsoluteMaximum());
            return normalizedAmplitude.ToArray();
        }
    }

    public static double[] ExtractPulsePBV(int fs, Matrix<float> rPPG, int fftLength = 300)
    {
        if (rPPG.ColumnCount < fftLength)
        {
            return new double[] { };
        }
        else
        {
            var pbv = new double[] { 0.307737615161693, 0.436069490554354, 0.236745815212185 };
            var fftRoi = Enumerable.Range(0, fftLength / 2 + 1).ToArray();
            double bpfDiv = 60.0 * fs / 2.0;
            var (b_BPF40220, a_BPF40220) = Signal.ButterworthBandpassFilter(10, new double[] { 40.0 / bpfDiv, 220.0 / bpfDiv });

            var colC = Matrix<double>.Build.Dense(3, fftLength);

            for (int col = 0; col < 3; col++)
            {
                var colStride = rPPG.Column(col).Subtract(rPPG.Column(col).Mean()).Divide(rPPG.Column(col).Mean());
                var yACDC = Signal.Detrend(colStride);
                colC.SetRow(col, Signal.FiltFilt(b_BPF40220, a_BPF40220, yACDC));
            }

            var S = colC.Multiply(colC.Transpose());
            var W = S.Solve(pbv.ToColumnMatrix());
            var xStrideMethod = colC.Transpose().Multiply(W).PointwiseDivide(pbv.ToRowMatrix().Multiply(W));
            var STFT = Fourier.Forward(xStrideMethod, fftLength)[fftRoi];
            var normalizedAmplitude = STFT.PointwiseDivide(STFT.AbsoluteMaximum());
            return normalizedAmplitude.ToArray();
        }
    }
}

class Signal
{
    public static (double[] b, double[] a) ButterworthBandpassFilter(int order, double[] freq)
    {
        double low = freq[0];
        double high = freq[1];
        double nyq = 0.5;
        double lowCut = low / nyq;
        double highCut = high / nyq;
        double b1 = Math.Tan(Math.PI * lowCut) * Math.PI;
        double a1 = Math.Sqrt(lowCut * highCut);
        double a2 = highCut;
        double a3 = Math.Tan(Math.PI * highCut) * Math.PI;

        double a0 = a1 + a2 + a3;
        double b0 = (b1 + 1) / a0;
        double b2 = (b1 - 1) / a0;
        double a4 = a1 / a0;
        double a5 = -2 * a0;
        double a6 = a0;

        return (new double[] { b0, b1, b2 }, new double[] { 1, a4, a5, a6 });
    }

    public static Matrix<double> Detrend(Matrix<double> data)
    {
        var n = data.ColumnCount;
        var x = Enumerable.Range(1, n).ToArray();
        var xMatrix = Matrix<double>.Build.Dense(n, 2, (i, j) => j == 0 ? 1.0 : x[i]);
        var xMatrixT = xMatrix.Transpose();
        var b = xMatrixT.Multiply(xMatrix).Inverse().Multiply(xMatrixT).Multiply(data);

        var trend = xMatrix.Multiply(b);
        var detrended = data.Subtract(trend);
        return detrended;
    }
}
