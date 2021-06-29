// Released under the MIT License.
// 
// Copyright (c) 2018 Ntreev Soft co., Ltd.
// Copyright (c) 2020 Jeesu Choi
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit
// persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the
// Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// Forked from https://github.com/NtreevSoft/Crema
// Namespaces and files starting with "Ntreev" have been renamed to "JSSoft".

using JSSoft.Communication;
using JSSoft.Crema.ServiceHosts.Properties;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using JSSoft.Library;
using JSSoft.Library.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading.Tasks;

namespace JSSoft.Crema.ServiceHosts
{
    class Peer
    {
        private static readonly Dictionary<Guid, Peer> peerByToken = new();
        private readonly Dictionary<Guid, Authentication> authenticaitonByToken = new();
        private readonly Dictionary<Guid, string> userIDByToken = new();
        private readonly HashSet<string> userIDs = new();

        public Peer(Guid id)
        {
            this.ID = id;
            peerByToken.Add(id, this);
        }

        public void Dispose()
        {
            peerByToken.Remove(this.ID);
        }

        public static Peer GetPeer(Guid id)
        {
            return peerByToken[id];
        }

        public Guid ID { get; }

        public bool Contains(string userID)
        {
            return this.userIDs.Contains(userID);
        }

        public Authentication this[Guid authenticationToken]
        {
            get
            {
                if (authenticationToken == Authentication.System.Token)
                {
                    return Authentication.System;
                }
                return this.authenticaitonByToken[authenticationToken];
            }
        }

        public Authentication[] Authentications => this.authenticaitonByToken.Values.ToArray();

        public void Add(Guid authenticationToken, Authentication authentication)
        {
            this.authenticaitonByToken.Add(authenticationToken, authentication);
            this.userIDByToken.Add(authenticationToken, authentication.ID);
            this.userIDs.Add(authentication.ID);
        }

        public void Remove(Guid authenticationToken)
        {
            this.userIDs.Remove(this.userIDByToken[authenticationToken]);
            this.userIDByToken.Remove(authenticationToken);
            this.authenticaitonByToken.Remove(authenticationToken);
        }


    }
}
