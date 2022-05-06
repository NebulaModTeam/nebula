using System;
using UnityEngine.UI;

namespace NebulaModel.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class UIContentTypeAttribute : Attribute
    {
        public readonly InputField.ContentType ContentType;

        public UIContentTypeAttribute(InputField.ContentType contentType)
        {
            ContentType = contentType;
        }
    }
}
