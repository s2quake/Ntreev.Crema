using System.Collections.Generic;

namespace JSSoft.Crema.Bot
{
    public class TaskStack : Stack<object>
    {
        public object Current => this.Peek();
    }
}
