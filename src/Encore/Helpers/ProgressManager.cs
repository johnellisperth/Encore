namespace Encore.Helpers;
public class ProgressManager
{
    public IProgress<int> Progress { get; set; } = new Progress<int>();

    private double CurrentPos_;
    private long TotalCount_;
    private long CurrentCount_;
    private int CurrentRange_;
    private int CurrentPosInt_;
    private double StartPos_;
    public ProgressManager() {}

    public void NextSubStep(long totalCount, int currentRange = 25)
    {
        CurrentCount_ = 0;
        TotalCount_ = totalCount;
        CurrentRange_ = currentRange;
        StartPos_ = CurrentPos_;
    }

    public void Update(long newCount)
    {
        CurrentCount_ += newCount;
        if (CurrentCount_ > TotalCount_)
            CurrentCount_ = TotalCount_;

        double fraction = CurrentCount_ / (double)TotalCount_;
        UpdateProgress((fraction * (double)CurrentRange_) + StartPos_);
    }

    public void UpdateProgress(double currentPos)
    {
        CurrentPos_ = currentPos;
        CurrentPos_ = Math.Min(100, CurrentPos_);

        if (CurrentPosInt_ != (int)CurrentPos_)
        {
            CurrentPosInt_ = (int)CurrentPos_;
            Progress.Report(CurrentPosInt_);
        }
    }

    public void Finish() => UpdateProgress(100);

    public void Reset() => UpdateProgress(0);
}
