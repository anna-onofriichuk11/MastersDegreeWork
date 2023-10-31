using System.Numerics;

public class Evaluator
{
    public double CurrentBpm { get; private set; }
    public double CurrentSnr { get; private set; }
    public List<double> Frequencies { get; private set; }
    public List<double> NormalizedAmplitude { get; private set; }

    public Evaluator(double fs, int fftLength = 300)
    {
        CurrentBpm = 0;
        CurrentSnr = 0;
        Frequencies = Enumerable.Range(0, fftLength / 2 + 1).Select(i => i * fs / 2 * 60).ToList();
        NormalizedAmplitude = new List<double>();
    }

    public void Evaluate(double fs, List<double> normalizedAmplitude, int fftLength = 300)
    {
        Frequencies = Enumerable.Range(0, fftLength / 2 + 1).Select(i => i * fs / 2 * 60).ToList();
        if (normalizedAmplitude.Count > 0)
        {
            NormalizedAmplitude = normalizedAmplitude;
            int bpmId = NormalizedAmplitude.IndexOf(NormalizedAmplitude.Max());
            CurrentBpm = Frequencies[bpmId];
            CurrentSnr = CalculateSignalToNoiseRatio(new Vector<double>(NormalizedAmplitude), bpmId);
        }
    }
    
    public static double CalculateSignalToNoiseRatio(Vector<double> hwfft, int f, int nsig = 1)
    {
        hwfft = hwfft.Squeeze();

        var signalBins = Enumerable.Range(f - nsig + 1, f + nsig + 2).Select(i => (long)i).Where(i => i > 0 && i <= hwfft.Count).ToList();

        double s = hwfft.SubVector(signalBins.Select(i => i - 1)).L2Norm();

        var noiseBins = Enumerable.Range(1, (int)hwfft.Count).Select(i => (long)i).Where(i => !signalBins.Contains(i)).ToList();

        double n = hwfft.SubVector(noiseBins.Select(i => i - 1)).L2Norm();

        return n == 0 ? double.PositiveInfinity : Decibels(s / n);
    }
    
    public static double Decibels(double x)
    {
        return 20 * Math.Log10(Math.Abs(x));
    }
}