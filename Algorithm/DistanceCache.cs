namespace GeneticTAP.Algorithm
{
    /// <summary>
    /// Avoid having to calculate distances between pubs more than once.
    /// </summary>

    // fixme: this is rarted
    internal class DistanceCache
    {
        private readonly Dictionary<(int, int), double> _distances = new Dictionary<(int, int), double>();
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly Func<int, int, double> _distanceFunction;
        public DistanceCache(Func<int, int, double> distanceFunction)
        {
            _distanceFunction = distanceFunction;
        }
        /// <summary>
        /// If the distance between pub1 and pub2 has already been calculated, return it.
        /// Otherwise calculate it, add it to the cache, and return it.
        /// </summary>
        public double GetCreateDistance(int pub1, int pub2)
        {
            var key = pub1 < pub2
                ? (pub1, pub2)
                : (pub2, pub1);

            _lock.EnterReadLock();
            try
            {
                if (_distances.TryGetValue(key, out var distance))
                {
                    return distance;
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }


            _lock.EnterWriteLock();
            try
            {
                // check again in case another thread calculated the distance while we were waiting for the write lock
                if (!_distances.TryGetValue(key, out var distance))
                {
                    distance = _distanceFunction(pub1, pub2);
                    _distances.Add(key, distance);
                }
                return distance;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}
