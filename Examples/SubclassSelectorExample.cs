using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "Subclass Selector Test Asset")]
public class SubclassSelectorExample : SerializedScriptableObject
{
	#region Inspector Fields

	[BoxGroup("F", LabelText = "Field Reference:")]
	[SerializeReference, SubclassSelector]
	[InfoBox("Works on both interfaces and base classes (including abstract classes).")]
	private PlainClasses.IBaseInterface _interfaceField;

	[BoxGroup("F")]
	[SerializeReference, SubclassSelector]
	private PlainClasses.ClassBase _baseClassField;

	[BoxGroup("C", LabelText = "Collections:")]
	[SerializeReference, SubclassSelector(AllowDuplicates = false)]
	[InfoBox("Will work on any collection type that Odin supports.")]
	private List<PlainClasses.IBaseInterface> _unitInterfaceList = new List<PlainClasses.IBaseInterface>();

	[BoxGroup("C")]
	[SerializeField, SubclassSelector(DrawDropdownForListElements = true)]
	private Queue<PlainClasses.ClassBase> _classQueue = new Queue<PlainClasses.ClassBase>();

	[BoxGroup("C")]
	[SerializeField, SubclassSelector(CustomTypeFilter = nameof(FilterDerivedTypes))]
	private Stack<PlainClasses.ClassBase> _classStack = new Stack<PlainClasses.ClassBase>();

	[BoxGroup("C")]
	[InfoBox("Does not work on anything inheriting from UnityEngine.Object.")]
	[SerializeField, SubclassSelector]
	private List<ScriptableObject> scriptableObjects = new();

	#endregion

	#region Resolver Methods

	// Supports a value resolver for filtering types after subtypes have been generated (collections only).
	// This example will filter out any types that are derived from or match an existing type in the stack collection.
	public bool FilterDerivedTypes(Type type)
	{
		foreach (var instance in _classStack)
		{
			if (instance.GetType().IsAssignableFrom(type))
			{
				return false;
			}
		}

		return true;
	}

	#endregion
}