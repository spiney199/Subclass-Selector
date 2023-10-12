#if UNITY_EDITOR && ODIN_INSPECTOR
namespace LBG.Editor.Drawers
{
	using UnityEngine;
	using Sirenix.OdinInspector.Editor;
	using Sirenix.Utilities;
	using Sirenix.Utilities.Editor;

	/// <summary>
	/// Attribute drawer for drawing collection elements with in a box if <see cref="SubclassSelectorAttribute.DrawBoxForListElements"/> is true.
	/// </summary>
	[DrawerPriority(0, 90, 1000)]
	internal sealed class SubclassSelectorCollectionAttributeDrawer<T> : OdinAttributeDrawer<SubclassSelectorAttribute, T> where T : class
	{
		#region Processor Overrides

		protected override bool CanDrawAttributeValueProperty(InspectorProperty property)
		{
			if (property.IsTreeRoot)
			{
				return false;
			}

			if (property.ChildResolver is ICollectionResolver)
			{
				return false;
			}
			else
			{
				bool parentIsCollection = property.ParentValueProperty.ChildResolver is ICollectionResolver;
				bool canDrawBox = property.GetAttribute<SubclassSelectorAttribute>()?.DrawBoxForListElements ?? false;
				return parentIsCollection && canDrawBox;
			}
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
			var subclassPath = this.Property.ValueEntry.TypeOfValue.GetAttribute<SubclassPathAttribute>();
			string groupName = subclassPath?.SubClassName ?? this.Property.ValueEntry.TypeOfValue.GetNiceName();
			groupName = groupName.Replace('.', '-');

			SirenixEditorGUI.BeginBox(groupName, centerLabel: true);
			{
				CallNextDrawer(label);
			}
			SirenixEditorGUI.EndBox();
		}

		#endregion
	}
}
#endif