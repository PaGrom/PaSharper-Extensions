using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using JetBrains.Metadata.Reader.API;
using JetBrains.ReSharper.Psi;

namespace PaSharperExtension
{
    /// <summary>
    /// Extension methods for IType
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class ITypeExtensions
    {
        [Pure]
        [ContractAnnotation("null => false")]
        public static bool IsHttpClient(this IType type) => type.IsClrType(ClrTypeNames.HttpClient);

        [Pure]
        [ContractAnnotation("null => false")]
        public static bool IsUri(this IType type) => type.IsClrType(ClrTypeNames.Uri);

        [Pure]
        [ContractAnnotation("typeElement:null => false", true)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool IsPredefinedTypeElement([CanBeNull] ITypeElement typeElement, [NotNull] IClrTypeName clrName)
            => typeElement != null && typeElement.GetClrName().Equals(clrName);

        [Pure]
        [ContractAnnotation("type:null => false", true)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsClrType([CanBeNull] this IType type, [NotNull] IClrTypeName clrName)
            => type is IDeclaredType declaredType && IsPredefinedTypeElement(declaredType.GetTypeElement(), clrName);
    }
}
