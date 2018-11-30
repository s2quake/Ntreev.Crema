using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Ntreev.Crema.Services
{
    class PingTimer : IDisposable
    {
        private readonly static double pingInterval = 15000;
        private readonly Func<bool> action;
        private Timer timer;
        private bool isProgressing;

        public PingTimer(Func<bool> action, int timeout)
        {
            this.action = action;
            if (timeout > 0)
            {
                this.timer = new Timer(Math.Max(pingInterval / 2, 1000));
                this.timer.Elapsed += Timer_Elapsed;
                this.timer.Start();
            }
        }

        public void Dispose()
        {
            if (this.timer != null)
            {
                this.timer.Stop();
                this.timer.Dispose();
            }
            this.timer = null;
        }

        public event EventHandler Faulted;

        protected virtual void OnFaulted(EventArgs e)
        {
            this.Faulted?.Invoke(this, e);
        }

        private async void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (this.isProgressing == true)
                return;
            try
            {
                this.isProgressing = true;
                await Task.Run(this.action);
            }
            catch
            {
                this.Dispose();
                this.OnFaulted(EventArgs.Empty);
            }
            finally
            {
                this.isProgressing = false;
            }
        }
    }
}
