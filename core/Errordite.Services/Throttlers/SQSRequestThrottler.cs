
namespace Errordite.Services.Throttlers
{
    public class SQSRequestThrottler : IRequestThrottler
    {
        public int GetDelayMilliseconds(int zeroMessageCount)
        {
            //wait an extra 10 secs after each attempt to receive messages that returns zero messages
            //until we hit a max of 18 attempts, at which point keep trying every 3 mins
            if (zeroMessageCount < 18)
                return (zeroMessageCount*10)*1000;

            return 180000; //maximum wait of 3 minutes
        }
    }
}
