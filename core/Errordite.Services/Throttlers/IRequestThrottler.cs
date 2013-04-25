namespace Errordite.Services.Throttlers
{
    public interface IRequestThrottler
    {
        int GetDelayMilliseconds(int zeroMessageCount);
    }
}