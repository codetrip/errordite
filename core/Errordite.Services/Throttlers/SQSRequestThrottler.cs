
namespace Errordite.Services.Throttlers
{
    public class SQSRequestThrottler : IRequestThrottler
    {
        public int GetDelayMilliseconds(int zeroMessageCount)
        {
            //wait an extra 20 secs after each attempt to receive messages that returns zero messages
            //until we hit a max of 30 attempts, at which point keep trying every 5 mins
            if (zeroMessageCount < 15)
                return (zeroMessageCount*20)*1000; 

            return 300000; //maximum wait of 5 minutes
        }
    }
}
