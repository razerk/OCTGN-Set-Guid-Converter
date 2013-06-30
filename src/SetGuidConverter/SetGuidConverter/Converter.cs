using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SetGuidConverter
{
    using System.Threading.Tasks;

    public class Converter: IDisposable
    {
        public event Action<string> OnEvent;
        internal string Directory { get; private set; }

        public Converter(string directory)
        {
            Directory = directory;
        }

        public Task<bool> Verify()
        {
            return Task.Factory.StartNew(()=>RealVerify());
        }

        public Task Convert()
        {
            return Task.Factory.StartNew(this.RealConvert);
        }

        internal void RealConvert()
        {
            //Todo convert set guids to random guids.
        }

        internal bool RealVerify()
        {
            //Todo verify the directory is a valid octgn directory
            return true;
        }

        internal void FireOnEvent(string str, params object[] args)
        {
            if (OnEvent != null)
            {
                OnEvent(String.Format(str, args));
            }
        }

        public void Dispose()
        {
            if (OnEvent == null) return;
            foreach (var a in OnEvent.GetInvocationList())
            {
                this.OnEvent -= a as Action<string>;
            }
        }
    }
}
