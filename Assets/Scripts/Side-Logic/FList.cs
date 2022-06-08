using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Side_Logic
{
	public class FList<T> : IEnumerable<T> 
	{
		private readonly IEnumerable<T> _collection;
		
		//Use FList.Create instead of this constructor directly
		public FList() => _collection = Array.Empty<T>();

		//Use FList.Create instead of this constructor directly
		public FList(T singleValue) => _collection = new T[1] { singleValue };

		//Use FList.Cast instead of this constructor directly
		public FList(IEnumerable<T> collection) => this._collection = collection;

		//Specificity to resolve ambiguity between FList-IEnumerable
		public static FList<T> operator +(FList<T> lhs, FList<T> rhs) => FList.Cast(lhs._collection.Concat(rhs._collection));

		public static FList<T> operator+(FList<T> lhs, IEnumerable<T> rhs) => FList.Cast(lhs._collection.Concat(rhs));

		public static FList<T> operator+(IEnumerable<T> lhs, FList<T> rhs) => FList.Cast(FList.Cast(lhs)._collection.Concat(rhs));

		//This isn't semantically correct, but it's cleaner to read.
		public static FList<T> operator+(FList<T> lhs, T rhs) => lhs + FList.Create(rhs);

		//This isn't semantically correct, but it's cleaner to read.
		public static FList<T> operator+(T lhs, FList<T> rhs) => FList.Create(lhs) + rhs;
		
		public IEnumerator<T> GetEnumerator() 
		{
			foreach(var element in _collection)
			{
				yield return element;
			}
		}
		
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	public static class FList 
	{
		public static FList<T> ToFList<T>(this IEnumerable<T> lhs) => Cast(lhs);

		public static FList<T> Create<T>() => new FList<T>();

		public static FList<T> Create<T>(T singleValue) => new FList<T>(singleValue);

		public static FList<T> Cast<T>(IEnumerable<T> collection) => new FList<T>(collection);
	}
}