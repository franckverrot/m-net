/*---------------------------------------------------------------------
 * @file: MediaNET.cs
 * 
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

using MediaNET.Components;
using MediaNET.Settings;
using MediaNET.UI;
using MediaNET.DB;

using System;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Globalization;
using System.Threading;
using System.Resources;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Text;
using System.Runtime;
using System.Text.RegularExpressions;
using System.Diagnostics;


namespace MediaNET 
{
        // Main Window class
        public sealed class MediaNET: MediaNETInterface
        {
                //#region members
                // Media collection, collect ALL files, from Mp3 to Mkv through 
                private CMediaCollection m_cCollection = CMediaCollection.Instance;
                static private CPluginManager plugins = CPluginManager.Instance;
                static private CConfig config = CConfig.Instance;
                static private volatile MediaNET s_vInstance;
                static private readonly string s_sFile = "MediaNET";
                static private bool s_bContinue = false;

				// Loading frontend
                private static Dialog s_LoadingDialog = null;
                private static Label s_LoadingDialog_label = null;

                /// <summary>
                /// Save the unhandled files when trying to add new elements
                /// </summary>
                private ArrayList s_alUnhandled = null; // Handle unknown types

                // Player's process
                private System.Diagnostics.Process s_playerProc
                        = new System.Diagnostics.Process();
                private bool m_bIsPlaying = false;
                static private string m_sCurrentText = "Wash your WC!";

                // Internationalization manager
                private ResourceManager rm;

                // WebServer thread
                private Thread m_tServerThread;
                static private string s_sServerName = "localhost";
                private IPAddress m_ipLlocalIP=IPAddress.Parse("127.0.0.1");                         
                private IPEndPoint m_ipServer;
                private System.Net.Sockets.Socket socket;
                private System.Net.Sockets.Socket accSocket;                         
                private bool m_bIsListening;

                //#end region members

				 /// <summary>
				 /// <p> Default constructor </p>
				 /// </summary>
                private MediaNET ()
                {
                        Mono.Posix.Catalog.Init ("medianet", "./locale");
                        InitializeComponents();
                        Application.Init();

                        // Launch Main Slapshscreen waiting the interface to be init
                        m_cSplashScreen = new SplashScreen();

                        // Launch main interface
                        m_xXML = new Glade.XML(null,"medianet.glade", "MainWindow", null);
                        m_xXML.Autoconnect (this);                

                        // Init plugins
                        try 
                        {
                                config.Load();
                                plugins.Load();
                        } 
                        catch(Exception e) 
                        {
                                ChangeStatusbar("Error! "+e.Message);
                                Console.WriteLine(Mono.Posix.Catalog.GetString("An exception has been raised: ")+e.Message);
                        }

                        // Init inferface
                        InitInterface();
                        InitDB();

                        // Show window
                        MainWindow.Resize(1024,768);
                        MainWindow.Visible = true;

                        // Destroy splash screen
                        m_cSplashScreen.Window.Destroy();

                        // Init 
                        MainWindow.DeleteEvent += new DeleteEventHandler(OnDelete);
                        itemList.KeyPressEvent += new KeyPressEventHandler(On_Delete_Key_Pressed);
                        searchEntry.Activated += new EventHandler(OnModifiedSearchEntry);
                        itemList.ButtonPressEvent += new ButtonPressEventHandler(OnItemListButtonPressEvent); 

                        // Init player
                        m_bIsPlaying = false;
                        s_playerProc.EnableRaisingEvents = true;
                        s_playerProc.Exited += new EventHandler(OnPlayerExcited);

                        // Launching webserver
                        WebServer(DAEMON_STATUS.START);

                        // Finally launch the app
                        Application.Run();
                }

				 /// <summary>
				 /// <p> Default destructor </p>
				  /// </summary>
                public void Finalize()
                        //~MediaNET()
                {
                        try
                        {
                                if(m_bIsPlaying)
                                {
                                        s_playerProc.Kill();
                                }
                                if(m_bIsListening)
                                {
                                        m_tServerThread.Abort();
                                }
                        }
                        catch(Exception e)
                        {
                        }
                        finally
                        {
                                Console.WriteLine(Mono.Posix.Catalog.GetString ("MediaNET is now closed."));
                                System.Environment.Exit(0);
                        }
                }

				/// <summary>
				/// <p> Instance is a double lock thread-safe singleton's accessors.</p>
				/// <returns>The unique instance of the class MediaNET.</returns> 
				/// </summary>
                static public MediaNET Instance
                {
                        get
                        {
                                if(s_vInstance == null)
                                {
                                        lock(s_sFile)
                                        {
                                                if( s_vInstance == null )
                                                        s_vInstance = new MediaNET();
                                        }
                                }
                                return s_vInstance;
                        }
                }

				/// <summary>
				/// <p> MediaNET's entry point.</p>
				/// </summary>
                //[STAThread]
                public static void Main(string[] args)
                {
                        Process aProcess = Process.GetCurrentProcess();
                        string aProcName = aProcess.ProcessName;

                        // Check if current class has already been instanciated
                        if (Process.GetProcessesByName(aProcName).Length > 1)
                        {
                                Console.WriteLine(MediaNET.GetContent());

                        }
                        else
                        {
                                m_sCurrentPath = Environment.CurrentDirectory;
                                MediaNET m = MediaNET.Instance;
                                m.Finalize();
                        }
                }

				/// <summary>
				/// <p>The OnItemQuit method is used whenever the user wants to exit the application.</p>
				/// </summary>
				/// <param name="o">The sender objet</param>
				/// <param name="args">Arguments to be passed</param>	
                override public void OnItemQuit(object obj,EventArgs args)
                {
                        Application.Quit();
                }

                /// <summary>
				/// <p>The ChangeStatusBar method takes a string to specify the current status bar text. </p>
				/// <p>This method is used to inform the user of what the program is actually doing.</p>
				/// </summary>
				/// <example><code>MyMediaNET.ChangeStatusbar("Playing foobar")</code></example>
				/// <param name="status_text">The string to display in the status bar</param>
                override public void ChangeStatusbar (string status_text) 
                {
                        m_sCurrentText = status_text;
                        statusbar.Push (1, status_text);
                }
                
                /// <summary>
				/// <p>The OnAddButton method is the default handler for any activation of the "Add" button</p>
				/// <p>This method is core the functioning of the application and is used widely.</p>
				/// </summary>
				/// <param name="o">The sender objet</param>
				/// <param name="args">Arguments to be passed</param>	
                override public void OnAddButton(object o,EventArgs args)
                {
                        s_bContinue = true;
                        s_alUnhandled = null;
                        FileSelection fs;
                        fs = new FileSelection(Mono.Posix.Catalog.GetString ("Choose a file"));
                        fs.HideFileopButtons ();
                        fs.Complete((string)config["Interface/currentPath"]);
                        fs.Resize(500,300);
                        fs.Resizable = true;
                        fs.Modal = true;
                        fs.SelectMultiple = true;
                        int res = fs.Run();
                        fs.Hide();
                        fs.Dispose();
                        if (res == (int)ResponseType.Ok)
                        {
                                if (s_alUnhandled == null)
                                {
                                        s_alUnhandled = new ArrayList();
                                }
                                LoadFiles(fs.Selections);
                                if(s_alUnhandled.Count != 0 )
                                {
                                        string message= "";
                                        foreach(object obj in s_alUnhandled) {
                                                message+= "\n"+(string)obj;
                                        }
                                        message =  Mono.Posix.Catalog.GetString ("Add the correct plugin or tell developpers about this:")+message;

                                        MessageDialog dialog = new MessageDialog(MainWindow,
                                                        DialogFlags.Modal | DialogFlags.DestroyWithParent,
                                                        MessageType.Question,
                                                        ButtonsType.Ok,
                                                        message);

                                        dialog.Title = Mono.Posix.Catalog.GetString ("Some files have unknown types...");
                                        dialog.TransientFor = MainWindow;
                                        int rc = dialog.Run();
                                        dialog.Hide();
                                        dialog.Destroy();

                                        ChangeStatusbar(Mono.Posix.Catalog.GetString ("Loading finished with ") + s_alUnhandled.Count + Mono.Posix.Catalog.GetString (" errors..."));
                                }
                                else
                                {
                                        ChangeStatusbar(Mono.Posix.Catalog.GetString ("Loading finished with success..."));
                                }
                        }
                        if(s_LoadingDialog != null) {
                                s_LoadingDialog.Destroy();
                                s_LoadingDialog = null;
                        }

                }

				/// <summary>
				/// <p>The DelFromCollection method is an easy way to delete an element from the database.</p>
				/// <p>Its existence is due to code refactoring and is used widely.</p>
				/// </summary>
				/// <example><code>MyMediaNETInstance.DelFromCollection("as_string_which_is_a_key_in_database")</code></example>
				/// <param name="gid">The key used in database</param>
                private void DelFromCollection(string gid)
                {
                        Query q = new Query();
                        q.AddCriteria("GID",gid);
                        foreach(object media in m_cCollection.Search(q))
                        {
                                m_cCollection.Remove(media);
                        }
                }
                
                /// <summary>
				/// <p>The DeleteSelectedItem method is triggered whenever the user wants to delete the selected item from the default treeview.</p>
				/// </summary>
				/// <example><code>MyMediaNETInstance.DeleteSelectedItem()/code></example>
                private void DeleteSelectedItem()
                {
                        TreeModel model=itemList.Model;
                        Gtk.TreeIter iter = new Gtk.TreeIter();
                        itemStore.GetIterFirst(out iter);
                        if ( itemList.Selection.IterIsSelected(iter)) 
                        {
                                string val = (string)model.GetValue(iter,5);

                                itemStore.Remove(ref iter);
                                DelFromCollection(val);
                        }
                        else 
                        {
                                while (itemStore.IterNext(ref iter)) 
                                {
                                        if ( itemList.Selection.IterIsSelected(iter)) 
                                        {
                                                string val = (string)model.GetValue(iter,5);
                                                itemStore.Remove(ref iter);
                                                DelFromCollection(val);
                                                break;
                                        }
                                }
                        }
                }

				/// <summary>
				/// <p>The OnDelButton event handler is triggered when the user click on the "Delete" button.</p>
				/// </summary>
				/// <param name="o">The sender objet</param>
				/// <param name="args">Arguments to be passed</param>	
                override public void OnDelButton(object o, EventArgs args)
                {
                        DeleteSelectedItem();
                }

				/// <summary>
				/// <p>The OnDelete_Key_Pressed event handler is triggered when the user click on the "Delete" key.</p>
				/// </summary>
				/// <param name="o">The sender objet</param>
				/// <param name="args">Arguments to be passed</param>	
                public void On_Delete_Key_Pressed(object o, KeyPressEventArgs args)
                {
                        switch(args.Event.HardwareKeycode)
                        {
                                case 22:
                                case 107:
                                        {
                                                TreeSelection tSelect = itemList.Selection;
                                                if(tSelect.CountSelectedRows() > 0)
                                                {
                                                        MessageDialog dialog = new MessageDialog(MainWindow,
                                                                        DialogFlags.Modal | DialogFlags.DestroyWithParent,
                                                                        MessageType.Question,
                                                                        ButtonsType.YesNo,
                                                                        Mono.Posix.Catalog.GetString ("Do you want to delete the selected file?"));

                                                        dialog.Title = Mono.Posix.Catalog.GetString ("Are you sure?");
                                                        dialog.TransientFor = MainWindow;
                                                        int rc = dialog.Run();
                                                        dialog.Hide();
                                                        dialog.Destroy();

                                                        if(rc == (int)ResponseType.Yes)
                                                        {
                                                                DeleteSelectedItem();
                                                        }
                                                }
                                                break;                  
                                        }
                        }
                }

                /// <summary>
				/// <p>The OnSearch event handler is triggered when the user click on the "Search" button.</p>
				/// <p> This function perform a regexp based search in files loaded in media collection</p>
				/// </summary>
				/// <param name="obj">The sender objet</param>
				/// <param name="args">Arguments to be passed</param>	
                override public void OnSearch(object obj,EventArgs args)
                {
                        if (searchButton.Active)
                        {
                                itemStore.Clear();


                                string reference_title = (string)((Gtk.Entry)searchEntry).Text;
                                Regex regex;
                                try
                                {
                                        regex = new Regex(reference_title, RegexOptions.IgnoreCase);
                                        foreach(object o in m_cCollection)
                                        {
                                                string [] res = (string[])CPluginManager.InvokeMember(o.GetType(),"Summary",o,CPluginManager.Flags.Field);
                                                foreach(string s in res)
                                                {
                                                        if(regex.IsMatch(s))
                                                        {
                                                                itemStore.AppendValues(res);
                                                                break;
                                                        }
                                                }
                                                        
                                        }
                                }
                                catch(ArgumentException e)
                                {
                                        ((Gtk.Entry)searchEntry).Text = Mono.Posix.Catalog.GetString ("**** Invalid regexp ****");
                                }

                                searchButton.Active = false;
                        }
                }

                 /// <summary>
				/// <p>The OnModifiedSearchEntry event handler is triggered when the user press the return key in the search entry.</p>
				/// <p> This function triggers the OnSearch method on entry modification.</p>
				/// </summary>
				/// <param name="obj">The sender objet</param>
				/// <param name="args">Arguments to be passed</param>	
				/// <seealso cref="MediaNET.OnSearch" />
                private void OnModifiedSearchEntry(object o, EventArgs args)
                {
                        searchButton.Active = true;
                }

                /// <summary>
				/// <p>The InitDB method initialize a database from a file</p>
				/// </summary>
                override public void InitDB()
                {
                        CMediaCollection c = null;
                        if(!File.Exists(MediaNET.Config["Interface/currentDbLocation"].ToString())) {
                                CMediaCollection coll = CMediaCollection.Instance;
                                coll.Location = "collection.odb";
                                coll.Load();
                                c = coll;
                        }
                        else
                        {
                                CMediaCollection coll = CMediaCollection.Instance;
                                coll.Location = MediaNET.Config["Interface/currentDbLocation"].ToString();
                                coll.Load();
                                c = coll;
                        }
                        itemStore.Clear();
                        foreach(CMedia media in c)
                        {
                                itemStore.AppendValues(media.Summary);
                        }
                }


                /// <summary>
				/// <p>The InitInterface method initialize the interface on first load.</p>
				/// </summary>
                override public void InitInterface()
                {
                        ChangeStatusbar(Mono.Posix.Catalog.GetString ("Welcome to MediaNET!"));

                        notebook.Visible = false;
                        //itemList.ColumnsAutosize();

                        itemViewColumn = new ArrayList();
                        itemViewColumn.Add(new TreeViewColumn(Mono.Posix.Catalog.GetString ("Title"),new CellRendererText(),"text",itemViewColumn.Count));
                        itemViewColumn.Add(new TreeViewColumn(Mono.Posix.Catalog.GetString ("Author"),new CellRendererText(),"text",itemViewColumn.Count));
                        itemViewColumn.Add(new TreeViewColumn(Mono.Posix.Catalog.GetString ("Type"),new CellRendererText(),"text",itemViewColumn.Count));
                        itemViewColumn.Add(new TreeViewColumn(Mono.Posix.Catalog.GetString ("Length"),new CellRendererText(),"text",itemViewColumn.Count));

                        // Rate... unused actually...
                        TreeViewColumn col = new TreeViewColumn(Mono.Posix.Catalog.GetString ("Rate"),new CellRendererText(),"text",itemViewColumn.Count);
                        col.Visible = false;
                        itemViewColumn.Add(col);

                        // Hide GUID column, looks ugly and user don't wanna
                        // know that in any case
                        col = new TreeViewColumn("GUID",new CellRendererText(),"text",itemViewColumn.Count);
                        col.Visible = false;
                        itemViewColumn.Add(col);

                        foreach(TreeViewColumn tvc in itemViewColumn)
                        {
                                tvc.Clickable = true;
                                tvc.SortIndicator = true;
                                tvc.Reorderable = false;
                                tvc.Resizable = true;
                                tvc.Sizing = TreeViewColumnSizing.Autosize;
                                tvc.Clicked += new EventHandler(OnItemSorted);
                                itemList.AppendColumn (tvc);
                        }

                        itemStore = new TreeStore (typeof (string), typeof (string),typeof(string),typeof(string),typeof(string),typeof(string));
                        itemList.Model = itemStore;
                        itemList.HeadersVisible = true;
                        // Connect media list with the frame
                        itemList.RowActivated += new RowActivatedHandler(OnItemSelectedChanged);
                        itemTypeList.RowActivated += new RowActivatedHandler(OnItemTypeSelectedChanged);
                        // Allow sort on a column
                        PopulateTypeList();
                        SetItemSort(0,SortType.Ascending);
                        SetTypeSort(SortType.Ascending);
                        OnResize(null,null);
                }


                /// <summary>
                /// <p>The AddToArrayListWithOutDoublon adds a file extension without doublon to an arraylist.</p>
                /// <p>This function works with the s_alUnhandled arraylist. </p>
                /// </summary>
                /// <param name="al">The targetted arraylist</param>
				/// <param name="o">The string to add</param>	
                private void AddToArrayListWithOutDoublon(ArrayList al, string o) {
                        string ext = Path.GetExtension(o);
                        if(!al.Contains(ext))
                                al.Add(ext);
                }

                /// <summary>
                /// <p> The LoadFiles function is a recursive loading function used whenever the user wants to add a file to his collection.</p>
                /// <p>It loads each file in the array of string, and its subdirectories if exist, with the LoadFile function.</p>
                /// </summary>
                /// <param name="fileNames">The list of files to load</param>
                /// <seealso cref="MediaNET.LoadFile" />
                override public void LoadFiles(string [] fileNames)
                {
                        if(s_bContinue == true)
                        {
                                foreach(string fileName in fileNames)
                                {
                                        if(Directory.Exists(fileName))
                                        {
                                                foreach(string file in Directory.GetFiles(fileName))
                                                {
                                                        try {
                                                                if(s_bContinue == true)
                                                                        LoadFile(file);
                                                                UpdateDialog (Mono.Posix.Catalog.GetString ("Loading ")+Path.GetFileName(file));
                                                        } catch(Exception e) {
                                                                AddToArrayListWithOutDoublon(s_alUnhandled,file);
                                                        }
                                                }
                                                if(s_bContinue == true)
                                                        LoadFiles(Directory.GetDirectories(fileName));
                                        }
                                        else if(File.Exists(fileName)) 
                                        {
                                                try {
                                                        if(s_bContinue == true)
                                                                LoadFile(fileName);
                                                        UpdateDialog (Mono.Posix.Catalog.GetString ("Loading ")+fileName);
                                                } catch(Exception e) {
                                                        AddToArrayListWithOutDoublon(s_alUnhandled,fileName);
                                                }
                                        }
                                }
                        }
                }


                /// <summary>
                /// <p> The LoadFile function is used by LoadFiles to load a single file.</p>
                /// <p> It loads any file and dispatch hadling to correct methods based on extension name. </p>
                /// </summary>
                /// <param name="fileName">The single file to load</param>
                /// <seealso cref="MediaNET.LoadFiles" />                 
                override public void LoadFile(string fileName)
                {
                        if (fileName.Length > 0 && File.Exists(fileName)) 
                        {
                                FileInfo fFileInfo = new FileInfo(fileName);

                                // Delegate instanciation to plugin!
                                object [] arguments = new object []  { fileName };
                                try 
                                {
                                        object t = plugins.CreateInstance(fFileInfo.Extension,BindingFlags.Default, arguments);
                                        if(t == null)
                                        {
                                                throw new Exception(Mono.Posix.Catalog.GetString ("Plugin ERROR"));
                                        }

                                        Type type = t.GetType();
                                        CPluginManager.InvokeMember(type,"loadFileCharacteristics", t,CPluginManager.Flags.Method);
                                        object res = CPluginManager.InvokeMember(type,"Summary",t,CPluginManager.Flags.Field);

                                        itemStore.AppendValues((string[])res);
                                        m_cCollection.Add(t);
                                        t = null; res = null;
                                } 
                                catch (Exception e) 
                                {
                                        ChangeStatusbar(Mono.Posix.Catalog.GetString ("Error! ")+e.Message);
                                        throw new Exception(Mono.Posix.Catalog.GetString ("Error! ")+ e.Message);
                                }
                                fFileInfo = null;
                        }

                }


                private static void UpdateDialog (string format, params object[] args) 
                {
                        string text = String.Format (format, args);

                        if (s_LoadingDialog == null)
                        {
                                s_LoadingDialog = new Dialog ();
                                s_LoadingDialog.Modal = true;
                                s_LoadingDialog.Title = Mono.Posix.Catalog.GetString ("Loading media files...");
                                s_LoadingDialog.SetDefaultSize (480, 100);
                                s_LoadingDialog.Resizable = false;

                                VBox vbox = s_LoadingDialog.VBox;
                                HBox hbox = new HBox (false, 4);
                                vbox.PackStart (hbox, true, true, 0);

                                Gtk.Image icon = new Gtk.Image (Stock.DialogInfo, IconSize.Dialog);
                                Gtk.Button button = new Gtk.Button(Mono.Posix.Catalog.GetString ("Cancel loading..."));
                                button.Clicked += new EventHandler(OnAbortLoading);
                                s_LoadingDialog_label = new Label (text);
                                hbox.PackStart (icon, false, false, 0);               
                                hbox.PackStart (s_LoadingDialog_label, false, false, 0);
                                hbox.PackStart (button, false, false, 0);
                                s_LoadingDialog.ShowAll ();
                        } 
                        else 
                        {
                                s_LoadingDialog_label.Text = text;
                                while (Application.EventsPending ())
                                        Application.RunIteration ();
                        }
                }

                static private void OnAbortLoading(object o, EventArgs args)
                {
                        s_bContinue = false;
                }

                [GLib.ConnectBefore]
                        public void OnItemListButtonPressEvent (object o,ButtonPressEventArgs args)
                        {
                                Gdk.EventButton eb = args.Event;

                                if (eb.Button == 3) { // Right click
                                        Console.WriteLine (Mono.Posix.Catalog.GetString ("Right click launched directly the music"));
                                        OnPlayButton(null,null);
                                        
                                }
                        }

                override public void OnItemSelectedChanged(object o, RowActivatedArgs args) 
                {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();

                        notebook.Visible = true;
                        TreeIter iter;
                        TreeModel model;

                        TreeSelection selection = ((TreeView)o).Selection;
                        if (selection.GetSelected (out model, out iter))
                        {
                                // Get back correct value
                                string val = (string)model.GetValue(iter,5);
                                Query q = new Query();
                                q.AddCriteria("GID",val);
                                foreach(object selected in m_cCollection.Search(q))
                                {
                                        try {
                                                DisplayInformation(selected);
                                        } catch(Exception e) {
                                                Console.WriteLine(Mono.Posix.Catalog.GetString ("Exception DisplayInformation raised: ") + e.Message);
                                        }
                                }
                        }
                }

                override public void OnItemTypeSelectedChanged(object o, RowActivatedArgs args) 
                {
                        notebook.Visible = false;
                        TreeIter iter;
                        TreeModel model;

                        TreeSelection selection = ((TreeView)o).Selection;
                        if (selection.GetSelected (out model, out iter))
                        {
                                // Get back correct value
                                string val = (string)model.GetValue(iter,0);
                                if(val == Mono.Posix.Catalog.GetString ("All"))
                                {
                                        itemStore.Clear();
                                        foreach(object s in m_cCollection)
                                        {
                                                itemStore.AppendValues((string[])CPluginManager.InvokeMember(s.GetType(),"Summary",s,CPluginManager.Flags.Field));
                                        }
                                }
                                else
                                {
                                        itemStore.Clear();
                                        foreach(object s in m_cCollection)
                                        {
                                                object res = CPluginManager.InvokeMember(s.GetType(),"Summary",s,CPluginManager.Flags.Field);
                                                if(((string[])res)[2] == val)
                                                {
                                                        itemStore.AppendValues((string[])res);
                                                }
                                        }
                                }
                        }
                }


                Gtk.Widget oldEdit,oldDisplay;

                public void DisplayInformation(object t)
                {
                        Type type = t.GetType();
                        Console.Write(Mono.Posix.Catalog.GetString ("Invoking ")+t+" ... ");
                        if(oldDisplay != null ) showAlignment.Remove(oldDisplay);
                        try {
                                oldDisplay = CPluginManager.InvokeMember(type,"Display",t,CPluginManager.Flags.Field) as Gtk.Frame;
                                oldDisplay.Reparent(showAlignment);
                                showAlignment.Show();
                        }
                        catch(Exception e)
                        {
                                Console.WriteLine(Mono.Posix.Catalog.GetString ("New display has been aborted: ")+e.Message);
                        }

                        if(oldEdit != null) editAlignment.Remove(oldEdit);
                        try {
                                oldEdit = CPluginManager.InvokeMember(type,"Edit",t,CPluginManager.Flags.Field) as Gtk.Frame;
                                oldEdit.Reparent(editAlignment);
                                editAlignment.Show();
                        }
                        catch(Exception e)
                        {
                                Console.WriteLine(Mono.Posix.Catalog.GetString ("New Edit has been aborted: ")+e.Message);
                        }
                        Console.WriteLine("[OK]");
                }

                override public void OnDelete (object o, DeleteEventArgs args)
                {
                        Application.Quit ();
                }

                override public void SaveState()
                {
                        m_cCollection.Save();
                }

                override public void InitializeComponents()
                {
                        try 
                        {
                                m_cCollection.Load();
                        } 
                        catch (Exception e) 
                        {
                                Console.WriteLine(Mono.Posix.Catalog.GetString ("Exception has been raised: ")+e.Message);
                        }
                }
                override public void ReleaseComponents()
                {
                        SaveState();
                }

                override public void OnSaveDatabase(object o, EventArgs args)
                {
                        SaveState();
                }


                override public void OnReloadDatabase(object o, EventArgs args)
                {
                        OnReloadButton(null,null);
                }

                override public void OnHelp(object o, EventArgs args)
                {
                }

                override public void OnEditSettings(object o, EventArgs args)
                {
                        m_cSettingsScreen = new SettingsScreen();
                }

                override public void OnGenerateXML(object o, EventArgs args)
                {
                        XmlTextWriter writer = new System.Xml.XmlTextWriter("collection.xml",null);
                        writer.Formatting = Formatting.Indented;
                        writer.WriteStartDocument();
                        writer.WriteStartElement("elements");

                        foreach(object obj in m_cCollection)
                        {
                                writer.WriteStartElement("element");
                                string [] summary = CPluginManager.InvokeMember(obj.GetType(),"Summary",obj,CPluginManager.Flags.Field) as string[];
                                string [] sections = { Mono.Posix.Catalog.GetString ("name"), Mono.Posix.Catalog.GetString ("author"), Mono.Posix.Catalog.GetString ("type"), Mono.Posix.Catalog.GetString ("length") };
                                int i = 0;
                                foreach(string s in sections)
                                {
                                        writer.WriteElementString(s, summary[i].Trim("\0".ToCharArray()));
                                        ++i;
                                }
                                writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
                        writer.WriteEndDocument();
                        writer.Flush();
                        writer.Close();
                        

                }

                override public void OnGenerateHTML(object o, EventArgs args)
                {
                        Console.WriteLine(Mono.Posix.Catalog.GetString ("OnGenerateHTML: Not yet supported"));
                }

                override public void SetItemSort(int id, SortType sortType)
                {
                        itemStore.SetSortColumnId(id,sortType);
                        itemStoreSortType = sortType;
                }

                override public void SetTypeSort(SortType sortType)
                {
                        itemTypeStore.SetSortColumnId(0,sortType);
                        itemTypeStoreSortType = sortType;
                }

                override public void OnItemSorted(object o, EventArgs args)
                {
                        // Find which column has to be sorted
                        // Avoid multiple callback but need
                        // iteration as much as there are columns
                        int i = 0;
                        foreach(TreeViewColumn tvc in itemViewColumn)
                        {
                                if(tvc == o)
                                { 
                                        if(itemStoreSortType == SortType.Ascending)
                                        {
                                                SetItemSort(i,SortType.Descending);

                                        }
                                        else 
                                        {
                                                SetItemSort(i,SortType.Ascending);
                                        }
                                        tvc.SortOrder = itemStoreSortType;
                                        break; 
                                }
                                else ++i;
                        }
                }
                override public void OnTypeSorted(object o, EventArgs args)
                {
                        if(itemTypeStoreSortType == SortType.Ascending)
                        {
                                SetTypeSort(SortType.Descending);
                        }
                        else
                        {
                                SetTypeSort(SortType.Ascending);
                        }
                        ((TreeViewColumn)o).SortOrder= itemTypeStoreSortType;
                }

                public void PopulateTypeList() 
                {
                        ////////////////////////////////:
                        /// item type
                        ////////////////////////////////:
                        if(itemTypeColumn == null)
                                itemTypeColumn = new ArrayList();
                        else itemTypeColumn.Clear();

                        Gtk.TreeViewColumn column = new TreeViewColumn(Mono.Posix.Catalog.GetString ("Type"),new CellRendererText(),"text",0);
                        itemTypeColumn.Add(column);

                        // Delete existing columns... is this a hack?
                        foreach(TreeViewColumn tvc in itemTypeList.Columns)
                        {
                                itemTypeList.RemoveColumn(tvc);
                        }

                        // Then add new column...
                        foreach(TreeViewColumn tvc in itemTypeColumn) {
                                tvc.Clickable = true;
                                tvc.SortIndicator = true;
                                tvc.Reorderable = true;
                                tvc.Resizable = true;
                                tvc.Clicked += new EventHandler(OnTypeSorted);
                                itemTypeList.AppendColumn (tvc);
                        }

                        itemTypeStore = new TreeStore (typeof(string));

                        itemTypeStore.AppendValues(Mono.Posix.Catalog.GetString ("All"));
                        foreach(string s in plugins.GetLoadedAssemblies())
                        {
                                Assembly a = (Assembly)plugins[s];
                                Type[] mytypes = a.GetTypes();
                                Type selected_type = null;
                                foreach(Type t in mytypes)
                                {
                                        System.Object obj = Activator.CreateInstance(t);
                                        try {
                                                itemTypeStore.AppendValues((string)CPluginManager.InvokeMember(obj.GetType(),"TypeIdentifier",obj,CPluginManager.Flags.Field));
                                                break;
                                        }
                                        catch(Exception e)
                                        { Console.WriteLine(s);}
                                }
                        }

                        itemTypeList.Model = itemTypeStore;
                        itemTypeList.HeadersVisible = true;
                }

                override public void OnAbout(object o, EventArgs args) 
                {
                        m_cAboutScreen = new AboutScreen ();
                }

                static public CConfig Config
                {
                        get
                        {
                                return config;
                        }
                }

                static public CPluginManager Plugins
                {
                        get
                        {
                                return plugins;
                        }
                }

                override public void OnReloadButton(object o, EventArgs args)
                {
                        OnResize(null,null);
                        m_cCollection.Location = MediaNET.Config["Interface/currentDbLocation"].ToString();
                        TreeModel model = itemList.Model;
                        Gtk.TreeIter iter = new Gtk.TreeIter();

                        TreeSelection selection = ((TreeView)itemList).Selection;
                        if(selection.GetSelected(out model, out iter))
                        {
                                string val = (string)model.GetValue(iter,5);
                                Query q = new Query();
                                q.AddCriteria("GID",val);
                                foreach(object selected in m_cCollection.Search(q))
                                {

                                        itemStore.AppendValues((string[])CPluginManager.InvokeMember(selected.GetType(),"Summary",selected,CPluginManager.Flags.Field));
                                        itemStore.Remove(ref iter);
                                }
                        }


                        PopulateTypeList();
                        ChangeStatusbar(Mono.Posix.Catalog.GetString ("Loading done!"));
                }

                override public void OnResize(object o, EventArgs args)
                {
                        itemList.ColumnsAutosize();
                }

                private void OnPlayerExcited(object o, EventArgs args)
                {
                        // if normal death, continue on the playlist !
                        if(m_bIsPlaying == true)
                        {
                                TreeIter iter;
                                TreeModel model;

                                if(itemList.Selection.GetSelected (out model, out iter) && (true == itemStore.IterNext(ref iter)))
                                {
                                        itemList.Selection.SelectIter(iter);
                                        itemList.ScrollToCell (itemStore.GetPath (iter), itemList.GetColumn (0), true,  0, 0);
                                        OnPlayButton(null,null);
                                }
                        }
                        
                }

                override public void OnPlayButton(object o, EventArgs args)
                {
                        OnStopButton(null,null);
                        TreeModel model = itemList.Model;
                        Gtk.TreeIter iter = new Gtk.TreeIter();
                        if(itemList.Selection.GetSelected (out model, out iter))
                        {
                                string val = (string) model.GetValue(iter,5);
                                Query q = new Query();
                                q.AddCriteria("GID",val);
                                foreach(object s in m_cCollection.Search(q))
                                {
                                        try
                                        {
                                                Type type = s.GetType();
                                                s_playerProc.StartInfo.FileName = "\""+(string)CPluginManager.GetStatic(type,"Player")+"\"";
                                                s_playerProc.StartInfo.Arguments = "\""+(string)CPluginManager.InvokeMember(type,"Path",s,CPluginManager.Flags.Field)+"\"";
                                                s_playerProc.Start();
                                                m_bIsPlaying = true;
                                                ChangeStatusbar((string) model.GetValue(iter,0));
                                                Console.WriteLine(Mono.Posix.Catalog.GetString ("Process ID ")+s_playerProc.Id);
                                        }
                                        catch(Exception e)
                                        {
                                                m_bIsPlaying = false;
                                                Console.WriteLine(Mono.Posix.Catalog.GetString ("Process can't be loaded : ")+e.Message);
                                        }
                                }

                        }
                        while (Application.EventsPending () && m_bIsPlaying)
                                Gtk.Application.RunIteration(false);
                }

                override public void OnStopButton(object o, EventArgs args)
                {
                        try
                        {
                                m_bIsPlaying = false;
                                s_playerProc.Kill();
                        }
                        catch(Exception e)
                        {
                                Console.WriteLine(Mono.Posix.Catalog.GetString ("Process can't be stopped : ")+e.Message);
                        }
                        ChangeStatusbar(Mono.Posix.Catalog.GetString ("MediaNET"));
                }


                public enum DAEMON_STATUS { START,STOP,RELOAD };

                private static string GetContent() {
                        Byte[] byteBuffer = new Byte[1024];
                        string serverResponse = string.Empty;

                        System.Net.Sockets.Socket sock = null;

                        try 
                        {
                                // Create a TCP socket instance 
                                sock = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                                // Creates server IPEndPoint instance. We assume Resolve returns at least one address
                                IPEndPoint serverEndPoint = new IPEndPoint(Dns.Resolve("127.0.0.1").AddressList[0], 1025);

                                // Connect the socket to server on specified port
                                sock.Connect( serverEndPoint );

                                int bytesReceived = 0;
                                bytesReceived = sock.Receive(byteBuffer, byteBuffer.Length -1, SocketFlags.None);
                                serverResponse = Encoding.ASCII.GetString(byteBuffer, 0, bytesReceived);

                        } 
                        catch (Exception e) 
                        {
                                // Handle errors
                                Console.WriteLine(Mono.Posix.Catalog.GetString ("Client: ") + e.Message);
                        } 
                        finally 
                        {
                                // Close connection
                                sock.Close();
                        }

                        return serverResponse;
                }

                private void DeliverContent()
                {
                        try
                        {
                                NetworkStream netStream=new NetworkStream(accSocket);

                                Byte[] byteMessage=new Byte[640];
                                string msg = "";
                                if(m_bIsPlaying)
                                {
                                        msg = (Mono.Posix.Catalog.GetString ((string)config["Interface/announcerSentence"])).Replace("[[title]]",m_sCurrentText);
                                }
                                else
                                {
                                        msg = Mono.Posix.Catalog.GetString (" MediaNET, en voila un soft qu'il est bien! ");
                                }
                                byteMessage=System.Text.Encoding.Default.GetBytes(msg.ToCharArray());
                                netStream.Write(byteMessage,0,byteMessage.Length);
                                netStream.Flush();
                                netStream.Close();
                                accSocket.Close();
                        }
                        catch(Exception e)
                        {
                                Console.WriteLine(Mono.Posix.Catalog.GetString ("DeliverContent: ")+e.Message);
                        }
                }
                
                private void StartListening() {
                        try {
                                m_ipServer=new IPEndPoint(m_ipLlocalIP,Int32.Parse("1025"));
                                socket=new System.Net.Sockets.Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
                                socket.Bind(m_ipServer);
                                socket.Listen(50);

                                while(true)
                                {
                                        try
                                        {
                                                accSocket=socket.Accept();
                                                if(accSocket.Connected)
                                                {
                                                        Thread thread=new Thread(new ThreadStart(DeliverContent));
                                                        thread.Start();
                                                }
                                                Console.WriteLine(Mono.Posix.Catalog.GetString ("One client connected and gone..."));
                                        }

                                        catch(Exception ee)
                                        {
                                                Console.WriteLine(Mono.Posix.Catalog.GetString ("SocketListener is now closed"));
                                        }

                                }
                        } catch ( ThreadAbortException e) {
                                Console.WriteLine(Mono.Posix.Catalog.GetString ("End of listening..."));
                        }

                }

                void WebServer(DAEMON_STATUS s)
                {
                        if(s == DAEMON_STATUS.START)
                        {
                                m_tServerThread = new Thread(new ThreadStart(StartListening));
                                m_tServerThread.Start();
                                Console.WriteLine(Mono.Posix.Catalog.GetString ("Starting web server..."));
                                m_bIsListening = true;
                        }
                        else if(s == DAEMON_STATUS.STOP)
                        {
                                m_tServerThread.Abort();
                                Console.WriteLine(Mono.Posix.Catalog.GetString ("Stopping web server..."));
                                m_bIsListening = false;
                        }
                        else
                        {
                                Console.WriteLine(Mono.Posix.Catalog.GetString ("Reloading web server..."));
                        }
                }
        } 
}
