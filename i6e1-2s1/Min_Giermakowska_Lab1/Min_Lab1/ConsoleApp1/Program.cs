using System;

namespace ConsoleApp1
{
    class Program
    {
        public static void function1_grad(double[] x, ref double func, double[] grad, object obj)
        {
            // this callback calculates f(x0,x1) = 100*(x0+3)^4 + (x1-3)^4
            // and its derivatives df/d0 and df/dx1
            func = 100 * System.Math.Pow(x[0] + 3, 4) + System.Math.Pow(x[1] - 3, 4);
            grad[0] = 400 * System.Math.Pow(x[0] + 3, 3);
            grad[1] = 4 * System.Math.Pow(x[1] - 3, 3);
        }
        public static int Main(string[] args)
        {
            //
            // This example demonstrates minimization of
            //
            //     f(x,y) = 100*(x+3)^4+(y-3)^4
            //
            // subject to box constraints
            //
            //     -1<=x<=+1, -1<=y<=+1
            //
            // using BLEIC optimizer with:
            // * initial point x=[0,0]
            // * unit scale being set for all variables (see minbleicsetscale for more info)
            // * stopping criteria set to "terminate after short enough step"
            // * OptGuard integrity check being used to check problem statement
            //   for some common errors like nonsmoothness or bad analytic gradient
            //
            // First, we create optimizer object and tune its properties:
            // * set box constraints
            // * set variable scales
            // * set stopping criteria
            //
            double[] x = new double[] { 0, 0 };
            double[] s = new double[] { 1, 1 };
            double[] bndl = new double[] { -1, -1 };
            double[] bndu = new double[] { +1, +1 };
            double epsg = 0;
            double epsf = 0;
            double epsx = 0.000001;
            int maxits = 0;
            alglib.minbleicstate state;
            alglib.minbleiccreate(x, out state);
            alglib.minbleicsetbc(state, bndl, bndu);
            alglib.minbleicsetscale(state, s);
            alglib.minbleicsetcond(state, epsg, epsf, epsx, maxits);

            //
            // Then we activate OptGuard integrity checking.
            //
            // OptGuard monitor helps to catch common coding and problem statement
            // issues, like:
            // * discontinuity of the target function (C0 continuity violation)
            // * nonsmoothness of the target function (C1 continuity violation)
            // * erroneous analytic gradient, i.e. one inconsistent with actual
            //   change in the target/constraints
            //
            // OptGuard is essential for early prototyping stages because such
            // problems often result in premature termination of the optimizer
            // which is really hard to distinguish from the correct termination.
            //
            // IMPORTANT: GRADIENT VERIFICATION IS PERFORMED BY MEANS OF NUMERICAL
            //            DIFFERENTIATION. DO NOT USE IT IN PRODUCTION CODE!!!!!!!
            //
            //            Other OptGuard checks add moderate overhead, but anyway
            //            it is better to turn them off when they are not needed.
            //
            alglib.minbleicoptguardsmoothness(state);
            alglib.minbleicoptguardgradient(state, 0.001);

            //
            // Optimize and evaluate results
            //
            alglib.minbleicreport rep;
            alglib.minbleicoptimize(state, function1_grad, null, null);
            alglib.minbleicresults(state, out x, out rep);
            System.Console.WriteLine("{0}", rep.terminationtype); // EXPECTED: 4
            System.Console.WriteLine("{0}", alglib.ap.format(x, 2)); // EXPECTED: [-1,1]

            //
            // Check that OptGuard did not report errors
            //
            // NOTE: want to test OptGuard? Try breaking the gradient - say, add
            //       1.0 to some of its components.
            //
            alglib.optguardreport ogrep;
            alglib.minbleicoptguardresults(state, out ogrep);
            System.Console.WriteLine("{0}", ogrep.badgradsuspected); // EXPECTED: false
            System.Console.WriteLine("{0}", ogrep.nonc0suspected); // EXPECTED: false
            System.Console.WriteLine("{0}", ogrep.nonc1suspected); // EXPECTED: false
            System.Console.ReadLine();
            return 0;
        }
    }
}
