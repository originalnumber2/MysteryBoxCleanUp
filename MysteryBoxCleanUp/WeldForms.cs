using System;
using System.IO;
using System.Windows.Forms;

namespace MysteryBoxCleanUp
{
    public class WeldForms
    {
        public WeldForms()
        {
        }

        private void weldToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DirectoryInfo d = new DirectoryInfo(@"C:\Users\VUWAL\Desktop\Weld Param Forms");//Assuming Test is your Folder
            FileInfo[] Files = d.GetFiles("*eld.txt"); //Getting Text files
            string[] str = new string[50];
            ToolStripMenuItem[] items = new ToolStripMenuItem[50];
            int i = 0;
            weldToolStripMenuItem.DropDownItems.Clear();
            foreach (FileInfo file in Files)
            {
                str[i] = file.Name;
                items[i] = new ToolStripMenuItem();
                items[i].Name = "dynamicItem";
                items[i].Tag = "specialDataHere";
                items[i].Text = Path.GetFileNameWithoutExtension(str[i]); ;
                items[i].Click += new EventHandler(MenuItemClickHandler);
                weldToolStripMenuItem.DropDownItems.Add(items[i]);
                i++;
            }
        }
        private void MenuItemClickHandler(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            WeldGUIForm frm = new WeldGUIForm(item.Text);
            frm.Show();
        }
    }
}
