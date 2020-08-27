using System.Collections.Generic;

namespace Ntreev.Crema.Bot
{
    public class TaskStack : Stack<object>
    {
        public object Current => this.Peek();
    }
}
