/*---------------------------------------------------------------------
 *	@file:MediaNETInterface.cs
 *---------------------------------------------------------------------
 * Copyright (C) 2004  Franck Verrot
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.

 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.

 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *---------------------------------------------------------------------*/
using Glade;
using Gtk;

using MediaNET.UI;

using System;
using System.IO;
using System.Globalization;
using System.Threading;
using System.Resources;
using System.Collections;
using System.Text.RegularExpressions;

namespace MediaNET.UI {
        // Main Window class
        abstract public class MediaNETInterface
        {
                // Constant definition
                public static readonly string GladeCommonFilename = "medianet.glade";

                // Copy of envrinment variable
                protected static string m_sCurrentPath;

                // Handle XML-based interface
                protected XML m_xXML;
                protected XML m_xSearch;

                // Other windows
                protected SplashScreen m_cSplashScreen;
                protected CrashScreen m_cCrashScreen;
                protected SettingsScreen m_cSettingsScreen;
                protected AboutScreen m_cAboutScreen;

                // Widgets
                [Glade.Widget] protected Gtk.TreeView itemList;
                [Glade.Widget] protected Gtk.TreeView itemTypeList;
                [Glade.Widget] protected Gtk.Window MainWindow;
                [Glade.Widget] protected Gtk.Window SearchWindow;
                [Glade.Widget] protected Gtk.OptionMenu mediaBox;
                [Glade.Widget] protected Gtk.Alignment showAlignment;
                [Glade.Widget] protected Gtk.Alignment editAlignment;
                [Glade.Widget] protected Gtk.Image itemImage;
                [Glade.Widget] protected Gtk.Statusbar statusbar;
                [Glade.Widget] protected Gtk.ToggleButton searchButton;
                [Glade.Widget] protected Gtk.Entry searchEntry;
                [Glade.Widget] protected Gtk.Notebook notebook;
                [Glade.Widget] protected Gtk.Button stopButton;

                // Associated object
                // items related
                protected TreeStore itemStore;
                protected SortType itemStoreSortType = SortType.Ascending;
                protected ArrayList itemViewColumn; // TreeViewColumn

                // Type related
                protected TreeStore itemTypeStore;
                protected SortType itemTypeStoreSortType = SortType.Ascending;
                protected ArrayList itemTypeColumn; // TreeViewColumn

                // categories related
                public virtual string CurrentPath
                {
                        get { return m_sCurrentPath; }
                }

                protected enum ItemType {
                        AUDIO = 0x0010,
                        VIDEO = 0x0020,
                        ALL = 0x0030
                }


                abstract public void OnItemQuit(object obj,EventArgs args);
                abstract public void ChangeStatusbar (string status_text);
                abstract public void OnAddButton(object o,EventArgs args);
                abstract public void OnDelButton(object o, EventArgs args);
                abstract public void OnSearch(object obj,EventArgs args);
                abstract public void InitDB();
                abstract public void InitInterface();
                abstract public void LoadFile(string fileName);
			    abstract public void LoadFiles(string [] fileNames);
                abstract public void OnItemSelectedChanged(object o, RowActivatedArgs args);
                abstract public void OnItemTypeSelectedChanged(object o, RowActivatedArgs args);
                abstract public void OnDelete (object o, DeleteEventArgs args);
                abstract public void SaveState();
                abstract public void InitializeComponents();
                abstract public void ReleaseComponents();
                abstract public void OnHelp(object o, EventArgs args);
                abstract public void OnSaveDatabase(object o, EventArgs args);
                abstract public void OnReloadDatabase(object o, EventArgs args);
                abstract public void OnEditSettings(object o, EventArgs args);
                abstract public void OnGenerateXML(object o, EventArgs args);
                abstract public void OnGenerateHTML(object o, EventArgs args);
                abstract public void SetItemSort(int id, SortType sortType);
                abstract public void SetTypeSort(SortType sortType);
                abstract public void OnItemSorted(object o, EventArgs args);
                abstract public void OnTypeSorted(object o, EventArgs args);
                abstract public void OnReloadButton(object o, EventArgs args); 
                abstract public void OnPlayButton(object o, EventArgs args); 
                abstract public void OnStopButton(object o, EventArgs args); 
                abstract public void OnAbout(object o, EventArgs args);
                abstract public void OnResize(object o, EventArgs args);
        }
} 
