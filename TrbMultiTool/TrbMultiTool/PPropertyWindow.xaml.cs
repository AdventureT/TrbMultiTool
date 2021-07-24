using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TrbMultiTool.FileFormats;
using TrbMultiTool.PPropertyTools;
using static TrbMultiTool.FileFormats.PProperty;

namespace TrbMultiTool
{
    /// <summary>
    /// Interaction logic for QuestWindow.xaml
    /// </summary>
    public partial class PPropertyWindow : Window
    {
        public List<TypeContent> TypeContentss { get; set; } = new();

        public List<TypeContent> TypeContentsWithString { get; set; } = new();

        public ContextMenu contextMenu { get; set; } = new();

        public PPropertyWindow()
        {
            InitializeComponent();

            MenuItem addButton = new();
            addButton.Header = "Create New";

            MenuItem removeButton = new();
            removeButton.Header = "Remove";
            removeButton.Click += RemoveButton_Click;

            // create subbuttons
            MenuItem addStringButton = new();
            addStringButton.Header = "String";
            addStringButton.Click += AddStringButton_Click;
            addButton.Items.Add(addStringButton);

            MenuItem addIntButton = new();
            addIntButton.Header = "Int";
            addIntButton.Click += AddIntButton_Click;
            addButton.Items.Add(addIntButton);

            MenuItem addUIntButton = new();
            addUIntButton.Header = "UInt";
            addUIntButton.Click += AddUIntButton_Click;
            addButton.Items.Add(addUIntButton);

            MenuItem addBooleanButton = new();
            addBooleanButton.Header = "Boolean";
            addBooleanButton.Click += AddBooleanButton_Click;
            addButton.Items.Add(addBooleanButton);

            MenuItem addArrayButton = new();
            addArrayButton.Header = "Array";
            addArrayButton.Click += AddArrayButton_Click;
            addButton.Items.Add(addArrayButton);

            MenuItem addSubButton = new();
            addSubButton.Header = "SubItem";
            addSubButton.Click += AddSubButton_Click;
            addButton.Items.Add(addSubButton);

            contextMenu.Items.Add(addButton);
            contextMenu.Items.Add(removeButton);
        }

        private void AddSubButton_Click(object sender, EventArgs e)
        {
            AddNewItem(PPropertyItemType.SubItem, null, "New SubItem");
        }

        private void AddArrayButton_Click(object sender, EventArgs e)
        {
            AddNewItem(PPropertyItemType.Array, null, "New Array");
        }

        private void AddBooleanButton_Click(object sender, EventArgs e)
        {
            AddNewItem(PPropertyItemType.Bool, "False", "New Boolean");
        }

        private void AddUIntButton_Click(object sender, EventArgs e)
        {
            AddNewItem(PPropertyItemType.UInt, "0", "New UInt");
        }

        private void AddIntButton_Click(object sender, EventArgs e)
        {
            AddNewItem(PPropertyItemType.Int, "0", "New Int");
        }

        private void AddStringButton_Click(object sender, EventArgs e)
        {
            AddNewItem(PPropertyItemType.String, "New Value", "New String");
        }

        private void AddNewItem(PPropertyItemType type, string value, string propertyName = "New Property")
        {
            if (treeView.SelectedItem == null) return;
            var tvi = (TreeViewItem)treeView.SelectedItem;
            var tag = tvi.Tag as TypeContent;

            if (tag == null || tag.Type == PPropertyItemType.Array || tag.Type == PPropertyItemType.SubItem)
            {
                var item = new TreeViewItem
                {
                    Header = tag.Type == PPropertyItemType.Array ? value : propertyName,
                    Tag = new TypeContent(type, value, 0, 0, tag != null ? tag.Type == PPropertyItemType.Array ? true : false : false)
                };

                tvi.Items.Add(item);

                item.IsSelected = true;
                tvi.IsExpanded = true;
            }
        }

        private void RemoveButton_Click(object sender, System.EventArgs e)
        {
            if (treeView.SelectedItem == null || (treeView.SelectedItem as TreeViewItem).Tag == null) return;
            var tvi = (TreeViewItem)treeView.SelectedItem;

            var root = GetItemRoot(treeView.Items.GetItemAt(0) as TreeViewItem, tvi);
            if (root != null)
                root.Items.Remove(tvi);
        }

