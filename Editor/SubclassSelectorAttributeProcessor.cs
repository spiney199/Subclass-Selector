#if UNITY_EDITOR && ODIN_INSPECTOR
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;

internal sealed class SubclassSelectorAttributeProcessor<T> : OdinAttributeProcessor<T> where T : class
{
	#region Internal Members

	private SubclassSelectorAttribute subclassSelector;
	private bool isCollection = false;
	private bool parentIsCollection = false;

	#endregion

	#region Processor Overrides

	public override bool CanProcessSelfAttributes(InspectorProperty property)
	{
		if ((subclassSelector = property.Attributes.GetAttribute<SubclassSelectorAttribute>()) == null)
		{
			return false;
		}

		bool isUnityObject;
		if (property.ChildResolver is ICollectionResolver collectionResolver)
		{
			isCollection = true;
			Type elementType = collectionResolver.ElementType;
			isUnityObject = typeof(UnityEngine.Object).IsAssignableFrom(elementType);
			return !isUnityObject;
		}
		else
		{
			isCollection = false;
			parentIsCollection = typeof(ICollectionResolver).IsAssignableFrom(property.ParentValueProperty?.ChildResolver?.GetType() ?? null);
			isUnityObject = typeof(UnityEngine.Object).IsAssignableFrom(property.ValueEntry.BaseValueType);
			
			if (parentIsCollection)
			{
				return !isUnityObject && subclassSelector.DrawDropdownForListElements;
			}
			else
			{
				return !isUnityObject;
			}
		}
	}

	public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
	{			
		var hideReferencePicker = property.Attributes.GetAttribute<HideReferenceObjectPickerAttribute>();
		if (subclassSelector.HideReferencePicker && hideReferencePicker == null)
		{
			hideReferencePicker = new HideReferenceObjectPickerAttribute();
			attributes.Add(hideReferencePicker);
		}

		if (isCollection)
		{
			var lds = property.Attributes.GetAttribute<ListDrawerSettingsAttribute>();

			if (lds == null)
			{
				lds = new ListDrawerSettingsAttribute();
				attributes.Add(lds);
			}

			lds.CustomAddFunction = SubclassSelectorUtilities.OpenSubclassSelectorString;
		}
		else
		{
			var hideLabel = property.Attributes.GetAttribute<HideLabelAttribute>();

			if (subclassSelector.HideClassLabel && hideLabel == null)
			{
				hideLabel = new HideLabelAttribute();
				attributes.Add(hideLabel);
			}

			var typeFilter = new TypeFilterAttribute(SubclassSelectorUtilities.TypeFilterResolverString);

			if (parentIsCollection)
			{
				typeFilter.DrawValueNormally = true;
			}
			else
			{
				bool valueIsNull = property.ValueEntry.WeakSmartValue == null;
				bool hidePicker = subclassSelector.HideReferencePicker;
				typeFilter.DrawValueNormally = !valueIsNull || !hidePicker;
			}

			attributes.Add(typeFilter);
		}
	}

	#endregion
}
#endif