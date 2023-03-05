#if UNITY_EDITOR && ODIN_INSPECTOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;

namespace Spiney.SubclassSelector
{
	/// <summary>
	/// Utility methods for use with <see cref="SubclassSelectorAttributeProcessor{T}"/>.
	/// </summary>
	public static class SubclassSelectorUtilities
	{
		#region Resolver Strings

		public static string OpenSubclassSelectorString => $"@{nameof(SubclassSelectorUtilities)}.{nameof(OpenSubclassCollectionSelector)}($property)";

		public static string TypeFilterResolverString => $"@{nameof(SubclassSelectorUtilities)}.{nameof(GetSubclassSelectorDropdownItems)}($property)";

		public static string OnBeginBoxSubclassElementString => $"@{nameof(SubclassSelectorUtilities)}.{nameof(BeginDrawBoxedSubclassElement)}($property, $index)";

		public static string OnEndBoxSubclassElementString => $"@{nameof(SubclassSelectorUtilities)}.{nameof(EndDrawBoxedSubclassElement)}()";

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
					new NamedValue("type", typeof(Type)));

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
				foreach (var selection in selectedTypes)
				{
					// prevent errors when selecting dropdown values
					if (selection == null)
					{
						continue;
					}

					bool hasDefaultConstructor = selection.GetConstructor(Type.EmptyTypes) != null;
					object[] values;

					if (hasDefaultConstructor)
					{
						var instance = Activator.CreateInstance(selection);
						values = new object[] { instance };
					}
					else
					{
						var instance = Sirenix.Serialization.UnitySerializationUtility.CreateDefaultUnityInitializedObject(selection);
						values = new object[] { instance };
					}

					collectionResolver.QueueAdd(values);
					property.Children.Update();
				}
			}
		}

		public static void BeginDrawBoxedSubclassElement(InspectorProperty property, int index)
		{
			var child = property.Children[index];
			var subclassPath = child.ValueEntry.TypeOfValue.GetAttribute<SubclassPathAttribute>();
			string groupName = subclassPath?.SubClassName ?? child.ValueEntry.TypeOfValue.GetNiceName();
			groupName = groupName.Replace('.', '-');
			SirenixEditorGUI.BeginBox(groupName, centerLabel: true);
		}

		public static void EndDrawBoxedSubclassElement()
		{
			SirenixEditorGUI.EndBox();
		}

		#endregion
	}
}
#endif