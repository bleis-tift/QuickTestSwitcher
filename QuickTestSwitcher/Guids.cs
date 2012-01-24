// Guids.cs
// MUST match guids.h
using System;

namespace bleistift.QuickTestSwitcher
{
    static class GuidList
    {
        public const string guidQuickTestSwitcherPkgString = "1401e4ff-559f-482b-89a8-05b4fcaa37cb";
        public const string guidQuickTestSwitcherCmdSetString = "0e497d82-f59a-4d71-80e5-a9af688aed71";

        public static readonly Guid guidQuickTestSwitcherCmdSet = new Guid(guidQuickTestSwitcherCmdSetString);
    };
}