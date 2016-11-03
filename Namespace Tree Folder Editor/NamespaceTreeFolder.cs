using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NamespaceTreeFolderEditor
{
    public class NamespaceTreeFolder
    {
        public string Guid { get; set; }
        public string Name { get; }
        public string Path { get; }
        public int Index { get; }
        public string IconPath { get; }
        public int IconIndex { get; }
        public bool ShowOnDesktop { get; }
        public bool Enabled { get; }
        public string TargetKnownFolder { get; }

        private static RegistryKey ClassesRoot32 = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Registry32);
        private static RegistryKey CurrentUser32 = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32);
        private static RegistryKey ClassesRoot = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Registry64);
        private static RegistryKey CurrentUser = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);

        public NamespaceTreeFolder(string name, string path, string targetKnownFolder, int index, string iconPath, int iconIndex, bool showOnDesktop, bool enabled)
        {
            Name = name;
            Path = path;
            TargetKnownFolder = targetKnownFolder;
            Index = index;
            IconPath = iconPath;
            IconIndex = iconIndex;
            ShowOnDesktop = showOnDesktop;
            Enabled = enabled;
        }

        public NamespaceTreeFolder(string guid, string name, string path, string targetKnownFolder, int index, string iconPath, int iconIndex, bool showOnDesktop, bool enabled) : this(name, path, targetKnownFolder, index, iconPath, iconIndex, showOnDesktop, enabled)
        {
            Guid = guid?.ToUpper();
        }

        private static readonly List<NamespaceTreeFolder> Folders = new List<NamespaceTreeFolder>();

        public static List<NamespaceTreeFolder> GetFolders(bool force = false)
        {
            if (Folders.Count == 0 || force)
            {
                Folders.Clear();

                var folderKeys32 = CurrentUser32.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace");
                var folderKeys64 = CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace");

                if (folderKeys32 != null && folderKeys64 != null)
                {
                    var keys = folderKeys32.GetSubKeyNames().Union(folderKeys64.GetSubKeyNames()).ToList();

                    NamespaceTreeFolder folder;

                    foreach (var guid in keys)
                    {
                        Debug.WriteLine(guid);
                        if ((folder = LoadFolder(guid)) != null)
                        {
                            Folders.Add(folder);
                            Debug.WriteLine(folder);
                        }
                    }
                }
            }

            return Folders;
        }

        public static NamespaceTreeFolder LoadFolder(string guid)
        {
            var key = ClassesRoot.OpenSubKey(@"CLSID")?.OpenSubKey(guid);

            var icon = key?.OpenSubKey("DefaultIcon");
            var instance = key?.OpenSubKey("Instance");
            var initPropertyBag = instance?.OpenSubKey("InitPropertyBag");
            var desktopNamespace = CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace");
            var hideDesktopIcons = CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel");

            var name = (string)key?.GetValue("");
            var enabled = (int?)key?.GetValue("System.IsPinnedToNamespaceTree", 0) == 1;
            var index = (int?)key?.GetValue("SortOrderIndex", 0) ?? 0;
            var path = (string)initPropertyBag?.GetValue("TargetFolderPath");
            var targetKnownFolder = (string)initPropertyBag?.GetValue("TargetKnownFolder");

            var iconStr = (string)icon?.GetValue("");

            var iconPath = "";
            var iconIndex = 0;

            if (iconStr != null)
            {
                iconPath = iconStr.Contains(",") ? iconStr.Substring(0, iconStr.LastIndexOf(",", StringComparison.Ordinal)) : iconStr;

                if (iconStr.Contains(","))
                    int.TryParse(iconStr.Substring(iconStr.LastIndexOf(",", StringComparison.Ordinal) + 1), out iconIndex);
            }

            var showOnDesktop = Array.Exists(hideDesktopIcons.GetValueNames(), str => str.Equals(guid)) && (int)hideDesktopIcons.GetValue(guid, 0) != 1;

            return new NamespaceTreeFolder(guid, name, path, targetKnownFolder, index, iconPath, iconIndex, showOnDesktop, enabled);
        }

        public static bool CheckDefinition(string guid)
        {
            // open registry key
            var key = ClassesRoot.OpenSubKey(@"CLSID")?.OpenSubKey(guid);
            
            if (key == null)
                return false;

            var icon = key.OpenSubKey("DefaultIcon");
            var server = key.OpenSubKey("InProcServer32");
            var shellFolder = key.OpenSubKey("ShellFolder");
            var instance = key.OpenSubKey("Instance");
            var desktopNamespace = CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace");
            var hideDesktopIcons = CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel");
            
            if (icon == null || server == null || shellFolder == null || instance == null)
                return false;

            var initPropertyBag = instance.OpenSubKey("InitPropertyBag");

            if (initPropertyBag == null)
                return false;
            
            var result = key.GetValue("", null) != null;
            result = result && key.GetValue("SortOrderIndex", null) != null;
            result = result && key.GetValue("System.IsPinnedToNamespaceTree") != null;

            result = result && icon.GetValue("", null) != null;

            result = result && server.GetValue("", null) is string && server.GetValue("", null, RegistryValueOptions.DoNotExpandEnvironmentNames).ToString().ToLower().StartsWith("%systemroot%");
            
            result = result && instance.GetValue("CLSID", null) is string && (string)instance.GetValue("CLSID", null) == "{0E5AAE11-A475-4c5b-AB00-C66DE400274E}";

            result = result && initPropertyBag.GetValue("Attributes", 0) is int && (int)initPropertyBag.GetValue("Attributes", 0) == 17;
            result = result && initPropertyBag.GetValue("TargetFolderPath", null) is string | initPropertyBag.GetValue("TargetKnownFolder") is string;

            result = result && shellFolder.GetValue("Attributes", 0) is int && (int)shellFolder.GetValue("Attributes", 0) == unchecked((int)4034920525);
            result = result && shellFolder.GetValue("FolderValueFlags", 0) is int && (int)shellFolder.GetValue("FolderValueFlags", 0) == 40;

            result = result && desktopNamespace?.OpenSubKey(guid) != null && desktopNamespace.OpenSubKey(guid)?.GetValue("", null) != null;
            result = result && hideDesktopIcons?.GetValue(guid, null) is int;

            // close registry key
            key.Close();

            return result;
        }

        public static void AddFolder(NamespaceTreeFolder folder)
        {
            if (folder.Guid == null)
            {
                do
                {
                    folder.Guid = "{" + System.Guid.NewGuid().ToString().ToUpper() + "}";
                }
                while (ClassesRoot.OpenSubKey("CLSID").OpenSubKey(folder.Guid) != null);
            }
                

            // open all registry keys
            RegistryKey key = ClassesRoot.OpenSubKey(@"CLSID", true).CreateSubKey(folder.Guid);
            RegistryKey icon = key.CreateSubKey("DefaultIcon");
            RegistryKey server = key.CreateSubKey("InProcServer32");
            RegistryKey instance = key.CreateSubKey("Instance");
            RegistryKey initPropertyBag = instance.CreateSubKey("InitPropertyBag");
            RegistryKey shellFolder = key.CreateSubKey("ShellFolder");

            // set name, index in sidebar, and pins to namespace tree (i.e. sidebar in explorer)
            key.SetValue("", folder.Name, RegistryValueKind.String);
            key.SetValue("SortOrderIndex", folder.Index, RegistryValueKind.DWord);
            key.SetValue("System.IsPinnedToNamespaceTree", folder.Enabled, RegistryValueKind.DWord);

            // set the icon path & index
            icon.SetValue("", folder.IconPath + "," + folder.IconIndex);

            // not sure what this is for
            server.SetValue("", @"%SYSTEMROOT%\system32\shell32.dll", RegistryValueKind.ExpandString);

            // this value must stay constant
            instance.SetValue("CLSID", "{0E5AAE11-A475-4c5b-AB00-C66DE400274E}");

            initPropertyBag.SetValue("Attributes", 17, RegistryValueKind.DWord);
            
            if (!string.IsNullOrEmpty(folder.Path))
                initPropertyBag.SetValue("TargetFolderPath", folder.Path, RegistryValueKind.String);

            shellFolder.SetValue("Attributes", unchecked((int)4034920525), RegistryValueKind.DWord);
            shellFolder.SetValue("FolderValueFlags", 40, RegistryValueKind.DWord);

            CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace", true).CreateSubKey(folder.Guid).SetValue("", folder.Name);
            CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", true).SetValue(folder.Guid, !folder.ShowOnDesktop, RegistryValueKind.DWord);

            key.Close();

            Folders.RemoveAll(f => f.Guid == folder.Guid);
            Folders.Add(folder);
        }

        public void Remove()
        {
            RemoveFolder(this.Guid);
        }

        public static void RemoveFolder(string guid)
        {
            if (string.IsNullOrEmpty(guid))
                return;

            RegistryKey clsid = ClassesRoot.OpenSubKey("CLSID", true);
            RegistryKey desktopNamespace = CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace", true);
            RegistryKey hideDesktopIcons = CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", true);

            if (clsid.OpenSubKey(guid) != null)
                clsid.DeleteSubKeyTree(guid);

            if (desktopNamespace.OpenSubKey(guid) != null)
                desktopNamespace.DeleteSubKey(guid);

            if (Array.Exists(hideDesktopIcons.GetValueNames(), str => str.Equals(guid)))
                hideDesktopIcons.DeleteValue(guid);

            clsid.Close();
            desktopNamespace.Close();
            hideDesktopIcons.Close();
            
            if (Environment.Is64BitOperatingSystem)
            {
                RegistryKey clsid32 = ClassesRoot32.OpenSubKey("CLSID", true);
                RegistryKey desktopNamespace32 = CurrentUser32.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace", true);
                RegistryKey hideDesktopIcons32 = CurrentUser32.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", true);

                if (clsid32.OpenSubKey(guid) != null)
                    clsid32.DeleteSubKeyTree(guid);

                if (desktopNamespace32.OpenSubKey(guid) != null)
                    desktopNamespace32.DeleteSubKey(guid);

                if (Array.Exists(hideDesktopIcons32.GetValueNames(), str => str.Equals(guid)))
                    hideDesktopIcons32.DeleteValue(guid);

                clsid32.Close();
                desktopNamespace32.Close();
                hideDesktopIcons32.Close();
            }

            Folders.RemoveAll(folder => folder.Guid == guid);
        }

        public override string ToString()
        {
            return string.Format(GetType().FullName + "[Guid='{0}',Name='{1}',Path='{2}',Index={3},IconPath='{4}',IconIndex={5},ShowOnDesktop={6},Enabled={7},TargetKnownFolder='{8}']", Guid, Name, Path, Index, IconPath, IconIndex, ShowOnDesktop, Enabled, TargetKnownFolder);
        }
    }
}
