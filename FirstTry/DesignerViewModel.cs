using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Activities;
using System.Activities.Core.Presentation;
using System.Activities.Presentation;
using System.Activities.Presentation.Metadata;
using System.Activities.Presentation.Toolbox;


namespace FirstTry
{
    class DesignerViewModel
    {
        public ICommand NewCommand { get; set; }
        public ICommand OpenCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand SaveAsCommand { get; set; }
        public ICommand ExitCommand { get; set; }
    }

    public string FileName
    {
        get
        {
            return this.fileName;
        }

        private set
        {
            this.fileName = value;
            this.Title = string.Format("{0} - {1}", title, this.FileName);
        }
    }

    public WorkflowDesigner RehostedWFDesigner
    {
        get
        {
            return this.rehostedWFDesigner;
        }

        private set
        {
            this.rehostedWFDesigner = value;
            this.NotifyChanged("WorkflowDesignerControl");
            this.NotifyChanged("WorkflowPropertyControl");
        }
    }

    public object WorkflowDesignerControl
    {
        get
        {
            return this.RehostedWFDesigner.View;
        }
    }

    public object WorkflowPropertyControl
    {
        get
        {
            return this.RehostedWFDesigner.PropertyInspectorView;
        }
    }

    public string XAML
    {
        get
        {
            if (this.RehostedWFDesigner.Text != null)
            {
                this.RehostedWFDesigner.Flush();
                return this.RehostedWFDesigner.Text;
            }

            return null;
        }
    }

    public DesignerViewModel()
    {
        (new DesignerMetadata()).Register();
        LoadBuiltInActivityIcons();
        this.WFToolboxControl = CreateWFToolbox();
        this.NewCommand = new RunCommand(this.ExecuteNew, CanExecuteNew);
        this.OpenCommand = new RunCommand(this.ExecuteOpen, CanExecuteOpen);
        this.SaveCommand = new RunCommand(this.ExecuteSave, CanExecuteSave);
        this.SaveAsCommand = new RunCommand(this.ExecuteSaveAs, CanExecuteSaveAs);
        this.ExitCommand = new RunCommand(this.ExecuteExit, CanExecuteExit);
        this.RehostedWFDesigner = new WorkflowDesigner();
    }

    private static void LoadBuiltInActivityIcons()
    {
        try
        {
            var sourcAssembly = Assembly.LoadFrom(@"Lib\Microsoft.VisualStudio.Activities.dll");

            var builder = new AttributeTableBuilder();

            if (sourcAssembly != null)
            {
                var stream =
                    sourcAssembly.GetManifestResourceStream(
                        "Microsoft.VisualStudio.Activities.Resources.resources");
                if (stream != null)
                {
                    var resourceReader = new ResourceReader(stream);

                    foreach (var itemType in
                        typeof(Activity).Assembly.GetTypes().Where(
                            t => t.Namespace == "System.Activities.Statements"))
                    {
                        SetToolboxBitmapAttribute(builder, resourceReader, itemType);
                    }
                }
            }

            MetadataStore.AddAttributeTable(builder.CreateTable());
        }
        catch (FileNotFoundException)
        {

        }
    }

    /// <summary>  
    /// Creates a Workflow toolbox.  
    /// </summary>  
    /// <param name="obj"></param>  
    private static ToolboxControl CreateWFToolbox()
    {
        var toolboxControl = new ToolboxControl();

        toolboxControl.Categories.Add(
            new ToolboxCategory("Control Flow")
                {
                       new ToolboxItemWrapper(typeof(DoWhile)),
                       new ToolboxItemWrapper(typeof(ForEach<>)),
                       new ToolboxItemWrapper(typeof(If)),
                       new ToolboxItemWrapper(typeof(Parallel)),
                       new ToolboxItemWrapper(typeof(ParallelForEach<>)),
                       new ToolboxItemWrapper(typeof(Pick)),
                       new ToolboxItemWrapper(typeof(PickBranch)),
                       new ToolboxItemWrapper(typeof(Sequence)),
                       new ToolboxItemWrapper(typeof(Switch<>)),
                       new ToolboxItemWrapper(typeof(While)),
                });

        toolboxControl.Categories.Add(
            new ToolboxCategory("Primitives")
                {
                       new ToolboxItemWrapper(typeof(Assign)),
                       new ToolboxItemWrapper(typeof(Delay)),
                       new ToolboxItemWrapper(typeof(InvokeMethod)),
                       new ToolboxItemWrapper(typeof(WriteLine)),
                });

        toolboxControl.Categories.Add(
            new ToolboxCategory("Error Handling")
                {
                       new ToolboxItemWrapper(typeof(Rethrow)),
                       new ToolboxItemWrapper(typeof(Throw)),
                       new ToolboxItemWrapper(typeof(TryCatch)),
                });

        return toolboxControl;
    }

    private void ExecuteNew(object obj)
    {
        this.RehostedWFDesigner = new WorkflowDesigner();
        this.RehostedWFDesigner.ModelChanged += this.WorkflowDesignerModelChanged;
        if (File.Exists(SampleWorkflow))
        {
            this.RehostedWFDesigner.Load(SampleWorkflow);
        }
        else
        {
            this.RehostedWFDesigner.Load(new Sequence());
        }

        this.RehostedWFDesigner.Flush();
        this.FileName = UnNamedFile;
    }

    private void ExecuteOpen(object obj)
    {
        var openFileDialog = new OpenFileDialog();
        if (openFileDialog.ShowDialog(Application.Current.MainWindow).Value)
        {
            this.LoadWF(openFileDialog.FileName);
        }
    }

    private void ExecuteSave(object obj)
    {
        if (this.FileName == UnNamedFile)
        {
            this.ExecuteSaveAs(obj);
        }
        else
        {
            this.Save();
        }
    }

    private void ExecuteSaveAs(object obj)
    {
        var saveFileDialog = new SaveFileDialog
        {
            AddExtension = true,
            DefaultExt = "xaml",
            FileName = this.FileName,
            Filter = "xaml files (*.xaml) | *.xaml;*.xamlx| All Files | *.*"
        };

        if (saveFileDialog.ShowDialog().Value)
        {
            this.FileName = saveFileDialog.FileName;
            this.Save();
        }
    }

    private void ExecuteExit(object obj)
    {
        Application.Current.Shutdown();
    }

    private void LoadWF(string name)
    {
        this.ResolveAssemblies(name);
        this.FileName = name;
        this.RehostedWFDesigner = new WorkflowDesigner();
        this.RehostedWFDesigner.ModelChanged += this.WorkflowDesignerModelChanged;
        this.RehostedWFDesigner.Load(name);
    }

    private void ResolveAssemblies(string name)
    {
        var references = WorkflowCLR.LoadWorkflow(name);

        var query = from reference in references.References where !reference.Loaded select reference;
        foreach (var workflowCLRAssembly in query)
        {
            this.CheckAssemblies(workflowCLRAssembly);
        }
    }

    private void CheckAssemblies(WorkflowCLRAssembly workflowCLRAssembly)
    {
        var openFileDialog = new OpenFileDialog
        {
            FileName = workflowCLRAssembly.CodeBase,
            CheckFileExists = true,
            Filter = "Assemblies (*.dll;*.exe)|*.dll;*.exe|All Files|*.*",
        };

        if (openFileDialog.ShowDialog(Application.Current.MainWindow).Value)
        {
            if (!workflowCLRAssembly.LoadWorkflow(openFileDialog.FileName))
            {
                MessageBox.Show("Error loading assembly...");
            }
        }
    }
}
