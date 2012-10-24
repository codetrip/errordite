using System;
using System.Net.Sockets;
using BookSleeve;
using CodeTrip.Core.Exceptions;

namespace CodeTrip.Core.Redis
{
    public interface IRedisSession
    {
        RedisConnection Connection { get; }
        bool EnsureConnectionIsOpen();
        bool TryOpenConnection();
    }

    public class RedisSession : ComponentBase, IRedisSession
    {
        private bool _connectionEstablished;
        private readonly RedisConfiguration _configuration;
        private readonly object _connectionSyncLock = new object();

        public RedisSession(RedisConfiguration redisConfiguration)
        {
            _configuration = redisConfiguration;
        }

        public RedisConnection Connection { get; private set; }

        public void Close()
        {
            if (_connectionEstablished && Connection != null)
            {
                Connection.Close(true);
                Connection.Dispose();
            }
        }

        public bool EnsureConnectionIsOpen()
        {
            if (!_connectionEstablished)
            {
                Trace("Connection not established with Redis");
                return false;
            } 

            if (Connection.State == RedisConnectionBase.ConnectionState.Open)
                return true;

            return TryOpenConnection();
        }

        public bool TryOpenConnection()
        {
            Trace("Attempting to establish connection to Redis ({0})...", _configuration.Endpoint);

            _connectionEstablished = false;

            lock (_connectionSyncLock)
            {
                //make sure queued threads do notattempt to reconnect after a successfull connection
                if (_connectionEstablished)
                    return true;

                if (Connection != null)
                {
                    Trace("...Previous connection established, disposing");

                    try
                    {
                        Connection.Close(true);
                        Connection.Dispose();
                        Trace("...Successfully disposed previous connection");
                    }
                    catch (Exception e)
                    {
                        Error(e);
                    }
                }

                try
                {
                    Trace("...Attempting to establish new connection");
                    Connection = new RedisConnection(_configuration.Endpoint, port: _configuration.Port, allowAdmin: true, syncTimeout: 5000);
                    var task = Connection.Open();
                    Connection.Wait(task);
                    _connectionEstablished = true;
                    Trace("...Connection established successfully");
                }
                catch (SocketException se)
                {
                    Error(new CodeTripRedisConnectivityException(string.Format("Unable to connect to Redis: {0}", se.Message), false, se));
                    _connectionEstablished = false;
                }
                catch (TimeoutException te)
                {
                    Error(new CodeTripRedisConnectivityException(string.Format("Unable to connect to Redis: {0}", te.Message), false, te));
                    _connectionEstablished = false;
                }
            }

            return _connectionEstablished;
        }
    }
}
