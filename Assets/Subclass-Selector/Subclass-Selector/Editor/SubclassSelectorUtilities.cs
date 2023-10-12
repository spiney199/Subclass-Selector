#if UNITY_EDITOR && ODIN_INSPECTOR
namespace LBG.Editor
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using UnityEngine;
	using UnityEditor;
	using Sirenix.OdinInspector;
	using Sirenix.OdinInspector.Editor;
	using Sirenix.OdinInspector.Editor.ValueResolvers;
	using Sirenix.OdinInspector.Editor.ActionResolvers;
	using Sirenix.Utilities;
	using Sirenix.Utilities.Editor;

	/// <summary>
	/// Editor utility methods for use with <see cref="SubclassSelectorAttribute"/> and related drawers.
	/// </summary>
	public static class SubclassSelectorUtilities
	{
		#region Resolver Strings

		public static string OpenSubclassSelectorString => $"@{nameof(SubclassSelectorUtilities)}.{nameof(OpenSubclassCollectionSelector)}($property)";

		#endregion

		#region Validation and Data Retreival

		/// <summary>
		/// Gets all child types of <paramref name="baseType"/>, filtering abstract types.
		/// </summary>
		/// <param name="baseType">Type to get all derived/child types of.</param>
		public static IEnumerable<Type> GetTypeDerivedTypes(Type baseType, InspectorProperty inspectorProperty)
		{
			var subclassSelector = inspectorProperty.Attributes.GetAttribute<SubclassSelectorAttribute>();
			ValueResolver<bool> typeFilterResolver = null;

			if (subclassSelector.HasCustomTypeFilter)
			{
				typeFilterResolver = ValueResolver.Get<bool>(inspectorProperty, subclassSelector.CustomTypeFilter,
					new Sirenix.OdinInspector.Editor.ValueResolvers.NamedValue("type", typeof(Type)));

				if (typeFilterResolver.HasError)
				{
					Debug.LogError(typeFilterResolver.ErrorMessage);
				}
			}

			var types = TypeCache.GetTypesDerivedFrom(baseType).Where(AbstractTypeFilter);

			if (!baseType.IsAbstract && PassesCustomTypeFilter(baseType))
			{
				yield return baseType;
			}

			foreach (Type type in types)
			{
				if (PassesCustomTypeFilter(type))
				{
					yield return type;
				}
			}

			// local methods
			static bool AbstractTypeFilter(Type type) => !type.IsAbstract;

			bool PassesCustomTypeFilter(Type type)
			{
				if (typeFilterResolver == null || typeFilterResolver.HasError)
				{
					return true;
				}
				else
				{
					typeFilterResolver.Context.NamedValues.Set("type", type);
					bool result = typeFilterResolver.GetValue();
					return result;
				}
			}
		}

		/// <summary>
		/// Returns the path for a type, checking for <see cref="SubclassPathAttribute"/> and 
		/// returning the specified path/name if present.
		/// </summary>
		public static string GetTypeSubclassPath(Type type)
		{
			SubclassPathAttribute pathAttribute = type.GetAttribute<SubclassPathAttribute>();

			if (pathAttribute == null || (!pathAttribute.HasSubClassPath && !pathAttribute.HasSubClassName))
			{
				return type.GetNiceName();
			}

			string path = string.Empty;
			if (pathAttribute.HasSubClassPath)
			{
				path += (pathAttribute.SubClassPath + "/");
			}

			path += pathAttribute.HasSubClassName ? pathAttribute.SubClassName : type.Name;

			return path;
		}

		/// <summary>
		/// Returns whether a particular type exists with children of an <see cref="InspectorProperty"/>.
		/// </summary>
		/// <param name="derivedAreDuplicates">If true, types derived from <paramref name="type"/> will also count.</param>
		public static bool TypeExistsInPropertyChildren(PropertyChildren children, Type type)
		{
			foreach (var child in children)
			{
				Type valueType = child.ValueEntry.TypeOfValue;
				bool duplicate = (valueType == type);

				if (duplicate)
				{
					return true;
				}
			}

			return false;
		}

		#endregion

		#region Field Methods

		/// <summary>
		/// Yields a collection of <see cref="ValueDropdownItem{T}"/> for all valid subtypes of the specified <see cref="InspectorProperty"/>.
		/// </summary>
		public static IEnumerable<ValueDropdownItem<Type>> GetSubclassSelectorDropdownItems(InspectorProperty property)
		{
			SubclassSelectorAttribute subclassSelector = property.Attributes.GetAttribute<SubclassSelectorAttribute>();
			IEnumerable<Type> types = GetTypeDerivedTypes(property.ValueEntry.BaseValueType, property);

			InspectorProperty parent = property.ParentValueProperty;
			bool parentIsCollection = parent != null && parent.ChildResolver is ICollectionResolver;

			// if the parent is not a collection,
			// or we don't care about duplicate values...
			if (!parentIsCollection || subclassSelector.AllowDuplicates)
			{
				foreach (var type in types)
				{
					yield return GetDropdownItem(type);
				}
			}
			else
			{
				foreach (var type in types)
				{
					// the current type should be selectable so that the dropdown
					// draws the correct Subclass path
					if (type == property.ValueEntry.TypeOfValue)
					{
						yield return GetDropdownItem(type);
						continue;
					}

					if (!TypeExistsInPropertyChildren(parent.Children, type))
					{
						yield return GetDropdownItem(type);
					}
				}
			}

			static ValueDropdownItem<Type> GetDropdownItem(Type type)
			{
				string name = GetTypeSubclassPath(type);
				return new ValueDropdownItem<Type>(name, type);
			}
		}

		#endregion

		#region Collection Methods

		public static void OpenSubclassCollectionSelector(InspectorProperty property)
		{
			var collectionResolver = (ICollectionResolver)property.ChildResolver;
			var selectorAttribute = property.Attributes.GetAttribute<SubclassSelectorAttribute>();

			Type elementType = collectionResolver.ElementType;
			IEnumerable<Type> types = GetTypeDerivedTypes(collectionResolver.ElementType, property).Where(GenericSelectorTypeFilter);

			List<GenericSelectorItem<Type>> selectorTypes = new List<GenericSelectorItem<Type>>();
			foreach (Type type in types)
			{
				string name = GetTypeSubclassPath(type);
				var selectorItem = new GenericSelectorItem<Type>(name, type);

				selectorTypes.Add(selectorItem);
			}

			GenericSelector<Type> selector = new GenericSelector<Type>(title: "Select Types", true, selectorTypes);

			foreach (var menuItem in selector.SelectionTree.EnumerateTree())
			{
				// skip menu item if it has children (a dropdown) or has a null value
				if (menuItem.ChildMenuItems.Count > 0 && menuItem.Value == null)
				{
					continue;
				}

				Type itemType = (Type)menuItem.Value;
				bool hasDefaultConstructor = itemType.GetConstructor(Type.EmptyTypes) != null;

				if (!hasDefaultConstructor)
				{
					menuItem.Icon = EditorIcons.AlertTriangle.Raw;
					menuItem.Name = $"{menuItem.Name} - No Default Constructor";
				}
				else
				{
					menuItem.Icon = EditorIcons.File.Raw;
				}
			}

			selector.DrawConfirmSelectionButton = true;
			selector.SelectionConfirmed += AddSelectedTypes;
			selector.ShowInPopup();

			// local methods
			bool GenericSelectorTypeFilter(Type type)
			{
				if (selectorAttribute.AllowDuplicates)
				{
					return true;
				}

				return !TypeExistsInPropertyChildren(property.Children, type);
			}

			void AddSelectedTypes(IEnumerable<Type> selectedTypes)
			{
				ActionResolver onTypeSelected = null;

				if (selectorAttribute.HasOnTypesSelected)
				{
					onTypeSelected = ActionResolver.Get(property, selectorAttribute.OnTypesSelected,
						new Sirenix.OdinInspector.Editor.ActionResolvers.NamedValue("types", typeof(List<Type>)));

					if (onTypeSelected.HasError)
					{
						Debug.LogError(onTypeSelected.ErrorMessage);
					}
				}

				if (onTypeSelected != null && !onTypeSelected.HasError)
				{
					List<Type> onTypes = selectedTypes.ToList();
					onTypeSelected.Context.NamedValues.Set("types", onTypes);
					onTypeSelected.DoActionForAllSelectionIndices();
				}
				else
				{
					var values = new object[1] { null };

					foreach (var selection in selectedTypes)
					{
						// prevent errors when selecting dropdown values
						if (selection == null)
						{
							continue;
						}

						var instance = GetInstanceFromType(selection);
						values[0] = instance;

						collectionResolver.QueueAdd(values);
						collectionResolver.ApplyChanges();
					}
				}
			}
		}

		/// <summary>
		/// Creates an instance from the specified type, respecting default constructors if present. 
		/// Otherwise a default Unity serialised instance is created.
		/// </summary>
		public static object GetInstanceFromType(Type type)
		{
			if (type == null)
			{
				return null;
			}

			if (typeof(UnityEngine.Object).IsAssignableFrom(type))
			{
				return null;
			}

			ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
			if (constructor != null)
			{
				var instance = constructor.Invoke(null);
				return instance;
			}
			else
			{
				var instance = Sirenix.Serialization.UnitySerializationUtility.CreateDefaultUnityInitializedObject(type); 
				return instance;
			}
		}

		#endregion
	}
}
#endif