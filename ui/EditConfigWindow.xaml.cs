using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace LantanaGroup.XmlHarvester
{
    /// <summary>
    /// Interaction logic for EditConfigWindow.xaml
    /// </summary>
    public partial class EditConfigWindow : Window
    {
        private string TestFileName = "C:\\Users\\sean.mcilvenna\\Code\\VIQRCXML2XLS\\XmlHarvester\\Samples\\Input\\CMS2v3_CatI_QrdaSample.xml";
        private XmlDocument xmlDoc;
        private XmlNamespaceManager nsManager;
        private MappingConfig config;

        public EditConfigWindow(string configFileName)
        {
            InitializeComponent();

            config = MappingConfig.LoadFromFileWithParents(configFileName);

            foreach (var column in config.Column)
                LoadColumn(column);

            foreach (var group in config.Group)
                LoadGroup(group);

            xmlDoc = new XmlDocument();
            xmlDoc.Load(TestFileName);

            nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("cda", "urn:hl7-org:v3");
        }

        private void LoadColumn(MappingColumn column, TreeViewItem parent = null)
        {
            TreeViewItem item = new TreeViewItem();
            item.Header = column.Name;
            item.Tag = column;

            if (parent == null)
                treeView.Items.Add(item);
            else
                parent.Items.Add(item);
        }

        private void LoadGroup(MappingGroup group, TreeViewItem parent = null)
        {
            TreeViewItem item = new TreeViewItem();
            item.Header = group.TableName;
            item.Tag = group;

            foreach (var column in group.Column)
                LoadColumn(column, item);

            foreach (var childGroup in group.Group)
                LoadGroup(childGroup, item);

            if (parent == null)
                treeView.Items.Add(item);
            else
                parent.Items.Add(item);
        }

        private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem item = e.NewValue as TreeViewItem;

            tableNameText.DataContext = null;
            prefixText.DataContext = null;
            columnNameText.DataContext = null;
            headingNameText.DataContext = null;
            xpathText.DataContext = null;

            if (item.Tag is MappingGroup)
            {
                tableNameText.IsEnabled = true;
                prefixText.IsEnabled = true;
                columnNameText.IsEnabled = false;
                headingNameText.IsEnabled = false;
                xpathText.IsEnabled = true;

                tableNameText.DataContext = item.Tag;
                prefixText.DataContext = item.Tag;
                xpathText.DataContext = item.Tag;
                xpathText.SetBinding(TextBox.TextProperty, "Context");
            }
            else if (item.Tag is MappingColumn)
            {
                tableNameText.IsEnabled = false;
                prefixText.IsEnabled = false;
                columnNameText.IsEnabled = true;
                headingNameText.IsEnabled = true;
                xpathText.IsEnabled = true;

                columnNameText.DataContext = item.Tag;
                headingNameText.DataContext = item.Tag;
                xpathText.DataContext = item.Tag;
                xpathText.SetBinding(TextBox.TextProperty, "Value");
            }
        }

        private void xpathText_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadResults();
        }

        private void xpathText_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            LoadResults();

            TreeViewItem item = treeView.SelectedItem as TreeViewItem;
            MappingGroup group = item.Tag as MappingGroup;
            MappingColumn column = item.Tag as MappingColumn;

            removeButton.IsEnabled =
                moveUpButton.IsEnabled =
                moveDownButton.IsEnabled = false;

            if (group != null)
            {
                ItemCollection list = item.Parent is TreeViewItem ? ((TreeViewItem)item.Parent).Items : ((TreeView)item.Parent).Items;
            }
        }

        private void LoadResults()
        {
            if (treeView.SelectedItem == null)
            {
                resultsText.Text = string.Empty;
                return;
            }

            TreeViewItem item = treeView.SelectedItem as TreeViewItem;
            MappingGroup group = item.Tag as MappingGroup;
            MappingColumn column = item.Tag as MappingColumn;

            if (group != null)
            {
                List<XmlNode> contexts = GetContext(group);
                string results = string.Empty;

                foreach (var context in contexts)
                {
                    results += context.OuterXml + "\r\n\r\n";
                }

                resultsText.Text = results;
            }
            else if (column != null)
            {
                var parent = item.Parent as TreeViewItem;
                List<XmlNode> contexts = parent != null ? GetContext(parent.Tag as MappingGroup) : new List<XmlNode>(new XmlNode[] { xmlDoc.DocumentElement });
                string results = string.Empty;

                foreach (var context in contexts)
                {
                    try
                    {
                        var selectedNodes = context.SelectNodes(column.Value, nsManager);

                        foreach (XmlNode selectedNode in selectedNodes)
                        {
                            results += selectedNode.InnerXml + "\r\n\r\n";
                        }
                    }
                    catch { }
                }

                resultsText.Text = results;
            }
        }

        private List<XmlNode> GetContext(MappingGroup group)
        {
            List<MappingGroup> groupList = new List<MappingGroup>();
            MappingGroup current = group;
            List<XmlNode> parentContexts = null;

            while (current != null)
            {
                groupList.Insert(0, current);
                current = current.Parent;
            }

            foreach (var next in groupList)
            {
                if (parentContexts == null)
                {
                    try
                    {
                        var selectedNodes = xmlDoc.SelectNodes(next.Context, nsManager);
                        parentContexts = new List<XmlNode>();

                        foreach (XmlNode selectedNode in selectedNodes)
                        {
                            parentContexts.Add(selectedNode);
                        }
                    }
                    catch { }
                }
                else
                {
                    var newParentContexts = new List<XmlNode>();

                    foreach (var parentContext in parentContexts)
                    {
                        try
                        {
                            var selectedNodes = parentContext.SelectNodes(next.Context, nsManager);

                            foreach (XmlNode selectedNode in selectedNodes)
                            {
                                newParentContexts.Add(selectedNode);
                            }
                        }
                        catch { }
                    }

                    parentContexts = newParentContexts;
                }
            }

            if (parentContexts == null)
                return new List<XmlNode>();

            return parentContexts;
        }

        private void addGroupButton_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = treeView.SelectedItem as TreeViewItem;
            ItemCollection treeList = null;
            List<MappingGroup> groupList = null;

            if (item == null || (item.Tag is MappingColumn && item.Parent is TreeView))
            {
                treeList = treeView.Items;
                groupList = config.Group;
            }
            else if (item.Tag is MappingGroup)
            {
                treeList = item.Items;
                groupList = ((MappingGroup)item.Tag).Group;
            }
            else if (item.Tag is MappingColumn && item.Parent is TreeViewItem && ((TreeViewItem)item.Parent).Tag is MappingGroup)
            {
                treeList = ((TreeViewItem)item.Parent).Items;
                groupList = ((MappingGroup)(((TreeViewItem)item.Parent).Tag)).Group;
            }

            MappingGroup newGroup = new MappingGroup();
            newGroup.TableName = "NewGroup";
            newGroup.Context = "./";
            groupList.Add(newGroup);

            TreeViewItem newGroupItem = new TreeViewItem();
            newGroupItem.Tag = newGroup;
            newGroupItem.Header = "NewGroup";

            treeList.Add(newGroupItem);
            newGroupItem.IsSelected = true;
        }

        private void addColumn_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = treeView.SelectedItem as TreeViewItem;
            ItemCollection treeList = null;
            List<MappingColumn> columnList = null;

            if (item == null)
            {
                treeList = treeView.Items;
                columnList = config.Column;
            }
            else if (item.Tag is MappingGroup)
            {
                treeList = item.Items;
                columnList = ((MappingGroup)item.Tag).Column;
            }
            else if (item.Tag is MappingColumn)
            {
                treeList = ((TreeViewItem)item.Parent).Items;
                columnList = item.Parent is TreeView ? config.Column : ((MappingGroup)((TreeViewItem)item.Parent).Tag).Column;
            }

            MappingColumn newColumn = new MappingColumn();
            newColumn.Name = "newColumn";
            newColumn.Value = "./";
            columnList.Add(newColumn);

            TreeViewItem newColumnItem = new TreeViewItem();
            newColumnItem.Header = "newColumn";
            newColumnItem.Tag = newColumn;

            TreeViewItem firstGroup = null;

            foreach (TreeViewItem nextItem in treeList)
            {
                if (nextItem.Tag is MappingGroup)
                {
                    firstGroup = nextItem;
                    break;
                }
            }

            if (firstGroup != null)
                treeList.Insert(treeList.IndexOf(firstGroup), newColumnItem);
            else
                treeList.Add(newColumnItem);

            newColumnItem.IsSelected = true;
        }

        private void removeButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void moveUpButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void moveDownButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
