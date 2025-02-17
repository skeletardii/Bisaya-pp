using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/*
The Evaluator executes the AST by walking through it.

For example, given the AST for 5 + 10, the evaluator:
    1. Visits the left node (5).
    2. Visits the right node (10).
    3. Applies the operator (+).
    4. Returns 15.

The evaluator can also:
    1. Handle variables (e.g., storing values in memory).
    2. Execute functions (e.g., calling built-in or user-defined functions).
    3. Manage scope (e.g., function-level vs. global variables).
 */
namespace Bisaya__.src.Core
{
    internal class Evaluator
    {
    }
}
