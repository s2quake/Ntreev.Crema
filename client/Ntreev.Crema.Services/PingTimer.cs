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

        public PingTimer(Func<bool> action)
        {
            this.action = action;
            this.timer = new Timer(pingInterval);
            this.timer.Elapsed += Timer_Elapsed;
            this.timer.Start();
        }

        public void Dispose()
        {
            this.timer.Stop();
            this.timer.Dispose();
            this.timer = null;
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

            }
            finally
            {
                this.isProgressing = false;
            }
        }
    }
}
