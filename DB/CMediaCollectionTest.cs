/*---------------------------------------------------------------------
 *	@file:CMediaCollectionTest.cs
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
using MediaNET.DB;
using NUnit.Framework;
using System.Collections;

namespace MediaNET.DB.Tests
{
        ///<summary>
        /// Unit Test on the Collection
        ///</summary>
        [TestFixture]
        public class Test
        {
                private CMediaCollection m_cCollection = null;
		private Query q = null;
		private Mere m;
		
                [Test]
                public void Instanciate()
                {
                        m_cCollection = CMediaCollection.Instance;
                        Assert.IsNotNull(m_cCollection,"Instanciation is working");
                }
		
		[Test]
		public void InstanciateOnlyOnce()
		{
			CMediaCollection c2 = CMediaCollection.Instance;
			Assert.AreSame(m_cCollection,c2,"The singleton's protection is working");
		}
		[Test]
		public void InstanciateEmpty()
		{
			Assert.IsTrue(m_cCollection.Count == 0,"Collection is empty at startup");
		}
		
                [Test]
                public void TestInsert()
                {
			m = new Mere("Foo",42);
                        m_cCollection.Add(m);
                        Assert.IsFalse(m_cCollection.Count == 0,"Add sucessfull");
                }

		[Test]
		public void TestRemove()
		{
			m_cCollection.Remove(m);
			Assert.IsTrue(m_cCollection.Count == 0,"Remove sucessfull");
		}  
		
		[Test]
		public void TestAddDoublon()
		{
			Fils f = new Fils("Jo",10,m);
			m_cCollection.Add(f);
			m_cCollection.Add(f);
		}                       
        
		[Test]
		public void TestSaveAndLoad()
		{
			m_cCollection.Save();
			m_cCollection.Clear();
			Assert.IsTrue(m_cCollection.Count == 0,"Clear is successfull");			
			m_cCollection.Load();
			Assert.IsTrue(m_cCollection.Count == 2,"Reload is successfull");			
		}
		
		[Test]
		public void TestQueryConstructor()
		{
			
			q = new Query();
			Assert.IsNotNull(q);
		}
		
		[Test]
		public void TestAddCriteria()
		{
			q.AddCriteria("Name","Jo");
			Assert.IsTrue(q.Count == 1,"Query AddCriteria Sucessfull");
		}
		
		[Test]
		public void TestSearchThroughCollection()
		{
			Array s = m_cCollection.Search(q);
			Assert.IsTrue(s.Length == 2,"Two elements found, good"+s.Length);
		}
		
		

        }
	
	[Serializable]
	abstract class Personne  {
		private int age;
		private string name;
	
		public Personne() {
			Age = 0;
			Name = "null";
		}

		abstract public void Parle();

		virtual public string Name
		{
			get { return name; }
			set { name = value; }
		}
		
		virtual public int Age
		{
			get { return age; }
			set { age = value; }
		}
	}


	[Serializable]
	class Mere: Personne {
		public Mere(string daName,int daAge) {
			Age = daAge;
			Name = daName;
		}

		override public void Parle() {
			Console.WriteLine("Maman: "+Name+": "+Age);
		}
	}
	[Serializable]
	class Fils: Personne {
		private Mere mere;
		public Fils(string daName,int daAge, Personne parent) {
			Age = daAge;
			Name = daName;
			mere = (Mere)parent;
		}

		override public void Parle() {
			Console.WriteLine("Fils: "+Name+": "+Age+", ne :"+mere.Name);
		}
	}
}