        public TreeViewItem GetItemRoot(TreeViewItem searchIn, TreeViewItem target)
        {
            foreach (TreeViewItem item in searchIn.Items)
            {
                if (target == item)
                {
                    return searchIn;
                }
                else if (item.Items.Count > 0)
                {
                    var found = GetItemRoot(item, target);
                    if (found != null) return found;
                }
            }

            return null;
        }

        private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            UpdateFields();
        }

        private void UpdateFields()
        {
            var tvi = (TreeViewItem)treeView.SelectedItem;

            if (tvi.Tag != null)
            {
                var tag = tvi.Tag as TypeContent;
                label.Text = tag.Type.ToString();

                if (tag.InArray)
                    propertyName.IsEnabled = false;
                else
                    propertyName.IsEnabled = true;

                if (tag.Type == PPropertyItemType.Array || tag.Type == PPropertyItemType.SubItem)
                    propertyValue.IsEnabled = false;
                else
                    propertyValue.IsEnabled = true;

                if (tag.Type == PPropertyItemType.String)
                    swapButton.IsEnabled = true;
                else
                    swapButton.IsEnabled = false;

                propertyId.Text = tag.index.ToString();

                propertyName.Text = tvi.Header.ToString();
                propertyValue.Text = tag.Value;
            }
        }

        private void swapButton_Click(object sender, RoutedEventArgs e)
        {
            if (comboBox.SelectedIndex == -1 || treeView.SelectedItem == null || (treeView.SelectedItem as TreeViewItem).Tag == null) return;

            var typeContentCb = TypeContentsWithString[comboBox.SelectedIndex];
            var tvi = (TreeViewItem)treeView.SelectedItem;
            var tag = tvi.Tag as TypeContent;

            if (tag.Type != PPropertyItemType.Array && tag.Type != PPropertyItemType.SubItem)
            {
                tag.Value = typeContentCb.Value;

                if (tag.InArray)
                    tvi.Header = typeContentCb.Value;
            }

            UpdateFields();
        }

        private void Apply(object sender, RoutedEventArgs e)
        {
            if (treeView.SelectedItem == null) return;
            var tvi = (TreeViewItem)treeView.SelectedItem;
            var tag = tvi.Tag as TypeContent;

            if (tag.InArray)
                tvi.Header = propertyValue.Text;
            else 
                tvi.Header = propertyName.Text;

            tag.Value = propertyValue.Text;

            UpdateFields();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TypeContentsWithString = TypeContentss.Where(x => x.Type == PPropertyItemType.String).GroupBy(p => p.Value).Select(grp => grp.FirstOrDefault()).ToList();
            var tCWS2 = TypeContentsWithString.Select(x => x.Value);
            comboBox.Items.Clear();

            foreach (TreeViewItem item in treeView.Items)
            {
                item.ContextMenu = contextMenu;
            }

            foreach (var item in tCWS2)
                comboBox.Items.Add(item);

            ApplyToolTips((treeView.Items.GetItemAt(0) as TreeViewItem).Items);
        }

        private void ScanItemsAndAdd(ref StringsLibrary stringsLibrary, ref List<PPropertyItem> properties, ItemCollection items, bool inArray = false)
        {
            foreach (TreeViewItem item in items)
            {
                var typeContent = item.Tag as TypeContent;
                StringInfo strName = new("");

                if (!inArray)
                    strName = stringsLibrary.Add(item.Header.ToString());

                switch (typeContent.Type)
                {
                    case PPropertyItemType.String:
                        var strValue = stringsLibrary.Add(typeContent.Value);
                        if (inArray)
                            properties.Add(new PPropertyString(strValue));
                        else
                            properties.Add(new PPropertyString(strName, strValue));
                        break;
                    case PPropertyItemType.Int:
                        if (inArray)
                            properties.Add(new PPropertyInt(Convert.ToInt32(typeContent.Value)));
                        else
                            properties.Add(new PPropertyInt(strName, Convert.ToInt32(typeContent.Value)));
                        break;
                    case PPropertyItemType.UInt:
                        if (inArray)
                            properties.Add(new PPropertyUInt(Convert.ToUInt32(typeContent.Value)));
                        else
                            properties.Add(new PPropertyUInt(strName, Convert.ToUInt32(typeContent.Value)));
                        break;
                    case PPropertyItemType.Bool:
                        if (inArray)
                            properties.Add(new PPropertyBool(typeContent.Value.ToLower() == "true"));
                        else
                            properties.Add(new PPropertyBool(strName, typeContent.Value.ToLower() == "true"));
                        break;
                    case PPropertyItemType.Float:
                        if (inArray)
                            properties.Add(new PPropertyFloat((float)Convert.ToDouble(typeContent.Value)));
                        else
                            properties.Add(new PPropertyFloat(strName, (float)Convert.ToDouble(typeContent.Value)));
                        break;
                    case PPropertyItemType.Array:
                        var arrProperty = new PPropertyArray(strName);
                        ScanItemsAndAdd(ref stringsLibrary, ref arrProperty.Value, item.Items, true);
                        properties.Add(arrProperty);
                        break;
                    case PPropertyItemType.SubItem:
                        var subProperty = new PPropertySubItem(strName);
                        ScanItemsAndAdd(ref stringsLibrary, ref subProperty.Value, item.Items);
                        properties.Add(subProperty);
                        break;
                }
            }
        }

