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
        /// <p> CMediaCollection: an obect database class </p>
        /// <p>Multithreaded Singleton with double-check locking idiom 
        /// This collection is an pure-object database with simple query
        /// support. All object are then serialized in collection.odb file.</p>
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
                        /// <p> Private constructor... </p>
                        ///</summary>
                        private CMediaCollection() { }

                        ///<summary>
                        /// <p>Get the unique instance of the collection</p>
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
                        /// <p> Location property </p>
                        /// <p>  Get/Set the collection file name</p>
                        ///</summary>
                        public string Location
                        {
                                get { return m_sFileLocation; }
                                set { m_sFileLocation = value; }
                        }
                        
                        ///<summary>
                        /// <p>Save all the data, object got to be serializable</p>
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
                        /// <p>Load all the data</p>
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

						/// <summary>
						/// <p>The Search method takes a series of parameters to specify the search criterion ordered in a Query class
						/// and returns an array containing the result set.</p>
						/// <p>This method is core the functioning of the application and is used widely.
						/// If you need more advanced options, look at <see cref="Query"/></p>
						/// </summary>
						/// <example><code>
						/// 	Query q = new Query();
                        /// 	q.AddCriteria("GID",gid);
                        ///		foreach(object media in CMediaCollection.Instance.Search(q))
                        ///		{
                        ///     CMediaCollection.Instance.Remove(media);
                        ///		}
						///</code></example>
						/// <param name="ref_object">The query object containing criterion that we are searching for</param>
						/// <returns>An array istance containing the matching objects. </returns>
						/// <remarks> Remember that when you use this method you should:
						/// <list type="bullet">
						/// <item>
						/// <term>Test</term>
						/// <description> for performance</description>
						/// </item>
						/// <item>
						/// <term>Test</term>
						/// <description> for scalability</description>
						/// </item>
						/// </list>
					    /// </remarks>
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
        /// <p>Provide a full customized selection of items</p>
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
