using System;
using System.Text;

namespace NamespaceTreeFolderEditor
{
    public class IconPicker
    {
        public string FileName { get; set; }
        public int IconIndex { get; set; }

        public bool ShowDialog()
        {
            StringBuilder sb = new StringBuilder(FileName, 260);
            int iconIndex = IconIndex;
            int result = NativeMethods.PickIconDlg(IntPtr.Zero, sb, sb.Capacity, ref iconIndex);
            FileName = sb.ToString();
            IconIndex = iconIndex;
            return result == 1;
        }
    }
}
