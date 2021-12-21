namespace Encore.Helpers;
public class ProgressManager
{
    public int Increment { get; set; }
    public int StartPos { get; set; }

    private int CurrentPos_;

    private IProgress<int> Progress_;

    public ProgressManager(IProgress<int> progress, int increment=1)
    {
        Progress_ = progress;
        Increment = increment;
    }
    public void Reset()
    {
        CurrentPos_ = 0;
        NextReport();
    }
    public void NextReport()
    {
        Progress_.Report(CurrentPos_);
        CurrentPos_ += Increment;
        CurrentPos_ = Math.Min(100, CurrentPos_);
    }
    public void Finish()
    {
        CurrentPos_ = 100;
        Progress_.Report(CurrentPos_);
    }
    public void AdjustIncrement(int number, int diviser) => Increment = Math.Max(number / diviser, 1);

}
