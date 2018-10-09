using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ntreev.Crema.Repository.Git
{
    class GitAddCommand : GitCommand
    {
        private readonly List<GitPath> items;

        public GitAddCommand(string basePath, GitPath[] items)
            : base(basePath, "add")
        {
            this.items = new List<GitPath>(items);
        }

        protected override void OnRun()
        {
            var itemList = new Queue<string>(this.items.Select(item => $"{item}"));
            var commandLength = this.ToString().Length;

            while (itemList.Any())
            {
                var item = itemList.Dequeue();
                if (commandLength + item.Length + 1 > 1024)
                {
                    base.OnRun();
                    this.Clear();
                    commandLength = this.ToString().Length;
                }
                commandLength += item.Length + 1;
                this.Add(item);
            }

            base.OnRun();
        }
    }
}
