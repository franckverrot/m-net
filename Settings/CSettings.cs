/*---------------------------------------------------------------------
 *	@file:CSettings.cs
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
using System.Xml;
using System.IO;
using System.Collections;
using System.Reflection;

namespace MediaNET.Settings
{
        /// <summary>
        /// CPluginManager: singleton thread safe double locked
        /// </summary>
        public class CPluginManager: Hashtable
        {

                private static volatile CPluginManager s_vInstance;
                private static string s_sFile = "Plugins";
                private Hashtable assembly_path = Hashtable.Synchronized(new Hashtable());

                ///<summary>
				/// Default constructor, nothing to be done
				///</summary>
                protected CPluginManager() 
                {
                }

				///<summary>
				/// Instance returns a pointer on the single CPluginManager Instance
				///</summary>					
                public static CPluginManager Instance
                {
                        get 
                        {
                                if (s_vInstance == null) 
                                {
                                        lock (s_sFile) 
                                        {
                                                if (s_vInstance == null) 
                                                        s_vInstance = new CPluginManager();
                                        }
                                }

                                return s_vInstance;
                        }
                }

				///<summary>
				/// Load() permits a full loading of all plugins stored
				/// previously in Conf["Interace/plugins.xml"] file.
				///</summary>	
                public void Load() {
                        try
                        {
                                XmlDocument doc = new XmlDocument();
                                doc.Load((string)MediaNET.Config["Interface/plugins.xml"]);
                                
                                string s = null;
                                XmlNodeList list = null;
                                list = doc.GetElementsByTagName("plugin");
                                foreach(XmlNode n in list)
                                {
                                        try
                                        {
                                                LoadAssembly(n["extension"].InnerText,n["path"].InnerText);
                                                SetStatic(GetOwningType(n["extension"].InnerText),"Player",n["player"].InnerText);

                                        }
                                        catch (Exception e)
                                        {
                                                Console.WriteLine("CPluginManager Load: "+e.Message);
                                        }
                                }
                        }
                        catch(Exception e)
                        {
                                Console.WriteLine("Can't load plugins: "+e.Message);
                        }
                }
                
                ///<summary>
				/// Save() is obsolete, all is being synchronized on the fly 
				///</summary>	
                public void Save() {
                }

				///<summary>
				/// LoadAssembly maintains an hashtable on currently loaded assemblies
				///</summary>	
                public void LoadAssembly(string key, string filename)
                {
                        string path = Path.GetDirectoryName(filename);
                        try 
                        {
                                // Search for previous loaded lib
                                ArrayList al = new ArrayList();
                                IDictionaryEnumerator myEnum = assembly_path.GetEnumerator();
                                while(myEnum.MoveNext())
                                {
                                        if(myEnum.Value.ToString().Equals(filename))
                                                throw new Exception("Already loaded assembly... aborting");
                                }

                                Assembly assembly = Assembly.LoadFile(filename);
                                Add(key,assembly);
                                assembly_path.Add(key,filename);
                        } 
                        catch(Exception e) 
                        {
                                throw new Exception("LoadAssembly failed: "+ e.Message);
                        }
                }

				///<summary>
				/// Remove assembly permits to unload an assembly on the fly
				/// This could not work properly on MS CLR, AppDomain could prevent this
				/// TODO : reviewing the ECMA 334 :(
				///</summary>	
                public void RemoveAssembly(string key)
                {
                        Remove(key);
                        assembly_path.Remove(key);
                }

				///<summary>
				/// GetLoadedAssemblies returns an array composed like { extension, assembly(CFoo.dll) }
				/// ie: { "jpg", assembly(CJpg.dll) }
				///</summary>	
                public Array GetLoadedAssemblies()
                {
                        ArrayList al = new ArrayList();
                        IDictionaryEnumerator myEnumerator = this.GetEnumerator();
                        while ( myEnumerator.MoveNext() )
                                al.Add(myEnumerator.Key);
                        return al.ToArray();
                }

                ///<summary>
				/// GetAssemblyPath returns the DLL' paths which handle the assembly "Foo.dll"
				///</summary>                        
                public string GetAssemblyPath(string assembly_name)
                {
                        return assembly_path[assembly_name].ToString();
                }
                
                ///<summary>
				/// GetOwningType() grabs from a extension fext(string) the most qualified type in the assembly 
				///</summary>
                public Type GetOwningType(string fext)
                {
                        Assembly owningAssembly = (Assembly)this[fext.ToLower()];
                        if (owningAssembly == null)
                        {
                                throw new Exception("Could not find assembly for type " + fext);
                        }
                        string typename = null;
                        Type currentType = null;
                        foreach (Type type in owningAssembly.GetTypes ()) 
                        {
                                typename = type.ToString();
                                if(typename.IndexOf("+") == -1 && typename.ToLower().IndexOf(fext.ToLower().Substring(1)) != -1)
                                {
                                        currentType = type;
                                        break;
                                }
                        }

                        if(currentType == null)
                                throw new Exception("CPluginManager::GetOwningType : Type "+fext+" not found");
                        return currentType;
                }

                ///<summary>
				/// CreateInstance is the main factory/object builder function.
				/// It parses the hashtables of loadd assemblies and then tries to
				/// activate the correct assembly in order to create an object
				///</summary>        
                public object CreateInstance(string fext, 
                                BindingFlags bindingFlags, object[] constructorParams)
                {
                        // Find each assembly can handle extension fext
                        Assembly owningAssembly = (Assembly)this[fext.ToLower()];
                        if (owningAssembly == null)
                        {
                                throw new Exception("Could not find assembly for type " + fext);
                        }
                        string typename = null;
                        Type currentType = null;
                        foreach (Type type in owningAssembly.GetTypes ()) 
                        {
                                typename = type.ToString();
                                if(typename.IndexOf("+") == -1 && typename.ToLower().IndexOf(fext.ToLower().Substring(1)) != -1)
                                {
                                        currentType = type;
                                        break;
                                }
                        }

                        if(currentType == null)
                                throw new Exception("CPluginManager::CreateInstance : Type not found");

                        object createdInstance = null;
                        try 
                        {
                                BindingFlags flags = BindingFlags.Instance |
                                        BindingFlags.NonPublic |
                                        BindingFlags.CreateInstance;
                                object o = null;
                                try 
                                {
                                        o = Activator.CreateInstance(currentType, constructorParams);
                                        return o;
                                } 
                                catch (Exception e) 
                                {
                                        throw new Exception("Can't instance: "+ e.Message);
                                }
                                return createdInstance;
                        }
                        catch(Exception e)
                        {
                                throw new Exception(e.Message);
                        }
                }

                ///<summary>
				/// Nested enum to qualify members types handled
				///</summary>
                public enum Flags
                {
                        Field = 0,
                        Method = 1,
                        StaticField = 2
                }
                
                ///<summary>
				/// InvokeMember can call whichever of the members ( handled by the Flags enum )
				/// from any type.
				///</summary>
                public static object InvokeMember(Type t, string n, object o, Flags f)
                {

                        BindingFlags [] flags = new BindingFlags[2];
                        flags[0] = BindingFlags.GetProperty  | BindingFlags.Default;
                        flags[1] = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic
                                   | BindingFlags.Instance | BindingFlags.InvokeMethod;
                        return t.InvokeMember(n,flags[(int)f],null,o,null);
                }
                
                ///<summary>
				/// InvokeMember ( obsolete InvokeProperty )
				///</summary>
                public static void InvokeMember(Type t, string n, object o,object [] args)
                {
                        BindingFlags flags = BindingFlags.SetProperty  | BindingFlags.Default;
                        t.InvokeMember(n,flags,null,o,args);
                }
                
                ///<summary>
				/// GetStatic calls a static function called "n" from a class of type t 
				///</summary>
                public static object GetStatic(Type t,string n)
                {
                        PropertyInfo register = t.GetProperty(n, BindingFlags.Public | BindingFlags.Static | BindingFlags.GetProperty);
                        if (register == null)
                                throw new InvalidOperationException("Instance of "+t.ToString()+" must have a static "+n+" field.");
                        return register.GetValue(null,new object[0]);
                }

				///<summary>
				/// GetStatic calls a static function called "n" from a class called "t"
				///</summary>
                public static object GetStatic(string t,string n)
                {
                        Type type = Type.GetType(t);
                        PropertyInfo register = type.GetProperty(n, BindingFlags.Public | BindingFlags.Static | BindingFlags.GetProperty);
                        if (register == null)
                                throw new InvalidOperationException("Instance of "+t.ToString()+" must have a static "+n+" field.");
                        return register.GetValue(null,new object[0]);
                }
                
				///<summary>
				/// SetStatic calls a static function of a class type t, and assign at field fieldname a value val  
				///</summary>
                public static void SetStatic(Type t,string fieldname, string val)
                {
                        try
                        {
                                t.InvokeMember(fieldname, BindingFlags.Default | BindingFlags.SetProperty | BindingFlags.Static, null, null, new object[] { val });
                        }
                        catch(Exception e) {
                                throw new Exception("SetStatic for type "+t.ToString()+" on "+fieldname+" : "+e.Message);
                        }
                }
        }
        
        /// <summary>
        /// CSettings: singleton thread safe double locked
        /// Can handle its internal data through GConf# or the homemade replacement Conf
        /// </summary>
        public class CConfig
        {
                private int l_unused_FranckV_Version = 20;
                
				#if USING_GCONF
                private GConf.Client  _Conf     = new GConf.Client();
				#else
                private MediaNET.Conf _Conf = Conf.Client();
				#endif
				
                private Hashtable 	  _Preferences = Hashtable.Synchronized(new Hashtable());

                // Singleton's things
                private static volatile CConfig s_vInstance;
                private static string s_sLock = "CConfig lock member";

				///<summary>
				/// This property hide's the fact that CConfig is not an hashtable but actually
				/// has one for its internal data
				///</summary>
                public object this[string key] 
                {
                        get 
                        {
                                if(_Preferences[key] == null) {
                                        _Preferences[key] = _Get(key,key);
                                }
                                return _Preferences[key];        
                        }
                        set 
                        {
                                _Preferences[key] = value;        
                        }
                }

				///<summary>
				/// CConfig default constructor
				///</summary>
                private CConfig()
                {
                        Load();
                }

				///<summary>
				/// Instance returns the single instance of CConfig file 
				///</summary>
                public static CConfig Instance
                {
                        get 
                        {
                                if (s_vInstance == null) 
                                {
                                        lock (s_sLock);
                                        {
                                                if (s_vInstance == null) 
                                                        s_vInstance = new CConfig();
                                        }
                                }
                                return s_vInstance;
                        }
                }
                
                ///<summary>
				/// _Get returns a string from whichever of the XML files
				///</summary>
                private object _Get(string key, string defaultvalue) 
                {
                        try {
                                return Convert.ChangeType((_Conf.Get("/apps/medianet/"+key)), defaultvalue.GetType());
                        } catch (NoSuchKeyException) {
                                _Set(key, defaultvalue);
                                return defaultvalue;
                        }
                }
                
				///<summary>
				/// _Set store in the XML file any data, all is synchronized on the fly 
				///</summary>
                private void _Set(string key, string valueobj)
                {
                        _Conf.Set("/apps/medianet/"+key, valueobj);
                }

				///<summary>
				/// Nothing to Load(), we read from the file whenever we need something
				/// TODO: handle a cache file to enhance speed and code quality
				///</summary>
                public void Load()
                {
                }

				///<summary>
				/// Save() force the hashtable's synchro
				///</summary>
                public void Save()
                {
                        foreach (string key in _Preferences.Keys) {
                                _Set(key, (string)_Preferences[key]);
                        }
                        _Conf.SuggestSync();
                }    

        }
}

