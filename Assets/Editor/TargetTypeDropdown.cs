// Editor/TargetTypeDropdown.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class TargetTypeDropdown : AdvancedDropdown
{
    private readonly Action<Type> _onTypeSelected;
    private readonly List<Type> _types;

    public TargetTypeDropdown(AdvancedDropdownState state, Action<Type> onTypeSelected) : base(state)
    {
        _onTypeSelected = onTypeSelected;
        _types = TypeCache.GetTypesDerivedFrom<INodeActionSource>()
            .Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition)
            .OrderBy(t => t.Name)
            .ToList();

        minimumSize = new Vector2(300, 400);
    }

    protected override AdvancedDropdownItem BuildRoot()
    {
        AdvancedDropdownItem root = new AdvancedDropdownItem("Target Type");
        foreach (Type type in _types)
        {
            root.AddChild(new TargetTypeDropdownItem(type));
        }
        return root;
    }

    protected override void ItemSelected(AdvancedDropdownItem item)
    {
        if (item is TargetTypeDropdownItem typeItem)
        {
            _onTypeSelected?.Invoke(typeItem.Type);
        }
    }

    private class TargetTypeDropdownItem : AdvancedDropdownItem
    {
        public readonly Type Type;

        public TargetTypeDropdownItem(Type type) : base(type.Name)
        {
            Type = type;
        }
    }
}
