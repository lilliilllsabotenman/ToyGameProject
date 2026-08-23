// Editor/ComponentTypeDropdown.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class ComponentTypeDropdown : AdvancedDropdown
{
    private readonly Action<Type> _onTypeSelected;
    private readonly List<Type> _types;

    public ComponentTypeDropdown(AdvancedDropdownState state, Action<Type> onTypeSelected) : base(state)
    {
        _onTypeSelected = onTypeSelected;
        _types = TypeCache.GetTypesDerivedFrom<Component>()
            .Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition)
            .OrderBy(t => t.Name)
            .ToList();

        minimumSize = new Vector2(300, 400);
    }

    protected override AdvancedDropdownItem BuildRoot()
    {
        AdvancedDropdownItem root = new AdvancedDropdownItem("Component");
        foreach (Type type in _types)
        {
            root.AddChild(new ComponentTypeDropdownItem(type));
        }
        return root;
    }

    protected override void ItemSelected(AdvancedDropdownItem item)
    {
        if (item is ComponentTypeDropdownItem typeItem)
        {
            _onTypeSelected?.Invoke(typeItem.Type);
        }
    }

    private class ComponentTypeDropdownItem : AdvancedDropdownItem
    {
        public readonly Type Type;

        public ComponentTypeDropdownItem(Type type) : base(type.Name)
        {
            Type = type;
        }
    }
}
