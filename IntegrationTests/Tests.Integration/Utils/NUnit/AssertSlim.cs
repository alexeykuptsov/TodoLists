// Forked from https://github.com/nunit/nunit/blob/master/src/NUnitFramework/framework/Assert.cs
//
// Changes from original code: 
// * Removed all but ReportFailure and IncrementAssertCount methods used by other forked code.
//
// Original copyright ad license:
// Copyright (c) Charlie Poole, Rob Prouse and Contributors. MIT License - see LICENSE.txt

using NUnit.Framework.Constraints;
using NUnit.Framework.Internal;

namespace TodoLists.Tests.Integration.Utils.NUnit
{
    /// <summary>
    /// The Assert class contains a collection of static methods that
    /// implement the most common assertions used in NUnit.
    /// </summary>
    // Abstract because we support syntax extension by inheriting and declaring new static members.
    public abstract partial class AssertSlim
    {
        #region Helper Methods

        private static void ReportFailure(ConstraintResult result, string? message)
        {
            ReportFailure(result, message, null);
        }

        private static void ReportFailure(ConstraintResult result, string? message, params object?[]? args)
        {
            MessageWriter writer = new TextMessageWriter(message, args);
            result.WriteMessageTo(writer);

            throw new AssertionException(writer.ToString());
        }

        private static void IncrementAssertCount()
        {}
        #endregion
    }
}