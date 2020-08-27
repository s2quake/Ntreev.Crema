namespace JSSoft.Crema.Bot
{
    public struct TaskStackItem
    {
        public object Target { get; set; }

        public int Count { get; set; }

        public override string ToString()
        {
            return this.Target?.ToString();
        }
    }
}
