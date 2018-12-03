using Ntreev.Library;
using Ntreev.ModernUI.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Ntreev.Crema.ApplicationHost.Dialogs.ViewModels
{
    class LoginViewModel : ModalDialogBase
    {
        private string userID;
        private SecureString password;

        public LoginViewModel()
        {
            this.DisplayName = "Login";
        }

        public void Login()
        {
            this.TryClose(true);
        }

        public string UserID
        {
            get => this.userID ?? string.Empty;
            set
            {
                this.userID = value;
                this.NotifyOfPropertyChange(nameof(this.UserID));
            }
        }

        public SecureString Password
        {
            get { return this.password; }
            set
            {
                this.password = value;
                this.NotifyOfPropertyChange(nameof(this.Password));
                this.NotifyOfPropertyChange(nameof(this.CanLogin));
            }
        }

        public bool CanLogin
        {
            get
            {
                if (this.UserID == string.Empty)
                    return false;
                if (this.Password == null)
                    return false;
                return this.IsProgressing == false;
            }
        }
    }
}
