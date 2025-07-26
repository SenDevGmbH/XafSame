using System;
using System.Reflection.Metadata;






#if !NET48
#endif

namespace SenDev.XafSame;

class SimpleTypeProvider : ICustomAttributeTypeProvider<object>
{
    public object GetPrimitiveType(PrimitiveTypeCode typeCode)
    {
        return typeof(string);
    }

    public object GetSystemType()
    {
        throw new NotImplementedException();
    }

    public object GetSZArrayType(object elementType)
    {
        throw new NotImplementedException();
    }

    public object GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind)
    {
        throw new NotImplementedException();
    }

    public object GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
    {
        throw new NotImplementedException();
    }

    public object GetTypeFromSerializedName(string name)
    {
        throw new NotImplementedException();
    }

    public PrimitiveTypeCode GetUnderlyingEnumType(object type)
    {
        throw new NotImplementedException();
    }

    public bool IsSystemType(object type)
    {
        throw new NotImplementedException();
    }
}
