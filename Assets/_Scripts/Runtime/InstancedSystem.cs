using System;
using UnityEngine;

public class InstancedSystem : MonoBehaviour
{
	public virtual Type Type => GetType();

	public virtual void Initialize() { }

	public virtual void OnSystemsInitialized() { }
}
