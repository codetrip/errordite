
namespace Errordite.Services.Throttlers
{
    public class SQSRequestThrottler : IRequestThrottler
    {
        public int GetDelayMilliseconds(int zeroMessageCount)
        {
            //wait an extra 20 secs after each attempt to receive messages that returns zero messages
            //until we hit a max of 9 attempts, at which point keep trying every 3 mins
            if (zeroMessageCount < 9)
                return (zeroMessageCount*20)*1000; 

            return 180000; //maximum wait of 3 minutes
        }
    }
}
