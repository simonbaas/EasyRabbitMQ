using System;

namespace EasyRabbitMQ.Infrastructure
{
    internal class DisposableAction : IDisposable
    {
        private readonly Action _action;

        public DisposableAction(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            _action = action;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                _action();
            }
        }
    }
}
