#region

using System;
using UnityEngine.UI;

#endregion

namespace NebulaModel.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class UIContentTypeAttribute : Attribute
{
    public readonly InputField.ContentType ContentType;

    public UIContentTypeAttribute(InputField.ContentType contentType)
    {
        ContentType = contentType;
    }
}
