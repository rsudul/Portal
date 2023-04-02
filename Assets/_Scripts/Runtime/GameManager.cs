using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System;

public class GameManager : MonoBehaviour
{
	[SerializeField]
	private List<GameObject> _instancedGameObjects; // add a text field which says what type of instanced system it is

	private Dictionary<Type, InstancedSystem> _instancedSystems;

	private readonly List<ISceneInjectee> _cachedInjectes = new List<ISceneInjectee>();

	private void Awake()
	{
		InstantiateSystems();
		InjectDependencies();
		InitializeSystems();
		CallOnSystemsInitialized();
		CallOnInjectedInAllInjectees();
	}

	private void InstantiateSystems()
	{
		_instancedSystems = new Dictionary<Type, InstancedSystem>();

		foreach (var item in _instancedGameObjects)
		{
			var instancedObject = Instantiate(item, transform);

			TryAddAsSystem(instancedObject);
		}
	}

	private void TryAddAsSystem(GameObject instancedObject)
	{
		if (instancedObject.TryGetComponent<InstancedSystem>(out var instancedSystem))
		{
			if (!_instancedSystems.ContainsKey(instancedSystem.Type))
			{
				_instancedSystems.Add(instancedSystem.Type, instancedSystem);
			}
		}

		foreach (var sys in instancedObject.GetComponentsInChildren<InstancedSystem>())
        {
			if (!_instancedSystems.ContainsKey(sys.Type))
            {
				_instancedSystems.Add(sys.Type, sys);
            }
        }
	}

	private void InjectDependencies()
	{
		var injectables = FindObjectsOfType<MonoBehaviour>().OfType<ISceneInjectee>();

		foreach (var injectable in injectables)
		{
			object obj = injectable;

			var properties = obj
				.GetType()
				.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
				.Where(prop => prop.IsDefined(typeof(InjectAttribute), false));

			foreach (var property in properties)
			{
				if (_instancedSystems.ContainsKey(property.FieldType))
				{
					property.SetValue(obj, _instancedSystems[property.FieldType]);
				}
			}

			_cachedInjectes.Add(injectable);
		}
	}

	private void InitializeSystems()
	{
		foreach (var system in _instancedSystems.Values)
		{
			system.Initialize();
		}
	}

	private void CallOnSystemsInitialized()
	{
		foreach (var system in _instancedSystems.Values)
		{
			system.OnSystemsInitialized();
		}
	}

	private void CallOnInjectedInAllInjectees()
	{
		foreach (var injectee in _cachedInjectes)
		{
			injectee.OnInjected();
		}
	}
}
