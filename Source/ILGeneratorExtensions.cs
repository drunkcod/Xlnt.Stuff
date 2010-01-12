using System.Reflection.Emit;

namespace NMeter
{
    public static class ILGeneratorExtensions
    {
        public static ILGenerator Ldc(this ILGenerator il, int value) {
            il.Emit(OpCodes.Ldc_I4, value);
            return il;
        }

        public static ILGenerator Nop(this ILGenerator il) {
            il.Emit(OpCodes.Nop);
            return il;
        }

        public static ILGenerator Ret(this ILGenerator il) {
            il.Emit(OpCodes.Ret);
            return il;
        }
    }
}
