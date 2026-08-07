using Microsoft.Win32;
using ST.Library.UI.NodeEditor;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WinNodeEditorDemo.NumberNode;

namespace WpfNodeEditorDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml.
    /// </summary>
    public partial class MainWindow : Window
    {
        private STNodeEditorHelper? _editorHelper;
        private string? _flowFile;
        private bool _initialized;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;
            STNodePropertyGrid1.Text = "Node Property";

            Assembly demoAssembly = Assembly.GetExecutingAssembly();
            STNodeEditorMain.LoadAssembly(demoAssembly);
            STNodeTreeView1.LoadAssembly();

            _editorHelper = new STNodeEditorHelper(STNodeEditorMain, STNodeTreeView1, STNodePropertyGrid1);
            _editorHelper.PropertyEditorRequested += EditorHelper_PropertyEditorRequested;
            STNodeEditorMain.ActiveChanged += STNodeEditorMain_ActiveChanged;
            DataContext = _editorHelper;

            if (STNodeEditorMain.Nodes.Count == 0)
            {
                CreateSampleCanvas();
            }
        }

        private void CreateSampleCanvas()
        {
            var first = new NumberInputNode { Number = 8 };
            var second = new NumberInputNode { Number = 12 };
            var third = new NumberInputNode { Number = 20 };
            var firstSum = new NumberAddNode();
            var total = new NumberAddNode();
            var attributes = new WinNodeEditorDemo.AttrTestNode
            {
                Color = System.Drawing.Color.CornflowerBlue,
                Bool = true,
                Int = 42,
                Float = 3.14f
            };

            STNodeEditorMain.Nodes.Add(first);
            STNodeEditorMain.Nodes.Add(second);
            STNodeEditorMain.Nodes.Add(third);
            STNodeEditorMain.Nodes.Add(firstSum);
            STNodeEditorMain.Nodes.Add(total);
            STNodeEditorMain.Nodes.Add(attributes);

            first.GetAllOutputOptions()[0].ConnectOption(firstSum.GetAllInputOptions()[0]);
            second.GetAllOutputOptions()[0].ConnectOption(firstSum.GetAllInputOptions()[1]);
            firstSum.GetAllOutputOptions()[0].ConnectOption(total.GetAllInputOptions()[0]);
            third.GetAllOutputOptions()[0].ConnectOption(total.GetAllInputOptions()[1]);

            // Re-publish source values after the graph is connected.
            first.Number = first.Number;
            second.Number = second.Number;
            third.Number = third.Number;

            STNodeEditorMain.AddSelectedNode(attributes);
            STNodeEditorMain.SetActiveNode(attributes);
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                _editorHelper?.AutoArrange();
                STNodeEditorMain.ClearHistory();
            }));
        }

        private void STNodeEditorMain_ActiveChanged(object? sender, EventArgs e)
        {
            if (STNodeEditorMain.ActiveNode != null)
            {
                PropertiesTab.IsSelected = true;
            }
        }

        private void EditorHelper_PropertyEditorRequested(object? sender, EventArgs e)
        {
            PropertiesTab.IsSelected = true;
        }

        private void AddNode_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement placementTarget)
            {
                _editorHelper?.ShowAddNodeMenu(placementTarget);
            }
        }

        private void Execute_Click(object sender, RoutedEventArgs e)
        {
            NumberInputNode[] inputs = STNodeEditorMain.Nodes.Cast<STNode>()
                .OfType<NumberInputNode>()
                .ToArray();
            foreach (NumberInputNode input in inputs)
            {
                input.Number = input.Number;
            }

            string message = inputs.Length > 0
                ? $"示例已执行：刷新 {inputs.Length} 个输入节点"
                : "当前画布没有 NumberInput 示例节点";
            STNodeEditorMain.ShowAlert(
                message,
                System.Drawing.Color.White,
                System.Drawing.Color.FromArgb(220, 36, 143, 81),
                1600,
                AlertLocation.RightBottom,
                true);
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            STNodeEditorMain.Undo();
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            STNodeEditorMain.Redo();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            STNodeEditorMain.DeleteSelectedNodes();
        }

        private void AutoLayout_Click(object sender, RoutedEventArgs e)
        {
            _editorHelper?.AutoArrange();
        }

        private void Properties_Click(object sender, RoutedEventArgs e)
        {
            PropertiesTab.IsSelected = true;
        }

        private void Button_Click_Open(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                DefaultExt = ".stn",
                Filter = "STNodeEditor canvas (*.stn)|*.stn|All files (*.*)|*.*",
                CheckFileExists = true
            };
            if (dialog.ShowDialog(this) != true)
            {
                return;
            }

            OpenFlow(dialog.FileName);
        }

        private void OpenFlow(string flowName)
        {
            try
            {
                STNodeEditorMain.LoadCanvas(flowName);
                _flowFile = flowName;
                STNodeEditorMain.ClearHistory();
                Title = $"STNodeEditor WPF Demo - {Path.GetFileName(flowName)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Unable to open canvas", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Click_New(object sender, RoutedEventArgs e)
        {
            ResetCanvas();
        }

        private void Button_Click_Clear(object sender, RoutedEventArgs e)
        {
            ResetCanvas();
        }

        private void ResetCanvas()
        {
            STNodeEditorMain.Nodes.Clear();
            STNodeEditorMain.ClearHistory();
            _flowFile = null;
            Title = "STNodeEditor WPF Demo";
        }

        private void Button_Click_Save(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void Save()
        {
            if (string.IsNullOrWhiteSpace(_flowFile))
            {
                var dialog = new SaveFileDialog
                {
                    DefaultExt = ".stn",
                    Filter = "STNodeEditor canvas (*.stn)|*.stn|All files (*.*)|*.*",
                    AddExtension = true,
                    Title = "Save canvas"
                };
                if (dialog.ShowDialog(this) != true)
                {
                    return;
                }
                _flowFile = dialog.FileName;
            }

            try
            {
                string fullPath = Path.GetFullPath(_flowFile);
                string? directory = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                STNodeEditorMain.SaveCanvas(fullPath);
                STNodeEditorMain.MarkSaved();
                Title = $"STNodeEditor WPF Demo - {Path.GetFileName(fullPath)}";
                MessageBox.Show(this, "Canvas saved.", "STNodeEditor", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Unable to save canvas", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AutoAlignment_Click(object sender, RoutedEventArgs e)
        {
            STNodeEditorMain.FitCanvasToNodes();
        }

        protected override void OnClosed(EventArgs e)
        {
            STNodeEditorMain.ActiveChanged -= STNodeEditorMain_ActiveChanged;
            if (_editorHelper != null)
            {
                _editorHelper.PropertyEditorRequested -= EditorHelper_PropertyEditorRequested;
            }
            _editorHelper?.Dispose();
            STNodeTreeView1.Dispose();
            STNodePropertyGrid1.Dispose();
            STNodeEditorMain.Dispose();
            base.OnClosed(e);
        }
    }
}
