using System.Collections.Concurrent;

namespace ExcelWebApp2.Repositories
{
    public interface IProcessingSessionStore
    {
        ProcessingSessionState Get(string userId);
        void Clear(string userId);
    }

    public class ProcessingSessionStore : IProcessingSessionStore
    {
        private readonly ConcurrentDictionary<string, ProcessingSessionState> _sessions = new();

        public ProcessingSessionState Get(string userId)
        {
            return _sessions.GetOrAdd(userId, _ => new ProcessingSessionState());
        }

        public void Clear(string userId)
        {
            _sessions.TryRemove(userId, out _);
        }
    }
}
