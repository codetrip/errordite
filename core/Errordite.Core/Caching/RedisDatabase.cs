using System.Threading.Tasks;
using BookSleeve;
using CodeTrip.Core;
using System.Linq;

namespace Errordite.Core.Caching
{
    public enum RedisDatabase
    {
        Users = 1,
        Groups = 2,
        Applications = 3,
        Organisations = 4,
        Issues = 5,
        Notifications = 6,
        Alerts = 7
    }

    public static class RedisExtensions
    {
        public static Task<byte[][]> Range(this IListCommands listCommands, RedisDatabase database, string key, int start, int stop, bool queueJump = false)
        {
            Task<byte[][]> range = listCommands.Range((int) database, key, start, stop, queueJump);
            return range;
            //Task<T[]> deserialisedRange;
            //range.ContinueWith(r => deserialisedRange = new Task<T[]>(
            //                            () => r.Result.Select(SerializationHelper.ProtobufDeserialize<T>).ToArray()
            //                            ));

            //return deserialisedRange;

        }

        public static Task<long> AddFirst<T>(this IListCommands listCommands, RedisDatabase database, string key, T value, bool createIfMissing = true, bool queueJump = false)
        {
            return listCommands.AddFirst((int) database, key, SerializationHelper.ProtobufSerialize(value), createIfMissing, queueJump);
        }
    }
}