using System.Diagnostics;

namespace AGRA_EASY_MOBILE.Services;

public sealed class KeyboardScanInputTracker
{
    private static readonly TimeSpan ScanMaximumDuration = TimeSpan.FromMilliseconds(500);
    private const int MinimumScanLength = 6;

    private readonly Stopwatch _stopwatch = new();
    private string _currentText = string.Empty;

    public void ObserveTextChanged(string? oldText, string? newText)
    {
        string previous = oldText ?? string.Empty;
        string current = newText ?? string.Empty;

        if (string.IsNullOrEmpty(current))
        {
            Reset();
            return;
        }

        if (string.IsNullOrEmpty(previous) || !_stopwatch.IsRunning)
            _stopwatch.Restart();

        _currentText = current;
    }

    public bool ConsumeCompletedAsScan(string? completedText)
    {
        string text = (completedText ?? _currentText).Trim();
        if (!_stopwatch.IsRunning || text.Length < MinimumScanLength)
        {
            Reset();
            return false;
        }

        TimeSpan duration = _stopwatch.Elapsed;
        Reset();
        return duration <= ScanMaximumDuration;
    }

    public void Reset()
    {
        _stopwatch.Reset();
        _currentText = string.Empty;
    }
}
