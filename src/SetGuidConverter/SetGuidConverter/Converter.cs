using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SetGuidConverter
{
    using System.IO;
    using System.Threading.Tasks;
    using System.Xml.Linq;

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
            var path = Path.Combine(Directory, "Sets");
            var dr = new DirectoryInfo(path);
            foreach (var d in dr.GetDirectories("*", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    var setFile = d.GetFiles("set.xml").FirstOrDefault();
                    if (setFile == null) continue;
                    this.FireOnEvent("Converting {0} Set", d.Name);
                    var result = this.ConvertSetFile(setFile);

                    // Check for Markers or Cards directories
                    foreach (var sd in d.GetDirectories())
                    {
                        if (
                            sd.Name.Equals("markers", StringComparison.InvariantCultureIgnoreCase)
                            || sd.Name.Equals("cards", StringComparison.InvariantCultureIgnoreCase)
                            )
                        {
                            foreach (var f in sd.GetFiles())
                            {
                                var name = (Path.GetFileNameWithoutExtension(f.FullName) ?? "").ToLowerInvariant();
                                if (string.IsNullOrWhiteSpace(name)) continue;
                                Guid old;
                                if (Guid.TryParse(name, out old))
                                {
                                    if (result.ContainsKey(old))
                                    {
                                        this.FireOnEvent("Converting Set {0} {1} File", d.Name, sd.Name);
                                        this.RenameFile(f, result[old]);
                                    }
                                }
                            }
                        }
                    }
                    this.FireOnEvent("Finished converting {0}", d.Name);

                }
                catch (Exception e)
                {
                    this.FireOnEvent("Error: {0}",e.Message);
                }
            }
            this.FireOnEvent("Complete");
        }

        internal bool RealVerify()
        {
            var dr = new DirectoryInfo(Directory);
            if(!dr.Exists)
                throw new MessageException("Directory {0} doesn't exist!",Directory);
            if(!dr.GetFiles("definition.xml").Any())
                throw new MessageException("Directory is not a game directory.");
            if(!dr.GetDirectories("Sets").Any())
                throw new MessageException("Directory does not contain a 'Sets' directory.");
            return true;
        }

        /// <summary>
        /// Converts a set file. Returns old System.Guid and new System.Guid values
        /// </summary>
        /// <param name="file"></param>
        /// <returns>old System.Guid and new System.Guid values</returns>
        internal Dictionary<Guid,Guid> ConvertSetFile(FileInfo file)
        {
            var ret = new Dictionary<Guid, Guid>();
            try
            {
                XDocument doc = XDocument.Load(file.FullName);
                foreach (var m in doc.Root.Elements())
                {
                    foreach (var e in m.Elements())
                    {
                        if (m.Name.LocalName.Equals("marker", StringComparison.InvariantCultureIgnoreCase)
                            || m.Name.LocalName.Equals("card", StringComparison.InvariantCultureIgnoreCase))
                        {
                            var oldStr = m.Attribute("id").Value;
                            var old = Guid.Parse(oldStr);
                            var ng = Guid.NewGuid();
                            m.SetAttributeValue("id", ng.ToString().ToLowerInvariant());
                            ret.Add(old,ng);
                        }
                    }
                }
                doc.Save(file.FullName);

            }
            catch (Exception e)
            {
                this.FireOnEvent("Error: {0}",e.Message);
            } 
            return ret;
        }

        internal void RenameFile(FileInfo file, Guid newName)
        {
            file.MoveTo(file.FullName.Replace(file.Name,newName.ToString() + file.Extension));
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
