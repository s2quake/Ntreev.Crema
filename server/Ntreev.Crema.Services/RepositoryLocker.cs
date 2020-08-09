using System;
using System.IO;

namespace Ntreev.Crema.Services
{
    class RepositoryLocker : IDisposable
    {
        private readonly CremaHost cremaHost;
        private readonly string path;
        private Stream stream;

        public RepositoryLocker(CremaHost cremaHost)
        {
            this.cremaHost = cremaHost;
            this.path = this.cremaHost.GetPath(CremaPath.Repository, CremaString.Lock);
            if (File.Exists(this.path) == true)
            {
                try
                {
                    this.stream = new FileStream(this.path, FileMode.Open, FileAccess.Read, FileShare.None);
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException("repository is used.", e);
                }
            }
            else
            {
                this.stream = new FileStream(this.path, FileMode.Create, FileAccess.Write, FileShare.None);
            }
        }

        public void Dispose()
        {
            if (this.stream != null)
            {
                this.stream.Dispose();
                this.stream = null;
                File.Delete(this.path);
            }
        }
    }
}
