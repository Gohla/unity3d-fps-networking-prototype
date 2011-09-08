using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IRecyclableObject {
	void Recycle();
}

public class RecyclableObjectPool<T> where T : IRecyclableObject, new()  {
	
	Stack<T> reusable = new Stack<T>();
	
	public T Get() {
		if(reusable.Count > 0) {
			return reusable.Pop();
		}
		
		return new T();
	}
	
	public void Recycle(T o) {
		o.Recycle();
		reusable.Push(o);
	}
	
}
