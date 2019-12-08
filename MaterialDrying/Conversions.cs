// ReSharper disable InconsistentNaming
namespace MaterialDrying
{
    using System.Diagnostics;

    internal static class Conversions
    {
        public static bool BetweenInclude(this int x, int left, int right)
        {
            Debug.Assert(left <= right, "left must be less or equal than right!");

            return x >= left
                   && x <= right;
        }

        public static decimal T_I_to_star(Constants c, decimal T_I)
        {
            return (T_I - c.T_s_3) / (c.T_s_eq - c.T_s_3);
        }

        public static decimal T_I_to_dimensional(Constants c, decimal T_I_star)
        {
            return (T_I_star * (c.T_s_eq - c.T_s_3)) + c.T_s_3;
        }

        public static decimal T_II_to_star(Constants c, decimal T_II)
        {
            return (T_II - c.T_II_ref) / (c.T_s_eq - c.T_II_ref);
        }

        public static decimal T_II_to_dimensional(Constants c, decimal T_II_star)
        {
            return (T_II_star * (c.T_s_eq - c.T_II_ref)) + c.T_II_ref;
        }
    }
}