using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirstTry
{
    class WorkflowCLRAssembly
    {
        private void GetAsssemblyName()
        {
            var parts = this.ClrNamespace.Split(';');

            if (parts.Length == 2)
            {
                var namevalue = parts[1].Split('=');

                if (namevalue.Length == 2)
                {
                    var name = namevalue[1].Trim();
                    this.assemblyName = new AssemblyName(name);
                }
            }
        }

        private void LoadWFDesignerAssembly()
        {
            if (this.assembly != null && !this.IsFrameworkAssembly())
            {
                var file = new Uri(assembly.CodeBase).AbsolutePath;
                var name = file.Insert(file.LastIndexOf('.'), ".design");

                try
                {
                    this.DesignerAssembly = Assembly.LoadFile(name);

                    this.CopyWFAssemblies(Assembly);
                    this.CopyWFAssemblies(DesignerAssembly);
                    RegisterWFDesigner();
                }
                catch (FileLoadException)
                {
                }
                catch (FileNotFoundException)
                {
                }
            }
        }

        private void RegisterWFDesigner()
        {
            foreach (var metadataType in DesignerAssembly.GetTypes().Where(t => typeof(IRegisterMetadata).IsAssignableFrom(t)))
            {
                var metadata = Activator.CreateInstance(metadataType) as IRegisterMetadata;
                if (metadata != null)
                {
                    metadata.Register();
                }
            }
        }

        private bool TryLoadWorkflow()
        {
            try
            {
                this.Assembly = Assembly.Load(this.assemblyName);
                return true;
            }
            catch (FileLoadException)
            {
            }
            catch (FileNotFoundException)
            {
            }

            return false;
        }

        private bool TryLoadWorkflow(string fxKey)
        {
            try
            {
                this.Assembly = Assembly.Load(this.GetNameByKey(fxKey));
                return true;
            }
            catch (FileLoadException)
            {
            }
            catch (FileNotFoundException)
            {
            }
            return false;
        }

        public bool LoadWorkflow()
        {
            return this.IsFrameworkAssembly() ? this.TryLoadWorkflow(PublicTokenKey1) || this.TryLoadWorkflow(PublicTokenKey2) : this.TryLoadWorkflow();
        }

        public bool LoadWorkflow(string fileName)
        {
            try
            {
                this.Assembly = Assembly.LoadFile(fileName);
                return true;
            }
            catch (FileLoadException)
            {
            }
            catch (FileNotFoundException)
            {
            }

            return false;
        }
    }
}
