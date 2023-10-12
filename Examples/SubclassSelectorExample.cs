namespace LBG.Demo
{
	using System;
	using System.Collections.Generic;
	using UnityEngine;
	using Sirenix.OdinInspector;
	using Sirenix.Utilities;

#if UNITY_EDITOR
	using Sirenix.OdinInspector.Editor;
#endif

	[CreateAssetMenu(menuName = "LBG-Testing/Subclass Selector Test Asset")]
	public sealed class SubclassSelectorExample : SerializedScriptableObject
	{
		#region Inspector Fields

		[BoxGroup("F", LabelText = "Field Reference:")]
		[SerializeReference, SubclassSelector(DrawClassFoldout = true)]
		[InfoBox("Works on both interfaces and base classes (including abstract classes).")]
		private PlainClasses.IBaseInterface _interfaceField;

		[BoxGroup("F")]
		[SerializeReference, SubclassSelector(OnTypesSelected = "@UpdateInstance($property, $type)", HideClassLabel = true)]
		[InfoBox("Updating/Adding instances can be handled manually with the OnTypesSelected property.")]
		private PlainClasses.ClassBase _baseClassField;

		[BoxGroup("C", LabelText = "Collections:")]
		[InfoBox("Will work on any collection type that Odin supports.")]
		[SerializeReference, SubclassSelector(AllowDuplicates = false, DrawBoxForListElements = true)]
		private List<PlainClasses.IBaseInterface> _interfaceList = new List<PlainClasses.IBaseInterface>();

		[BoxGroup("C")]
		[SerializeField, SubclassSelector(DrawDropdownForListElements = true)]
		[InfoBox("Use the DrawDropdownForListElements property to be able to change the type of existing list elements.")]
		private Queue<PlainClasses.ClassBase> _classQueue = new Queue<PlainClasses.ClassBase>();

		[BoxGroup("C")]
		[SerializeField]
		[SubclassSelector(DrawBoxForListElements = true, 
			OnTypesSelected = "@AddSelectedTypes($property, $types)",
			CustomTypeFilter = "@FilterDerivedTypes($type)")]
		[InfoBox("More specific type filtering can be provided using the CustomTypeFilter property.")]
		private Stack<PlainClasses.ClassBase> _classStack = new Stack<PlainClasses.ClassBase>();
		
		[BoxGroup("C")]
		[InfoBox("Does not work on anything inheriting from UnityEngine.Object.")]
		[SerializeField]
		private List<ScriptableObject> scriptableObjects = new();

		#endregion

		#region Resolver Methods

		// Supports overriding the assignment of selected type, or the adding of selected types to a collection.
		// Fields provide a $type named value, and collections a $types named value.

#if UNITY_EDITOR
		public void UpdateInstance(InspectorProperty property, Type type)
		{
			Debug.Log(type.GetNiceName());
			var instance = (PlainClasses.ClassBase)Activator.CreateInstance(type);
			property.ValueEntry.WeakSmartValue = instance;
		}

		public void AddSelectedTypes(InspectorProperty property, IEnumerable<Type> selectedTypes)
		{
			var values = new object[1] { null };
			ICollectionResolver collectionResolver = (ICollectionResolver)property.ChildResolver;

			foreach (Type type in selectedTypes)
			{
				Debug.Log(type.GetNiceName());
				var instance = (PlainClasses.ClassBase)Activator.CreateInstance(type);
				values[0] = instance;
				collectionResolver.QueueAdd(values);
				collectionResolver.ApplyChanges();
			}
		}
#endif

		// Supports a value resolver for filtering types after subtypes have been generated.
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
}