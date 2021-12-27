namespace Encore.Helpers;
public class ProgressManager
{
    public int Increment { get; set; }
    public int StartPos { get; set; }

    private int CurrentPos_;

    public IProgress<int> Progress { get; set; }

    public ProgressManager(int increment=1)
    {
        //Progress_ = progress;
        Increment = increment;
    }

    public void Setup(ProgressBar progressBar)
    {
      //  Progress_
    }
    public void Reset()
    {
        CurrentPos_ = 0;
        NextReport();
    }
    public void NextReport()
    {
        Progress.Report(CurrentPos_);
        CurrentPos_ += Increment;
        CurrentPos_ = Math.Min(100, CurrentPos_);
    }
    public void Finish()
    {
        CurrentPos_ = 100;
        Progress.Report(CurrentPos_);
    }
    public void AdjustIncrement(int number, int diviser) => Increment = Math.Max(number / diviser, 1);

}
