/*---------------------------------------------------------------------
 *	@file:CMediaCollection.cs
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
using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

namespace MediaNET.DB 
{
        ///<summary>
        /// Multithreaded Singleton with double-check locking idiom 
        /// This collection is an pure-object database with simple query
        /// support. All object are then serialized in collection.odb file.
        ///</summary>
        [Serializable]
                public sealed class CMediaCollection: ArrayList
                {
                        ///<summary>
                        /// Members
                        ///</summary>
                        private static volatile CMediaCollection s_vInstance;
                        private readonly static string s_sFile = "collection.odb";
                        private string m_sFileLocation = s_sFile;


                        ///<summary>
                        ///Private constructor...
                        ///</summary>
                        private CMediaCollection() { }

                        ///<summary>
                        /// Get the unique instance of the collection
                        ///</summary>
                        public static CMediaCollection Instance
                        {
                                get 
                                {
                                        if (s_vInstance == null) 
                                        {
                                                lock (s_sFile) 
                                                {
                                                        if (s_vInstance == null) 
                                                                s_vInstance = new CMediaCollection();
                                                }
                                        }

                                        return s_vInstance;
                                }
                        }			

                        ///<summary>
                        /// Get/Set the collection file name
                        ///</summary>
                        public string Location
                        {
                                get { return m_sFileLocation; }
                                set { m_sFileLocation = value; }
                        }
                        
                        ///<summary>
                        /// Save all the data, object got to be serializable
                        ///</summary>
                        public bool Save()
                        {
                                FileStream myStream=null;
                                myStream = File.OpenWrite(m_sFileLocation);
                                if (myStream != null)
                                {
                                        IFormatter formatter = new BinaryFormatter();
                                        formatter.Serialize(myStream, this);
                                        myStream.Close();
                                        return true;
                                } 
                                else 
                                {
                                        myStream.Close();
                                        return false;
                                }
                        }

                        ///<summary>
                        /// Load all the data
                        ///</summary>
                        public bool Load()
                        {
                                Clear();
                                FileStream myStream = null;
                                IFormatter formatter;
                                ArrayList al;
                                try 
                                {
                                        myStream = File.OpenRead(m_sFileLocation);
                                } 
                                catch
                                {
                                        Console.WriteLine("Creating file....");
                                        Save();
                                }

                                if (myStream != null)
                                {
                                        formatter = new BinaryFormatter();
                                        al = (ArrayList)formatter.Deserialize(myStream);
                                        foreach(object o in al)
                                        {
                                                Add(o);
                                        }
                                        myStream.Close();
                                        return true;
                                }
                                myStream.Close();
                                return false;
                        }

                        public Array Search(Query ref_object)
                        {
                                ArrayList array = new ArrayList();

                                /// Search for the same members in collection
                                /// and add to the list corresponding objects
                                foreach (string FieldName in ref_object.Keys)
                                {
                                        foreach(object currentObject in this)
                                        {
                                                foreach(MemberInfo info in currentObject.GetType().GetMembers())
                                                {
                                                        if (info.Name == FieldName)
                                                        {
                                                                Type t = currentObject.GetType();
                                                                object val = (object)t.InvokeMember(FieldName,
                                                                                BindingFlags.Default | BindingFlags.GetProperty, null, currentObject, null);
                                                                if(ref_object[FieldName].Equals(val)) 
                                                                {
                                                                        array.Add(currentObject);
                                                                } 
                                                        }

                                                }
                                        }
                                }
                                return array.ToArray();
                        }
                }
        ///<summary>
        /// Provide a full customized selection of items
        ///</summary>
        public class Query: Hashtable 
        {
                public Query() {  }
                public void AddCriteria(object field, object val) 
                {
                        Add(field,val);
                }
        }

}
