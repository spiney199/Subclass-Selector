![Header](https://imagizer.imageshack.com/img922/7262/vLmyBr.jpg)

# Subclass-Selector

One of Odin's best out-of-the-box features is its support for `[SerializeReference]`, alongside supporting polymorphism through the Odin Serialiser. 

However, the default object reference picker is (in my opinion) somewhat janky and awkward to use. And as the most common use case of `[SerializeReference]` is to serialise a base class or interface, I made a `[SubclassSelector]` attribute to expedite this common use case in a more user friendly manner. It comes partnered with a `[SubclassPath]` attribute to customise the display-name and path when selecting types.

The `[SubclassSelector]` attribute can simply be applied to a Non-UnityEngine.Object reference type member or collection:
```csharp
public class SomeScriptableObject : ScriptableObject
{
    [SerializeReference, SubclassSelector]
    public SomeBaseClass BaseClassField;
	
    [SerializeReference, SubclassSelector]
    public List<SomeBaseClass> SomeBaseClassCollection;
}
```

`[SubclassPath]` can be used on your class definitions to define their selection grouping and to give them designer friendly names:
```csharp
public static class PlainClasses
{
	public interface IBaseInterface { }

	[System.Serializable]
	[SubclassPath(SubClassName = "Base Class")]
	public class ClassBase : IBaseInterface
	{
		public string TestString;
	}

	[System.Serializable]
	[SubclassPath("Derived", "Class A")]
	public class ClassA : ClassBase
	{
		public float ClassAFloat;
	}

	[System.Serializable]
	[SubclassPath("Derived", "Class B")]
	public class ClassB : ClassA
	{
		public GameObject ClassCGameObject;
	}

	[System.Serializable]
	[SubclassPath("Derived", "Class C")]
	public class ClassC : ClassBase
	{
		public ClassC(ClassBase classBase)
		{
			this.TestString = classBase.TestString;
			this._classCBool = true;
		}

		public bool ClassCBool;
	}
}
```
![Grouping](https://imagizer.imageshack.com/img922/2271/SWhjJS.jpg)

`[SubclassSelector]` has been set up to work with any type of collection that Unity or Odin supports. The attribute also has a small number of properties that let you customise various aspects of the attributes behaviour.

+ **AllowDuplicates** - True by default. If set to fales, types already in a collection will not able to be added to said collection. Note that this doesn't include types derived from existing types.
+ **HideReferencePicker** - True by default. Hides or shows the default Odin object reference picker. Effectively behaves as an integrated `[HideReferenceObjectPicker]`.
+ **DrawDropdownForListElements** - False by default. Shows or hides the type selection dropdown when *[SubclassSelector]* is used on a collection.
+ **HideClassLabel** - False by default. Acts as in integrated `[HideLabel]` if true. Only works on non-collection members.
+ **DrawClassFoldout** - False by default. If true, the type selector will also include a dropdown to show/hide the class' properties.
+ **DrawBoxForListElements** - If true, will draw collection elements in a BoxGroup-style box. Only affects the elements of collections.
+ **CustomTypeFilter** - A string resolved as a ValueResolver. Can be used to add extra filtering logic after all sub-types are generated. Provides a System.Type `$type` named value. Note that abstract types are already filtered.
+ **OnTypesSelected** - An action resolver that can be used to override how types are initialised and added to either fields or collections. Collections provide a $type named parameter and passing a collection of selected types, and fields have a $type named parameter and pass the selected type.

# Installation
This plugin can be installed by adding this package via a git URL: `https://github.com/spiney199/Subclass-Selector.git?path=Assets/Subclass-Selector/Subclass-Selector`