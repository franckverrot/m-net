/*---------------------------------------------------------------------
 *	@file:SettingsScreen.cs
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
using System;
using System.IO;
using System.Xml;
using System.Reflection;

using MediaNET;
using MediaNET.UI;
using MediaNET.Components;
using MediaNET.DB;
using MediaNET.Settings;

using Gtk;
using Gdk;

namespace MediaNET.UI
{
        public class SettingsScreen
        {
                // Interface
                Glade.XML m_xXML = null;
                [Glade.Widget("SettingsScreenWindow")] Gtk.Window m_cWindow;
                [Glade.Widget] Gtk.Entry pathEntry;
                [Glade.Widget] Gtk.Entry pluginFile;
                [Glade.Widget] Gtk.OptionMenu langBox;
                [Glade.Widget] Gtk.TreeView pluginList;
                [Glade.Widget] Gtk.TextView pluginInfo;

                private Gtk.TextBuffer buffer;
                private CConfig configuration = CConfig.Instance;
                private CPluginManager plugins = CPluginManager.Instance;
                private CMediaCollection m_cCollection = CMediaCollection.Instance;

                protected TreeStore pluginStore;

                public Gtk.Window Window
                {
                        get
                        {
                                return m_cWindow;
                        }
                }

                public SettingsScreen()
                {
                        try
                        {
                                m_xXML = new Glade.XML(null,MediaNET.GladeCommonFilename, "SettingsScreenWindow", null);
                                m_xXML.BindFields(this);
                                m_xXML.Autoconnect(this);

                                /** Init handlers **/
                                pluginStore = new TreeStore(typeof(string),typeof(string),typeof(string),typeof(string));
                                pluginList.Model = pluginStore; 
                                pluginList.HeadersVisible = true;
                                pluginList.AppendColumn(new TreeViewColumn("Type",new CellRendererText(),"text",0));
                                pluginList.AppendColumn(new TreeViewColumn("Identifier",new CellRendererText(),"text",1));
                                pluginList.AppendColumn(new TreeViewColumn("Player",new CellRendererText(),"text",2));

                                // Hidden column
                                TreeViewColumn col = new TreeViewColumn ();
                                CellRenderer NameRenderer = new CellRendererText ();
                                col.Title = "Path";
                                col.Visible = false;
                                col.PackStart (NameRenderer, true);
                                col.AddAttribute (NameRenderer, "text", 3);
                                pluginList.AppendColumn(col);

                                PopulatePluginList();
                                pluginList.Show();

                                /** Bindings from the UI **/
                                ((Gtk.Entry)m_xXML["pathEntry"]).Changed += new EventHandler(OnChanged);
                                ((Gtk.Entry)m_xXML["pathDbLocation"]).Changed += new EventHandler(OnChanged);
                                ((Gtk.Entry)m_xXML["pluginFile"]).Changed += new EventHandler(OnChanged);
                                ((Gtk.Entry)m_xXML["announcerSentence"]).Changed += new EventHandler(OnChanged);
                                pluginList.RowActivated += new RowActivatedHandler(OnPluginSelectedChanged);
                                buffer = pluginInfo.Buffer;

                                /** Populate GUI **/
                                string list  = (string)MediaNET.Config["Interface/currentLang"];
                                Menu m = new Menu ();
                                MenuItem miOne = new MenuItem (list);
                                m.Append (miOne);
                                langBox.Menu = m;
                                LoadSettings();
                        }
                        catch(Exception e)
                        {
                                throw new Exception("SettingsScreen raised an error: "+ e.Message);
                        }
                }

                public void OnPluginSelectedChanged(object o, RowActivatedArgs args) 
                {
                        TreeIter iter;
                        TreeModel model;

                        TreeSelection selection = ((TreeView)o).Selection;
                        if (selection.GetSelected (out model, out iter))
                        {
                                string val = (string)model.GetValue(iter,0);
                                buffer.Text = (string)CPluginManager.GetStatic(plugins.GetOwningType(val),"Description");
                        }
                }

                public void OnGetPluginInfo(object o, EventArgs args)
                {
                        OnPluginSelectedChanged(pluginList,null);
                }

                private void PopulatePluginList() {
                        try {
                                pluginStore.Clear();
                                foreach(string s in plugins.GetLoadedAssemblies())
                                {
                                        Assembly a = (Assembly)plugins[s];
                                        Type[] mytypes = a.GetTypes();
                                        string identifier = "";
                                        string path = "";
                                        string player = "";
                                        // Parse all types and break at first
                                        // implementing identifier property
                                        Type selected_type = null;
                                        foreach(Type t in mytypes)
                                        {
                                                try 
                                                {
                                                        identifier = (string)CPluginManager.InvokeMember(t,"TypeIdentifier",Activator.CreateInstance(t),CPluginManager.Flags.Field);
                                                        player = (string)CPluginManager.InvokeMember(t,"Player",Activator.CreateInstance(t),CPluginManager.Flags.Field);
                                                        selected_type = t;
                                                        break;
                                                }
                                                catch(Exception e)
                                                { }
                                        }
                                        path = plugins.GetAssemblyPath(s);
                                        pluginStore.AppendValues(s,identifier,player,path);
                                }
                        } 
                        catch(Exception e)
                        {
                                Console.WriteLine("Exception raised during PopulatePluginList: "+ e.Message);
                        }
                }

                private void LoadSettings()
                {
                        try {
                                ((Gtk.Entry)m_xXML["pathEntry"]).Text  = (string)MediaNET.Config["Interface/currentPath"];
                                ((Gtk.Entry)m_xXML["pathDbLocation"]).Text  = (string)MediaNET.Config["Interface/currentDbLocation"];
                                ((Gtk.Entry)m_xXML["pluginFile"]).Text = (string)MediaNET.Config["Interface/plugins.xml"] ;
                                ((Gtk.Entry)m_xXML["announcerSentence"]).Text = (string)MediaNET.Config["Interface/announcerSentence"] ;
                                ((Gtk.Button)m_xXML["applyButton"]).Sensitive = false;
                        } catch(Exception e)
                        {
                                Console.WriteLine("LoadSettings "+e.Message);
                        }
                }

                private void Save()
                {
                        try
                        {
                                MediaNET.Config["Interface/plugins.xml"] = ((Gtk.Entry)m_xXML["pluginFile"]).Text;
                                MediaNET.Config["Interface/currentPath"] = ((Gtk.Entry)m_xXML["pathEntry"]).Text;
                                MediaNET.Config["Interface/currentDbLocation"] = ((Gtk.Entry)m_xXML["pathDbLocation"]).Text;
                                MediaNET.Config["Interface/announcerSentence"] = ((Gtk.Entry)m_xXML["announcerSentence"]).Text;
                                SavePluginListToXMLFile((string)MediaNET.Config["Interface/plugins.xml"]);
                                MediaNET.Config.Save();
                        }
                        catch(Exception e)
                        {
                                Console.WriteLine("Save: "+e.Message);
                        }
                }

                private void OnChanged(object obj, EventArgs args)
                {
                        ((Gtk.Button)m_xXML["applyButton"]).Sensitive = true;
                }

                private void OnCloseButton(object obj, EventArgs args)
                {
                        //TODO: Apply button should be required I guess... any
                        //suggestion?
                        //Save();
                        Window.Destroy();
                }

                private void OnApplyButton(object obj, EventArgs args)
                {
                        Save();
                        ((Gtk.Button)m_xXML["applyButton"]).Sensitive = false;
                }

                private void OnCancelButton(object obj, EventArgs args)
                {
                        Window.Destroy();
                }

                private void OnAddLangButton(object o, EventArgs args)
                {
                        Console.WriteLine("Add language not yet implemented");
                }

                private void OnRemoveLangButton(object o, EventArgs args)
                {
                        Console.WriteLine("Remove language not yet implemented");
                }

                //////////////////////////////
                /// Handle plugin extension
                //////////////////////////////
                public void OnAddPluginButton(object o, EventArgs args)
                {
                        FileSelection fs = new FileSelection("Choose a plugin");
                        fs.Resize(500,300);
                        fs.Resizable = true;
                        fs.Modal = true;
                        fs.SelectMultiple = true;
                        int res = fs.Run();
                        fs.Hide();
                        fs.Dispose();

                        if(res == (int)ResponseType.Ok)
                        {
                                ConfirmLoadAssembly(fs.Selections);
                                
                                // Plugin autorecognition is not working
                                // properly, must be easy
                                // TODO: fix this!
                                /*
                                foreach(string s in fs.Selections)
                                {
                                        try
                                        {
                                                plugins.LoadAssembly("lol",s);
                                                string default_ext= (string)CPluginManager.GetStatic(plugins.GetOwningType("lol"),"Player");
                                                Console.WriteLine("ok: "+default_ext);
                                                plugins.RemoveAssembly("lol");
                                                plugins.LoadAssembly(default_ext,s);
                                        }
                                        catch(Exception e)
                                        {
                                                Console.WriteLine("LoadAssembly has failed: "+e.Message);
                                        }
                                        PopulatePluginList();
                                }
                                ((Gtk.Button)m_xXML["applyButton"]).Sensitive = true;
                                */
                        }
                }

                private void ConfirmLoadAssembly(string [] selection)
                {
                        Gtk.Entry e_sPlugin = null;
                        string l_sChoosenPlayer = "";
                        MessageDialog confirmationDialog = null;
                        foreach(string s in selection)
                        {
                                confirmationDialog = new MessageDialog(this.Window,
                                                DialogFlags.Modal | 
                                                DialogFlags.DestroyWithParent,
                                                MessageType.Question,
                                                ButtonsType.YesNo,
                                                "What extension "+ Path.GetFileName(s) +" should handle ?");
                                VBox vbox = confirmationDialog.VBox;
                                HBox hbox = new HBox (false, 4);
                                vbox.PackStart (hbox, true, true, 0);

                                e_sPlugin = new Gtk.Entry(".foobar");
                                hbox.PackStart (e_sPlugin, false, false, 0);               

                                confirmationDialog.ShowAll();
                                int res = confirmationDialog.Run();
                                confirmationDialog.Hide();
                                confirmationDialog.Dispose();
                                
                                if(res == (int)ResponseType.Yes)
                                {
                                        try
                                        {
                                                FileSelection fs = new FileSelection("Select your favorite player");
                                                fs.HideFileopButtons();
                                                CConfig config = CConfig.Instance;
                                                fs.Complete((string)config["Interface/currentPath"]);
                                                fs.Resizable = true;
                                                fs.Modal = true;
                                                int r = fs.Run();
                                                fs.Hide();
                                                fs.Dispose();
                                                if( r == (int)ResponseType.Ok )
                                                {
                                                        l_sChoosenPlayer = fs.Selections[0];
                                                        plugins.LoadAssembly((string)((Gtk.Entry)e_sPlugin).Text,s);
                                                        try
                                                        {
                                                        CPluginManager.SetStatic(plugins.GetOwningType((string)((Gtk.Entry)e_sPlugin).Text),"Player",l_sChoosenPlayer);
                                                        }
                                                        catch(Exception e) { Console.WriteLine("Player not found"); }
                                                        PopulatePluginList();
                                                        ((Gtk.Button)m_xXML["applyButton"]).Sensitive = true;
                                                }
                                                else throw new Exception("Loading aborted by user.");
                                        }
                                        catch (Exception e)
                                        {
                                                Console.WriteLine("Plugin error: "+e.Message);
                                        }
                                }
                        }
                }

                private void OnRemovePluginButton(object o, EventArgs args)
                {
                        TreeModel model = pluginList.Model;
                        TreeIter iter = new Gtk.TreeIter();
                        if(pluginStore.GetIterFirst(out iter))
                        {
                                do
                                {
                                        if(pluginList.Selection.IterIsSelected(iter))
                                        {
                                                plugins.RemoveAssembly((string)model.GetValue(iter,0));
                                                pluginStore.Remove(ref iter);
                                        }
                                } while(pluginStore.IterNext(ref iter));
                        }
                        // Trigger a modified state event
                        OnChanged(null,null);
                }

                public void OnBrowseDefaultPath(object o, EventArgs args)
                {
                        FileSelection fs = new FileSelection("Select where to find media");
                        fs.FileList.Parent.Hide();
                        //fs.SelectionEntry.Hide();
                        fs.FileopDelFile.Hide();
                        fs.FileopRenFile.Hide();
                        fs.HideFileopButtons();
                        CConfig config = CConfig.Instance;
                        if(File.Exists((string)config["Interface/currentPat        h"])) {
                                        fs.Complete((string)config["Interface/currentPath"]);
                        }
                        //fs.Complete("*");
                        fs.Resizable = true;
                        fs.Modal = true;
                        int res = fs.Run();
                        fs.Hide();
                        fs.Dispose();
                        if( res == (int)ResponseType.Ok )
                        {
                                Console.WriteLine(fs.Filename);
                                Console.WriteLine(fs.Selections[0]);
                                pathEntry.Text = fs.Filename;
                        }
                }

                public void OnBrowseConfigurationFile(object o, EventArgs args)
                {
                        FileSelection fs = new FileSelection("Select where to write XML coonfiguration file");
                        fs.HideFileopButtons();
                        CConfig config = CConfig.Instance;
                        fs.Resizable = true;
                        fs.Modal = true;
                        int res = fs.Run();
                        fs.Hide();
                        fs.Dispose();
                        if( res == (int)ResponseType.Ok )
                        {
                                pluginFile.Text = fs.Selections[0];
                        }
                }

                private void SavePluginListToXMLFile(string filename)
                {
                        try
                        {
                                Console.WriteLine("Saving plugins entries to "+(string)MediaNET.Config["Interface/plugins.xml"]);
                                XmlTextWriter writer = new System.Xml.XmlTextWriter((string)MediaNET.Config["Interface/plugins.xml"],null);
                                writer.Formatting = Formatting.Indented;
                                writer.WriteStartDocument();
                                writer.WriteStartElement("plugins");

                                TreeModel model = pluginList.Model;
                                TreeIter iter;
                                if(pluginStore.GetIterFirst(out iter))
                                {
                                        do
                                        {
                                                writer.WriteStartElement("plugin");
                                                writer.WriteElementString("extension", (string)model.GetValue(iter,0));
                                                // Second field ignored -> found using reflection
                                                writer.WriteElementString("player", (string)model.GetValue(iter,2));
                                                writer.WriteElementString("path", (string)model.GetValue(iter,3));
                                                writer.WriteEndElement();
                                        }
                                        while (pluginStore.IterNext(ref iter));
                                }
                                writer.WriteEndElement();
                                writer.WriteEndDocument();
                                writer.Flush();
                                writer.Close();
                        }
                        catch(Exception e)
                        {
                                throw new Exception("Save settings failed: " + e.Message);
                        }
                }
        }
}