        private void ScanItemsForExport(ref string data, ItemCollection items, int cycle = 0)
        {
            int index = 0;

            foreach (TreeViewItem item in items)
            {
                index++;
                var typeContent = item.Tag as TypeContent;

                if (item.Tag != null)
                {
                    var name = item.Header.ToString();

                    for (int i = 0; i < cycle; i++)
                        data += "----";

                    data += name;
                    switch (typeContent.Type)
                    {
                        case PPropertyItemType.Array:
                            data += $" [{typeContent.index}; Array]\n";
                            ScanItemsForExport(ref data, item.Items, cycle + 1);
                            break;
                        case PPropertyItemType.SubItem:
                            data += $" [{typeContent.index}; SubItem]\n";
                            ScanItemsForExport(ref data, item.Items, cycle + 1);
                            break;
                        default:
                            data += $" = {typeContent.Value} [{typeContent.index}; {typeContent.Type}]\n";
                            break;
                    }
                }

                if (cycle == 0) data += "\n";
            }
        }

        private void ApplyToolTips(ItemCollection items)
        {
            foreach (TreeViewItem item in items)
            {
                var typeContent = item.Tag as TypeContent;

                if (item.Tag != null)
                {
                    switch (typeContent.Type)
                    {
                        case PPropertyItemType.Array:
                            ApplyToolTips(item.Items);
                            break;
                        case PPropertyItemType.SubItem:
                            ApplyToolTips(item.Items);
                            break;
                        default:
                            item.ToolTip = typeContent.Value;
                            break;
                    }
                }
            }
        }

        private void SaveFile(string path)
        {
            var properties = new List<PPropertyItem>();
            var stringsLibrary = new StringsLibrary();

            ScanItemsAndAdd(ref stringsLibrary, ref properties, (treeView.Items.GetItemAt(0) as TreeViewItem).Items);
            Maker.MakeFile(path, stringsLibrary, properties);
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            SaveFile(Trb._fileName);
        }

        private void SaveAs(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "PProperty.trb";
            dlg.Filter = $"TRB File|*.trb|TRZ File|*.trz";

            if (dlg.ShowDialog() == true && !string.IsNullOrWhiteSpace(dlg.FileName))
                SaveFile(dlg.FileName);
        }

        private void Export(object sender, RoutedEventArgs e)
        {
            string data = "";
            ScanItemsForExport(ref data, (treeView.Items.GetItemAt(0) as TreeViewItem).Items);

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Exported.txt";
            dlg.Filter = $"Text File|*.txt";

            if (dlg.ShowDialog() == true && !string.IsNullOrWhiteSpace(dlg.FileName))
                File.WriteAllText(dlg.FileName, data);
        }

        private void FindById()
        {
            if (treeView.SelectedItem == null) return;
            var tvi = (TreeViewItem)treeView.SelectedItem;
            var root = GetItemRoot(treeView.Items.GetItemAt(0) as TreeViewItem, tvi);
            var tag = tvi.Tag as TypeContent;

            var id = Convert.ToInt32(searchId.Text);

            if (root.Items.Count > 0 && id < root.Items.Count)
            {
                var foundItem = root.Items[id] as TreeViewItem;
                foundItem.IsSelected = true;
                foundItem.IsExpanded = true;
            }
        }

        private void Find(object sender, RoutedEventArgs e)
        {
            FindById();
        }

        //IEnumerable<TreeViewItem> Collect(TreeView nodes)
        //{
        //    foreach (TreeViewItem node in nodes)
        //    {
        //        yield return node;

        //        foreach (var child in Collect(node.Items))
        //            yield return child;
        //    }
        //}
    }
}
